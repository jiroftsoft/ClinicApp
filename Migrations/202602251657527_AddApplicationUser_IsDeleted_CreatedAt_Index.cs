namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicationUser_IsDeleted_CreatedAt_Index : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AspNetUsers", new[] { "IsDeleted", "CreatedAt" }, name: "IX_ApplicationUser_IsDeleted_CreatedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_IsDeleted_CreatedAt");
        }
    }
}
