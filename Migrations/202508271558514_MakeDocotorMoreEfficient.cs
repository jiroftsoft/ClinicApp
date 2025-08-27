namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class MakeDocotorMoreEfficient : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.AspNetUsers", "IX_Patient_Gender");
            DropIndex("dbo.Doctors", "IX_Doctor_Specialization_IsActive_IsDeleted");
            DropIndex("dbo.Doctors", "IX_Doctor_Specialization");
            CreateTable(
                "dbo.Specializations",
                c => new
                    {
                        SpecializationId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Specialization_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.SpecializationId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_Specialization_Name")
                .Index(t => t.IsActive, name: "IX_Specialization_IsActive")
                .Index(t => t.DisplayOrder, name: "IX_Specialization_DisplayOrder")
                .Index(t => t.IsDeleted, name: "IX_Specialization_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Specialization_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Specialization_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Specialization_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Specialization_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Specialization_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Specialization_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorSpecializations",
                c => new
                    {
                        SpecializationId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SpecializationId, t.DoctorId })
                .ForeignKey("dbo.Specializations", t => t.SpecializationId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .Index(t => t.SpecializationId)
                .Index(t => t.DoctorId);
            
            AddColumn("dbo.Doctors", "Degree", c => c.Byte(nullable: false));
            AddColumn("dbo.Doctors", "GraduationYear", c => c.Int());
            AddColumn("dbo.Doctors", "University", c => c.String(maxLength: 200));
            AddColumn("dbo.Doctors", "Gender", c => c.Byte(nullable: false));
            AddColumn("dbo.Doctors", "DateOfBirth", c => c.DateTime());
            AddColumn("dbo.Doctors", "HomeAddress", c => c.String(maxLength: 500));
            AddColumn("dbo.Doctors", "OfficeAddress", c => c.String(maxLength: 500));
            AddColumn("dbo.Doctors", "ConsultationFee", c => c.Decimal(precision: 18, scale: 0));
            AddColumn("dbo.Doctors", "ExperienceYears", c => c.Int());
            AddColumn("dbo.Doctors", "ProfileImageUrl", c => c.String(maxLength: 500));
            AddColumn("dbo.Doctors", "NationalCode", c => c.String(maxLength: 10));
            AddColumn("dbo.Doctors", "MedicalCouncilCode", c => c.String(maxLength: 20));
            AddColumn("dbo.Doctors", "Email", c => c.String(maxLength: 100));
            AddColumn("dbo.Departments", "Description", c => c.String());
            AddColumn("dbo.DoctorDepartments", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorDepartments", "DeletedAt", c => c.DateTime());
            AddColumn("dbo.DoctorDepartments", "DeletedByUserId", c => c.String(maxLength: 128));
            AddColumn("dbo.DoctorServiceCategories", "IsDeleted", c => c.Boolean(nullable: false));
            AlterColumn("dbo.AspNetUsers", "Gender", c => c.Byte(nullable: false));
            AlterColumn("dbo.Patients", "Gender", c => c.Byte(nullable: false));
            CreateIndex("dbo.AspNetUsers", "Gender", name: "IX_Patient_Gender");
            CreateIndex("dbo.Doctors", new[] { "University", "IsActive", "IsDeleted" }, name: "IX_Doctor_University_IsActive_IsDeleted");
            CreateIndex("dbo.Doctors", "Degree", name: "IX_Doctor_Degree");
            CreateIndex("dbo.Doctors", "GraduationYear", name: "IX_Doctor_GraduationYear");
            CreateIndex("dbo.Doctors", "University", name: "IX_Doctor_University");
            CreateIndex("dbo.Doctors", "Gender", name: "IX_Doctor_Gender");
            CreateIndex("dbo.Doctors", "DateOfBirth", name: "IX_Doctor_DateOfBirth");
            CreateIndex("dbo.Doctors", "ConsultationFee", name: "IX_Doctor_ConsultationFee");
            CreateIndex("dbo.Doctors", "ExperienceYears", name: "IX_Doctor_ExperienceYears");
            CreateIndex("dbo.Doctors", "NationalCode", name: "IX_Doctor_NationalCode");
            CreateIndex("dbo.Doctors", "MedicalCouncilCode", name: "IX_Doctor_MedicalCouncilCode");
            CreateIndex("dbo.Doctors", "Email", name: "IX_Doctor_Email");
            CreateIndex("dbo.DoctorDepartments", "DeletedByUserId");
            AddForeignKey("dbo.DoctorDepartments", "DeletedByUserId", "dbo.AspNetUsers", "Id");
            DropColumn("dbo.Doctors", "Specialization");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Doctors", "Specialization", c => c.String(maxLength: 250));
            DropForeignKey("dbo.Specializations", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSpecializations", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorSpecializations", "SpecializationId", "dbo.Specializations");
            DropForeignKey("dbo.Specializations", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Specializations", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorDepartments", "DeletedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.DoctorSpecializations", new[] { "DoctorId" });
            DropIndex("dbo.DoctorSpecializations", new[] { "SpecializationId" });
            DropIndex("dbo.Specializations", "IX_Specialization_UpdatedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_UpdatedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_CreatedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_CreatedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_DeletedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_DeletedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_IsDeleted");
            DropIndex("dbo.Specializations", "IX_Specialization_DisplayOrder");
            DropIndex("dbo.Specializations", "IX_Specialization_IsActive");
            DropIndex("dbo.Specializations", "IX_Specialization_Name");
            DropIndex("dbo.DoctorDepartments", new[] { "DeletedByUserId" });
            DropIndex("dbo.Doctors", "IX_Doctor_Email");
            DropIndex("dbo.Doctors", "IX_Doctor_MedicalCouncilCode");
            DropIndex("dbo.Doctors", "IX_Doctor_NationalCode");
            DropIndex("dbo.Doctors", "IX_Doctor_ExperienceYears");
            DropIndex("dbo.Doctors", "IX_Doctor_ConsultationFee");
            DropIndex("dbo.Doctors", "IX_Doctor_DateOfBirth");
            DropIndex("dbo.Doctors", "IX_Doctor_Gender");
            DropIndex("dbo.Doctors", "IX_Doctor_University");
            DropIndex("dbo.Doctors", "IX_Doctor_GraduationYear");
            DropIndex("dbo.Doctors", "IX_Doctor_Degree");
            DropIndex("dbo.Doctors", "IX_Doctor_University_IsActive_IsDeleted");
            DropIndex("dbo.AspNetUsers", "IX_Patient_Gender");
            AlterColumn("dbo.Patients", "Gender", c => c.Int(nullable: false));
            AlterColumn("dbo.AspNetUsers", "Gender", c => c.Int(nullable: false));
            DropColumn("dbo.DoctorServiceCategories", "IsDeleted");
            DropColumn("dbo.DoctorDepartments", "DeletedByUserId");
            DropColumn("dbo.DoctorDepartments", "DeletedAt");
            DropColumn("dbo.DoctorDepartments", "IsDeleted");
            DropColumn("dbo.Departments", "Description");
            DropColumn("dbo.Doctors", "Email");
            DropColumn("dbo.Doctors", "MedicalCouncilCode");
            DropColumn("dbo.Doctors", "NationalCode");
            DropColumn("dbo.Doctors", "ProfileImageUrl");
            DropColumn("dbo.Doctors", "ExperienceYears");
            DropColumn("dbo.Doctors", "ConsultationFee");
            DropColumn("dbo.Doctors", "OfficeAddress");
            DropColumn("dbo.Doctors", "HomeAddress");
            DropColumn("dbo.Doctors", "DateOfBirth");
            DropColumn("dbo.Doctors", "Gender");
            DropColumn("dbo.Doctors", "University");
            DropColumn("dbo.Doctors", "GraduationYear");
            DropColumn("dbo.Doctors", "Degree");
            DropTable("dbo.DoctorSpecializations");
            DropTable("dbo.Specializations",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Specialization_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            CreateIndex("dbo.Doctors", "Specialization", name: "IX_Doctor_Specialization");
            CreateIndex("dbo.Doctors", new[] { "Specialization", "IsActive", "IsDeleted" }, name: "IX_Doctor_Specialization_IsActive_IsDeleted");
            CreateIndex("dbo.AspNetUsers", "Gender", name: "IX_Patient_Gender");
        }
    }
}
