using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class PatientChart
    {
        public int? PatientId { get; set; }
        public decimal? SBloodPressure { get; set; }
        public decimal? DBloodPressure { get; set; }
        public decimal HeartRate { get; set; }
        public decimal? RespRate { get; set; }
        public decimal? Tempk { get; set; }

        public virtual Patient? Patient { get; set; }
    }
}
