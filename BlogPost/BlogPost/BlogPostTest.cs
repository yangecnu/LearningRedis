using BlogSystem.Model;
using NUnit.Framework;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace BlogSystem
{
        [TestFixture, Explicit, Category("Integration")]
        public class BlogPostTest
        {
            readonly RedisClient redisClient = new RedisClient("localhost");
            private IBlogRepository repository;

            [SetUp]
            public void OnBeforeEachTest()
            {
                redisClient.FlushAll();
                repository = new BlogRepository(redisClient);

                InsertTestData(repository);
            }

            public static void InsertTestData(IBlogRepository repository)
            {
                var ayende = new User { Name = "ayende" };
                var mythz = new User { Name = "mythz" };

                repository.StoreUser(ayende, mythz);

                var ayendeBlog = ayende.CreateNewBlog(new Blog { Tags = { "Architecture", ".NET", "Databases" } });

                var mythzBlog = mythz.CreateNewBlog(new Blog { Tags = { "Architecture", ".NET", "Databases" } });

                ayendeBlog.StoreNewBlogPosts(new BlogPost
                {
                    Title = "RavenDB",
                    Categories = new List<string> { "NoSQL", "DocumentDB" },
                    Tags = new List<string> { "Raven", "NoSQL", "JSON", ".NET" },
                    Comments = new List<BlogPostComment>
					{
						new BlogPostComment { Content = "First Comment!", CreatedDate = DateTime.UtcNow,},
						new BlogPostComment { Content = "Second Comment!", CreatedDate = DateTime.UtcNow,},
					}
                },
                    new BlogPost
                    {
                        BlogId = ayendeBlog.Id,
                        Title = "Cassandra",
                        Categories = new List<string> { "NoSQL", "Cluster" },
                        Tags = new List<string> { "Cassandra", "NoSQL", "Scalability", "Hashing" },
                        Comments = new List<BlogPostComment>
					{
						new BlogPostComment { Content = "First Comment!", CreatedDate = DateTime.UtcNow,}
					}
                    });

                mythzBlog.StoreNewBlogPosts(
                    new BlogPost
                    {
                        Title = "Redis",
                        Categories = new List<string> { "NoSQL", "Cache" },
                        Tags = new List<string> { "Redis", "NoSQL", "Scalability", "Performance" },
                        Comments = new List<BlogPostComment>
					{
						new BlogPostComment { Content = "First Comment!", CreatedDate = DateTime.UtcNow,}
					}
                    },
                    new BlogPost
                    {
                        Title = "Couch Db",
                        Categories = new List<string> { "NoSQL", "DocumentDB" },
                        Tags = new List<string> { "CouchDb", "NoSQL", "JSON" },
                        Comments = new List<BlogPostComment>
					{
						new BlogPostComment {Content = "First Comment!", CreatedDate = DateTime.UtcNow,}
					}
                    });
            }

            [Test]
            public void View_test_data()
            {
                var mythz = repository.GetAllUsers().First(x => x.Name == "mythz");
                var mythzBlogPostIds = mythz.GetBlogs().SelectMany(x => x.BlogPostIds);
                var mythzBlogPosts = repository.GetBlogPosts(mythzBlogPostIds);

                Debug.WriteLine(mythzBlogPosts.Dump());
                /* Output:
                [
                    {
                        Id: 3,
                        BlogId: 2,
                        Title: Redis,
                        Categories: 
                        [
                            NoSQL,
                            Cache
                        ],
                        Tags: 
                        [
                            Redis,
                            NoSQL,
                            Scalability,
                            Performance
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:24:47.516949Z
                            }
                        ]
                    },
                    {
                        Id: 4,
                        BlogId: 2,
                        Title: Couch Db,
                        Categories: 
                        [
                            NoSQL,
                            DocumentDB
                        ],
                        Tags: 
                        [
                            CouchDb,
                            NoSQL,
                            JSON
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:24:47.516949Z
                            }
                        ]
                    }
                ]			 
                */
            }

            [Test]
            public void Show_a_list_of_blogs()
            {
                var blogs = repository.GetAllBlogs();
                Debug.WriteLine(blogs.Dump());
                /* Output: 
                [
                    {
                        Id: 1,
                        UserId: 1,
                        UserName: ayende,
                        Tags: 
                        [
                            Architecture,
                            .NET,
                            Databases
                        ],
                        BlogPostIds: 
                        [
                            1,
                            2
                        ]
                    },
                    {
                        Id: 2,
                        UserId: 2,
                        UserName: mythz,
                        Tags: 
                        [
                            Architecture,
                            .NET,
                            Databases
                        ],
                        BlogPostIds: 
                        [
                            3,
                            4
                        ]
                    }
                ]
                */
            }

            [Test]
            public void Show_a_list_of_recent_posts_and_comments()
            {
                //Recent posts are already maintained in the repository
                var recentPosts = repository.GetRecentBlogPosts();
                var recentComments = repository.GetRecentBlogPostComments();

                Debug.WriteLine("Recent Posts:\n" + recentPosts.Dump());
                Debug.WriteLine("Recent Comments:\n" + recentComments.Dump());
                /* 
                Recent Posts:
                [
                    {
                        Id: 4,
                        BlogId: 2,
                        Title: Couch Db,
                        Categories: 
                        [
                            NoSQL,
                            DocumentDB
                        ],
                        Tags: 
                        [
                            CouchDb,
                            NoSQL,
                            JSON
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:25:39.7419361Z
                            }
                        ]
                    },
                    {
                        Id: 3,
                        BlogId: 2,
                        Title: Redis,
                        Categories: 
                        [
                            NoSQL,
                            Cache
                        ],
                        Tags: 
                        [
                            Redis,
                            NoSQL,
                            Scalability,
                            Performance
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:25:39.7419361Z
                            }
                        ]
                    },
                    {
                        Id: 2,
                        BlogId: 1,
                        Title: Cassandra,
                        Categories: 
                        [
                            NoSQL,
                            Cluster
                        ],
                        Tags: 
                        [
                            Cassandra,
                            NoSQL,
                            Scalability,
                            Hashing
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:25:39.7039339Z
                            }
                        ]
                    },
                    {
                        Id: 1,
                        BlogId: 1,
                        Title: RavenDB,
                        Categories: 
                        [
                            NoSQL,
                            DocumentDB
                        ],
                        Tags: 
                        [
                            Raven,
                            NoSQL,
                            JSON,
                            .NET
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:25:39.7039339Z
                            },
                            {
                                Content: Second Comment!,
                                CreatedDate: 2010-04-26T02:25:39.7039339Z
                            }
                        ]
                    }
                ]

                Recent Comments:
                [
                    {
                        Content: First Comment!,
                        CreatedDate: 2010-04-26T02:25:39.7419361Z
                    },
                    {
                        Content: First Comment!,
                        CreatedDate: 2010-04-26T02:25:39.7419361Z
                    },
                    {
                        Content: First Comment!,
                        CreatedDate: 2010-04-26T02:25:39.7039339Z
                    },
                    {
                        Content: Second Comment!,
                        CreatedDate: 2010-04-26T02:25:39.7039339Z
                    },
                    {
                        Content: First Comment!,
                        CreatedDate: 2010-04-26T02:25:39.7039339Z
                    }
                ]
			 
                 */
            }

            [Test]
            public void Show_a_TagCloud()
            {
                //Tags are maintained in the repository
                var tagCloud = repository.GetTopTags(5);
                Debug.WriteLine(tagCloud.Dump());
                /* Output:
                [
                    [
                        NoSQL,
                         4
                    ],
                    [
                        Scalability,
                         2
                    ],
                    [
                        JSON,
                         2
                    ],
                    [
                        Redis,
                         1
                    ],
                    [
                        Raven,
                         1
                    ]
                ]
                 */
            }

            [Test]
            public void Show_all_Categories()
            {
                //Categories are maintained in the repository
                var allCategories = repository.GetAllCategories();
                Debug.WriteLine(allCategories.Dump());
                /* Output:
                [
                    DocumentDB,
                    NoSQL,
                    Cluster,
                    Cache
                ]
                 */
            }

            [Test]
            public void Show_post_and_all_comments()
            {
                var postId = 1;
                var blogPost = repository.GetBlogPost(postId);
                Debug.WriteLine(blogPost.Dump());
                /* Output:
                {
                    Id: 1,
                    BlogId: 1,
                    Title: RavenDB,
                    Categories: 
                    [
                        NoSQL,
                        DocumentDB
                    ],
                    Tags: 
                    [
                        Raven,
                        NoSQL,
                        JSON,
                        .NET
                    ],
                    Comments: 
                    [
                        {
                            Content: First Comment!,
                            CreatedDate: 2010-04-26T02:00:24.5982749Z
                        },
                        {
                            Content: Second Comment!,
                            CreatedDate: 2010-04-26T02:00:24.5982749Z
                        }
                    ]
                }
                */
            }

            [Test]
            public void Add_comment_to_existing_post()
            {
                var postId = 1;
                var blogPost = repository.GetBlogPost(postId);

                blogPost.Comments.Add(
                    new BlogPostComment { Content = "Third Comment!", CreatedDate = DateTime.UtcNow });

                repository.StoreBlogPost(blogPost);

                var refreshBlogPost = repository.GetBlogPost(postId);
                Debug.WriteLine(refreshBlogPost.Dump());
                /* Output:
                {
                    Id: 1,
                    BlogId: 1,
                    Title: RavenDB,
                    Categories: 
                    [
                        NoSQL,
                        DocumentDB
                    ],
                    Tags: 
                    [
                        Raven,
                        NoSQL,
                        JSON,
                        .NET
                    ],
                    Comments: 
                    [
                        {
                            Content: First Comment!,
                            CreatedDate: 2010-04-26T02:08:13.5580978Z
                        },
                        {
                            Content: Second Comment!,
                            CreatedDate: 2010-04-26T02:08:13.5580978Z
                        },
                        {
                            Content: Third Comment!,
                            CreatedDate: 2010-04-26T02:08:13.6871052Z
                        }
                    ]
                }
                 */
            }

            [Test]
            public void Show_all_Posts_for_a_Category()
            {
                var documentDbPosts = repository.GetBlogPostByCategry("DocumentDB");
                Debug.WriteLine(documentDbPosts.Dump());
                /* Output:
                [
                    {
                        Id: 4,
                        BlogId: 2,
                        Title: Couch Db,
                        Categories: 
                        [
                            NoSQL,
                            DocumentDB
                        ],
                        Tags: 
                        [
                            CouchDb,
                            NoSQL,
                            JSON
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:16:08.0332362Z
                            }
                        ]
                    },
                    {
                        Id: 1,
                        BlogId: 1,
                        Title: RavenDB,
                        Categories: 
                        [
                            NoSQL,
                            DocumentDB
                        ],
                        Tags: 
                        [
                            Raven,
                            NoSQL,
                            JSON,
                            .NET
                        ],
                        Comments: 
                        [
                            {
                                Content: First Comment!,
                                CreatedDate: 2010-04-26T02:16:07.9662324Z
                            },
                            {
                                Content: Second Comment!,
                                CreatedDate: 2010-04-26T02:16:07.9662324Z
                            }
                        ]
                    }
                ]
                */
            }

        }
}
