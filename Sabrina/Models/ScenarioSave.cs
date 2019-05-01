using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioSave
    {
        public ScenarioSave()
        {
            ScenarioSavePlayer = new HashSet<ScenarioSavePlayer>();
        }

        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public int LocationId { get; set; }
        public int LocationModifierId { get; set; }

        public virtual ICollection<ScenarioSavePlayer> ScenarioSavePlayer { get; set; }
    }
}
