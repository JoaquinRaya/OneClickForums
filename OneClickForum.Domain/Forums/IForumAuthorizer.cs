using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OneClickForums.Model;

namespace OneClickForum.Domain.Forums
{
    public interface IForumAuthorizer<TForum, TUserProfile>
        where TForum : Forum
        where TUserProfile : UserProfile
    {
        Task<bool> AuthorizeAdd(TUserProfile user);
        Task<bool> AuthorizeDelete(TUserProfile user, TForum forum);
        Expression<Func<TForum, bool>> AuthorizeGet(TUserProfile user);
        Task<bool> AuthorizeUpdate(TUserProfile user, TForum forum);
    }
}