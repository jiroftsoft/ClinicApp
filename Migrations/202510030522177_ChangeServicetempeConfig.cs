namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeServicetempeConfig : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_DeletedAt");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_CreatedAt");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_UpdatedAt");
            RenameIndex(table: "dbo.ServiceTemplates", name: "IX_ServiceTemplate_CreatedByUserId", newName: "IX_CreatedByUserId");
            AlterColumn("dbo.ServiceTemplates", "DefaultTechnicalCoefficient", c => c.Decimal(nullable: false, precision: 8, scale: 4));
            AlterColumn("dbo.ServiceTemplates", "DefaultProfessionalCoefficient", c => c.Decimal(nullable: false, precision: 8, scale: 4));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ServiceTemplates", "DefaultProfessionalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.ServiceTemplates", "DefaultTechnicalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            RenameIndex(table: "dbo.ServiceTemplates", name: "IX_CreatedByUserId", newName: "IX_ServiceTemplate_CreatedByUserId");
            CreateIndex("dbo.ServiceTemplates", "UpdatedAt", name: "IX_ServiceTemplate_UpdatedAt");
            CreateIndex("dbo.ServiceTemplates", "CreatedAt", name: "IX_ServiceTemplate_CreatedAt");
            CreateIndex("dbo.ServiceTemplates", "DeletedAt", name: "IX_ServiceTemplate_DeletedAt");
        }
    }
}
