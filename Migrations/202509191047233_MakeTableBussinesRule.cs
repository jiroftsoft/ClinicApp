namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class MakeTableBussinesRule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BusinessRule",
                c => new
                    {
                        BusinessRuleId = c.Int(nullable: false, identity: true),
                        RuleName = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        RuleType = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        Conditions = c.String(maxLength: 2000),
                        Actions = c.String(maxLength: 2000),
                        InsurancePlanId = c.Int(),
                        ServiceCategoryId = c.Int(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_BusinessRule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.BusinessRuleId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.InsurancePlans", t => t.InsurancePlanId)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.InsurancePlanId)
                .Index(t => t.ServiceCategoryId)
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BusinessRule", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BusinessRule", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.BusinessRule", "InsurancePlanId", "dbo.InsurancePlans");
            DropForeignKey("dbo.BusinessRule", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BusinessRule", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.BusinessRule", new[] { "UpdatedByUserId" });
            DropIndex("dbo.BusinessRule", new[] { "CreatedByUserId" });
            DropIndex("dbo.BusinessRule", new[] { "DeletedByUserId" });
            DropIndex("dbo.BusinessRule", new[] { "ServiceCategoryId" });
            DropIndex("dbo.BusinessRule", new[] { "InsurancePlanId" });
            DropTable("dbo.BusinessRule",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_BusinessRule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
