using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model.Ef
{
    public class OneClickForumsDefaultDbContext : OneClickForumsDbContext<Forum, Thread, Post, UserProfile>
    {
        

        public OneClickForumsDefaultDbContext(DbContextOptions<OneClickForumsDbContext<Forum, Thread, Post, UserProfile>> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("OneClickForums");
        }
    }
}
