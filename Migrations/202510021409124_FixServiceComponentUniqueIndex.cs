namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixServiceComponentUniqueIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");
            CreateIndex("dbo.ServiceComponents", new[] { "ServiceId", "ComponentType", "IsDeleted" }, name: "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");
            CreateIndex("dbo.ServiceComponents", new[] { "ServiceId", "ComponentType", "IsDeleted" }, unique: true, name: "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");
        }
    }
}
