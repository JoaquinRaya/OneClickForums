using System.Collections.Generic;
using System.Threading.Tasks;
using OneClickForums.Model;

namespace OneClickForum.Domain.Forums
{
    public interface IForumManager<TForum, TUserProfile>
        where TForum : Forum, new()
        where TUserProfile : UserProfile
    {
        Task<DataResult<TForum>> AddForumAsync(string title, string description, TUserProfile user);
        Task<DataResult> DeleteForumAsync(int forumId, TUserProfile user);
        Task<DataResult<TForum>> GetForumSingleAsync(TUserProfile user, int forumId);
        Task<DataResult<PagedList<TForum>>> GetForumListAsync(TUserProfile user, int page, int pageSize);
        Task<DataResult<TForum>> UpdateForumAsync(int forumId, string title, string description, TUserProfile user);
    }
}