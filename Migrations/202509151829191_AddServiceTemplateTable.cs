namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddServiceTemplateTable : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Services", "IX_Service_TechnicalPart");
            DropIndex("dbo.Services", "IX_Service_ProfessionalPart");
            CreateTable(
                "dbo.ServiceTemplates",
                c => new
                    {
                        ServiceTemplateId = c.Int(nullable: false, identity: true),
                        ServiceCode = c.String(nullable: false, maxLength: 50),
                        ServiceName = c.String(nullable: false, maxLength: 200),
                        DefaultTechnicalCoefficient = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DefaultProfessionalCoefficient = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(nullable: false, maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceTemplateId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ServiceCode, unique: true, name: "IX_ServiceTemplate_ServiceCode")
                .Index(t => t.DefaultTechnicalCoefficient, name: "IX_ServiceTemplate_TechnicalCoefficient")
                .Index(t => t.DefaultProfessionalCoefficient, name: "IX_ServiceTemplate_ProfessionalCoefficient")
                .Index(t => t.IsActive, name: "IX_ServiceTemplate_IsActive")
                .Index(t => t.IsDeleted, name: "IX_ServiceTemplate_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_ServiceTemplate_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ServiceTemplate_CreatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
            DropColumn("dbo.Services", "TechnicalPart");
            DropColumn("dbo.Services", "ProfessionalPart");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Services", "ProfessionalPart", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Services", "TechnicalPart", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropForeignKey("dbo.ServiceTemplates", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceTemplates", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceTemplates", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ServiceTemplates", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_CreatedByUserId");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_CreatedAt");
            DropIndex("dbo.ServiceTemplates", new[] { "DeletedByUserId" });
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsDeleted");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_IsActive");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_ProfessionalCoefficient");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_TechnicalCoefficient");
            DropIndex("dbo.ServiceTemplates", "IX_ServiceTemplate_ServiceCode");
            DropTable("dbo.ServiceTemplates",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            CreateIndex("dbo.Services", "ProfessionalPart", name: "IX_Service_ProfessionalPart");
            CreateIndex("dbo.Services", "TechnicalPart", name: "IX_Service_TechnicalPart");
        }
    }
}
