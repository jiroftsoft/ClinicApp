namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeNewSetting : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Amount");
            CreateTable(
                "dbo.ServiceComponents",
                c => new
                    {
                        ServiceComponentId = c.Int(nullable: false, identity: true),
                        ServiceId = c.Int(nullable: false),
                        ComponentType = c.Int(nullable: false),
                        Coefficient = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
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
                    { "DynamicFilter_ServiceComponent_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceComponentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ServiceId, name: "IX_ServiceComponent_ServiceId")
                .Index(t => new { t.ServiceId, t.ComponentType, t.IsDeleted }, unique: true, name: "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted")
                .Index(t => new { t.ServiceId, t.IsActive, t.IsDeleted }, name: "IX_ServiceComponent_ServiceId_IsActive_IsDeleted")
                .Index(t => t.ComponentType, name: "IX_ServiceComponent_ComponentType")
                .Index(t => t.Coefficient, name: "IX_ServiceComponent_Coefficient")
                .Index(t => t.IsActive, name: "IX_ServiceComponent_IsActive")
                .Index(t => t.IsDeleted, name: "IX_ServiceComponent_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ServiceComponent_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ServiceComponent_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ServiceComponent_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ServiceComponent_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ServiceComponent_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ServiceComponent_UpdatedByUserId");
            
            CreateTable(
                "dbo.SharedServices",
                c => new
                    {
                        SharedServiceId = c.Int(nullable: false, identity: true),
                        ServiceId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        OverrideTechnicalFactor = c.Decimal(precision: 18, scale: 2),
                        OverrideProfessionalFactor = c.Decimal(precision: 18, scale: 2),
                        DepartmentSpecificNotes = c.String(maxLength: 500),
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
                    { "DynamicFilter_SharedService_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.SharedServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .Index(t => t.ServiceId, name: "IX_SharedService_ServiceId")
                .Index(t => new { t.ServiceId, t.DepartmentId, t.IsDeleted }, unique: true, name: "IX_SharedService_ServiceId_DepartmentId_IsDeleted")
                .Index(t => t.DepartmentId, name: "IX_SharedService_DepartmentId")
                .Index(t => new { t.DepartmentId, t.IsActive, t.IsDeleted }, name: "IX_SharedService_DepartmentId_IsActive_IsDeleted")
                .Index(t => t.IsActive, name: "IX_SharedService_IsActive")
                .Index(t => t.IsDeleted, name: "IX_SharedService_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_SharedService_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_SharedService_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_SharedService_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_SharedService_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_SharedService_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_SharedService_UpdatedByUserId");
            
            CreateTable(
                "dbo.FactorSettings",
                c => new
                    {
                        FactorSettingId = c.Int(nullable: false, identity: true),
                        FactorType = c.Int(nullable: false),
                        IsHashtagged = c.Boolean(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EffectiveFrom = c.DateTime(nullable: false),
                        EffectiveTo = c.DateTime(),
                        FinancialYear = c.Int(nullable: false),
                        IsActiveForCurrentYear = c.Boolean(nullable: false),
                        IsFrozen = c.Boolean(nullable: false),
                        FrozenAt = c.DateTime(),
                        FrozenByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                        Description = c.String(maxLength: 500),
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
                    { "DynamicFilter_FactorSetting_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.FactorSettingId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.FrozenByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.FactorType, name: "IX_FactorSetting_FactorType")
                .Index(t => new { t.FactorType, t.IsHashtagged, t.FinancialYear, t.IsActive, t.IsDeleted }, name: "IX_FactorSetting_FactorType_IsHashtagged_FinancialYear_IsActive_IsDeleted")
                .Index(t => t.IsHashtagged, name: "IX_FactorSetting_IsHashtagged")
                .Index(t => t.Value, name: "IX_FactorSetting_Value")
                .Index(t => t.EffectiveFrom, name: "IX_FactorSetting_EffectiveFrom")
                .Index(t => t.EffectiveTo, name: "IX_FactorSetting_EffectiveTo")
                .Index(t => t.FinancialYear, name: "IX_FactorSetting_FinancialYear")
                .Index(t => new { t.FinancialYear, t.IsFrozen, t.IsDeleted }, name: "IX_FactorSetting_FinancialYear_IsFrozen_IsDeleted")
                .Index(t => t.IsActiveForCurrentYear, name: "IX_FactorSetting_IsActiveForCurrentYear")
                .Index(t => new { t.IsActiveForCurrentYear, t.IsDeleted }, name: "IX_FactorSetting_IsActiveForCurrentYear_IsDeleted")
                .Index(t => t.IsFrozen, name: "IX_FactorSetting_IsFrozen")
                .Index(t => t.FrozenByUserId)
                .Index(t => t.IsDeleted, name: "IX_FactorSetting_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_FactorSetting_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            AddColumn("dbo.Services", "IsHashtagged", c => c.Boolean(nullable: false));
            AddColumn("dbo.Services", "TechnicalPart", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Services", "ProfessionalPart", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Appointments", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.InsurancePlans", "Deductible", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Receptions", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Receptions", "PatientCoPay", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Receptions", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.CashSessions", "OpeningBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.CashSessions", "CashBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.CashSessions", "PosBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.OnlinePayments", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.OnlinePayments", "GatewayFee", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.OnlinePayments", "NetAmount", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.PaymentGateways", "FixedFee", c => c.Decimal(precision: 18, scale: 4));
            AlterColumn("dbo.AppointmentSlots", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.Services", "IsHashtagged", name: "IX_Service_IsHashtagged");
            CreateIndex("dbo.Services", "TechnicalPart", name: "IX_Service_TechnicalPart");
            CreateIndex("dbo.Services", "ProfessionalPart", name: "IX_Service_ProfessionalPart");
            CreateIndex("dbo.OnlinePayments", "Amount", name: "IX_OnlinePayment_Amount");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FactorSettings", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FactorSettings", "FrozenByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FactorSettings", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FactorSettings", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SharedServices", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.SharedServices", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SharedServices", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.SharedServices", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SharedServices", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceComponents", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceComponents", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceComponents", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceComponents", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.FactorSettings", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FactorSettings", new[] { "CreatedByUserId" });
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_CreatedAt");
            DropIndex("dbo.FactorSettings", new[] { "DeletedByUserId" });
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsDeleted");
            DropIndex("dbo.FactorSettings", new[] { "FrozenByUserId" });
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsFrozen");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsActiveForCurrentYear_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsActiveForCurrentYear");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FinancialYear_IsFrozen_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FinancialYear");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_EffectiveTo");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_EffectiveFrom");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_Value");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_IsHashtagged");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FactorType_IsHashtagged_FinancialYear_IsActive_IsDeleted");
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_FactorType");
            DropIndex("dbo.OnlinePayments", "IX_OnlinePayment_Amount");
            DropIndex("dbo.SharedServices", "IX_SharedService_UpdatedByUserId");
            DropIndex("dbo.SharedServices", "IX_SharedService_UpdatedAt");
            DropIndex("dbo.SharedServices", "IX_SharedService_CreatedByUserId");
            DropIndex("dbo.SharedServices", "IX_SharedService_CreatedAt");
            DropIndex("dbo.SharedServices", "IX_SharedService_DeletedByUserId");
            DropIndex("dbo.SharedServices", "IX_SharedService_DeletedAt");
            DropIndex("dbo.SharedServices", "IX_SharedService_IsDeleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_IsActive");
            DropIndex("dbo.SharedServices", "IX_SharedService_DepartmentId_IsActive_IsDeleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_DepartmentId");
            DropIndex("dbo.SharedServices", "IX_SharedService_ServiceId_DepartmentId_IsDeleted");
            DropIndex("dbo.SharedServices", "IX_SharedService_ServiceId");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_UpdatedByUserId");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_UpdatedAt");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_CreatedByUserId");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_CreatedAt");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_DeletedByUserId");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_DeletedAt");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_IsDeleted");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_IsActive");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_Coefficient");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ComponentType");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId_IsActive_IsDeleted");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId_ComponentType_IsDeleted");
            DropIndex("dbo.ServiceComponents", "IX_ServiceComponent_ServiceId");
            DropIndex("dbo.Services", "IX_Service_ProfessionalPart");
            DropIndex("dbo.Services", "IX_Service_TechnicalPart");
            DropIndex("dbo.Services", "IX_Service_IsHashtagged");
            AlterColumn("dbo.AppointmentSlots", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.PaymentGateways", "FixedFee", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.OnlinePayments", "NetAmount", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.OnlinePayments", "GatewayFee", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.OnlinePayments", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.CashSessions", "PosBalance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.CashSessions", "CashBalance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.CashSessions", "OpeningBalance", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Receptions", "InsurerShareAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Receptions", "PatientCoPay", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Receptions", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.InsurancePlans", "Deductible", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Appointments", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Services", "ProfessionalPart");
            DropColumn("dbo.Services", "TechnicalPart");
            DropColumn("dbo.Services", "IsHashtagged");
            DropTable("dbo.FactorSettings",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_FactorSetting_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.SharedServices",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_SharedService_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ServiceComponents",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceComponent_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            CreateIndex("dbo.OnlinePayments", "Amount", name: "IX_OnlinePayment_Amount");
        }
    }
}
