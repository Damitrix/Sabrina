using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class WheelOutcome
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public DateTime Time { get; set; }
        public int Type { get; set; }
        public byte IsUserReport { get; set; }
    }
}
