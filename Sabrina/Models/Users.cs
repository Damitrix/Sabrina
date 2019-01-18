using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Users
    {
        public Users()
        {
            DungeonSession = new HashSet<DungeonSession>();
            Messages = new HashSet<Messages>();
            SankakuImageVote = new HashSet<SankakuImageVote>();
            Slavereports = new HashSet<Slavereports>();
        }

        public long UserId { get; set; }
        public DateTime? BanTime { get; set; }
        public string BanReason { get; set; }
        public DateTime? MuteTime { get; set; }
        public string MuteReason { get; set; }
        public DateTime? LockTime { get; set; }
        public string LockReason { get; set; }
        public DateTime? DenialTime { get; set; }
        public DateTime? RuinTime { get; set; }
        public DateTime? SpecialTime { get; set; }
        public string SpecialReason { get; set; }
        public int? WalletEdges { get; set; }
        public int? TotalEdges { get; set; }

        public virtual KinkHashes KinkHashes { get; set; }
        public virtual UserSettings UserSettings { get; set; }
        public virtual ICollection<DungeonSession> DungeonSession { get; set; }
        public virtual ICollection<Messages> Messages { get; set; }
        public virtual ICollection<SankakuImageVote> SankakuImageVote { get; set; }
        public virtual ICollection<Slavereports> Slavereports { get; set; }
    }
}
