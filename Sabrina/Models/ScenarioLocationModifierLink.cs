using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioLocationModifierLink
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int LocationModifierId { get; set; }

        public virtual ScenarioLocation Location { get; set; }
        public virtual ScenarioLocationModifier LocationModifier { get; set; }
    }
}
