using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class CreatorPlatformLink
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public int PlatformId { get; set; }
        public string Identification { get; set; }

        public virtual Creator Creator { get; set; }
        public virtual Joiplatform Platform { get; set; }
    }
}
