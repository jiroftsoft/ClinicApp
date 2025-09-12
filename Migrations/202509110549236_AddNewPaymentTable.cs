namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewPaymentTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OnlinePayments",
                c => new
                    {
                        OnlinePaymentId = c.Int(nullable: false, identity: true),
                        PaymentGatewayId = c.Int(nullable: false),
                        ReceptionId = c.Int(),
                        AppointmentId = c.Int(),
                        PatientId = c.Int(nullable: false),
                        PaymentType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GatewayFee = c.Decimal(precision: 18, scale: 2),
                        NetAmount = c.Decimal(precision: 18, scale: 2),
                        GatewayTransactionId = c.String(maxLength: 100),
                        GatewayReferenceCode = c.String(maxLength: 100),
                        InternalTransactionId = c.String(maxLength: 100),
                        PaymentToken = c.String(maxLength: 500),
                        PaymentUrl = c.String(maxLength: 1000),
                        PaymentStartDate = c.DateTime(),
                        PaymentCompletionDate = c.DateTime(),
                        PaymentExpiryDate = c.DateTime(),
                        UserIpAddress = c.String(maxLength: 50),
                        UserAgent = c.String(maxLength: 500),
                        ErrorCode = c.String(maxLength: 50),
                        ErrorMessage = c.String(maxLength: 1000),
                        Description = c.String(maxLength: 1000),
                        AdditionalData = c.String(maxLength: 2000),
                        IsRefunded = c.Boolean(nullable: false),
                        RefundDate = c.DateTime(),
                        RefundAmount = c.Decimal(precision: 18, scale: 2),
                        RefundReason = c.String(maxLength: 500),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_OnlinePayment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.OnlinePaymentId)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.PaymentGateways", t => t.PaymentGatewayId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.PaymentGatewayId, name: "IX_OnlinePayment_PaymentGatewayId")
                .Index(t => new { t.PaymentGatewayId, t.Status }, name: "IX_OnlinePayment_GatewayId_Status")
                .Index(t => t.ReceptionId, name: "IX_OnlinePayment_ReceptionId")
                .Index(t => t.AppointmentId, name: "IX_OnlinePayment_AppointmentId")
                .Index(t => t.PatientId, name: "IX_OnlinePayment_PatientId")
                .Index(t => new { t.PatientId, t.PaymentType }, name: "IX_OnlinePayment_PatientId_PaymentType")
                .Index(t => t.PaymentType, name: "IX_OnlinePayment_PaymentType")
                .Index(t => new { t.PaymentType, t.Status, t.CreatedAt }, name: "IX_OnlinePayment_Type_Status_CreatedAt")
                .Index(t => t.Status, name: "IX_OnlinePayment_Status")
                .Index(t => new { t.Status, t.CreatedAt }, name: "IX_OnlinePayment_Status_CreatedAt")
                .Index(t => t.Amount, name: "IX_OnlinePayment_Amount")
                .Index(t => t.GatewayTransactionId, name: "IX_OnlinePayment_GatewayTransactionId")
                .Index(t => t.GatewayReferenceCode, name: "IX_OnlinePayment_GatewayReferenceCode")
                .Index(t => t.InternalTransactionId, name: "IX_OnlinePayment_InternalTransactionId")
                .Index(t => t.PaymentToken, name: "IX_OnlinePayment_PaymentToken")
                .Index(t => t.PaymentStartDate, name: "IX_OnlinePayment_PaymentStartDate")
                .Index(t => t.PaymentCompletionDate, name: "IX_OnlinePayment_PaymentCompletionDate")
                .Index(t => t.PaymentExpiryDate, name: "IX_OnlinePayment_PaymentExpiryDate")
                .Index(t => t.IsRefunded, name: "IX_OnlinePayment_IsRefunded")
                .Index(t => t.RefundDate, name: "IX_OnlinePayment_RefundDate")
                .Index(t => t.IsDeleted, name: "IX_OnlinePayment_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_OnlinePayment_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_OnlinePayment_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_OnlinePayment_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_OnlinePayment_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_OnlinePayment_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_OnlinePayment_UpdatedByUserId");
            
            CreateTable(
                "dbo.PaymentGateways",
                c => new
                    {
                        PaymentGatewayId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        GatewayType = c.Int(nullable: false),
                        MerchantId = c.String(nullable: false, maxLength: 100),
                        ApiKey = c.String(nullable: false, maxLength: 500),
                        PrivateKey = c.String(maxLength: 500),
                        GatewayUrl = c.String(nullable: false, maxLength: 500),
                        CallbackUrl = c.String(nullable: false, maxLength: 500),
                        SuccessUrl = c.String(maxLength: 500),
                        ErrorUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsTestMode = c.Boolean(nullable: false),
                        MinAmount = c.Decimal(precision: 18, scale: 2),
                        MaxAmount = c.Decimal(precision: 18, scale: 2),
                        FeePercentage = c.Decimal(precision: 5, scale: 2),
                        FixedFee = c.Decimal(precision: 18, scale: 2),
                        Description = c.String(maxLength: 1000),
                        AdditionalSettings = c.String(maxLength: 2000),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PaymentGateway_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PaymentGatewayId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_PaymentGateway_Name")
                .Index(t => t.GatewayType, name: "IX_PaymentGateway_GatewayType")
                .Index(t => t.MerchantId, name: "IX_PaymentGateway_MerchantId")
                .Index(t => t.IsActive, name: "IX_PaymentGateway_IsActive")
                .Index(t => t.IsTestMode, name: "IX_PaymentGateway_IsTestMode")
                .Index(t => t.IsDeleted, name: "IX_PaymentGateway_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_PaymentGateway_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_PaymentGateway_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_PaymentGateway_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_PaymentGateway_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PaymentGateway_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_PaymentGateway_UpdatedByUserId");
            
            AddColumn("dbo.PaymentTransactions", "PaymentGatewayId", c => c.Int());
            AddColumn("dbo.PaymentTransactions", "OnlinePaymentId", c => c.Int());
            CreateIndex("dbo.PaymentTransactions", "PaymentGatewayId", name: "IX_PaymentTransaction_PaymentGatewayId");
            CreateIndex("dbo.PaymentTransactions", "OnlinePaymentId", name: "IX_PaymentTransaction_OnlinePaymentId");
            AddForeignKey("dbo.PaymentTransactions", "OnlinePaymentId", "dbo.OnlinePayments", "OnlinePaymentId");
            AddForeignKey("dbo.PaymentTransactions", "PaymentGatewayId", "dbo.PaymentGateways", "PaymentGatewayId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaymentTransactions", "PaymentGatewayId", "dbo.PaymentGateways");
            DropForeignKey("dbo.PaymentTransactions", "OnlinePaymentId", "dbo.OnlinePayments");
            DropForeignKey("dbo.OnlinePayments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlinePayments", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.OnlinePayments", "PaymentGatewayId", "dbo.PaymentGateways");
            DropForeignKey("dbo.PaymentGateways", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentGateways", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentGateways", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlinePayments", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.OnlinePayments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlinePayments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlinePayments", "AppointmentId", "dbo.Appointments");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_UpdatedByUserId");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_UpdatedAt");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_CreatedByUserId");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_CreatedAt");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_DeletedByUserId");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_DeletedAt");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_IsDeleted");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_IsTestMode");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_IsActive");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_MerchantId");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_GatewayType");
            DropIndex("dbo.PaymentGateways", "IX_PaymentGateway_Name");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_UpdatedByUserId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_UpdatedAt");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_CreatedByUserId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_CreatedAt");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_DeletedByUserId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_DeletedAt");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_IsDeleted");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_RefundDate");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_IsRefunded");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentExpiryDate");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentCompletionDate");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentStartDate");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentToken");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_InternalTransactionId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_GatewayReferenceCode");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_GatewayTransactionId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Amount");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Status_CreatedAt");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Status");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Type_Status_CreatedAt");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentType");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PatientId_PaymentType");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PatientId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_AppointmentId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_ReceptionId");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_GatewayId_Status");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_PaymentGatewayId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_OnlinePaymentId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_PaymentGatewayId");
            DropColumn("dbo.PaymentTransactions", "OnlinePaymentId");
            DropColumn("dbo.PaymentTransactions", "PaymentGatewayId");
            DropTable("dbo.PaymentGateways",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PaymentGateway_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.OnlinePayments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_OnlinePayment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
