using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CS310_SRS_App.Model
{
    public partial class CS310SRSDatabaseContext : DbContext
    {
        public CS310SRSDatabaseContext()
        {
        }

        public CS310SRSDatabaseContext(DbContextOptions<CS310SRSDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<Appointment> Appointments { get; set; } = null!;
        public virtual DbSet<CalenderBlock> CalenderBlocks { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; } = null!;
        public virtual DbSet<Doctor> Doctors { get; set; } = null!;
        public virtual DbSet<LegacyCalenderBlock> LegacyCalenderBlocks { get; set; } = null!;
        public virtual DbSet<Log> Logs { get; set; } = null!;
        public virtual DbSet<LogLine> LogLines { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Patient> Patients { get; set; } = null!;
        public virtual DbSet<PatientChart> PatientCharts { get; set; } = null!;
        public virtual DbSet<PatientDocument> PatientDocuments { get; set; } = null!;
        public virtual DbSet<Prescription> Prescriptions { get; set; } = null!;
        public virtual DbSet<User> Users{ get; set; } = null!;
        public virtual DbSet<staff> staff { get; set; } = null!;

        public virtual DbSet<ResetToken> ResetToken { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=tcp:cs310-srs-server.database.windows.net;Database=CS310-SRS-Database;User Id=CS310-Server-Administrator;Password=C!S!310!13579;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("Admin");

                entity.Property(e => e.AdminId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("AdminID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Admins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Admin_User");
            });

       

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => new { e.PatientId, e.StaffId, e.DateTime});

                entity.ToTable("Appointment");

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Location).IsUnicode(false);

                entity.Property(e => e.PatientId).HasColumnName("PatientID");

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.Property(e => e.Topic).IsUnicode(false);

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK_Appointment_Patient");

                entity.HasOne(d => d.Staff)
                    .WithMany()
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK_Appointment_Staff");
            });

            modelBuilder.Entity<CalenderBlock>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("CalenderBlock");

                entity.Property(e => e.End).HasColumnType("datetime");

                entity.Property(e => e.Start).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CalenderBlock_User");
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("Contact");

                entity.Property(e => e.ContactId)
                    .ValueGeneratedNever()
                    .HasColumnName("ContactID");

                entity.Property(e => e.User1Id).HasColumnName("User1ID");

                entity.Property(e => e.User2Id).HasColumnName("User2ID");

                entity.HasOne(d => d.User1)
                    .WithMany(p => p.ContactUser1s)
                    .HasForeignKey(d => d.User1Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Contact_User1");

                entity.HasOne(d => d.User2)
                    .WithMany(p => p.ContactUser2s)
                    .HasForeignKey(d => d.User2Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Contact_User2");
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Doctor");

                entity.Property(e => e.Specialty)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.HasOne(d => d.Staff)
                    .WithMany()
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Doctor_Staff");
            });

            modelBuilder.Entity<LegacyCalenderBlock>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.End).HasColumnType("datetime");

                entity.Property(e => e.Start).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_LegacyCalenderBlocks_User");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Date);

                entity.ToTable("Log");

                entity.Property(e => e.Date).HasColumnType("date");
            });

            modelBuilder.Entity<LogLine>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("LogLine");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.DateNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.Date)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LogLine_Log");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LogLine_User");
            });

            modelBuilder.Entity<Message>(entity =>
            {
               
                entity.ToTable("Message");

                entity.Property(e => e.MessageId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("MessageID");

                entity.Property(e => e.ContactId).HasColumnName("ContactID");

                entity.Property(e => e.DateSent).HasColumnType("datetime");

                entity.Property(e => e.Message1)
                    .IsUnicode(false)
                    .HasColumnName("Message");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Contact)
                    .WithMany()
                    .HasForeignKey(d => d.ContactId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_Contact");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_User");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patient");

                entity.Property(e => e.PatientId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PatientID");

                entity.Property(e => e.PrPhysicianId).HasColumnName("PrPhysicianID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.PatientNavigation)
                    .WithOne(p => p.Patient)
                    .HasForeignKey<Patient>(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Patient_Staff");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Patients)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Patient_User");
            });

            modelBuilder.Entity<PatientChart>(entity =>
            {
                entity.HasKey(e => e.PatientChartID); // Define primary key

                entity.ToTable("PatientChart");

                entity.Property(e => e.DBloodPressure)
                    .HasColumnType("decimal(30, 15)")
                    .HasColumnName("dBloodPressure");

                entity.Property(e => e.HeartRate).HasColumnType("decimal(30, 15)");

                entity.Property(e => e.PatientId).HasColumnName("PatientID");

                entity.Property(e => e.RespRate).HasColumnType("decimal(30, 15)");

                entity.Property(e => e.SBloodPressure)
                    .HasColumnType("decimal(30, 15)")
                    .HasColumnName("sBloodPressure");

                entity.Property(e => e.Tempk).HasColumnType("decimal(30, 15)");

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK_PatientChart_Patient");
            });
        

        modelBuilder.Entity<PatientDocument>(entity =>
            {
                entity.HasKey(e => e.PatientId);

                entity.ToTable("PatientDocument");

                entity.Property(e => e.PatientId)
                    .ValueGeneratedNever()
                    .HasColumnName("PatientID");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DocumentName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UploadUserId).HasColumnName("UploadUserID");

                entity.HasOne(d => d.Patient)
                    .WithOne(p => p.PatientDocument)
                    .HasForeignKey<PatientDocument>(d => d.PatientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PateintID");

                entity.HasOne(d => d.UploadUser)
                    .WithMany(p => p.PatientDocuments)
                    .HasForeignKey(d => d.UploadUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_uploadUserID");
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Prescription");

                entity.Property(e => e.DateDistributed).HasColumnType("datetime");

                entity.Property(e => e.DatePrescribed).HasColumnType("datetime");

                entity.Property(e => e.DirectionsForUse).IsUnicode(false);

                entity.Property(e => e.DosageForm)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Dose)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Expiration).HasColumnType("datetime");

                entity.Property(e => e.PatientId).HasColumnName("PatientID");

                entity.Property(e => e.PrescriberStaffId).HasColumnName("PrescriberStaffID");

                entity.Property(e => e.PrescriptionName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Pharmacy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("int");

                entity.HasOne(d => d.Patient)
                    .WithMany()
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("FK_Prescription_Patient");

                entity.HasOne(d => d.PrescriberStaff)
                    .WithMany()
                    .HasForeignKey(d => d.PrescriberStaffId)
                    .HasConstraintName("FK_Prescription_Staff");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Address).IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.Username).IsUnicode(false);
            });

            modelBuilder.Entity<staff>(entity =>
            {
                entity.ToTable("Staff");

                entity.Property(e => e.StaffId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("StaffID");

                entity.Property(e => e.Salary).HasColumnType("decimal(30, 15)");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.staff)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Staff_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
