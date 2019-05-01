using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SabrinaVersion
    {
        public int VersionNumber { get; set; }
        public string Description { get; set; }
        public short WasAnnounced { get; set; }
    }
}
