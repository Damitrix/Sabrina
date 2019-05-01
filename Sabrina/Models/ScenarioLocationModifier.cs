using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioLocationModifier
    {
        public ScenarioLocationModifier()
        {
            ScenarioLocationModifierLink = new HashSet<ScenarioLocationModifierLink>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ScenarioLocationModifierLink> ScenarioLocationModifierLink { get; set; }
    }
}
