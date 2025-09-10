namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateEmailOnPatient : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "Email", c => c.String(maxLength: 256));
            CreateIndex("dbo.Patients", "Email", name: "IX_Patient_Email");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Patients", "IX_Patient_Email");
            DropColumn("dbo.Patients", "Email");
        }
    }
}
