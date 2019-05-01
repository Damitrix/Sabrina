using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioRaceLink
    {
        public int Id { get; set; }
        public int RaceId { get; set; }
        public int RaceModifierId { get; set; }

        public virtual ScenarioRace Race { get; set; }
        public virtual ScenarioRaceModifier RaceModifier { get; set; }
    }
}
