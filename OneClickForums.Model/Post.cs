using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneClickForums.Model
{
    public class Post
    {
        public int Id { get; set; }
        public Thread Thread { get; set; }
        public string Text { get; set; }
        public UserProfile CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastEditedOn { get; set; }

        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
