using System;
using System.Collections.Generic;

namespace CS310_SRS_App.Model
{
    public partial class Admin
    {
        public int AdminId { get; set; }
        public int? UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
