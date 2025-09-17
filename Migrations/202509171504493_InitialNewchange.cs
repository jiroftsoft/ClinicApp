namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialNewchange : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_PatientShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsurerShare");
            AlterColumn("dbo.InsuranceTariffs", "PatientShare", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "InsurerShare", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.InsuranceTariffs", "PatientShare", name: "IX_InsuranceTariff_PatientShare");
            CreateIndex("dbo.InsuranceTariffs", "InsurerShare", name: "IX_InsuranceTariff_InsurerShare");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsurerShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_PatientShare");
            AlterColumn("dbo.InsuranceTariffs", "InsurerShare", c => c.Decimal(precision: 5, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "PatientShare", c => c.Decimal(precision: 5, scale: 2));
            CreateIndex("dbo.InsuranceTariffs", "InsurerShare", name: "IX_InsuranceTariff_InsurerShare");
            CreateIndex("dbo.InsuranceTariffs", "PatientShare", name: "IX_InsuranceTariff_PatientShare");
        }
    }
}
