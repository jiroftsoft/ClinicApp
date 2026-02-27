namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddInsuranceClaimsAndBatches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InsuranceBatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BatchNumber = c.String(nullable: false, maxLength: 50),
                        InsuranceProviderId = c.Int(nullable: false),
                        SubmissionDate = c.DateTime(nullable: false),
                        SettlementDate = c.DateTime(),
                        TotalClaimed = c.Decimal(nullable: false, precision: 18, scale: 0),
                        TotalApproved = c.Decimal(nullable: false, precision: 18, scale: 0),
                        TotalDeduction = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Status = c.Byte(nullable: false),
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
                    { "DynamicFilter_InsuranceBatch_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.InsuranceProviders", t => t.InsuranceProviderId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.BatchNumber, unique: true, name: "UX_InsuranceBatch_BatchNumber")
                .Index(t => new { t.InsuranceProviderId, t.SubmissionDate }, name: "IX_InsuranceBatch_Provider_Submission")
                .Index(t => new { t.Status, t.IsDeleted }, name: "IX_InsuranceBatch_Status_Deleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.InsuranceClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        InsurancePlanId = c.Int(nullable: false),
                        BatchId = c.Int(),
                        ClaimedAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        ApprovedAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        DeductionAmount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        FinalSettlement = c.Decimal(nullable: false, precision: 18, scale: 0),
                        SubmissionDate = c.DateTime(nullable: false),
                        ApprovalDate = c.DateTime(),
                        PaymentDate = c.DateTime(),
                        Status = c.Byte(nullable: false),
                        RejectionReason = c.String(maxLength: 500),
                        DeductionDetails = c.String(maxLength: 2000),
                        PaymentTransactionId = c.Int(),
                        ReceptionId = c.Int(),
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
                    { "DynamicFilter_InsuranceClaim_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InsuranceBatches", t => t.BatchId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.InsurancePlans", t => t.InsurancePlanId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.PaymentTransactions", t => t.PaymentTransactionId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.PatientId)
                .Index(t => new { t.InsurancePlanId, t.SubmissionDate }, name: "IX_InsuranceClaim_Plan_Submission")
                .Index(t => t.BatchId, name: "IX_InsuranceClaim_BatchId")
                .Index(t => new { t.Status, t.IsDeleted }, name: "IX_InsuranceClaim_Status_Deleted")
                .Index(t => t.PaymentTransactionId)
                .Index(t => t.ReceptionId)
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InsuranceBatches", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceBatches", "InsuranceProviderId", "dbo.InsuranceProviders");
            DropForeignKey("dbo.InsuranceBatches", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceBatches", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceClaims", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceClaims", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.InsuranceClaims", "PaymentTransactionId", "dbo.PaymentTransactions");
            DropForeignKey("dbo.InsuranceClaims", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.InsuranceClaims", "InsurancePlanId", "dbo.InsurancePlans");
            DropForeignKey("dbo.InsuranceClaims", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceClaims", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceClaims", "BatchId", "dbo.InsuranceBatches");
            DropIndex("dbo.InsuranceClaims", new[] { "UpdatedByUserId" });
            DropIndex("dbo.InsuranceClaims", new[] { "CreatedByUserId" });
            DropIndex("dbo.InsuranceClaims", new[] { "DeletedByUserId" });
            DropIndex("dbo.InsuranceClaims", new[] { "ReceptionId" });
            DropIndex("dbo.InsuranceClaims", new[] { "PaymentTransactionId" });
            DropIndex("dbo.InsuranceClaims", "IX_InsuranceClaim_Status_Deleted");
            DropIndex("dbo.InsuranceClaims", "IX_InsuranceClaim_BatchId");
            DropIndex("dbo.InsuranceClaims", "IX_InsuranceClaim_Plan_Submission");
            DropIndex("dbo.InsuranceClaims", new[] { "PatientId" });
            DropIndex("dbo.InsuranceBatches", new[] { "UpdatedByUserId" });
            DropIndex("dbo.InsuranceBatches", new[] { "CreatedByUserId" });
            DropIndex("dbo.InsuranceBatches", new[] { "DeletedByUserId" });
            DropIndex("dbo.InsuranceBatches", "IX_InsuranceBatch_Status_Deleted");
            DropIndex("dbo.InsuranceBatches", "IX_InsuranceBatch_Provider_Submission");
            DropIndex("dbo.InsuranceBatches", "UX_InsuranceBatch_BatchNumber");
            DropTable("dbo.InsuranceClaims",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_InsuranceClaim_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.InsuranceBatches",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_InsuranceBatch_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
