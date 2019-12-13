using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string ForumUserName { get; set; }
        public List<Post> Posts { get; set; }
        public List<ForumMembership> ForumMemberships { get; set; }
        public bool CanCreateForums { get; set; }
        public bool CanUpdateForums { get; set; }
        public bool CanDeleteForums { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
