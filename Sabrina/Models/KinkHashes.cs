using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class KinkHashes
    {
        public long UserId { get; set; }
        public string Hash { get; set; }
        public int? Privacy { get; set; }
        public string KinkList { get; set; }

        public virtual Users User { get; set; }
    }
}
