using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class DungeonSession
    {
        public int SessionId { get; set; }
        public long UserId { get; set; }
        public string DungeonData { get; set; }
        public string RoomGuid { get; set; }

        public virtual Users User { get; set; }
    }
}
