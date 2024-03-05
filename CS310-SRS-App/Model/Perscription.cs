using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Perscription
    {
        public int? PatientId { get; set; }
        public int? PerscriberStaffId { get; set; }
        public DateTime? DateDistributed { get; set; }
        public string? PerscriptionName { get; set; }
        public string? Pharmacy { get; set; }
        public decimal? Quantity { get; set; }
        public string? Dose { get; set; }
        public string? DosageForm { get; set; }
        public string? DirectionsForUse { get; set; }
        public int? Refills { get; set; }
        public DateTime? DatePerscribed { get; set; }
        public DateTime? Expiration { get; set; }

        public virtual Patient? Patient { get; set; }
        public virtual staff? PerscriberStaff { get; set; }
    }
}
