namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Drop_Id_Column_FromPaymentTransaction : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_Amount");
            AlterColumn("dbo.PaymentTransactions", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.PaymentTransactions", "Amount", name: "IX_PaymentTransaction_Amount");
            DropColumn("dbo.PaymentTransactions", "Id");
            DropColumn("dbo.PaymentTransactions", "PaymentMethod");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PaymentTransactions", "PaymentMethod", c => c.Byte(nullable: false));
            AddColumn("dbo.PaymentTransactions", "Id", c => c.Int(nullable: false));
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_Amount");
            AlterColumn("dbo.PaymentTransactions", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.PaymentTransactions", "Amount", name: "IX_PaymentTransaction_Amount");
        }
    }
}
