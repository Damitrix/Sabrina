using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioRaceModifier
    {
        public ScenarioRaceModifier()
        {
            ScenarioRaceLink = new HashSet<ScenarioRaceLink>();
            ScenarioSavePlayer = new HashSet<ScenarioSavePlayer>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ScenarioRaceLink> ScenarioRaceLink { get; set; }
        public virtual ICollection<ScenarioSavePlayer> ScenarioSavePlayer { get; set; }
    }
}
