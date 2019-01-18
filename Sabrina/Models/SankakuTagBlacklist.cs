using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuTagBlacklist
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public long ChannelId { get; set; }

        public virtual SankakuTag Tag { get; set; }
    }
}
