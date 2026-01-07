namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingExpiresAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "PendingExpiresAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "PendingExpiresAt");
        }
    }
}
