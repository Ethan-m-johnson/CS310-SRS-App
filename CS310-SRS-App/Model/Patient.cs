using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Patient
    {
        public int UserId { get; set; }
        public int PatientId { get; set; }
        public int? PrPhysicianId { get; set; }

        public virtual staff PatientNavigation { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual PatientDocument? PatientDocument { get; set; }
    }
}
