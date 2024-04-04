using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class staff
    {
        public int UserId { get; set; }

        public string ?Specialty { get; set; }
        public int StaffId { get; set; }
        public decimal? Salary { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Patient? Patient { get; set; }
    }
}
