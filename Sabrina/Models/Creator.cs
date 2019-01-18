using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Creator
    {
        public Creator()
        {
            CreatorPlatformLink = new HashSet<CreatorPlatformLink>();
            Finisher = new HashSet<Finisher>();
            IndexedVideo = new HashSet<IndexedVideo>();
        }

        public int Id { get; set; }
        public long? DiscordUserId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<CreatorPlatformLink> CreatorPlatformLink { get; set; }
        public virtual ICollection<Finisher> Finisher { get; set; }
        public virtual ICollection<IndexedVideo> IndexedVideo { get; set; }
    }
}
