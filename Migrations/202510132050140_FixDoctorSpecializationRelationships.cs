namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixDoctorSpecializationRelationships : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DoctorSchedules", "ShiftType", c => c.Byte(nullable: false));
            AddColumn("dbo.DoctorSchedules", "IsShiftActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.DoctorSchedules", "ShiftStartTime", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.DoctorSchedules", "ShiftEndTime", c => c.Time(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DoctorSchedules", "ShiftEndTime");
            DropColumn("dbo.DoctorSchedules", "ShiftStartTime");
            DropColumn("dbo.DoctorSchedules", "IsShiftActive");
            DropColumn("dbo.DoctorSchedules", "ShiftType");
        }
    }
}
