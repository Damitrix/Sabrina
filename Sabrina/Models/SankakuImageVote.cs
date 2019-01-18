using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuImageVote
    {
        public int Id { get; set; }
        public long ImageId { get; set; }
        public int VoteValue { get; set; }
        public long UserId { get; set; }

        public virtual SankakuImage Image { get; set; }
        public virtual Users User { get; set; }
    }
}
