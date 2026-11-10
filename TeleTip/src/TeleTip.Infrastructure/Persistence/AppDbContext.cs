using MediatR;
using Microsoft.EntityFrameworkCore;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Infrastructure.Persistence
{
    public sealed class AppDbContext(
        DbContextOptions<AppDbContext> options,
        IPublisher publisher,
        ITenantService tenantService) : DbContext(options), IUnitOfWork
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<TelemedicineConsultation> TelemedicineConsultations => Set<TelemedicineConsultation>();
        public DbSet<IoTDevice> IoTDevices => Set<IoTDevice>();
        public DbSet<HealthMeasurement> HealthMeasurements => Set<HealthMeasurement>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<MedicalReport> MedicalReports => Set<MedicalReport>();
        public DbSet<AiAnalysis> AiAnalyses => Set<AiAnalysis>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Multi-tenant global query filter
            var tenantId = tenantService.GetCurrentTenantId();
            if (!string.IsNullOrEmpty(tenantId))
            {
                modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<Patient>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<Doctor>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<TelemedicineConsultation>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<IoTDevice>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<HealthMeasurement>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<Prescription>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<MedicalReport>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<AiAnalysis>().HasQueryFilter(p => p.TenantId == tenantId);
                modelBuilder.Entity<Notification>().HasQueryFilter(p => p.TenantId == tenantId);
            }

            ConfigureTeleCareEntities(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigureTeleCareEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(p => p.LastName).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Email).HasMaxLength(320);
                entity.Property(p => p.PhoneNumber).HasMaxLength(30);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("Doctors");
                entity.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(p => p.LastName).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Specialization).HasMaxLength(150).IsRequired();
                entity.Property(p => p.LicenseNumber).HasMaxLength(100).IsRequired();
                entity.HasIndex(p => p.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<TelemedicineConsultation>(entity =>
            {
                entity.ToTable("TelemedicineConsultations");
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(p => p.Notes).HasMaxLength(4000);
                entity.HasIndex(p => new { p.PatientId, p.ScheduledAt });
            });

            modelBuilder.Entity<IoTDevice>(entity =>
            {
                entity.ToTable("IoTDevices");
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
                entity.Property(p => p.SerialNumber).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(40);
                entity.HasIndex(p => p.SerialNumber).IsUnique();
            });

            modelBuilder.Entity<HealthMeasurement>(entity =>
            {
                entity.ToTable("HealthMeasurements");
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(40);
                entity.Property(p => p.Unit).HasMaxLength(30).IsRequired();
                entity.Property(p => p.Value).HasPrecision(18, 4);
                entity.HasIndex(p => new { p.PatientId, p.MeasuredAt });
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.ToTable("Prescriptions");
                entity.Property(p => p.MedicationName).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Dosage).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Instructions).HasMaxLength(2000).IsRequired();
            });

            modelBuilder.Entity<MedicalReport>(entity =>
            {
                entity.ToTable("MedicalReports");
                entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Content).HasMaxLength(10000).IsRequired();
            });

            modelBuilder.Entity<AiAnalysis>(entity =>
            {
                entity.ToTable("AiAnalyses");
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(40);
                entity.Property(p => p.AnalysisResult).HasMaxLength(10000).IsRequired();
                entity.Property(p => p.RiskScore).HasPrecision(5, 4);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(40);
                entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Message).HasMaxLength(2000).IsRequired();
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Publish domain events before saving
            var domainEvents = ChangeTracker.Entries<Entity>()
                .Select(e => e.Entity)
                .SelectMany(e =>
                {
                    var events = e.DomainEvents.ToList();
                    e.ClearDomainEvents();
                    return events;
                })
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
                await publisher.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
