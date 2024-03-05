using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class LogLine
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public TimeSpan? Time { get; set; }
        public string? Description { get; set; }
        public virtual Log DateNavigation { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
