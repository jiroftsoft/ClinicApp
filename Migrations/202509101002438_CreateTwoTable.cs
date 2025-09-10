namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTwoTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MedicalHistories",
                c => new
                    {
                        MedicalHistoryId = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        Type = c.Byte(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 2000),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        Severity = c.String(maxLength: 50),
                        DoctorName = c.String(maxLength: 100),
                        MedicalCenter = c.String(maxLength: 200),
                        Attachments = c.String(maxLength: 1000),
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
                    { "DynamicFilter_MedicalHistory_ActiveMedicalHistories", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_MedicalHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.MedicalHistoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.PatientId, t.Type, t.IsActive }, name: "IX_MedicalHistory_PatientId_Type_IsActive")
                .Index(t => new { t.PatientId, t.IsDeleted }, name: "IX_MedicalHistory_PatientId_IsDeleted")
                .Index(t => t.Type, name: "IX_MedicalHistory_Type")
                .Index(t => new { t.Type, t.StartDate, t.IsActive }, name: "IX_MedicalHistory_Type_StartDate_IsActive")
                .Index(t => t.StartDate, name: "IX_MedicalHistory_StartDate")
                .Index(t => t.IsActive, name: "IX_MedicalHistory_IsActive")
                .Index(t => t.IsDeleted, name: "IX_MedicalHistory_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_MedicalHistory_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_MedicalHistory_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_MedicalHistory_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_MedicalHistory_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_MedicalHistory_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_MedicalHistory_UpdatedByUserId");
            
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        ReportId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Type = c.Byte(nullable: false),
                        Description = c.String(maxLength: 1000),
                        Parameters = c.String(maxLength: 2000),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        FilePath = c.String(maxLength: 500),
                        Status = c.String(nullable: false, maxLength: 50),
                        RecordCount = c.Int(),
                        FileSize = c.Long(),
                        IsDownloadable = c.Boolean(nullable: false),
                        ExpiryDate = c.DateTime(),
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
                    { "DynamicFilter_Report_DownloadableReports", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Report_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ReportId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Type, name: "IX_Report_Type")
                .Index(t => new { t.Type, t.Status, t.IsDeleted }, name: "IX_Report_Type_Status_IsDeleted")
                .Index(t => new { t.StartDate, t.EndDate, t.Type }, name: "IX_Report_StartDate_EndDate_Type")
                .Index(t => t.StartDate, name: "IX_Report_StartDate")
                .Index(t => t.EndDate, name: "IX_Report_EndDate")
                .Index(t => t.Status, name: "IX_Report_Status")
                .Index(t => t.IsDownloadable, name: "IX_Report_IsDownloadable")
                .Index(t => t.IsDeleted, name: "IX_Report_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Report_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Report_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Report_CreatedAt")
                .Index(t => new { t.CreatedByUserId, t.CreatedAt }, name: "IX_Report_CreatedByUserId_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Report_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Report_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Report_UpdatedByUserId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reports", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reports", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reports", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalHistories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalHistories", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.MedicalHistories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalHistories", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Reports", "IX_Report_UpdatedByUserId");
            DropIndex("dbo.Reports", "IX_Report_UpdatedAt");
            DropIndex("dbo.Reports", "IX_Report_CreatedByUserId");
            DropIndex("dbo.Reports", "IX_Report_CreatedByUserId_CreatedAt");
            DropIndex("dbo.Reports", "IX_Report_CreatedAt");
            DropIndex("dbo.Reports", "IX_Report_DeletedByUserId");
            DropIndex("dbo.Reports", "IX_Report_DeletedAt");
            DropIndex("dbo.Reports", "IX_Report_IsDeleted");
            DropIndex("dbo.Reports", "IX_Report_IsDownloadable");
            DropIndex("dbo.Reports", "IX_Report_Status");
            DropIndex("dbo.Reports", "IX_Report_EndDate");
            DropIndex("dbo.Reports", "IX_Report_StartDate");
            DropIndex("dbo.Reports", "IX_Report_StartDate_EndDate_Type");
            DropIndex("dbo.Reports", "IX_Report_Type_Status_IsDeleted");
            DropIndex("dbo.Reports", "IX_Report_Type");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_UpdatedByUserId");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_UpdatedAt");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_CreatedByUserId");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_CreatedAt");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_DeletedByUserId");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_DeletedAt");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_IsDeleted");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_IsActive");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_StartDate");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_Type_StartDate_IsActive");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_Type");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_PatientId_IsDeleted");
            DropIndex("dbo.MedicalHistories", "IX_MedicalHistory_PatientId_Type_IsActive");
            DropTable("dbo.Reports",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Report_DownloadableReports", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Report_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.MedicalHistories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_MedicalHistory_ActiveMedicalHistories", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_MedicalHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
