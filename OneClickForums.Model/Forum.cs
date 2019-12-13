using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model
{
    public class Forum
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Thread> Threads { get; set; }
        public UserProfile CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserProfile LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public List<ForumMembership> ForumMemberships { get; set; }
        public Forum()
        {
            this.Threads = new List<Thread>();
            this.ForumMemberships = new List<ForumMembership>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
