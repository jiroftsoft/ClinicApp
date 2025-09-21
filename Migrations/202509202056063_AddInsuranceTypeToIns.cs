namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInsuranceTypeToIns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsurancePlans", "InsuranceType", c => c.Int(nullable: false));
            CreateIndex("dbo.InsurancePlans", "InsuranceType", name: "IX_InsurancePlan_InsuranceType");
            CreateIndex("dbo.InsurancePlans", new[] { "InsuranceType", "IsActive", "IsDeleted" }, name: "IX_InsurancePlan_Type_IsActive_IsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InsurancePlans", "IX_InsurancePlan_Type_IsActive_IsDeleted");
            DropIndex("dbo.InsurancePlans", "IX_InsurancePlan_InsuranceType");
            DropColumn("dbo.InsurancePlans", "InsuranceType");
        }
    }
}
