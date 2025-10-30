namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change1404080302 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Receptions", "ReceptionNo", c => c.String(maxLength: 20));
            AddColumn("dbo.Receptions", "ClinicId", c => c.Int(nullable: false));
            AddColumn("dbo.Receptions", "DepartmentId", c => c.Int(nullable: false));
            AddColumn("dbo.Receptions", "Gross", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AddColumn("dbo.Receptions", "BasePay", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AddColumn("dbo.Receptions", "SuppPay", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AddColumn("dbo.Receptions", "PatientPay", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AddColumn("dbo.Receptions", "PaymentMethod", c => c.String(maxLength: 10));
            AddColumn("dbo.Receptions", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            CreateIndex("dbo.Receptions", "ReceptionNo", name: "IX_Reception_ReceptionNo");
            CreateIndex("dbo.Receptions", "ClinicId", name: "IX_Reception_ClinicId");
            CreateIndex("dbo.Receptions", "DepartmentId", name: "IX_Reception_DepartmentId");
            AddForeignKey("dbo.Receptions", "ClinicId", "dbo.Clinics", "ClinicId");
            AddForeignKey("dbo.Receptions", "DepartmentId", "dbo.Departments", "DepartmentId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Receptions", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Receptions", "ClinicId", "dbo.Clinics");
            DropIndex("dbo.Receptions", "IX_Reception_DepartmentId");
            DropIndex("dbo.Receptions", "IX_Reception_ClinicId");
            DropIndex("dbo.Receptions", "IX_Reception_ReceptionNo");
            DropColumn("dbo.Receptions", "RowVersion");
            DropColumn("dbo.Receptions", "PaymentMethod");
            DropColumn("dbo.Receptions", "PatientPay");
            DropColumn("dbo.Receptions", "SuppPay");
            DropColumn("dbo.Receptions", "BasePay");
            DropColumn("dbo.Receptions", "Gross");
            DropColumn("dbo.Receptions", "DepartmentId");
            DropColumn("dbo.Receptions", "ClinicId");
            DropColumn("dbo.Receptions", "ReceptionNo");
        }
    }
}
