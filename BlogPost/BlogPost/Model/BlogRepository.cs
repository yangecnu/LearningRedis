using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogSystem.Model
{
    public class BlogRepository : IBlogRepository
    {

        private readonly IRedisClient redis;
        const string TagCloudKey = "urn:TagCloud";
        const string RecentBlogPostsKey = "urn:BlogPosts:RecentPosts";
        const string RecentBlogPostCommentsKey = "urn:BlogPostComment:RecentComments";
        const string AllCategoriesKey = "urn:Categories";
        const string CategoryTypeName = "Category";

        public BlogRepository(IRedisClient client)
        {
            this.redis = client;
        }

        public List<Blog> GetAllBlogs()
        {
            var redisBlogs = redis.As<Blog>();
            return Inject(redisBlogs.GetAll());
        }

        public List<T> Inject<T>(IEnumerable<T> entities)
                             where T : IHasBlogRepository
        {
            var entitiesList = entities.ToList();
            entitiesList.ForEach(x => x.Repository = this);
            return entitiesList;
        }

        public List<BlogPost> GetRecentBlogPosts()
        {
            var redisBlogPosts = redis.As<BlogPost>();
            return redisBlogPosts.Lists[RecentBlogPostsKey].GetAll();
        }
  
        public List<BlogPostComment> GetRecentBlogPostComments()
        {
            var redisComments = redis.As<BlogPostComment>();
            return redisComments.Lists[RecentBlogPostCommentsKey].GetAll();
        }

        public void StoreUser(params User[] users)
        {
            var redisUsers = redis.As<User>();
            Inject(users);
            users.Where(x => x.Id == default(int))
                .Each(x => x.Id = redisUsers.GetNextSequence());

            redisUsers.StoreAll(users);
        }

        public List<User> GetAllUsers()
        {
            var redisUsers = redis.As<User>();
            return Inject(redisUsers.GetAll());
        }

        public void StoreBlogs(User user, params Blog[] blogs)
        {
            var redisBlogs = redis.As<Blog>();
            foreach (var blog in blogs)
            {
                blog.Id = blog.Id != default(int) ? blog.Id : redisBlogs.GetNextSequence();
                blog.UserId = user.Id;
                blog.UserName = user.Name;

                user.BlogIds.AddIfNotExists(blog.Id);
            }

            using (var trans = redis.CreateTransaction())
            {
                trans.QueueCommand(x => x.Store(user));
                trans.QueueCommand(x => x.StoreAll(blogs));

                trans.Commit();
            }

            Inject(blogs);
        }

        public List<Blog> GetBlogs(IEnumerable<long> blogIds)
        {
            var redisBlogs = redis.As<Blog>();
            return Inject(
                redisBlogs.GetByIds(blogIds.Map(x => x.ToString())));
        }

        public List<BlogPost> GetBlogPosts(IEnumerable<long> blogPostIds)
        {
            var redisBlogPosts = redis.As<BlogPost>();
            return redisBlogPosts.GetByIds(blogPostIds.Map(x => x.ToString())).ToList();
        }

        public void StoreNewBlogPosts(Blog blog, params BlogPost[] blogPosts)
        {
            var redisBlogPosts = redis.As<BlogPost>();
            var redisComments = redis.As<BlogPostComment>();

            //Get wrapper around a strongly-typed Redis server-side List
            var recentPosts = redisBlogPosts.Lists[RecentBlogPostsKey];
            var recentComments = redisComments.Lists[RecentBlogPostCommentsKey];

            foreach (var blogPost in blogPosts)
            {
                blogPost.Id = blogPost.Id != default(int) ? blogPost.Id : redisBlogPosts.GetNextSequence();
                blogPost.BlogId = blog.Id;
                blog.BlogPostIds.AddIfNotExists(blogPost.Id);

                //List of Recent Posts and comments
                recentPosts.Prepend(blogPost);
                blogPost.Comments.ForEach(recentComments.Prepend);

                //Tag Cloud
                blogPost.Tags.ForEach(x =>
                    redis.IncrementItemInSortedSet(TagCloudKey, x, 1));

                //List of all post categories
                blogPost.Categories.ForEach(x =>
                        redis.AddItemToSet(AllCategoriesKey, x));

                //Map of Categories to BlogPost Ids
                blogPost.Categories.ForEach(x =>
                        redis.AddItemToSet(UrnId.Create(CategoryTypeName, x), blogPost.Id.ToString()));
            }

            //Rolling list of recent items, only keep the last 5
            recentPosts.Trim(0, 4);
            recentComments.Trim(0, 4);

            using (var trans = redis.CreateTransaction())
            {
                trans.QueueCommand(x => x.Store(blog));
                trans.QueueCommand(x => x.StoreAll(blogPosts));

                trans.Commit();
            }
        }

        public IDictionary<string, double> GetTopTags(int take)
        {
            return redis.GetRangeWithScoresFromSortedSetDesc(TagCloudKey, 0, take - 1);
        }
       
        public HashSet<string> GetAllCategories()
        {
            return redis.GetAllItemsFromSet(AllCategoriesKey);
        }

        public void StoreBlogPost(BlogPost blogPost)
        {
            redis.Store(blogPost);
        }

        public BlogPost GetBlogPost(int postId)
        {
            return redis.GetById<BlogPost>(postId);
        }

        public List<BlogPost> GetBlogPostByCategry(string categoryName)
        {
            var categoryUrn = UrnId.Create(CategoryTypeName, categoryName);
            var documentDbPostIds = redis.GetAllItemsFromSet(categoryUrn);

            return redis.GetByIds<BlogPost>(documentDbPostIds.ToArray()).ToList();
        }
    }
}
