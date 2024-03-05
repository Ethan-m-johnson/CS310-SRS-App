using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Doctor
    {
        public int StaffId { get; set; }
        public string? Specialty { get; set; }
        public virtual staff Staff { get; set; } = null!;
    }
}
