using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogSystem.Model
{
public class Blog : IHasBlogRepository
{
    public IBlogRepository Repository { get; set; }

    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Tags { get; set; }
    public List<long> BlogPostIds { get; set; }

    public Blog()
    {
        this.Tags = new List<string>();
        this.BlogPostIds = new List<long>();
    }

    public List<BlogPost> GetBlogPosts()
    {
        return this.Repository.GetBlogPosts(this.BlogPostIds);
    }

    public void StoreNewBlogPosts(params BlogPost[] blogPosts)
    {
        this.Repository.StoreNewBlogPosts(this, blogPosts);
    }
}
}
