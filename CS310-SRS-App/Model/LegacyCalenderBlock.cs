using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class LegacyCalenderBlock
    {
        public int? UserId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public virtual User? User { get; set; }
    }
}
