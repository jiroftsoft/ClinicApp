namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewChangeFaterFault : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ServiceComponents", new[] { "ServiceId", "ComponentType" }, unique: true, name: "IX_ServiceComponent_ServiceId_ComponentType_Unique");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId_ComponentType_Unique");
        }
    }
}
