using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model
{
    public class ForumMembership
    {
        public int Id { get; set; }
        public Forum Forum { get; set; }
        public UserProfile User { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime FinishedOn { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
