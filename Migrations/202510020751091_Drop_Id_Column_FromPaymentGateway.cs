namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Drop_Id_Column_FromPaymentGateway : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PaymentGateways", "MinAmount", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.PaymentGateways", "MaxAmount", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.PaymentGateways", "FixedFee", c => c.Decimal(precision: 18, scale: 0));
            DropColumn("dbo.PaymentGateways", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PaymentGateways", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentGateways", "FixedFee", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.PaymentGateways", "MaxAmount", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.PaymentGateways", "MinAmount", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
