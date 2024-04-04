using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS310_SRS_App.Model
{
    public partial class Prescription
    {
       
        public int? PrescriptionId { get; set; }
        public int? PatientId { get; set; }
        public int? PrescriberStaffId { get; set; }
        public DateTime? DateDistributed { get; set; }
        public string? PrescriptionName { get; set; }
        public string? Pharmacy { get; set; }
        public decimal? Quantity { get; set; }
        public string? Dose { get; set; }
        public string? DosageForm { get; set; }
        public string? DirectionsForUse { get; set; }
        public int? Refills { get; set; }
        public DateTime? DatePrescribed { get; set; }
        public DateTime? Expiration { get; set; }
        public bool RefillRequested { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual staff? PrescriberStaff { get; set; }
    }
}
