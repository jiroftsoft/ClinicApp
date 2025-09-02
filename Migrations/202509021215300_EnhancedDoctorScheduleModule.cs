namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class EnhancedDoctorScheduleModule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppointmentSlots",
                c => new
                    {
                        SlotId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        SlotDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Status = c.Byte(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PatientId = c.Int(),
                        AppointmentId = c.Int(),
                        IsEmergencySlot = c.Boolean(nullable: false),
                        IsWalkInAllowed = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_AppointmentSlot_ActiveAppointmentSlots", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_AppointmentSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.SlotId)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.DoctorSchedules", t => t.ScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.ScheduleId, t.SlotDate, t.Status }, name: "IX_AppointmentSlot_ScheduleId_Date_Status")
                .Index(t => new { t.SlotDate, t.StartTime, t.Status }, name: "IX_AppointmentSlot_Date_Time_Status")
                .Index(t => new { t.PatientId, t.SlotDate }, name: "IX_AppointmentSlot_PatientId_Date")
                .Index(t => t.AppointmentId)
                .Index(t => t.IsDeleted, name: "IX_AppointmentSlot_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.ScheduleExceptions",
                c => new
                    {
                        ExceptionId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        Type = c.Byte(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        StartTime = c.Time(precision: 7),
                        EndTime = c.Time(precision: 7),
                        Reason = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        IsRecurring = c.Boolean(nullable: false),
                        RecurrencePattern = c.String(maxLength: 100),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ScheduleException_ActiveScheduleExceptions", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_ScheduleException_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ExceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorSchedules", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => new { t.ScheduleId, t.StartDate, t.EndDate }, name: "IX_ScheduleException_ScheduleId_DateRange")
                .Index(t => new { t.StartDate, t.Type, t.IsDeleted }, name: "IX_ScheduleException_StartDate_Type_IsDeleted")
                .Index(t => t.IsDeleted, name: "IX_ScheduleException_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.ScheduleTemplates",
                c => new
                    {
                        TemplateId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        TemplateName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        Type = c.Byte(nullable: false),
                        TemplateData = c.String(maxLength: 4000),
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
                    { "DynamicFilter_ScheduleTemplate_ActiveScheduleTemplates", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_ScheduleTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TemplateId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorSchedules", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => new { t.ScheduleId, t.Type, t.IsActive }, name: "IX_ScheduleTemplate_ScheduleId_Type_IsActive")
                .Index(t => new { t.IsDefault, t.IsActive, t.IsDeleted }, name: "IX_ScheduleTemplate_Default_Active_Deleted")
                .Index(t => t.IsDeleted, name: "IX_ScheduleTemplate_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            AddColumn("dbo.DoctorSchedules", "MaxAppointmentsPerDay", c => c.Int(nullable: false));
            AddColumn("dbo.DoctorSchedules", "MinAdvanceBookingDays", c => c.Int(nullable: false));
            AddColumn("dbo.DoctorSchedules", "MaxAdvanceBookingDays", c => c.Int(nullable: false));
            AddColumn("dbo.DoctorSchedules", "AllowSameDayBooking", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorSchedules", "RequirePatientRegistration", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorSchedules", "ConsultationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.DoctorSchedules", "CancellationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.DoctorSchedules", "CancellationNoticeHours", c => c.Int(nullable: false));
            AddColumn("dbo.DoctorSchedules", "AllowEmergencyBooking", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorSchedules", "AllowWalkInPatients", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorSchedules", "MaxWalkInPatientsPerDay", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ScheduleTemplates", "ScheduleId", "dbo.DoctorSchedules");
            DropForeignKey("dbo.ScheduleTemplates", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleTemplates", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleTemplates", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleExceptions", "ScheduleId", "dbo.DoctorSchedules");
            DropForeignKey("dbo.ScheduleExceptions", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleExceptions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ScheduleExceptions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AppointmentSlots", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AppointmentSlots", "ScheduleId", "dbo.DoctorSchedules");
            DropForeignKey("dbo.AppointmentSlots", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.AppointmentSlots", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AppointmentSlots", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AppointmentSlots", "AppointmentId", "dbo.Appointments");
            DropIndex("dbo.ScheduleTemplates", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ScheduleTemplates", new[] { "CreatedByUserId" });
            DropIndex("dbo.ScheduleTemplates", new[] { "DeletedByUserId" });
            DropIndex("dbo.ScheduleTemplates", "IX_ScheduleTemplate_IsDeleted");
            DropIndex("dbo.ScheduleTemplates", "IX_ScheduleTemplate_Default_Active_Deleted");
            DropIndex("dbo.ScheduleTemplates", "IX_ScheduleTemplate_ScheduleId_Type_IsActive");
            DropIndex("dbo.ScheduleExceptions", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ScheduleExceptions", new[] { "CreatedByUserId" });
            DropIndex("dbo.ScheduleExceptions", new[] { "DeletedByUserId" });
            DropIndex("dbo.ScheduleExceptions", "IX_ScheduleException_IsDeleted");
            DropIndex("dbo.ScheduleExceptions", "IX_ScheduleException_StartDate_Type_IsDeleted");
            DropIndex("dbo.ScheduleExceptions", "IX_ScheduleException_ScheduleId_DateRange");
            DropIndex("dbo.AppointmentSlots", new[] { "UpdatedByUserId" });
            DropIndex("dbo.AppointmentSlots", new[] { "CreatedByUserId" });
            DropIndex("dbo.AppointmentSlots", new[] { "DeletedByUserId" });
            DropIndex("dbo.AppointmentSlots", "IX_AppointmentSlot_IsDeleted");
            DropIndex("dbo.AppointmentSlots", new[] { "AppointmentId" });
            DropIndex("dbo.AppointmentSlots", "IX_AppointmentSlot_PatientId_Date");
            DropIndex("dbo.AppointmentSlots", "IX_AppointmentSlot_Date_Time_Status");
            DropIndex("dbo.AppointmentSlots", "IX_AppointmentSlot_ScheduleId_Date_Status");
            DropColumn("dbo.DoctorSchedules", "MaxWalkInPatientsPerDay");
            DropColumn("dbo.DoctorSchedules", "AllowWalkInPatients");
            DropColumn("dbo.DoctorSchedules", "AllowEmergencyBooking");
            DropColumn("dbo.DoctorSchedules", "CancellationNoticeHours");
            DropColumn("dbo.DoctorSchedules", "CancellationFee");
            DropColumn("dbo.DoctorSchedules", "ConsultationFee");
            DropColumn("dbo.DoctorSchedules", "RequirePatientRegistration");
            DropColumn("dbo.DoctorSchedules", "AllowSameDayBooking");
            DropColumn("dbo.DoctorSchedules", "MaxAdvanceBookingDays");
            DropColumn("dbo.DoctorSchedules", "MinAdvanceBookingDays");
            DropColumn("dbo.DoctorSchedules", "MaxAppointmentsPerDay");
            DropTable("dbo.ScheduleTemplates",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ScheduleTemplate_ActiveScheduleTemplates", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_ScheduleTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ScheduleExceptions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ScheduleException_ActiveScheduleExceptions", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_ScheduleException_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.AppointmentSlots",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_AppointmentSlot_ActiveAppointmentSlots", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_AppointmentSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
