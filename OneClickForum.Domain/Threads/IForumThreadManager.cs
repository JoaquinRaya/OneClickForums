using System.Threading.Tasks;
using OneClickForums.Model;

namespace OneClickForum.Domain.Threads
{
    public interface IForumThreadManager<TForum, TThread, TPost, TUserProfile>
        where TForum : Forum, new()
        where TThread : Thread, new()
        where TPost : Post, new()
        where TUserProfile : UserProfile
    {
        Task<DataResult<TThread>> CreateThread(TUserProfile user, int forumId, string title, string firstPostText);
        Task<DataResult> DeleteThread(TUserProfile user, int forumId, int threadId);
        Task<DataResult<ForumThreadWithPosts<TForum, TThread, TPost>>> GetForumThreadWithPosts(TUserProfile user, int forumId, int threadId, int currentPage, int pageSize);
        Task<DataResult<ForumWithThreads<TThread, TForum>>> GetThreadsListForForumAsync(TUserProfile user, int forumId, int currentPage, int pageSize);
        Task<DataResult<TThread>> UpdateThread(TUserProfile user, int forumId, int threadId, string title, string firstPostText);
    }
}