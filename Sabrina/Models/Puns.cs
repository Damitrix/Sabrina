using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Puns
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime? LastUsed { get; set; }
    }
}
