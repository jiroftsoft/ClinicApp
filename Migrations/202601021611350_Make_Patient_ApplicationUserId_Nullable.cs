namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Make_Patient_ApplicationUserId_Nullable : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            AlterColumn("dbo.Patients", "ApplicationUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Patients", "ApplicationUserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            AlterColumn("dbo.Patients", "ApplicationUserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Patients", "ApplicationUserId");
        }
    }
}
