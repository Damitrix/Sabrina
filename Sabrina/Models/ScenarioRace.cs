using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioRace
    {
        public ScenarioRace()
        {
            ScenarioRaceLink = new HashSet<ScenarioRaceLink>();
            ScenarioRaceNameLink = new HashSet<ScenarioRaceNameLink>();
            ScenarioSavePlayer = new HashSet<ScenarioSavePlayer>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ScenarioRaceLink> ScenarioRaceLink { get; set; }
        public virtual ICollection<ScenarioRaceNameLink> ScenarioRaceNameLink { get; set; }
        public virtual ICollection<ScenarioSavePlayer> ScenarioSavePlayer { get; set; }
    }
}
