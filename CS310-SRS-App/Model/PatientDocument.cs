using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class PatientDocument
    {
        public int PatientId { get; set; }
        public string DocumentName { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public int UploadUserId { get; set; }

        public virtual Patient Patient { get; set; } = null!;
        public virtual User UploadUser { get; set; } = null!;
    }
}
