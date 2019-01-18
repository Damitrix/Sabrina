using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Joiplatform
    {
        public Joiplatform()
        {
            CreatorPlatformLink = new HashSet<CreatorPlatformLink>();
            IndexedVideo = new HashSet<IndexedVideo>();
        }

        public int Id { get; set; }
        public string Url { get; set; }
        public string BaseUrl { get; set; }
        public string VideoIndexRoute { get; set; }
        public string VideoRoute { get; set; }

        public virtual ICollection<CreatorPlatformLink> CreatorPlatformLink { get; set; }
        public virtual ICollection<IndexedVideo> IndexedVideo { get; set; }
    }
}
