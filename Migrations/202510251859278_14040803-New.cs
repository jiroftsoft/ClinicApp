namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _14040803New : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Receptions", "FinancialYear", c => c.Int(nullable: false));
            AddColumn("dbo.Receptions", "BasePlanId", c => c.Int());
            AddColumn("dbo.Receptions", "SupplementaryPlanId", c => c.Int());
            AddColumn("dbo.PaymentTransactions", "IdempotencyKey", c => c.String(maxLength: 100));
            AddColumn("dbo.PaymentTransactions", "TerminalId", c => c.String(maxLength: 50));
            AddColumn("dbo.PaymentTransactions", "CardLast4", c => c.String(maxLength: 4));
            CreateIndex("dbo.Receptions", "FinancialYear", name: "IX_Reception_FinancialYear");
            CreateIndex("dbo.Receptions", "BasePlanId", name: "IX_Reception_BasePlanId");
            CreateIndex("dbo.Receptions", "SupplementaryPlanId", name: "IX_Reception_SupplementaryPlanId");
            CreateIndex("dbo.PaymentTransactions", "IdempotencyKey", name: "IX_PaymentTransaction_IdempotencyKey");
            CreateIndex("dbo.PaymentTransactions", "TerminalId", name: "IX_PaymentTransaction_TerminalId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_TerminalId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_IdempotencyKey");
            DropIndex("dbo.Receptions", "IX_Reception_SupplementaryPlanId");
            DropIndex("dbo.Receptions", "IX_Reception_BasePlanId");
            DropIndex("dbo.Receptions", "IX_Reception_FinancialYear");
            DropColumn("dbo.PaymentTransactions", "CardLast4");
            DropColumn("dbo.PaymentTransactions", "TerminalId");
            DropColumn("dbo.PaymentTransactions", "IdempotencyKey");
            DropColumn("dbo.Receptions", "SupplementaryPlanId");
            DropColumn("dbo.Receptions", "BasePlanId");
            DropColumn("dbo.Receptions", "FinancialYear");
        }
    }
}
