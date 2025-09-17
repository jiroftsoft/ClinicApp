namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeNewChnage140402426 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InsuranceTariffs", "IsActive");
        }
    }
}
