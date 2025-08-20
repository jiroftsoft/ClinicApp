namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InsuranceTariffs", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.InsuranceTariffs", "DeletedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.InsuranceTariffs", "DeletedByUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Patients", "UpdatedByUserId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Patients", "CreatedByUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Patients", "CreatedByUserId");
            CreateIndex("dbo.Patients", "UpdatedByUserId");
            CreateIndex("dbo.InsuranceTariffs", "DeletedByUserId");
            AddForeignKey("dbo.Patients", "CreatedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.InsuranceTariffs", "DeletedByUserId", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Patients", "UpdatedByUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Patients", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceTariffs", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Patients", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.InsuranceTariffs", new[] { "DeletedByUserId" });
            DropIndex("dbo.Patients", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Patients", new[] { "CreatedByUserId" });
            AlterColumn("dbo.Patients", "CreatedByUserId", c => c.String());
            AlterColumn("dbo.Patients", "UpdatedByUserId", c => c.String());
            DropColumn("dbo.InsuranceTariffs", "DeletedByUserId");
            DropColumn("dbo.InsuranceTariffs", "DeletedAt");
            DropColumn("dbo.InsuranceTariffs", "IsDeleted");
        }
    }
}
