using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuImageTag
    {
        public int Id { get; set; }
        public long ImageId { get; set; }
        public int TagId { get; set; }

        public virtual SankakuImage Image { get; set; }
        public virtual SankakuTag Tag { get; set; }
    }
}
