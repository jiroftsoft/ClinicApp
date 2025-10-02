namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceTemplateConfigChange01 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_TechnicalCoefficient");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_ProfessionalCoefficient");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsActive");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsDeleted");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_CreatedByUserId");
            AlterColumn("dbo.ServiceTemplates", "DefaultTechnicalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.ServiceTemplates", "DefaultProfessionalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.ServiceTemplates", "CreatedByUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ServiceTemplates", new[] { "IsDeleted", "IsActive" }, name: "IX_ServiceTemplate_IsDeleted_IsActive");
            CreateIndex("dbo.ServiceTemplates", "DeletedAt", name: "IX_ServiceTemplate_DeletedAt");
            CreateIndex("dbo.ServiceTemplates", "CreatedByUserId", name: "IX_ServiceTemplate_CreatedByUserId");
            CreateIndex("dbo.ServiceTemplates", "UpdatedAt", name: "IX_ServiceTemplate_UpdatedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_UpdatedAt");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_CreatedByUserId");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_DeletedAt");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsDeleted_IsActive");
            AlterColumn("dbo.ServiceTemplates", "CreatedByUserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.ServiceTemplates", "DefaultProfessionalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.ServiceTemplates", "DefaultTechnicalCoefficient", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.ServiceTemplates", "CreatedByUserId", name: "IX_ServiceTemplate_CreatedByUserId");
            CreateIndex("dbo.ServiceTemplates", "IsDeleted", name: "IX_ServiceTemplate_IsDeleted");
            CreateIndex("dbo.ServiceTemplates", "IsActive", name: "IX_ServiceTemplate_IsActive");
            CreateIndex("dbo.ServiceTemplates", "DefaultProfessionalCoefficient", name: "IX_ServiceTemplate_ProfessionalCoefficient");
            CreateIndex("dbo.ServiceTemplates", "DefaultTechnicalCoefficient", name: "IX_ServiceTemplate_TechnicalCoefficient");
        }
    }
}
