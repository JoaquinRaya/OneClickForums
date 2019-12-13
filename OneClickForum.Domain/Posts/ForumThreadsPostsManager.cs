using Microsoft.EntityFrameworkCore;
using OneClickForum.Domain.Forums;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickForum.Domain.Posts
{
    public class ForumThreadsPostsManager<TForum, TThread, TPost, TUserProfile> where TForum : Forum, new()
        where TThread : Thread, new()
        where TPost : Post, new()
        where TUserProfile : UserProfile
    {
        private readonly OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext;
        private readonly IForumManager<TForum, TUserProfile> forumManager;

        public ForumThreadsPostsManager(OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext, IForumManager<TForum, TUserProfile> forumManager)
        {
            this.oneClickForumsDbContext = oneClickForumsDbContext;
            this.forumManager = forumManager;
        }

        public async Task<DataResult<TPost>> CreatePost(TUserProfile user, int forumId, int threadId, string postText)
        {
            if (string.IsNullOrEmpty(postText)) return new DataResult<TPost>() { Errors = new List<string>() { "Post text must not be empty" } };

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<TPost>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;
            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);
            if (thread == null) return new DataResult<TPost>() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };
            

            var now = DateTime.UtcNow;
            var post = new TPost()
            {
                Thread = thread,
                CreatedBy = user,
                CreatedOn = now,
                Text = postText,
            };
            thread.Posts.Add(post);
            thread.LatestPostDate = now;

            this.oneClickForumsDbContext.Posts.Add(post);

            return new DataResult<TPost>() { Data = post };
        }
        public async Task<DataResult<TPost>> UpdatePost(TUserProfile user, int forumId, int threadId, int postId, string postText)
        {
            if (string.IsNullOrEmpty(postText)) return new DataResult<TPost>() { Errors = new List<string>() { "Post text must not be empty" } };

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<TPost>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;
            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);
            if (thread == null) return new DataResult<TPost>() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };

            var post = await this.oneClickForumsDbContext.Posts.SingleOrDefaultAsync(post => post.Thread.Forum.Id == forumId && post.Thread.Id == threadId && post.Id == postId);
            if (post == null) return new DataResult<TPost>() { Errors = new List<string>() { $"Post {postId} does not exist on forum {forumId} and thread {threadId}" } };

            var now = DateTime.UtcNow;
            if(post.Text != postText)
            {
                post.Text = postText;
                post.LastEditedOn = now;
            }

            return new DataResult<TPost>() { Data = post };
        }

        public async Task<DataResult> DeletePost(TUserProfile user, int forumId, int threadId, int postId, string postText)
        {
            if (string.IsNullOrEmpty(postText)) return new DataResult() { Errors = new List<string>() { "Post text must not be empty" } };

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;
            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);
            if (thread == null) return new DataResult() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };

            var post = await this.oneClickForumsDbContext.Posts.SingleOrDefaultAsync(post => post.Thread.Forum.Id == forumId && post.Thread.Id == threadId && post.Id == postId);
            if (post == null) return new DataResult() { Errors = new List<string>() { $"Post {postId} does not exist on forum {forumId} and thread {threadId}" } };

            this.oneClickForumsDbContext.Posts.Remove(post);

            return new DataResult();
        }
    }
}
