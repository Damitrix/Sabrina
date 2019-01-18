using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class DungeonVariable
    {
        public int Id { get; set; }
        public int TextId { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }

        public virtual DungeonText Text { get; set; }
    }
}
