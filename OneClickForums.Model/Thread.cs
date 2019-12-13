using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model
{
    public class Thread
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Forum Forum { get; set; }
        public UserProfile CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<Post> Posts { get; set; }
        public DateTime LatestPostDate { get; set; }

        public Thread()
        {
            this.Posts = new List<Post>();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
