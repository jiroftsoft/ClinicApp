namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewPerformance1205 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Receptions", new[] { "Status", "CreatedByUserId", "CreatedAt" }, name: "IX_Reception_Status_CreatedByUserId_CreatedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Receptions", "IX_Reception_Status_CreatedByUserId_CreatedAt");
        }
    }
}
