using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Message
    {
        public int ContactId { get; set; }
        public int UserId { get; set; }
        public DateTime? DateSent { get; set; }
        public string? Message1 { get; set; }

        public virtual Contact Contact { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
