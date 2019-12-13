using Microsoft.EntityFrameworkCore;
using Moq;
using OneClickForum.Domain.Forums;
using OneClickForum.Domain.Threads;
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
    public class ThreadManagerTests
    {
        private const int VisibleForumId = 999;

        [Fact]
        public void When_Getting_A_Thread_List_For_A_Forum_We_Get_Paged_Threads_For_That_Forum()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var result = forumThreadManager.GetThreadsListForForumAsync(new UserProfile() { Id = 999 }, VisibleForumId, 1, 20).Result;
                result.Data.PageItems.Any(thread => thread.Forum.Id != VisibleForumId).ShouldBeFalse();
                result.Data.Forum.ShouldNotBeNull();
            }
        }
        [Fact]
        public void When_Getting_A_Thread_List_For_A_Forum_We_Get_Threads_Ordered_By_Latest_Post()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var result = forumThreadManager.GetThreadsListForForumAsync(new UserProfile() { Id = 999 }, VisibleForumId, 1, 20).Result;
                var firstItem = result.Data.PageItems.First();
                var lastItem = result.Data.PageItems.Last();
                firstItem.LatestPostDate.ShouldBeGreaterThan(lastItem.LatestPostDate);
            }
        }

        [Fact]
        public void When_Getting_A_Thread_We_Get_The_Thread_Parent_Forum_And_Posts()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var result = forumThreadManager.GetForumThreadWithPosts(new UserProfile() { Id = 999 }, VisibleForumId, 101, 1, 20).Result;

                result.IsSuccess.ShouldBeTrue();
                result.Data.Forum.Id.ShouldBe(VisibleForumId);
                result.Data.Thread.Id.ShouldBe(101);
                result.Data.PageItems.Count.ShouldBe(20);
            }
        }
        [Fact]
        public void When_Getting_A_Thread_We_Get_Posts_Ordered_By_Earliest_First()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var result = forumThreadManager.GetForumThreadWithPosts(new UserProfile() { Id = 999 }, VisibleForumId, 101, 1, 20).Result;

                result.IsSuccess.ShouldBeTrue();
                result.Data.PageItems.First().CreatedOn.ShouldBeLessThan(result.Data.PageItems.Last().CreatedOn);
            }
        }
        [Fact]
        public void When_Adding_A_Thread_A_Thread_Gets_Added()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            const string TestThreadTitle = "Create test";
            const string TestFirstPostText = "This is post was created just now";
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.CreateThread(user, VisibleForumId, TestThreadTitle, TestFirstPostText).Result;
                result.Data.Title.ShouldBe(TestThreadTitle);
                result.Data.Posts.First().Text.ShouldBe(TestFirstPostText);

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Title == TestThreadTitle);
                thread.ShouldNotBeNull();
                thread.Posts.First().Text.ShouldBe(TestFirstPostText);
            }
        }
        [Fact]
        public void When_Adding_A_Thread_And_Forum_Does_Not_Exist_We_Get_Error_And_Thread_Is_Not_Created()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            const string TestThreadTitle = "Create test";
            const string TestFirstPostText = "This is post was created just now";
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = null }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.CreateThread(user, VisibleForumId, TestThreadTitle, TestFirstPostText).Result;
                result.Errors.Any().ShouldBeTrue();

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Title == TestThreadTitle);
                thread.ShouldBeNull();
            }
        }

        [Fact]
        public void When_Updating_A_Thread_A_Thread_And_FirstPost_Gets_Updated()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            const string TestThreadTitle = "Update test";
            const string TestFirstPostText = "This is post was updated just now";
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.UpdateThread(user, VisibleForumId, 101, TestThreadTitle, TestFirstPostText).Result;
                result.Data.Title.ShouldBe(TestThreadTitle);
                var firstPost = result.Data.Posts.First();
                firstPost.Text.ShouldBe(TestFirstPostText);
                firstPost.LastEditedOn.Year.ShouldBe(DateTime.UtcNow.Year);
                firstPost.LastEditedOn.Month.ShouldBe(DateTime.UtcNow.Month);
                firstPost.LastEditedOn.Day.ShouldBe(DateTime.UtcNow.Day);
                firstPost.LastEditedOn.Hour.ShouldBe(DateTime.UtcNow.Hour);
                firstPost.LastEditedOn.Minute.ShouldBe(DateTime.UtcNow.Minute);

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Id == 101);
                thread.ShouldNotBeNull();
                var firstPost = thread.Posts.OrderBy(post => post.CreatedOn).First();
                firstPost.Text.ShouldBe(TestFirstPostText);
                firstPost.LastEditedOn.Year.ShouldBe(DateTime.UtcNow.Year);
                firstPost.LastEditedOn.Month.ShouldBe(DateTime.UtcNow.Month);
                firstPost.LastEditedOn.Day.ShouldBe(DateTime.UtcNow.Day);
                firstPost.LastEditedOn.Hour.ShouldBe(DateTime.UtcNow.Hour);
                firstPost.LastEditedOn.Minute.ShouldBe(DateTime.UtcNow.Minute);
            }
        }
        [Fact]
        public void When_Updating_A_Thread_And_Forum_Does_Not_Exist_We_Get_An_Error()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            const string TestThreadTitle = "Update test";
            const string TestFirstPostText = "This is post was updated just now";
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = null }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.UpdateThread(user, VisibleForumId, 101, TestThreadTitle, TestFirstPostText).Result;
                result.Errors.Any().ShouldBeTrue();

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Id == 101);
                thread.Title.ShouldNotBe(TestThreadTitle);
            }
        }
        [Fact]
        public void When_Updating_A_Thread_And_Thread_Does_Not_Exist_We_Get_An_Error()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            const string TestThreadTitle = "Update test";
            const string TestFirstPostText = "This is post was updated just now";
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.UpdateThread(user, VisibleForumId, int.MaxValue, TestThreadTitle, TestFirstPostText).Result;
                result.Errors.Any().ShouldBeTrue();

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Id == 101);
                thread.Title.ShouldNotBe(TestThreadTitle);
            }
        }

        [Fact]
        public void When_Deleting_A_Thread_A_Thread_And_AllPosts_Get_Deleted()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.DeleteThread(user, VisibleForumId, 101).Result;
                result.IsSuccess.ShouldBeTrue();

                context.SaveChanges();
            }
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var thread = context.Threads.Include(thread => thread.Posts).SingleOrDefault(thread => thread.Id == 101);
                thread.ShouldBeNull();
                var postCountForThread = context.Posts.Count(post => post.Thread.Id == 101);
                postCountForThread.ShouldBe(0);
            }
        }
        [Fact]
        public void When_Deleting_A_Thread_And_Forum_Does_Not_Exist_We_Get_An_Error()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = null }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.DeleteThread(user, VisibleForumId, 101).Result;
                result.Errors.Any().ShouldBeTrue();

                context.SaveChanges();
            }
        }
        [Fact]
        public void When_Deleting_A_Thread_And_Thread_Does_Not_Exist_We_Get_An_Error()
        {
            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                SetupDatabase(context);
                context.SaveChangesAsync();
            }

            using (var context = new OneClickForumsDbContext<Forum, Thread, Post, UserProfile>(TestUtils.GetDatabaseOptions()))
            {
                var forum = context.Forums.Single(forum => forum.Id == VisibleForumId);
                var forumManagerMock = new Mock<IForumManager<Forum, UserProfile>>();
                forumManagerMock
                    .Setup(o => o.GetForumSingleAsync(It.IsAny<UserProfile>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(new DataResult<Forum>() { Data = forum }));

                var forumThreadManager = new ForumThreadManager<Forum, Thread, Post, UserProfile>(context, forumManagerMock.Object);

                var user = context.UserProfiles.Single(userprofile => userprofile.Id == 999);
                var result = forumThreadManager.DeleteThread(user, VisibleForumId, int.MaxValue).Result;
                result.Errors.Any().ShouldBeTrue();

                context.SaveChanges();
            }
        }

        private static void SetupDatabase(OneClickForumsDbContext<Forum, Thread, Post, UserProfile> context)
        {
            var user = new UserProfile() { Id = 999 };
            var visibleForum = new Forum() { Id = VisibleForumId, CreatedBy = user };
            var notVisibleForum = new Forum() { Id = 998, };

            for (int i = 1; i < 51; i++)
            {
                var thread = new Thread() { Id = i, Forum = notVisibleForum, };
                for (int j = 1; j < 100 - i; j++)
                {
                    var post = new Post() { Thread = thread, CreatedOn = new DateTime(1984, 11, 26).AddDays(j), Text = "Unhappy birthday" };
                    thread.Posts.Add(post);
                }
                notVisibleForum.Threads.Add(thread);
            }
            for (int i = 1; i < 51; i++)
            {
                var thread = new Thread() { Id = i + 100, Forum = notVisibleForum, };
                for (int j = 1; j < 100 - i; j++)
                {
                    var post = new Post() { Thread = thread, CreatedOn = new DateTime(1984, 11, 26).AddDays(j), Text = "Happy birthday" };
                    thread.Posts.Add(post);
                }
                visibleForum.Threads.Add(thread);
            }

            context.UserProfiles.Add(user);
            context.Forums.Add(visibleForum);
            context.Forums.Add(notVisibleForum);
            context.SaveChangesAsync();

            context.Threads.Include(thread => thread.Posts).ToList().ForEach(thread =>
            {
                var latestPostDate = thread.Posts.Max(o => o.CreatedOn);
                thread.LatestPostDate = latestPostDate;
            });
        }
    }
}
