namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCashierAuditAndPerformanceEnt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CashierPerformanceMetrics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CashierId = c.String(nullable: false, maxLength: 128),
                        Date = c.DateTime(nullable: false, storeType: "date"),
                        TotalTransactions = c.Int(nullable: false),
                        PosTransactions = c.Int(nullable: false),
                        CashTransactions = c.Int(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        PosAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        CashAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        AverageTransactionTime = c.Decimal(nullable: false, precision: 10, scale: 2),
                        SuccessfulTransactions = c.Int(nullable: false),
                        FailedTransactions = c.Int(nullable: false),
                        SuccessRate = c.Decimal(nullable: false, precision: 5, scale: 2),
                        DiscrepancyCount = c.Int(nullable: false),
                        TotalDiscrepancy = c.Decimal(nullable: false, precision: 18, scale: 0),
                        SessionsOpened = c.Int(nullable: false),
                        SessionsClosed = c.Int(nullable: false),
                        AverageSessionDuration = c.Time(precision: 7),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CashierId)
                .Index(t => t.CashierId, name: "IX_CashierPerformanceMetrics_CashierId")
                .Index(t => new { t.CashierId, t.Date }, unique: true, name: "IX_CashierPerformanceMetrics_CashierId_Date_Unique")
                .Index(t => t.Date, name: "IX_CashierPerformanceMetrics_Date");
            
            CreateTable(
                "dbo.CashSessionAuditLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CashSessionId = c.Int(nullable: false),
                        Action = c.String(nullable: false, maxLength: 50),
                        OldValue = c.String(),
                        NewValue = c.String(),
                        Reason = c.String(maxLength: 500),
                        PerformedByUserId = c.String(nullable: false, maxLength: 128),
                        PerformedAt = c.DateTime(nullable: false),
                        IpAddress = c.String(maxLength: 50),
                        UserAgent = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CashSessions", t => t.CashSessionId)
                .ForeignKey("dbo.AspNetUsers", t => t.PerformedByUserId)
                .Index(t => t.CashSessionId, name: "IX_CashSessionAuditLog_CashSessionId")
                .Index(t => new { t.CashSessionId, t.PerformedAt }, name: "IX_CashSessionAuditLog_CashSessionId_PerformedAt")
                .Index(t => t.Action, name: "IX_CashSessionAuditLog_Action")
                .Index(t => t.PerformedByUserId, name: "IX_CashSessionAuditLog_PerformedByUserId")
                .Index(t => t.PerformedAt, name: "IX_CashSessionAuditLog_PerformedAt");
            
            CreateTable(
                "dbo.PaymentDiscrepancies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CashSessionId = c.Int(nullable: false),
                        PaymentTransactionId = c.Int(),
                        Type = c.Int(nullable: false),
                        ExpectedAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        ActualAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Difference = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Reason = c.String(maxLength: 500),
                        Resolution = c.String(maxLength: 500),
                        Status = c.Int(nullable: false),
                        ReportedByUserId = c.String(nullable: false, maxLength: 128),
                        ReportedAt = c.DateTime(nullable: false),
                        ResolvedByUserId = c.String(maxLength: 128),
                        ResolvedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CashSessions", t => t.CashSessionId)
                .ForeignKey("dbo.PaymentTransactions", t => t.PaymentTransactionId)
                .ForeignKey("dbo.AspNetUsers", t => t.ReportedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ResolvedByUserId)
                .Index(t => t.CashSessionId, name: "IX_PaymentDiscrepancy_CashSessionId")
                .Index(t => new { t.CashSessionId, t.Status }, name: "IX_PaymentDiscrepancy_CashSessionId_Status")
                .Index(t => t.PaymentTransactionId)
                .Index(t => t.Type, name: "IX_PaymentDiscrepancy_Type")
                .Index(t => t.Status, name: "IX_PaymentDiscrepancy_Status")
                .Index(t => new { t.Status, t.ReportedAt }, name: "IX_PaymentDiscrepancy_Status_ReportedAt")
                .Index(t => t.ReportedByUserId, name: "IX_PaymentDiscrepancy_ReportedByUserId")
                .Index(t => t.ReportedAt, name: "IX_PaymentDiscrepancy_ReportedAt")
                .Index(t => t.ResolvedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaymentDiscrepancies", "ResolvedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentDiscrepancies", "ReportedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentDiscrepancies", "PaymentTransactionId", "dbo.PaymentTransactions");
            DropForeignKey("dbo.PaymentDiscrepancies", "CashSessionId", "dbo.CashSessions");
            DropForeignKey("dbo.CashSessionAuditLogs", "PerformedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashSessionAuditLogs", "CashSessionId", "dbo.CashSessions");
            DropForeignKey("dbo.CashierPerformanceMetrics", "CashierId", "dbo.AspNetUsers");
            DropIndex("dbo.PaymentDiscrepancies", new[] { "ResolvedByUserId" });
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_ReportedAt");
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_ReportedByUserId");
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_Status_ReportedAt");
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_Status");
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_Type");
            DropIndex("dbo.PaymentDiscrepancies", new[] { "PaymentTransactionId" });
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_CashSessionId_Status");
            DropIndex("dbo.PaymentDiscrepancies", "IX_PaymentDiscrepancy_CashSessionId");
            DropIndex("dbo.CashSessionAuditLogs", "IX_CashSessionAuditLog_PerformedAt");
            DropIndex("dbo.CashSessionAuditLogs", "IX_CashSessionAuditLog_PerformedByUserId");
            DropIndex("dbo.CashSessionAuditLogs", "IX_CashSessionAuditLog_Action");
            DropIndex("dbo.CashSessionAuditLogs", "IX_CashSessionAuditLog_CashSessionId_PerformedAt");
            DropIndex("dbo.CashSessionAuditLogs", "IX_CashSessionAuditLog_CashSessionId");
            DropIndex("dbo.CashierPerformanceMetrics", "IX_CashierPerformanceMetrics_Date");
            DropIndex("dbo.CashierPerformanceMetrics", "IX_CashierPerformanceMetrics_CashierId_Date_Unique");
            DropIndex("dbo.CashierPerformanceMetrics", "IX_CashierPerformanceMetrics_CashierId");
            DropTable("dbo.PaymentDiscrepancies");
            DropTable("dbo.CashSessionAuditLogs");
            DropTable("dbo.CashierPerformanceMetrics");
        }
    }
}
