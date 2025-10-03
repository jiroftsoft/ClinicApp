namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChnageFactorSetting : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FactorType");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FactorType_IsHashtagged_FinancialYear_IsActive_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsHashtagged");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FinancialYear");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FinancialYear_IsFrozen_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsActiveForCurrentYear_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsDeleted");
            AddColumn("dbo.FactorSettings", "Scope", c => c.Int(nullable: false));
            AddColumn("dbo.FactorSettings", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            CreateIndex("dbo.FactorSettings", new[] { "FinancialYear", "FactorType", "Scope", "IsHashtagged", "IsActive", "IsDeleted" }, unique: true, name: "UX_FactorSetting_Unique");
        }
        
        public override void Down()
        {
            DropIndex("dbo.FactorSettings", "UX_FactorSetting_Unique");
            DropColumn("dbo.FactorSettings", "RowVersion");
            DropColumn("dbo.FactorSettings", "Scope");
            CreateIndex("dbo.FactorSettings", "IsDeleted", name: "IX_FactorSetting_IsDeleted");
            CreateIndex("dbo.FactorSettings", new[] { "IsActiveForCurrentYear", "IsDeleted" }, name: "IX_FactorSetting_IsActiveForCurrentYear_IsDeleted");
            CreateIndex("dbo.FactorSettings", new[] { "FinancialYear", "IsFrozen", "IsDeleted" }, name: "IX_FactorSetting_FinancialYear_IsFrozen_IsDeleted");
            CreateIndex("dbo.FactorSettings", "FinancialYear", name: "IX_FactorSetting_FinancialYear");
            CreateIndex("dbo.FactorSettings", "IsHashtagged", name: "IX_FactorSetting_IsHashtagged");
            CreateIndex("dbo.FactorSettings", new[] { "FactorType", "IsHashtagged", "FinancialYear", "IsActive", "IsDeleted" }, name: "IX_FactorSetting_FactorType_IsHashtagged_FinancialYear_IsActive_IsDeleted");
            CreateIndex("dbo.FactorSettings", "FactorType", name: "IX_FactorSetting_FactorType");
        }
    }
}
