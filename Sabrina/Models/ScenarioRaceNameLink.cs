using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioRaceNameLink
    {
        public int Id { get; set; }
        public int NameId { get; set; }
        public int RaceId { get; set; }

        public virtual ScenarioName Name { get; set; }
        public virtual ScenarioRace Race { get; set; }
    }
}
