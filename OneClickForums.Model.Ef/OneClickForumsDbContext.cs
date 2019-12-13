using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model.Ef
{
    public class OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> : DbContext 
        where TForum : Forum
        where TThread: Thread
        where TPost : Post
        where TUserProfile : UserProfile
    {
        public DbSet<TForum> Forums { get; set; }
        public DbSet<TThread> Threads { get; set; }
        public DbSet<TPost> Posts { get; set; }
        public DbSet<TUserProfile> UserProfiles { get; set; }

        public OneClickForumsDbContext(DbContextOptions<OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile>> options) : base(options)
        {
        }
    }
}
