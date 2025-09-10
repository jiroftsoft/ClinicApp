namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Reception01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Duration", c => c.Int(nullable: false));
            AddColumn("dbo.Appointments", "Priority", c => c.Byte(nullable: false));
            AddColumn("dbo.Appointments", "IsEmergency", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "IsOnlineBooking", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "ConflictResolution", c => c.String(maxLength: 500));
            AddColumn("dbo.Patients", "BloodType", c => c.String(maxLength: 10));
            AddColumn("dbo.Patients", "Allergies", c => c.String(maxLength: 1000));
            AddColumn("dbo.Patients", "ChronicDiseases", c => c.String(maxLength: 1000));
            AddColumn("dbo.Patients", "EmergencyContactName", c => c.String(maxLength: 100));
            AddColumn("dbo.Patients", "EmergencyContactPhone", c => c.String(maxLength: 50));
            AddColumn("dbo.Patients", "EmergencyContactRelationship", c => c.String(maxLength: 50));
            AddColumn("dbo.Receptions", "Type", c => c.Byte(nullable: false));
            AddColumn("dbo.Receptions", "Priority", c => c.Byte(nullable: false));
            AddColumn("dbo.Receptions", "Notes", c => c.String(maxLength: 1000));
            AddColumn("dbo.Receptions", "IsEmergency", c => c.Boolean(nullable: false));
            AddColumn("dbo.Receptions", "IsOnlineReception", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Receptions", "IsOnlineReception");
            DropColumn("dbo.Receptions", "IsEmergency");
            DropColumn("dbo.Receptions", "Notes");
            DropColumn("dbo.Receptions", "Priority");
            DropColumn("dbo.Receptions", "Type");
            DropColumn("dbo.Patients", "EmergencyContactRelationship");
            DropColumn("dbo.Patients", "EmergencyContactPhone");
            DropColumn("dbo.Patients", "EmergencyContactName");
            DropColumn("dbo.Patients", "ChronicDiseases");
            DropColumn("dbo.Patients", "Allergies");
            DropColumn("dbo.Patients", "BloodType");
            DropColumn("dbo.Appointments", "ConflictResolution");
            DropColumn("dbo.Appointments", "IsOnlineBooking");
            DropColumn("dbo.Appointments", "IsEmergency");
            DropColumn("dbo.Appointments", "Priority");
            DropColumn("dbo.Appointments", "Duration");
        }
    }
}
