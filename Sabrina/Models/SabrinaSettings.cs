using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SabrinaSettings
    {
        public long GuildId { get; set; }
        public DateTime? LastTumblrPost { get; set; }
        public DateTime? LastIntroductionPost { get; set; }
        public DateTime? LastTumblrUpdate { get; set; }
        public DateTime? LastWheelHelpPost { get; set; }
        public DateTime? LastDeepLearningPost { get; set; }
        public long? WheelChannel { get; set; }
        public long? FeetChannel { get; set; }
        public long? ContentChannel { get; set; }
    }
}
