using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Appointment
    {
        public int? StaffId { get; set; }
        public int? PatientId { get; set; }
        public DateTime? DateTime { get; set; }
        public string? Location { get; set; }
        public string? Topic { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual staff? Staff { get; set; }
    }
}
