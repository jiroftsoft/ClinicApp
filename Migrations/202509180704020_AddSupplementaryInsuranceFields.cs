namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSupplementaryInsuranceFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "InsuranceType", c => c.Int(nullable: false));
            AddColumn("dbo.InsuranceTariffs", "SupplementarySettings", c => c.String(maxLength: 2000));
            AddColumn("dbo.InsuranceTariffs", "SupplementaryCoveragePercent", c => c.Decimal(precision: 5, scale: 2));
            AddColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.InsuranceTariffs", "InsuranceType", name: "IX_InsuranceTariff_InsuranceType");
            CreateIndex("dbo.InsuranceTariffs", "SupplementaryCoveragePercent", name: "IX_InsuranceTariff_SupplementaryCoveragePercent");
            CreateIndex("dbo.InsuranceTariffs", "SupplementaryMaxPayment", name: "IX_InsuranceTariff_SupplementaryMaxPayment");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryMaxPayment");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryCoveragePercent");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsuranceType");
            DropColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment");
            DropColumn("dbo.InsuranceTariffs", "SupplementaryCoveragePercent");
            DropColumn("dbo.InsuranceTariffs", "SupplementarySettings");
            DropColumn("dbo.InsuranceTariffs", "InsuranceType");
        }
    }
}
