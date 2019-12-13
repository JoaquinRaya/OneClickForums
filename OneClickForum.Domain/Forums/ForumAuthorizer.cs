using OneClickForums.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OneClickForum.Domain.Forums
{
    public class ForumAuthorizer<TForum, TUserProfile> : IForumAuthorizer<TForum, TUserProfile> where TForum : Forum
        where TUserProfile : UserProfile
    {
        public virtual Expression<Func<TForum, bool>> AuthorizeGet(TUserProfile user)
        {
            Expression<Func<TForum, bool>> authorizeRule = forum => forum.CreatedBy.Id == user.Id || forum.ForumMemberships.Any(membership => membership.User.Id == user.Id);
            return authorizeRule;
        }
        public virtual Task<bool> AuthorizeAdd(TUserProfile user)
        {
            return Task.FromResult(user.CanCreateForums);
        }
        public virtual Task<bool> AuthorizeUpdate(TUserProfile user, TForum forum)
        {
            return Task.FromResult(user.CanUpdateForums || forum.CreatedBy.Id == user.Id);
        }
        public virtual Task<bool> AuthorizeDelete(TUserProfile user, TForum forum)
        {
            return Task.FromResult(user.CanDeleteForums || forum.CreatedBy.Id == user.Id);
        }
    }
}
