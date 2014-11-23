using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogSystem.Model
{
    public class User : IHasBlogRepository
    {
        public IBlogRepository Repository { private get; set; }

        public long Id { get; set; }
        public string Name { get; set; }
        public List<long> BlogIds { get; set; }

        public User()
        {
            this.BlogIds = new List<long>();
        }

        public List<Blog> GetBlogs()
        {
            return this.Repository.GetBlogs(this.BlogIds);
        }

        public Blog CreateNewBlog(Blog blog)
        {
            this.Repository.StoreBlogs(this, blog);

            return blog;
        }
    }
}
