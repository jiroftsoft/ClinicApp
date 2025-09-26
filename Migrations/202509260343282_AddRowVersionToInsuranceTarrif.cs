namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRowVersionToInsuranceTarrif : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InsuranceTariffs", "RowVersion");
        }
    }
}
