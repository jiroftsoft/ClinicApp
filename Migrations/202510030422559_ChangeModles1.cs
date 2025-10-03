namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeModles1 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SharedServices", "IX_SharedService_Service_Department_Deleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_Department_Active_Deleted");
            CreateIndex("dbo.SharedServices", "ServiceId", name: "IX_SharedService_ServiceId");
            CreateIndex("dbo.SharedServices", "DepartmentId", name: "IX_SharedService_DepartmentId");
            CreateIndex("dbo.SharedServices", "IsActive", name: "IX_SharedService_IsActive");
            CreateIndex("dbo.SharedServices", "IsDeleted", name: "IX_SharedService_IsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SharedServices", "IX_SharedService_IsDeleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_IsActive");
            DropIndex("dbo.SharedServices", "IX_SharedService_DepartmentId");
            DropIndex("dbo.SharedServices", "IX_SharedService_ServiceId");
            CreateIndex("dbo.SharedServices", new[] { "DepartmentId", "IsActive", "IsDeleted" }, name: "IX_SharedService_Department_Active_Deleted");
            CreateIndex("dbo.SharedServices", "ServiceId", unique: true, name: "IX_SharedService_Service_Department_Deleted");
        }
    }
}
