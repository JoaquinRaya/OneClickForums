using OneClickForums.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForum.Domain
{
    public class PagedList<T>
    {
        public List<T> PageItems { get; set; }
        public int TotalCount { get; set; }
    }
    public class ForumWithThreads<TThread, TForum> : PagedList<TThread> where TForum : Forum
        where TThread : Thread
    {
        public TForum Forum { get; set; }
    }
    public class ForumThreadWithPosts<TForum, TThread, TPost> : PagedList<TPost> where TForum : Forum
        where TThread : Thread
        where TPost : Post
    {
        public Forum Forum { get; set; }
        public Thread Thread { get; set; }
    }
}
