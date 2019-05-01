using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioSavePlayer
    {
        public ScenarioSavePlayer()
        {
            ScenarioSavePlayerNameLink = new HashSet<ScenarioSavePlayerNameLink>();
        }

        public int Id { get; set; }
        public long UserId { get; set; }
        public int RaceId { get; set; }
        public int RaceModifierId { get; set; }
        public int SaveId { get; set; }

        public virtual ScenarioRace Race { get; set; }
        public virtual ScenarioRaceModifier RaceModifier { get; set; }
        public virtual ScenarioSave Save { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<ScenarioSavePlayerNameLink> ScenarioSavePlayerNameLink { get; set; }
    }
}
