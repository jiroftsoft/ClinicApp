namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class ClinicBankAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClinicBankAccounts",
                c => new
                    {
                        ClinicBankAccountId = c.Int(nullable: false, identity: true),
                        ClinicId = c.Int(nullable: false),
                        IbanNumber = c.String(nullable: false, maxLength: 26),
                        BankName = c.String(nullable: false, maxLength: 100),
                        AccountNumber = c.String(maxLength: 50),
                        AccountHolderName = c.String(nullable: false, maxLength: 200),
                        IsDefault = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Description = c.String(maxLength: 500),
                        CreatedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ClinicBankAccount_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ClinicBankAccountId)
                .ForeignKey("dbo.Clinics", t => t.ClinicId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ClinicId, unique: true, name: "IX_ClinicBankAccount_ClinicId")
                .Index(t => new { t.ClinicId, t.IsActive, t.IsDeleted }, name: "IX_ClinicBankAccount_ClinicId_IsActive_IsDeleted")
                .Index(t => t.IbanNumber, name: "IX_ClinicBankAccount_IbanNumber")
                .Index(t => t.IsActive, name: "IX_ClinicBankAccount_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted }, name: "IX_ClinicBankAccount_IsActive_IsDeleted")
                .Index(t => t.CreatedByUserId, name: "IX_ClinicBankAccount_CreatedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ClinicBankAccount_CreatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ClinicBankAccount_UpdatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ClinicBankAccount_UpdatedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ClinicBankAccount_DeletedByUserId")
                .Index(t => t.IsDeleted, name: "IX_ClinicBankAccount_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ClinicBankAccount_DeletedAt");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClinicBankAccounts", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicBankAccounts", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicBankAccounts", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicBankAccounts", "ClinicId", "dbo.Clinics");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_DeletedAt");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_IsDeleted");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_DeletedByUserId");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_UpdatedAt");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_UpdatedByUserId");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_CreatedAt");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_CreatedByUserId");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_IsActive_IsDeleted");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_IsActive");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_IbanNumber");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_ClinicId_IsActive_IsDeleted");
            DropIndex("dbo.ClinicBankAccounts", "IX_ClinicBankAccount_ClinicId");
            DropTable("dbo.ClinicBankAccounts",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ClinicBankAccount_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
