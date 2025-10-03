namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeServiceComponentConfig : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_Coefficient");
            AlterColumn("dbo.ServiceComponents", "Coefficient", c => c.Decimal(nullable: false, precision: 6, scale: 4));
            CreateIndex("dbo.ServiceComponents", "Coefficient", name: "IX_ServiceComponent_Coefficient");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_Coefficient");
            AlterColumn("dbo.ServiceComponents", "Coefficient", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.ServiceComponents", "Coefficient", name: "IX_ServiceComponent_Coefficient");
        }
    }
}
