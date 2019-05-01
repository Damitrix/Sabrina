using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class WheelUserItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public long UserId { get; set; }

        public virtual Users User { get; set; }
    }
}
