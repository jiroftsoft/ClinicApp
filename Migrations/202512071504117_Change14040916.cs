namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change14040916 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Appointments", new[] { "DoctorId", "AppointmentDate", "Status", "IsDeleted" }, name: "IX_Appointment_DoctorId_Date_Status_Deleted");
            CreateIndex("dbo.ScheduleExceptions", new[] { "ScheduleId", "StartDate", "EndDate", "Type", "IsActive", "IsDeleted" }, name: "IX_ScheduleException_ScheduleId_DateRange_Type_Active_Deleted");
            CreateIndex("dbo.ScheduleExceptions", new[] { "ScheduleId", "StartDate", "EndDate", "StartTime", "EndTime", "IsActive", "IsDeleted" }, name: "IX_ScheduleException_ScheduleId_DateTimeRange_Active_Deleted");
            CreateIndex("dbo.DoctorWorkDays", new[] { "ScheduleId", "DayOfWeek", "IsActive", "IsDeleted" }, name: "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsActive_IsDeleted");
            CreateIndex("dbo.DoctorTimeRanges", new[] { "WorkDayId", "IsActive", "IsDeleted" }, name: "IX_DoctorTimeRange_WorkDayId_IsActive_IsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_IsActive_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsActive_IsDeleted");
            DropIndex("dbo.ScheduleExceptions", "IX_ScheduleException_ScheduleId_DateTimeRange_Active_Deleted");
            DropIndex("dbo.ScheduleExceptions", "IX_ScheduleException_ScheduleId_DateRange_Type_Active_Deleted");
            DropIndex("dbo.Appointments", "IX_Appointment_DoctorId_Date_Status_Deleted");
        }
    }
}
