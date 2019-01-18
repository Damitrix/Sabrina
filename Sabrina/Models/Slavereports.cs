using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Slavereports
    {
        public int SlaveReportId { get; set; }
        public DateTime TimeOfReport { get; set; }
        public long UserId { get; set; }
        public int Edges { get; set; }
        public string SessionOutcome { get; set; }
        public long TimeSpan { get; set; }

        public virtual Users User { get; set; }
    }
}
