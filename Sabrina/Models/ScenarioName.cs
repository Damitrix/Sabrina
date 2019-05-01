using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioName
    {
        public ScenarioName()
        {
            ScenarioRaceNameLink = new HashSet<ScenarioRaceNameLink>();
            ScenarioSavePlayerNameLink = new HashSet<ScenarioSavePlayerNameLink>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }

        public virtual ICollection<ScenarioRaceNameLink> ScenarioRaceNameLink { get; set; }
        public virtual ICollection<ScenarioSavePlayerNameLink> ScenarioSavePlayerNameLink { get; set; }
    }
}
