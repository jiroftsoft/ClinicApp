namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class Change14040621 : DbMigration
    {
        public override void Up()
        {
            AlterTableAnnotations(
                "dbo.PaymentGateways",
                c => new
                    {
                        PaymentGatewayId = c.Int(nullable: false, identity: true),
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        GatewayType = c.Int(nullable: false),
                        MerchantId = c.String(nullable: false, maxLength: 100),
                        ApiKey = c.String(nullable: false, maxLength: 500),
                        PrivateKey = c.String(maxLength: 500),
                        GatewayUrl = c.String(nullable: false, maxLength: 500),
                        CallbackUrl = c.String(nullable: false, maxLength: 500),
                        SuccessUrl = c.String(maxLength: 500),
                        ApiSecret = c.String(),
                        WebhookUrl = c.String(),
                        IsDefault = c.Boolean(nullable: false),
                        CreatedByUserName = c.String(),
                        UpdatedByUserName = c.String(),
                        TotalTransactions = c.Int(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SuccessRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AverageResponseTime = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastTransactionDate = c.DateTime(),
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
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_PaymentGateway_ActivePaymentGateways",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AlterTableAnnotations(
                "dbo.PosTerminals",
                c => new
                    {
                        PosTerminalId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        TerminalId = c.String(nullable: false, maxLength: 50),
                        MerchantId = c.String(nullable: false, maxLength: 50),
                        SerialNumber = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 50),
                        MacAddress = c.String(maxLength: 50),
                        Provider = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Protocol = c.Int(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        Port = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_PosTerminal_ActivePosTerminals",
                        new AnnotationValues(oldValue: null, newValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition")
                    },
                });
            
            AddColumn("dbo.PaymentTransactions", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentTransactions", "PatientName", c => c.String(maxLength: 200));
            AddColumn("dbo.PaymentTransactions", "DoctorName", c => c.String(maxLength: 200));
            AddColumn("dbo.PaymentTransactions", "ReceptionNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.PaymentTransactions", "CreatedByUserName", c => c.String(maxLength: 200));
            AddColumn("dbo.PaymentTransactions", "PaymentMethod", c => c.Byte(nullable: false));
            AddColumn("dbo.OnlinePayments", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.OnlinePayments", "ReferenceCode", c => c.String(maxLength: 100));
            AddColumn("dbo.OnlinePayments", "CompletedAt", c => c.DateTime());
            AddColumn("dbo.OnlinePayments", "ReceptionNumber", c => c.String());
            AddColumn("dbo.OnlinePayments", "PatientName", c => c.String());
            AddColumn("dbo.OnlinePayments", "DoctorName", c => c.String());
            AddColumn("dbo.OnlinePayments", "PaymentGatewayName", c => c.String());
            AddColumn("dbo.OnlinePayments", "CreatedByUserName", c => c.String());
            AddColumn("dbo.PaymentGateways", "Id", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentGateways", "ApiSecret", c => c.String());
            AddColumn("dbo.PaymentGateways", "WebhookUrl", c => c.String());
            AddColumn("dbo.PaymentGateways", "IsDefault", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentGateways", "CreatedByUserName", c => c.String());
            AddColumn("dbo.PaymentGateways", "UpdatedByUserName", c => c.String());
            AddColumn("dbo.PaymentGateways", "TotalTransactions", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentGateways", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PaymentGateways", "SuccessRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PaymentGateways", "AverageResponseTime", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PaymentGateways", "LastTransactionDate", c => c.DateTime());
            AddColumn("dbo.PosTerminals", "IsDefault", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PosTerminals", "IsDefault");
            DropColumn("dbo.PaymentGateways", "LastTransactionDate");
            DropColumn("dbo.PaymentGateways", "AverageResponseTime");
            DropColumn("dbo.PaymentGateways", "SuccessRate");
            DropColumn("dbo.PaymentGateways", "TotalAmount");
            DropColumn("dbo.PaymentGateways", "TotalTransactions");
            DropColumn("dbo.PaymentGateways", "UpdatedByUserName");
            DropColumn("dbo.PaymentGateways", "CreatedByUserName");
            DropColumn("dbo.PaymentGateways", "IsDefault");
            DropColumn("dbo.PaymentGateways", "WebhookUrl");
            DropColumn("dbo.PaymentGateways", "ApiSecret");
            DropColumn("dbo.PaymentGateways", "Id");
            DropColumn("dbo.OnlinePayments", "CreatedByUserName");
            DropColumn("dbo.OnlinePayments", "PaymentGatewayName");
            DropColumn("dbo.OnlinePayments", "DoctorName");
            DropColumn("dbo.OnlinePayments", "PatientName");
            DropColumn("dbo.OnlinePayments", "ReceptionNumber");
            DropColumn("dbo.OnlinePayments", "CompletedAt");
            DropColumn("dbo.OnlinePayments", "ReferenceCode");
            DropColumn("dbo.OnlinePayments", "Id");
            DropColumn("dbo.PaymentTransactions", "PaymentMethod");
            DropColumn("dbo.PaymentTransactions", "CreatedByUserName");
            DropColumn("dbo.PaymentTransactions", "ReceptionNumber");
            DropColumn("dbo.PaymentTransactions", "DoctorName");
            DropColumn("dbo.PaymentTransactions", "PatientName");
            DropColumn("dbo.PaymentTransactions", "Id");
            AlterTableAnnotations(
                "dbo.PosTerminals",
                c => new
                    {
                        PosTerminalId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        TerminalId = c.String(nullable: false, maxLength: 50),
                        MerchantId = c.String(nullable: false, maxLength: 50),
                        SerialNumber = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 50),
                        MacAddress = c.String(maxLength: 50),
                        Provider = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Protocol = c.Int(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        Port = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_PosTerminal_ActivePosTerminals",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
            AlterTableAnnotations(
                "dbo.PaymentGateways",
                c => new
                    {
                        PaymentGatewayId = c.Int(nullable: false, identity: true),
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        GatewayType = c.Int(nullable: false),
                        MerchantId = c.String(nullable: false, maxLength: 100),
                        ApiKey = c.String(nullable: false, maxLength: 500),
                        PrivateKey = c.String(maxLength: 500),
                        GatewayUrl = c.String(nullable: false, maxLength: 500),
                        CallbackUrl = c.String(nullable: false, maxLength: 500),
                        SuccessUrl = c.String(maxLength: 500),
                        ApiSecret = c.String(),
                        WebhookUrl = c.String(),
                        IsDefault = c.Boolean(nullable: false),
                        CreatedByUserName = c.String(),
                        UpdatedByUserName = c.String(),
                        TotalTransactions = c.Int(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SuccessRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AverageResponseTime = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastTransactionDate = c.DateTime(),
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
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "DynamicFilter_PaymentGateway_ActivePaymentGateways",
                        new AnnotationValues(oldValue: "EntityFramework.DynamicFilters.DynamicFilterDefinition", newValue: null)
                    },
                });
            
        }
    }
}
