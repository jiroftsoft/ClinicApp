namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewFiledToTarrif : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "Priority", c => c.Int());
            AddColumn("dbo.InsuranceTariffs", "StartDate", c => c.DateTime());
            AddColumn("dbo.InsuranceTariffs", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.InsuranceTariffs", "EndDate");
            DropColumn("dbo.InsuranceTariffs", "StartDate");
            DropColumn("dbo.InsuranceTariffs", "Priority");
        }
    }
}
