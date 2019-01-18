using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class UserSettings
    {
        public long UserId { get; set; }
        public int? WheelDifficulty { get; set; }
        public int? WheelTaskPreference { get; set; }
        public int? DungeonDifficulty { get; set; }
        public int? DungeonLevel { get; set; }

        public virtual Users User { get; set; }
    }
}
