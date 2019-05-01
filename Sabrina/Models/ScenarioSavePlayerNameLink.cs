using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class ScenarioSavePlayerNameLink
    {
        public int Id { get; set; }
        public int SavePlayerId { get; set; }
        public int NameId { get; set; }

        public virtual ScenarioName Name { get; set; }
        public virtual ScenarioSavePlayer SavePlayer { get; set; }
    }
}
