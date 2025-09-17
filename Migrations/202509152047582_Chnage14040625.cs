namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Chnage14040625 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceTemplates", "IsHashtagged", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ServiceTemplates", "IsHashtagged", name: "IX_ServiceTemplate_IsHashtagged");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsHashtagged");
            DropColumn("dbo.ServiceTemplates", "IsHashtagged");
        }
    }
}
