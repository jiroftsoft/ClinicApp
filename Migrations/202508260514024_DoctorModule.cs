namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class DoctorModule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DoctorSchedules",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        AppointmentDuration = c.Int(nullable: false),
                        DefaultStartTime = c.Time(precision: 7),
                        DefaultEndTime = c.Time(precision: 7),
                        IsActive = c.Boolean(nullable: false),
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
                    { "DynamicFilter_DoctorSchedule_ActiveDoctorSchedules", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorSchedule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ScheduleId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ScheduleId, name: "IX_DoctorSchedule_ScheduleId")
                .Index(t => t.DoctorId, name: "IX_DoctorSchedule_DoctorId")
                .Index(t => new { t.DoctorId, t.IsActive }, name: "IX_DoctorSchedule_DoctorId_IsActive")
                .Index(t => new { t.DoctorId, t.IsDeleted }, name: "IX_DoctorSchedule_DoctorId_IsDeleted")
                .Index(t => new { t.DoctorId, t.IsActive, t.IsDeleted }, unique: true, name: "IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique")
                .Index(t => t.AppointmentDuration, name: "IX_DoctorSchedule_AppointmentDuration")
                .Index(t => t.IsActive, name: "IX_DoctorSchedule_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorSchedule_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorSchedule_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorSchedule_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorSchedule_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorSchedule_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorSchedule_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorSchedule_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorSchedule_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorWorkDays",
                c => new
                    {
                        WorkDayId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
                    { "DynamicFilter_DoctorWorkDay_ActiveDoctorWorkDays", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorWorkDay_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.WorkDayId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorSchedules", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.WorkDayId, name: "IX_DoctorWorkDay_WorkDayId")
                .Index(t => t.ScheduleId, name: "IX_DoctorWorkDay_ScheduleId")
                .Index(t => new { t.ScheduleId, t.DayOfWeek }, name: "IX_DoctorWorkDay_ScheduleId_DayOfWeek")
                .Index(t => new { t.ScheduleId, t.IsActive }, name: "IX_DoctorWorkDay_ScheduleId_IsActive")
                .Index(t => new { t.ScheduleId, t.IsDeleted }, name: "IX_DoctorWorkDay_ScheduleId_IsDeleted")
                .Index(t => new { t.ScheduleId, t.DayOfWeek, t.IsDeleted }, unique: true, name: "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique")
                .Index(t => t.DayOfWeek, name: "IX_DoctorWorkDay_DayOfWeek")
                .Index(t => t.IsActive, name: "IX_DoctorWorkDay_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorWorkDay_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorWorkDay_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorWorkDay_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorWorkDay_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorWorkDay_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorWorkDay_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorWorkDay_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorWorkDay_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorTimeRanges",
                c => new
                    {
                        TimeRangeId = c.Int(nullable: false, identity: true),
                        WorkDayId = c.Int(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        IsActive = c.Boolean(nullable: false),
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
                    { "DynamicFilter_DoctorTimeRange_ActiveDoctorTimeRanges", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorTimeRange_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TimeRangeId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorWorkDays", t => t.WorkDayId, cascadeDelete: true)
                .Index(t => t.TimeRangeId, name: "IX_DoctorTimeRange_TimeRangeId")
                .Index(t => t.WorkDayId, name: "IX_DoctorTimeRange_WorkDayId")
                .Index(t => new { t.WorkDayId, t.StartTime }, name: "IX_DoctorTimeRange_WorkDayId_StartTime")
                .Index(t => new { t.WorkDayId, t.IsActive }, name: "IX_DoctorTimeRange_WorkDayId_IsActive")
                .Index(t => new { t.WorkDayId, t.IsDeleted }, name: "IX_DoctorTimeRange_WorkDayId_IsDeleted")
                .Index(t => new { t.WorkDayId, t.StartTime, t.EndTime }, name: "IX_DoctorTimeRange_WorkDayId_StartTime_EndTime")
                .Index(t => t.StartTime, name: "IX_DoctorTimeRange_StartTime")
                .Index(t => t.EndTime, name: "IX_DoctorTimeRange_EndTime")
                .Index(t => t.IsActive, name: "IX_DoctorTimeRange_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorTimeRange_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorTimeRange_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorTimeRange_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorTimeRange_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorTimeRange_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorTimeRange_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorTimeRange_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorTimeRange_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorTimeSlots",
                c => new
                    {
                        TimeSlotId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Duration = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        AppointmentId = c.Int(),
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
                    { "DynamicFilter_DoctorTimeSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TimeSlotId)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.TimeSlotId, name: "IX_DoctorTimeSlot_TimeSlotId")
                .Index(t => t.DoctorId, name: "IX_DoctorTimeSlot_DoctorId")
                .Index(t => new { t.DoctorId, t.AppointmentDate }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.Status }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status")
                .Index(t => new { t.DoctorId, t.IsDeleted }, name: "IX_DoctorTimeSlot_DoctorId_IsDeleted")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.StartTime, t.Status }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.StartTime, t.EndTime, t.IsDeleted }, unique: true, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique")
                .Index(t => t.AppointmentDate, name: "IX_DoctorTimeSlot_AppointmentDate")
                .Index(t => new { t.AppointmentDate, t.Status, t.IsDeleted }, name: "IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted")
                .Index(t => t.StartTime, name: "IX_DoctorTimeSlot_StartTime")
                .Index(t => t.EndTime, name: "IX_DoctorTimeSlot_EndTime")
                .Index(t => t.Duration, name: "IX_DoctorTimeSlot_Duration")
                .Index(t => t.Status, name: "IX_DoctorTimeSlot_Status")
                .Index(t => t.AppointmentId, name: "IX_DoctorTimeSlot_AppointmentId")
                .Index(t => t.IsDeleted, name: "IX_DoctorTimeSlot_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorTimeSlot_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorTimeSlot_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorTimeSlot_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorTimeSlot_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorTimeSlot_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorTimeSlot_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorTimeSlot_UpdatedByUserId");
            
            AddColumn("dbo.Appointments", "IsNewPatient", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "PatientName", c => c.String(maxLength: 200));
            AddColumn("dbo.Appointments", "PatientPhone", c => c.String(maxLength: 20));
            AddColumn("dbo.Appointments", "ServiceCategoryId", c => c.Int());
            CreateIndex("dbo.Appointments", "ServiceCategoryId");
            AddForeignKey("dbo.Appointments", "ServiceCategoryId", "dbo.ServiceCategories", "ServiceCategoryId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Appointments", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.DoctorTimeSlots", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorTimeSlots", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "AppointmentId", "dbo.Appointments");
            DropForeignKey("dbo.DoctorWorkDays", "ScheduleId", "dbo.DoctorSchedules");
            DropForeignKey("dbo.DoctorWorkDays", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "WorkDayId", "dbo.DoctorWorkDays");
            DropForeignKey("dbo.DoctorTimeRanges", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorWorkDays", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorWorkDays", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorSchedules", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_UpdatedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_UpdatedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DeletedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DeletedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_Duration");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_EndTime");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_StartTime");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentDate");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_TimeSlotId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_UpdatedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_UpdatedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_DeletedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_DeletedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_IsActive");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_EndTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_StartTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_StartTime_EndTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_IsActive");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_StartTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_TimeRangeId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_UpdatedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_UpdatedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DeletedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DeletedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_IsActive");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DayOfWeek");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_IsActive");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_DayOfWeek");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_WorkDayId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_UpdatedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_UpdatedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DeletedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DeletedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_IsActive");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_AppointmentDuration");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsActive");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_ScheduleId");
            DropIndex("dbo.Appointments", new[] { "ServiceCategoryId" });
            DropColumn("dbo.Appointments", "ServiceCategoryId");
            DropColumn("dbo.Appointments", "PatientPhone");
            DropColumn("dbo.Appointments", "PatientName");
            DropColumn("dbo.Appointments", "IsNewPatient");
            DropTable("dbo.DoctorTimeSlots",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorTimeRanges",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeRange_ActiveDoctorTimeRanges", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorTimeRange_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorWorkDays",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorWorkDay_ActiveDoctorWorkDays", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorWorkDay_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorSchedules",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorSchedule_ActiveDoctorSchedules", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorSchedule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
