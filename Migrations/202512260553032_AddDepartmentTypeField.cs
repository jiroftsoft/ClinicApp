namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDepartmentTypeField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Departments", "Type", c => c.Byte(nullable: false));
            CreateIndex("dbo.Departments", new[] { "Type", "IsActive", "IsDeleted" }, name: "IX_Department_Type_IsActive_IsDeleted");
            CreateIndex("dbo.Departments", "Type", name: "IX_Department_Type");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Departments", "IX_Department_Type");
            DropIndex("dbo.Departments", "IX_Department_Type_IsActive_IsDeleted");
            DropColumn("dbo.Departments", "Type");
        }
    }
}
