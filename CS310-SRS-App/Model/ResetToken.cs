namespace CS310_SRS_App.Model
{
    public class ResetToken
    {
        public int ResetTokenId { get; set; } // Primary key
        public string? Token { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int UserId { get; set; } // Foreign key referencing User
        public virtual User User { get; set; } // Navigation property
    }
}
