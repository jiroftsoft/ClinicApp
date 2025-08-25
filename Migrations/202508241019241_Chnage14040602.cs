namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Chnage14040602 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceCategories", "Description", c => c.String());
            AddColumn("dbo.Services", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Services", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "Notes");
            DropColumn("dbo.Services", "IsActive");
            DropColumn("dbo.ServiceCategories", "Description");
        }
    }
}
