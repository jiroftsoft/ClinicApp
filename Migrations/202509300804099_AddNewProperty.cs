namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "SupplementaryDeductible", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.InsuranceTariffs", "MinPatientCopay", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InsuranceTariffs", "MinPatientCopay");
            DropColumn("dbo.InsuranceTariffs", "SupplementaryDeductible");
        }
    }
}
