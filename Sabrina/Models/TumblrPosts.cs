using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class TumblrPosts
    {
        public long TumblrId { get; set; }
        public long IsLoli { get; set; }
        public DateTime? LastPosted { get; set; }
    }
}
