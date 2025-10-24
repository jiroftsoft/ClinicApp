namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix_Money_Fields_To_Decimal18_0_IRR : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UnitPrice");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_PatientShareAmount");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_InsurerShareAmount");
            AlterColumn("dbo.Receptions", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.Receptions", "PatientCoPay", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.Receptions", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.ReceptionItems", "UnitPrice", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.ReceptionItems", "PatientShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.ReceptionItems", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.OnlinePayments", "RefundAmount", c => c.Decimal(precision: 18, scale: 0));
            CreateIndex("dbo.Receptions", "TotalAmount", name: "IX_Reception_TotalAmount");
            CreateIndex("dbo.Receptions", "PatientCoPay", name: "IX_Reception_PatientCoPay");
            CreateIndex("dbo.ReceptionItems", "UnitPrice", name: "IX_ReceptionItem_UnitPrice");
            CreateIndex("dbo.ReceptionItems", "PatientShareAmount", name: "IX_ReceptionItem_PatientShareAmount");
            CreateIndex("dbo.ReceptionItems", "InsurerShareAmount", name: "IX_ReceptionItem_InsurerShareAmount");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_InsurerShareAmount");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_PatientShareAmount");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UnitPrice");
            DropIndex("dbo.Receptions", "IX_Reception_PatientCoPay");
            DropIndex("dbo.Receptions", "IX_Reception_TotalAmount");
            AlterColumn("dbo.OnlinePayments", "RefundAmount", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.ReceptionItems", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.ReceptionItems", "PatientShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.ReceptionItems", "UnitPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Receptions", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Receptions", "PatientCoPay", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Receptions", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.ReceptionItems", "InsurerShareAmount", name: "IX_ReceptionItem_InsurerShareAmount");
            CreateIndex("dbo.ReceptionItems", "PatientShareAmount", name: "IX_ReceptionItem_PatientShareAmount");
            CreateIndex("dbo.ReceptionItems", "UnitPrice", name: "IX_ReceptionItem_UnitPrice");
        }
    }
}
