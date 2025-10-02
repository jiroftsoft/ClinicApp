namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cashSession01 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CashSessions", "OpeningBalance", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.CashSessions", "CashBalance", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.CashSessions", "PosBalance", c => c.Decimal(nullable: false, precision: 18, scale: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CashSessions", "PosBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.CashSessions", "CashBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.CashSessions", "OpeningBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
    }
}
