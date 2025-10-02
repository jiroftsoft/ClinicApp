namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Drop_Id_Column_From_OnlinePayments : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Amount");
            AlterColumn("dbo.OnlinePayments", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.OnlinePayments", "GatewayFee", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.OnlinePayments", "NetAmount", c => c.Decimal(precision: 18, scale: 0));
            CreateIndex("dbo.OnlinePayments", "Amount", name: "IX_OnlinePayment_Amount");
            DropColumn("dbo.OnlinePayments", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OnlinePayments", "Id", c => c.Int(nullable: false));
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Amount");
            AlterColumn("dbo.OnlinePayments", "NetAmount", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.OnlinePayments", "GatewayFee", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.OnlinePayments", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.OnlinePayments", "Amount", name: "IX_OnlinePayment_Amount");
        }
    }
}
