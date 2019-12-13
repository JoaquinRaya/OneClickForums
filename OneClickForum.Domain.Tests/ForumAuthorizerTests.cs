using Microsoft.EntityFrameworkCore;
using OneClickForum.Domain.Forums;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace OneClickForum.Domain.Tests
{
    public class ForumAuthorizerTests
    {
        [Fact]
        public void When_Authorizing_Get_We_Get_A_Filter_Expression_That_Gets_Forums_Where_We_Are_The_Creator()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);

                context.Forums.Add(new Forum() { Id = 1, CreatedBy = user });
                context.Forums.Add(new Forum() { Id = 2, });
                context.Forums.Add(new Forum() { Id = 3, });
                context.Forums.Add(new Forum() { Id = 4, });
                context.Forums.Add(new Forum() { Id = 5, });

                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.Forums.Where(authorizer.AuthorizeGet(user)).Count().ShouldBe(1);
            }
        }
        [Fact]
        public void When_Authorizing_Get_We_Get_A_Filter_Expression_That_Gets_Forums_Where_We_Are_Members()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);

                var forumWithMembership = new Forum() { Id = 2, };
                var membership = new ForumMembership() { Forum = forumWithMembership, User = user };
                forumWithMembership.ForumMemberships.Add(membership);

                context.Forums.Add(new Forum() { Id = 1, });                
                context.Forums.Add(forumWithMembership);
                context.Forums.Add(new Forum() { Id = 3, });
                context.Forums.Add(new Forum() { Id = 4, });
                context.Forums.Add(new Forum() { Id = 5, });                

                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.Forums.Where(authorizer.AuthorizeGet(user)).Count().ShouldBe(1);
            }
        }
        [Fact]
        public void When_Authorizing_Add_We_Are_Authorized_When_User_Can_Add_Forums()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var authorizedUser = new UserProfile() { Id = 999, CanCreateForums = true };
            var unAuthorizedUser = new UserProfile() { Id = 998, CanCreateForums = false };

            authorizer.AuthorizeAdd(authorizedUser).Result.ShouldBeTrue();
            authorizer.AuthorizeAdd(unAuthorizedUser).Result.ShouldBeFalse();
        }
        [Fact]
        public void When_Authorizing_Update_We_Are_Authorized_When_User_Can_Update_Forums_Or_Is_Forum_Creator()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var authorizedUser = new UserProfile() { Id = 999, CanUpdateForums = true };
            var unAuthorizedUser = new UserProfile() { Id = 998, CanUpdateForums = false };
            var forumCreator = new UserProfile() { Id = 997, CanUpdateForums = false };
            var forum = new Forum() { Id = 1, CreatedBy = forumCreator };

            authorizer.AuthorizeUpdate(authorizedUser, forum).Result.ShouldBeTrue();
            authorizer.AuthorizeUpdate(unAuthorizedUser, forum).Result.ShouldBeFalse();
            authorizer.AuthorizeUpdate(forumCreator, forum).Result.ShouldBeTrue();
        }
        [Fact]
        public void When_Authorizing_Delete_We_Are_Authorized_When_User_Can_Delete_Forums_Or_Is_Forum_Creator()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var authorizedUser = new UserProfile() { Id = 999, CanDeleteForums = true };
            var unAuthorizedUser = new UserProfile() { Id = 998, CanDeleteForums = false };
            var forumCreator = new UserProfile() { Id = 997, CanDeleteForums = false };
            var forum = new Forum() { Id = 1, CreatedBy = forumCreator };

            authorizer.AuthorizeDelete(authorizedUser, forum).Result.ShouldBeTrue();
            authorizer.AuthorizeDelete(unAuthorizedUser, forum).Result.ShouldBeFalse();
            authorizer.AuthorizeDelete(forumCreator, forum).Result.ShouldBeTrue();
        }
    }
}
