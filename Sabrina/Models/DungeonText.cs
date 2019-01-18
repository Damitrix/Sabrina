using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class DungeonText
    {
        public DungeonText()
        {
            DungeonVariable = new HashSet<DungeonVariable>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int RoomType { get; set; }
        public int TextType { get; set; }

        public virtual ICollection<DungeonVariable> DungeonVariable { get; set; }
    }
}
