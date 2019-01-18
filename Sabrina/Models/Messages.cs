using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Messages
    {
        public int MessageId { get; set; }
        public long AuthorId { get; set; }
        public string MessageText { get; set; }
        public long ChannelId { get; set; }
        public DateTime CreationDate { get; set; }

        public virtual Users Author { get; set; }
    }
}
