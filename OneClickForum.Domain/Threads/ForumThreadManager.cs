using Microsoft.EntityFrameworkCore;
using OneClickForum.Domain.Forums;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickForum.Domain.Threads
{
    public class ForumThreadManager<TForum, TThread, TPost, TUserProfile> : IForumThreadManager<TForum, TThread, TPost, TUserProfile> where TForum : Forum, new()
        where TThread : Thread, new()
        where TPost : Post, new()
        where TUserProfile : UserProfile
    {
        protected OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext;
        private readonly IForumManager<TForum, TUserProfile> forumManager;

        public ForumThreadManager(OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext, IForumManager<TForum, TUserProfile> forumManager)
        {
            this.oneClickForumsDbContext = oneClickForumsDbContext;
            this.forumManager = forumManager;
        }
        public async Task<DataResult<ForumWithThreads<TThread, TForum>>> GetThreadsListForForumAsync(TUserProfile user, int forumId, int currentPage, int pageSize)
        {
            var ret = new DataResult<ForumWithThreads<TThread, TForum>>();
            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<ForumWithThreads<TThread, TForum>>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;

            var query = this.oneClickForumsDbContext.Threads
                .Where(thread => thread.Forum.Id == forum.Id)
                .OrderByDescending(thread => thread.Posts.Max(post => post.CreatedOn));

            var count = await query.CountAsync();
            var data = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
            ret.Data = new ForumWithThreads<TThread, TForum>()
            {
                Forum = forum,
                TotalCount = count,
                PageItems = data,
            };

            return ret;
        }
        public async Task<DataResult<ForumThreadWithPosts<TForum, TThread, TPost>>> GetForumThreadWithPosts(TUserProfile user, int forumId, int threadId, int currentPage, int pageSize)
        {
            var ret = new DataResult<ForumThreadWithPosts<TForum, TThread, TPost>>();

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<ForumThreadWithPosts<TForum, TThread, TPost>>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;
            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);

            if (thread == null) return new DataResult<ForumThreadWithPosts<TForum, TThread, TPost>>() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };

            ret.Data = new ForumThreadWithPosts<TForum, TThread, TPost>();
            ret.Data.Forum = forum;
            ret.Data.Thread = thread;

            var query = this.oneClickForumsDbContext.Posts
                .Where(post => post.Thread.Forum.Id == forumId)
                .Where(post => post.Thread.Id == threadId)
                .OrderBy(post => post.CreatedOn);

            var count = await query.CountAsync();
            var data = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            ret.Data.PageItems = data;

            return ret;
        }

        public async Task<DataResult<TThread>> CreateThread(TUserProfile user, int forumId, string title, string firstPostText)
        {
            if (string.IsNullOrEmpty(title)) return new DataResult<TThread>() { Errors = new List<string>() { "Title cannot be empty" } };
            if (string.IsNullOrEmpty(firstPostText)) return new DataResult<TThread>() { Errors = new List<string>() { "First post text cannot be empty" } };

            var ret = new DataResult<TThread>();

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<TThread>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;

            var now = DateTime.UtcNow;
            var thread = new TThread()
            {
                CreatedBy = user,
                CreatedOn = now,
                Forum = forum,
                LatestPostDate = now,
                Title = title,
            };
            TPost firstPost = new TPost()
            {
                CreatedBy = user,
                CreatedOn = now,
                Thread = thread,
                Text = firstPostText,
            };
            thread.Posts.Add(firstPost);
            forum.Threads.Add(thread);

            ret.Data = thread;

            this.oneClickForumsDbContext.Threads.Add(thread);
            this.oneClickForumsDbContext.Posts.Add(firstPost);

            return ret;
        }

        public async Task<DataResult<TThread>> UpdateThread(TUserProfile user, int forumId, int threadId, string title, string firstPostText)
        {
            var ret = new DataResult<TThread>();

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult<TThread>() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;

            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);
            if (thread == null) return new DataResult<TThread>() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };

            var now = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(title)) thread.Title = title;
            if (!string.IsNullOrEmpty(firstPostText))
            {
                var firstPost = await this.oneClickForumsDbContext.Posts.Where(post => post.Thread.Id == threadId).OrderBy(post => post.CreatedOn).FirstAsync();
                firstPost.Text = firstPostText;
                firstPost.LastEditedOn = now;
            }

            ret.Data = thread;
            return ret;
        }
        public async Task<DataResult> DeleteThread(TUserProfile user, int forumId, int threadId)
        {
            var ret = new DataResult();

            var getForumResult = await forumManager.GetForumSingleAsync(user, forumId);
            if (getForumResult.Data == null) return new DataResult() { Errors = new List<string>() { $"Forum {forumId} does not exist or user {user.ForumUserName} can't access it" } };

            var forum = getForumResult.Data;

            var thread = await this.oneClickForumsDbContext.Threads.SingleOrDefaultAsync(thread => thread.Forum.Id == forumId && thread.Id == threadId);
            if (thread == null) return new DataResult() { Errors = new List<string>() { $"Thread {threadId} does not exist on forum {forumId}" } };

            this.oneClickForumsDbContext.Threads.Remove(thread);
            this.oneClickForumsDbContext.RemoveRange(this.oneClickForumsDbContext.Posts.Where(post => post.Thread.Id == threadId));

            return ret;
        }
    }
}
