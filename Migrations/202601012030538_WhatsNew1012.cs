namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WhatsNew1012 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OtpStates", "PhoneNumber", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OtpStates", "PhoneNumber", c => c.String(nullable: false, maxLength: 11));
        }
    }
}
