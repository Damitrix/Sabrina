using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioLocation
    {
        public ScenarioLocation()
        {
            ScenarioLocationModifierLink = new HashSet<ScenarioLocationModifierLink>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ScenarioLocationModifierLink> ScenarioLocationModifierLink { get; set; }
    }
}
