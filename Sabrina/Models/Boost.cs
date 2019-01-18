using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Boost
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public long? Channel { get; set; }
    }
}
