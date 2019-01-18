using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Finisher
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public int CreatorId { get; set; }
        public int Type { get; set; }

        public virtual Creator Creator { get; set; }
    }
}
