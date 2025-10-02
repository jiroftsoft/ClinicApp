namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeSharedServiceConfig01 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SharedServices", "IX_SharedService_ServiceId");
            DropIndex("dbo.SharedServices", "IX_SharedService_ServiceId_DepartmentId_IsDeleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_DepartmentId");
            DropIndex("dbo.SharedServices", "IX_SharedService_IsActive");
            DropIndex("dbo.SharedServices", "IX_SharedService_IsDeleted");
            RenameIndex(table: "dbo.SharedServices", name: "IX_SharedService_DepartmentId_IsActive_IsDeleted", newName: "IX_SharedService_Department_Active_Deleted");
            AlterColumn("dbo.SharedServices", "OverrideTechnicalFactor", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.SharedServices", "OverrideProfessionalFactor", c => c.Decimal(precision: 18, scale: 4));
            CreateIndex("dbo.SharedServices", "ServiceId", unique: true, name: "IX_SharedService_Service_Department_Deleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SharedServices", "IX_SharedService_Service_Department_Deleted");
            AlterColumn("dbo.SharedServices", "OverrideProfessionalFactor", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.SharedServices", "OverrideTechnicalFactor", c => c.Decimal(precision: 18, scale: 2));
            RenameIndex(table: "dbo.SharedServices", name: "IX_SharedService_Department_Active_Deleted", newName: "IX_SharedService_DepartmentId_IsActive_IsDeleted");
            CreateIndex("dbo.SharedServices", "IsDeleted", name: "IX_SharedService_IsDeleted");
            CreateIndex("dbo.SharedServices", "IsActive", name: "IX_SharedService_IsActive");
            CreateIndex("dbo.SharedServices", "DepartmentId", name: "IX_SharedService_DepartmentId");
            CreateIndex("dbo.SharedServices", new[] { "ServiceId", "DepartmentId", "IsDeleted" }, unique: true, name: "IX_SharedService_ServiceId_DepartmentId_IsDeleted");
            CreateIndex("dbo.SharedServices", "ServiceId", name: "IX_SharedService_ServiceId");
        }
    }
}
