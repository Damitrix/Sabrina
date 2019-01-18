using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuTagWhitelist
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public long ChannelId { get; set; }
        public int Weight { get; set; }

        public virtual SankakuTag Tag { get; set; }
    }
}
