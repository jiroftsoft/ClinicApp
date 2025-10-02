using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Models.Configurations;

public class BusinessRuleConfig : EntityTypeConfiguration<BusinessRule>
{
    public BusinessRuleConfig()
    {
        ToTable("BusinessRules");
        HasKey(b => b.BusinessRuleId);

        Property(b => b.RuleName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_RuleName") { IsUnique = false }));

        Property(b => b.Description).IsOptional().HasMaxLength(1000);

        Property(b => b.RuleType)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_RuleType")));

        Property(b => b.Priority)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Priority")));

        // JSON به nvarchar(max)
        Property(b => b.Conditions).IsOptional().HasColumnType("nvarchar(max)");
        Property(b => b.Actions).IsOptional().HasColumnType("nvarchar(max)");

        Property(b => b.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_IsActive")));

        Property(b => b.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_IsDeleted")));

        Property(b => b.StartDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_StartDate")));

        Property(b => b.EndDate)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_EndDate")));

        Property(b => b.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_CreatedAt")));

        Property(b => b.CreatedByUserId).IsOptional().HasMaxLength(128);
        Property(b => b.UpdatedByUserId).IsOptional().HasMaxLength(128);
        Property(b => b.DeletedByUserId).IsOptional().HasMaxLength(128);

        // روابط
        HasOptional(b => b.InsurancePlan)
            .WithMany()
            .HasForeignKey(b => b.InsurancePlanId)
            .WillCascadeOnDelete(false);

        HasOptional(b => b.ServiceCategory)
            .WithMany()
            .HasForeignKey(b => b.ServiceCategoryId)
            .WillCascadeOnDelete(false);

        // اگر ServiceId اضافه شد
        HasOptional(b => b.Service)
            .WithMany()
            .HasForeignKey(b => b.ServiceId)
            .WillCascadeOnDelete(false);

        HasOptional(b => b.CreatedByUser).WithMany().HasForeignKey(b => b.CreatedByUserId).WillCascadeOnDelete(false);
        HasOptional(b => b.UpdatedByUser).WithMany().HasForeignKey(b => b.UpdatedByUserId).WillCascadeOnDelete(false);
        HasOptional(b => b.DeletedByUser).WithMany().HasForeignKey(b => b.DeletedByUserId).WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی پیشنهادی (سریع‌ترین کوئری‌ها)
        Property(b => b.InsurancePlanId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 1 }));

        Property(b => b.ServiceCategoryId)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 2 }));

        Property(b => b.RuleType)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 3 }));

        Property(b => b.IsActive)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 4 }));

        Property(b => b.IsDeleted)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 5 }));

        Property(b => b.StartDate)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 6 }));

        Property(b => b.EndDate)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_BusinessRule_Plan_Type_Active_Date")
                    { Order = 7 }));
    }
}