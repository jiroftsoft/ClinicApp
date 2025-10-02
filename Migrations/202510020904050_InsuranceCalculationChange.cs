namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InsuranceCalculationChange : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_ServiceAmount");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_InsuranceShare");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_PatientShare");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_Copay");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CoverageOverride");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_Deductible");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CalculationType");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CalculationType_IsValid");
            AlterColumn("dbo.InsuranceCalculations", "ServiceAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "InsuranceShare", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "PatientShare", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "Copay", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "CoverageOverride", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "Deductible", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceCalculations", "CalculationType", c => c.Int(nullable: false));
            AlterColumn("dbo.InsurancePlans", "Deductible", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.InsuranceCalculations", "ServiceAmount", name: "IX_InsuranceCalculation_ServiceAmount");
            CreateIndex("dbo.InsuranceCalculations", "InsuranceShare", name: "IX_InsuranceCalculation_InsuranceShare");
            CreateIndex("dbo.InsuranceCalculations", "PatientShare", name: "IX_InsuranceCalculation_PatientShare");
            CreateIndex("dbo.InsuranceCalculations", "Copay", name: "IX_InsuranceCalculation_Copay");
            CreateIndex("dbo.InsuranceCalculations", "CoverageOverride", name: "IX_InsuranceCalculation_CoverageOverride");
            CreateIndex("dbo.InsuranceCalculations", "Deductible", name: "IX_InsuranceCalculation_Deductible");
            CreateIndex("dbo.InsuranceCalculations", "CalculationType", name: "IX_InsuranceCalculation_CalculationType");
            CreateIndex("dbo.InsuranceCalculations", new[] { "CalculationType", "IsValid" }, name: "IX_InsuranceCalculation_CalculationType_IsValid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CalculationType_IsValid");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CalculationType");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_Deductible");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_CoverageOverride");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_Copay");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_PatientShare");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_InsuranceShare");
            DropIndex("dbo.InsuranceCalculations", "IX_InsuranceCalculation_ServiceAmount");
            AlterColumn("dbo.InsurancePlans", "Deductible", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.InsuranceCalculations", "CalculationType", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.InsuranceCalculations", "Deductible", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceCalculations", "CoverageOverride", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceCalculations", "Copay", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceCalculations", "PatientShare", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceCalculations", "InsuranceShare", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceCalculations", "ServiceAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.InsuranceCalculations", new[] { "CalculationType", "IsValid" }, name: "IX_InsuranceCalculation_CalculationType_IsValid");
            CreateIndex("dbo.InsuranceCalculations", "CalculationType", name: "IX_InsuranceCalculation_CalculationType");
            CreateIndex("dbo.InsuranceCalculations", "Deductible", name: "IX_InsuranceCalculation_Deductible");
            CreateIndex("dbo.InsuranceCalculations", "CoverageOverride", name: "IX_InsuranceCalculation_CoverageOverride");
            CreateIndex("dbo.InsuranceCalculations", "Copay", name: "IX_InsuranceCalculation_Copay");
            CreateIndex("dbo.InsuranceCalculations", "PatientShare", name: "IX_InsuranceCalculation_PatientShare");
            CreateIndex("dbo.InsuranceCalculations", "InsuranceShare", name: "IX_InsuranceCalculation_InsuranceShare");
            CreateIndex("dbo.InsuranceCalculations", "ServiceAmount", name: "IX_InsuranceCalculation_ServiceAmount");
        }
    }
}
