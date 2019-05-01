using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuImage
    {
        public SankakuImage()
        {
            SankakuImageTag = new HashSet<SankakuImageTag>();
            SankakuImageVote = new HashSet<SankakuImageVote>();
            SankakuPost = new HashSet<SankakuPost>();
        }

        public long Id { get; set; }
        public int Score { get; set; }
        public int Rating { get; set; }
        public int RatingCount { get; set; }

        public virtual ICollection<SankakuImageTag> SankakuImageTag { get; set; }
        public virtual ICollection<SankakuImageVote> SankakuImageVote { get; set; }
        public virtual ICollection<SankakuPost> SankakuPost { get; set; }
    }
}
