using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class User
    {
        public User()
        {
            Admins = new HashSet<Admin>();
            ContactUser1s = new HashSet<Contact>();
            ContactUser2s = new HashSet<Contact>();
            PatientDocuments = new HashSet<PatientDocument>();
            Patients = new HashSet<Patient>();
            staff = new HashSet<staff>();
        }

        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool? IsVerified { get; set; }

        public virtual ICollection<Admin> Admins { get; set; }
        public virtual ICollection<Contact> ContactUser1s { get; set; }
        public virtual ICollection<Contact> ContactUser2s { get; set; }
        public virtual ICollection<PatientDocument> PatientDocuments { get; set; }
        public virtual ICollection<Patient> Patients { get; set; }
        public virtual ICollection<staff> staff { get; set; }
    }
}
