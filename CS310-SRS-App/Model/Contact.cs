using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Contact
    {
        public int ContactId { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }

        public virtual User User1 { get; set; } = null!;
        public virtual User User2 { get; set; } = null!;
    }
}
