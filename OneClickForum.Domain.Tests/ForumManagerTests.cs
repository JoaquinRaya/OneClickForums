using Moq;
using OneClickForum.Domain.Forums;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OneClickForum.Domain.Tests
{
    public class ForumManagerTests
    {
        [Fact]
        public void When_Getting_Forums_List_We_Get_Forums_Where_We_Are_Authorized()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);

                //We are authorized when we are members of the forum or the creator
                var forumWithMembership = new Forum() { Id = 2, };
                var membership = new ForumMembership() { Forum = forumWithMembership, User = user };
                forumWithMembership.ForumMemberships.Add(membership);
                context.Forums.Add(new Forum() { Id = 1, CreatedBy = user });
                context.Forums.Add(forumWithMembership);

                context.Forums.Add(new Forum() { Id = 3, });
                context.Forums.Add(new Forum() { Id = 4, });
                context.Forums.Add(new Forum() { Id = 5, });

                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizer);
                var result = forumManager.GetForumListAsync(user, 1, 20).Result;
                result.Data.PageItems.Count.ShouldBe(2);
                result.Data.TotalCount.ShouldBe(2);
            }
        }
        [Fact]
        public void When_Getting_Forums_List_And_There_Are_No_Forums_We_Get_A_Warning()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizer);
                var result = forumManager.GetForumListAsync(user, 1, 20).Result;
                result.Warnings.Any().ShouldBeTrue();
                result.Data.ShouldBeNull();
            }
        }
        [Fact]
        public void When_Getting_Forums_List_The_List_Should_Be_Paged()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);

                //We are authorized when we are members of the forum or the creator
                var forumWithMembership = new Forum() { Id = 2, };
                var membership = new ForumMembership() { Forum = forumWithMembership, User = user };
                forumWithMembership.ForumMemberships.Add(membership);
                context.Forums.Add(new Forum() { Id = 1, CreatedBy = user });
                context.Forums.Add(forumWithMembership);

                for (int i = 3; i < 101; i++)
                {
                    context.Forums.Add(new Forum() { Id = i, CreatedBy = user });
                }
                for (int i = 200; i < 210; i++)
                {
                    context.Forums.Add(new Forum() { Id = i, });
                }

                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizer);
                const int pageSize = 20;

                var result = forumManager.GetForumListAsync(user, 2, pageSize).Result;
                result.Data.PageItems.Count.ShouldBe(pageSize);
                result.Data.TotalCount.ShouldBe(100);
            }
        }
        [Fact]
        public void When_Getting_Single_Forum_We_Get_A_Forum_Where_We_Are_Authorized()
        {
            var authorizer = new ForumAuthorizer<Forum, UserProfile>();
            var user = new UserProfile() { Id = 999 };

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);

                //We are authorized when we are members of the forum or the creator
                var forumWithMembership = new Forum() { Id = 2, };
                var membership = new ForumMembership() { Forum = forumWithMembership, User = user };
                forumWithMembership.ForumMemberships.Add(membership);
                context.Forums.Add(new Forum() { Id = 1, CreatedBy = user });
                context.Forums.Add(forumWithMembership);

                context.Forums.Add(new Forum() { Id = 3, });
                context.Forums.Add(new Forum() { Id = 4, });
                context.Forums.Add(new Forum() { Id = 5, });

                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizer);
                forumManager.GetForumSingleAsync(user, 2).Result.Data.ShouldNotBeNull();
                forumManager.GetForumSingleAsync(user, 3).Result.Warnings.Any().ShouldBeTrue();
                forumManager.GetForumSingleAsync(user, 3).Result.Data.ShouldBeNull();
            }
        }
        [Fact]
        public void When_Adding_A_Forum_The_Forum_Gets_Added()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeAdd(It.IsAny<UserProfile>())).Returns(Task.FromResult(true));

            var user = new UserProfile() { Id = 999, };
            DataResult<Forum> result;
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                result = forumManager.AddForumAsync("Test", "Description", user).Result;
                context.SaveChanges();
            }
            result.Data.Id.ShouldNotBe(0);
            result.Data.Title.ShouldBe("Test");
        }
        [Fact]
        public void When_Adding_A_Forum_And_We_Are_Not_Authorized_An_Error_Is_Returned()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeAdd(It.IsAny<UserProfile>())).Returns(Task.FromResult(false));

            var user = new UserProfile() { Id = 999, };
            
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                var result = forumManager.AddForumAsync("Test", "Description", user).Result;
                result.Errors.Any();
            }
        }

        [Fact]
        public void When_Updating_A_Forum_The_Forum_Gets_Updated()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeUpdate(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(true));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };
            var forum = new Forum() { Id = 999, CreatedBy = user, Title = "Test" };
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);
                context.Forums.Add(forum);
                context.SaveChanges();
            }

            DataResult<Forum> result;
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                result = forumManager.UpdateForumAsync(forum.Id, "Changed", "Description", user).Result;
                context.SaveChanges();
            }

            Forum resultFromDb;
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                resultFromDb = context.Forums.Single(o => o.Id == forum.Id);
            }

            resultFromDb.Title.ShouldBe("Changed");
        }
        [Fact]
        public void When_Updating_A_Forum_And_We_Are_Not_Authorized_An_Error_Is_Returned()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeUpdate(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(false));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };
            var forum = new Forum() { Id = 999, Title = "Test" };
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);
                context.Forums.Add(forum);
                context.SaveChanges();
            }
                        
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                var result = forumManager.UpdateForumAsync(forum.Id, "Test", "Description", user).Result;
                result.Errors.Any();
            }
        }
        [Fact]
        public void When_Updating_A_Forum_That_Does_Not_Exist_Returns_Error()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeUpdate(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(true));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };
            

            DataResult<Forum> result;
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                result = forumManager.UpdateForumAsync(999, "Changed", "Description", user).Result;
                result.Errors.Any();
            }
        }

        [Fact]
        public void When_Deleting_A_Forum_The_Forum_Gets_Deleted()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeDelete(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(true));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };
            var forum = new Forum() { Id = 999, CreatedBy = user, Title = "Test" };
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);
                context.Forums.Add(forum);
                context.SaveChanges();
            }

            DataResult result;
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                result = forumManager.DeleteForumAsync(forum.Id, user).Result;
                context.SaveChanges();
            }

            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.Forums.SingleOrDefault(o => o.Id == forum.Id).ShouldBeNull();
            }
        }
        [Fact]
        public void When_Deleting_A_Forum_And_We_Are_Not_Authorized_Exception_Gets_Thrown()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeDelete(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(false));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };
            var forum = new Forum() { Id = 999, Title = "Test" };
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                context.UserProfiles.Add(user);
                context.Forums.Add(forum);
                context.SaveChanges();
            }

            
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                var result = forumManager.DeleteForumAsync(forum.Id, user).Result;
                result.Errors.Any();
            }
        }
        [Fact]
        public void When_Deleting_A_Forum_That_Does_Not_Exist_Returns_Error()
        {
            var authorizerMock = new Mock<IForumAuthorizer<Forum, UserProfile>>();
            authorizerMock.Setup(o => o.AuthorizeUpdate(It.IsAny<UserProfile>(), It.IsAny<Forum>())).Returns(Task.FromResult(true));
            authorizerMock.Setup(o => o.AuthorizeGet(It.IsAny<UserProfile>())).Returns((o) => true);

            var user = new UserProfile() { Id = 999, };


            
            using (var context = new OneClickForumsDefaultDbContext(TestUtils.GetDatabaseOptions()))
            {
                var forumManager = new ForumManager<Forum, Thread, Post, UserProfile>(context, authorizerMock.Object);
                var result = forumManager.DeleteForumAsync(999, user).Result;
                result.Errors.Any();
            }
        }
    }
}
