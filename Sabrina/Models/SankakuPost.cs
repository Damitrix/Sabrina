using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuPost
    {
        public int Id { get; set; }
        public long ImageId { get; set; }
        public long MessageId { get; set; }
        public DateTime Date { get; set; }

        public virtual SankakuImage Image { get; set; }
    }
}
