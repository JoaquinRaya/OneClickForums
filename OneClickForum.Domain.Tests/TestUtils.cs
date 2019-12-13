using Microsoft.EntityFrameworkCore;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OneClickForum.Domain.Tests
{
    public static class TestUtils
    {
        public static DbContextOptions<OneClickForumsDbContext<Forum, Thread, Post, UserProfile>> GetDatabaseOptions([CallerMemberName] string databaseName = null)
        {
            var builder = new DbContextOptionsBuilder<OneClickForumsDbContext<Forum, Thread, Post, UserProfile>>();
            builder.UseInMemoryDatabase(databaseName);
            var options = builder.Options;
            return options;
        }
    }
}
