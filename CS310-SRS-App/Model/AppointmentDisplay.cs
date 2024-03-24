using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class AppointmentDisplay
    {
        public int? StaffId { get; set; }
        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? StaffName { get; set; }
        public DateTime? dateTime { get; set; }
        public string? Location { get; set; }
        public string? Topic { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual staff? Staff { get; set; }
        public static int Compare(AppointmentDisplay o1, AppointmentDisplay o2)
        {
            return DateTime.Compare((DateTime)o1.dateTime, ((DateTime)o2.dateTime));
        }

    }

   
}
