namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDescriptionToNewsletterTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NewsletterTemplates", "Description", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NewsletterTemplates", "Description");
        }
    }
}
