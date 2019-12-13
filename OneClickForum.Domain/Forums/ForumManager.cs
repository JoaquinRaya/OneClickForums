using Microsoft.EntityFrameworkCore;
using OneClickForums.Model;
using OneClickForums.Model.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OneClickForum.Domain.Forums
{
    public class ForumManager<TForum, TThread, TPost, TUserProfile> : IForumManager<TForum, TUserProfile> where TForum : Forum, new()
        where TThread : Thread
        where TPost : Post
        where TUserProfile : UserProfile
    {
        protected OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext;
        protected IForumAuthorizer<TForum, TUserProfile> forumAuthorizer;

        public ForumManager(OneClickForumsDbContext<TForum, TThread, TPost, TUserProfile> oneClickForumsDbContext, IForumAuthorizer<TForum, TUserProfile> forumAuthorizer)
        {
            this.oneClickForumsDbContext = oneClickForumsDbContext;
            this.forumAuthorizer = forumAuthorizer;
        }

        public virtual async Task<DataResult<PagedList<TForum>>> GetForumListAsync(TUserProfile user, int page, int pageSize)
        {
            var query = oneClickForumsDbContext.Forums
                .Where(forumAuthorizer.AuthorizeGet(user));
            var count = await query.CountAsync();

            if (count == 0) return new DataResult<PagedList<TForum>>() { Warnings = new List<string>() { $"Could not find any forums for user {user.ForumUserName} or he is not authorized to see any" } };

            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new DataResult<PagedList<TForum>>()
            {
                Data = new PagedList<TForum>()
                {
                    PageItems = data,
                    TotalCount = count,
                }
            };
        }
        public virtual async Task<DataResult<TForum>> GetForumSingleAsync(TUserProfile user, int forumId)
        {
            var result = await oneClickForumsDbContext.Forums
                .Where(forumAuthorizer.AuthorizeGet(user))
                .SingleOrDefaultAsync(forum => forum.Id == forumId);

            return new DataResult<TForum>()
            {
                Data = result,
                Warnings = result == null ? new List<string>() { $"Could not find forum { forumId } for user { user.ForumUserName } or he is not authorized to see any" } : null
            };
        }

        public virtual async Task<DataResult<TForum>> AddForumAsync(string title, string description, TUserProfile user)
        {
            if (!await forumAuthorizer.AuthorizeAdd(user)) return new DataResult<TForum>() { Errors = new List<string>() { $"User {user.ForumUserName} can't create new forums" } };

            var forum = new TForum()
            {
                Title = title,
                Description = description,
                CreatedBy = user,
                CreatedOn = DateTime.UtcNow,
            };

            var entry = oneClickForumsDbContext.Forums.Add(forum);

            return new DataResult<TForum>() { Data = entry.Entity };
        }

        public virtual async Task<DataResult<TForum>> UpdateForumAsync(int forumId, string title, string description, TUserProfile user)
        {
            var getForumResult = await GetForumSingleAsync(user, forumId);
            if (!getForumResult.IsSuccess) return getForumResult;
            if (getForumResult.Data is null) return new DataResult<TForum>() { Errors = new List<string>() { $"Could not find forum { forumId } for user { user.ForumUserName } or he is not authorized to see any" } };

            var forum = getForumResult.Data;
            if (!await forumAuthorizer.AuthorizeUpdate(user, forum)) return new DataResult<TForum>() { Errors = new List<string>() { $"User { user.ForumUserName } can't update forums and/or is not the forum creator" } };

            if (forum.Title != title) forum.Title = title;
            if (forum.Description != description) forum.Description = description;

            getForumResult.Data = forum;

            return getForumResult;
        }
        public virtual async Task<DataResult> DeleteForumAsync(int forumId, TUserProfile user)
        {
            var getForumResult = await GetForumSingleAsync(user, forumId);
            if (!getForumResult.IsSuccess) return getForumResult;
            if (getForumResult.Data is null) return new DataResult() { Errors = new List<string>() { $"Could not find forum { forumId } for user { user.ForumUserName } or he is not authorized to see any" } };
            if (!await forumAuthorizer.AuthorizeDelete(user, getForumResult.Data)) return new DataResult() { Errors = new List<string>() { $"User {user.ForumUserName} can't delete forums and/or is not the forum creator" } };

            var forum = getForumResult.Data;
            
            oneClickForumsDbContext.Forums.Remove(forum);

            return new DataResult();
        }
    }

}
