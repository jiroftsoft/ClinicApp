using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Models.Entities.Clinic;

namespace ClinicApp.Models.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای ServiceTemplate
/// </summary>
public class ServiceTemplateConfig : EntityTypeConfiguration<ServiceTemplate>
{
    public ServiceTemplateConfig()
    {
        // نام جدول
        ToTable("ServiceTemplates");

        // کلید اصلی
        HasKey(st => st.ServiceTemplateId);

        // پیکربندی فیلدها
        Property(st => st.ServiceTemplateId)
            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        Property(st => st.ServiceCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_ServiceCode") { IsUnique = true }));

        Property(st => st.ServiceName)
            .IsRequired()
            .HasMaxLength(200);

        Property(st => st.DefaultTechnicalCoefficient)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_TechnicalCoefficient")));

        Property(st => st.DefaultProfessionalCoefficient)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_ProfessionalCoefficient")));

        Property(st => st.Description)
            .HasMaxLength(500);

        Property(st => st.IsHashtagged)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsHashtagged")));

        Property(st => st.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsActive")));

        // پیکربندی ISoftDelete
        Property(st => st.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsDeleted")));

        Property(st => st.DeletedAt)
            .IsOptional();

        Property(st => st.DeletedByUserId)
            .HasMaxLength(450);

        // پیکربندی ITrackable
        Property(st => st.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_CreatedAt")));

        Property(st => st.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_CreatedByUserId")));

        Property(st => st.UpdatedAt)
            .IsOptional();

        Property(st => st.UpdatedByUserId)
            .HasMaxLength(450);
    }
}
