namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewPropertyToTariffe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "Notes", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InsuranceTariffs", "Notes");
        }
    }
}
