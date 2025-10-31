namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFatherName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Patients", "FatherName", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Patients", "FatherName");
        }
    }
}
