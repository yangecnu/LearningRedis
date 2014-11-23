using BlogSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace BlogSystem
{
public interface IBlogRepository
{
    void StoreUser(params User[] users);
    List<User> GetAllUsers();

    void StoreBlogs(User user, params Blog[] blogs);
    List<Blog> GetBlogs(IEnumerable<long> blogIds);
    List<Blog> GetAllBlogs();

    List<BlogPost> GetBlogPosts(IEnumerable<long> blogPostIds);
    void StoreNewBlogPosts(Blog blog, params BlogPost[] blogPosts);

    List<BlogPost> GetRecentBlogPosts();
    List<BlogPostComment> GetRecentBlogPostComments();
    IDictionary<string, double> GetTopTags(int take);
    HashSet<string> GetAllCategories();

    void StoreBlogPost(BlogPost blogPost);
    BlogPost GetBlogPost(int postId);
    List<BlogPost> GetBlogPostByCategry(string categoryName);
}
}
