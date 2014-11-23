﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogSystem.Model
{
    public class BlogPost
    {
        public BlogPost()
        {
            this.Categories = new List<string>();
            this.Tags = new List<string>();
            this.Comments = new List<BlogPostComment>();
        }

        public long Id { get; set; }
        public long BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
        public List<BlogPostComment> Comments { get; set; }
    }
}
