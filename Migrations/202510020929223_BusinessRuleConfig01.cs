namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BusinessRuleConfig01 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.BusinessRule", newName: "BusinessRules");
            DropIndex("dbo.BusinessRules", new[] { "InsurancePlanId" });
            DropIndex("dbo.BusinessRules", new[] { "ServiceCategoryId" });
            AddColumn("dbo.BusinessRules", "ServiceId", c => c.Int());
            AddColumn("dbo.BusinessRules", "IsHashtagged", c => c.Boolean());
            AddColumn("dbo.BusinessRules", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AlterColumn("dbo.BusinessRules", "Conditions", c => c.String());
            AlterColumn("dbo.BusinessRules", "Actions", c => c.String());
            CreateIndex("dbo.BusinessRules", "RuleName", name: "IX_BusinessRule_RuleName");
            CreateIndex("dbo.BusinessRules", new[] { "InsurancePlanId", "ServiceCategoryId", "RuleType", "IsActive", "IsDeleted", "StartDate", "EndDate" }, name: "IX_BusinessRule_Plan_Type_Active_Date");
            CreateIndex("dbo.BusinessRules", "Priority", name: "IX_BusinessRule_Priority");
            CreateIndex("dbo.BusinessRules", "ServiceId");
            CreateIndex("dbo.BusinessRules", "CreatedAt", name: "IX_BusinessRule_CreatedAt");
            AddForeignKey("dbo.BusinessRules", "ServiceId", "dbo.Services", "ServiceId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BusinessRules", "ServiceId", "dbo.Services");
            DropIndex("dbo.BusinessRules", "IX_BusinessRule_CreatedAt");
            DropIndex("dbo.BusinessRules", new[] { "ServiceId" });
            DropIndex("dbo.BusinessRules", "IX_BusinessRule_Priority");
            DropIndex("dbo.BusinessRules", "IX_BusinessRule_Plan_Type_Active_Date");
            DropIndex("dbo.BusinessRules", "IX_BusinessRule_RuleName");
            AlterColumn("dbo.BusinessRules", "Actions", c => c.String(maxLength: 2000));
            AlterColumn("dbo.BusinessRules", "Conditions", c => c.String(maxLength: 2000));
            DropColumn("dbo.BusinessRules", "RowVersion");
            DropColumn("dbo.BusinessRules", "IsHashtagged");
            DropColumn("dbo.BusinessRules", "ServiceId");
            CreateIndex("dbo.BusinessRules", "ServiceCategoryId");
            CreateIndex("dbo.BusinessRules", "InsurancePlanId");
            RenameTable(name: "dbo.BusinessRules", newName: "BusinessRule");
        }
    }
}
