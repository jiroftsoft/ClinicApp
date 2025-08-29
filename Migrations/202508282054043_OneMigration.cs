namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OneMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DoctorServiceCategories", "DeletedAt", c => c.DateTime());
            AddColumn("dbo.DoctorServiceCategories", "DeletedByUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.DoctorServiceCategories", "IsDeleted", name: "IX_DoctorServiceCategory_IsDeleted");
            CreateIndex("dbo.DoctorServiceCategories", "DeletedAt", name: "IX_DoctorServiceCategory_DeletedAt");
            CreateIndex("dbo.DoctorServiceCategories", "DeletedByUserId", name: "IX_DoctorServiceCategory_DeletedByUserId");
            AddForeignKey("dbo.DoctorServiceCategories", "DeletedByUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DoctorServiceCategories", "DeletedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DeletedByUserId");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DeletedAt");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_IsDeleted");
            DropColumn("dbo.DoctorServiceCategories", "DeletedByUserId");
            DropColumn("dbo.DoctorServiceCategories", "DeletedAt");
        }
    }
}
