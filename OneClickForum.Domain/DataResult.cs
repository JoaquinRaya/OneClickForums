using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneClickForum.Domain
{
    public class DataResult
    {
        public bool IsSuccess
        {
            get
            {
                return !this.Errors.Any();
            }
        }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Messages { get; set; }

        public DataResult()
        {
            this.Errors = new List<string>();
            this.Warnings = new List<string>();
            this.Messages = new List<string>();
        }
    }
    public class DataResult<T> : DataResult where T : new()
    {
        public T Data { get; set; }

    }
}
