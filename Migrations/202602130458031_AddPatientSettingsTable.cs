namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPatientSettingsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PatientSettings",
                c => new
                    {
                        PatientId = c.Int(nullable: false),
                        EmailNotifications = c.Boolean(nullable: false),
                        SmsNotifications = c.Boolean(nullable: false),
                        AppointmentReminders = c.Boolean(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.PatientId)
                .ForeignKey("dbo.Patients", t => t.PatientId, cascadeDelete: true)
                .Index(t => t.PatientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PatientSettings", "PatientId", "dbo.Patients");
            DropIndex("dbo.PatientSettings", new[] { "PatientId" });
            DropTable("dbo.PatientSettings");
        }
    }
}
