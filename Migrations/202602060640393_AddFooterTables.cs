namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddFooterTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FooterCertifications",
                c => new
                    {
                        FooterCertificationId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        ImageUrl = c.String(maxLength: 500),
                        LinkUrl = c.String(maxLength: 500),
                        LicenseNumber = c.String(maxLength: 100),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ClinicId = c.Int(),
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
                    { "DynamicFilter_FooterCertification_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.FooterCertificationId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.IsActive, name: "IX_FooterCertification_IsActive")
                .Index(t => t.IsDeleted, name: "IX_FooterCertification_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.FooterLinks",
                c => new
                    {
                        FooterLinkId = c.Int(nullable: false, identity: true),
                        LinkType = c.Byte(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Url = c.String(nullable: false, maxLength: 500),
                        Icon = c.String(maxLength: 100),
                        IsExternal = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ClinicId = c.Int(),
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
                    { "DynamicFilter_FooterLink_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.FooterLinkId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.LinkType, name: "IX_FooterLink_LinkType")
                .Index(t => t.IsActive, name: "IX_FooterLink_IsActive")
                .Index(t => t.IsDeleted, name: "IX_FooterLink_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.FooterSettings",
                c => new
                    {
                        FooterSettingsId = c.Int(nullable: false, identity: true),
                        ClinicId = c.Int(),
                        BrandClinicName = c.String(maxLength: 200),
                        BrandLogoUrl = c.String(maxLength: 500),
                        BrandTagline = c.String(maxLength: 300),
                        BrandDescription = c.String(maxLength: 1000),
                        BrandHomeUrl = c.String(maxLength: 200),
                        ContactPhone = c.String(maxLength: 50),
                        ContactEmergencyPhone = c.String(maxLength: 50),
                        ContactEmail = c.String(maxLength: 200),
                        ContactAddress = c.String(maxLength: 500),
                        ContactWhatsAppNumber = c.String(maxLength: 50),
                        LegalCopyrightText = c.String(maxLength: 500),
                        LegalPrivacyPolicyUrl = c.String(maxLength: 500),
                        LegalTermsOfServiceUrl = c.String(maxLength: 500),
                        LegalComplaintsUrl = c.String(maxLength: 500),
                        LegalMedicalPrivacyNotice = c.String(maxLength: 1000),
                        WorkingHoursTitle = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.FooterSettingsId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ClinicId, name: "IX_FooterSettings_ClinicId")
                .Index(t => t.IsActive, name: "IX_FooterSettings_IsActive")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.FooterSocials",
                c => new
                    {
                        FooterSocialId = c.Int(nullable: false, identity: true),
                        Platform = c.String(nullable: false, maxLength: 100),
                        Url = c.String(nullable: false, maxLength: 500),
                        Icon = c.String(maxLength: 100),
                        AriaLabel = c.String(maxLength: 200),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ClinicId = c.Int(),
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
                    { "DynamicFilter_FooterSocial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.FooterSocialId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.IsActive, name: "IX_FooterSocial_IsActive")
                .Index(t => t.IsDeleted, name: "IX_FooterSocial_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FooterSocials", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterSocials", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterSocials", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterSettings", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterSettings", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterLinks", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterLinks", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterLinks", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterCertifications", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterCertifications", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FooterCertifications", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.FooterSocials", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FooterSocials", new[] { "CreatedByUserId" });
            DropIndex("dbo.FooterSocials", new[] { "DeletedByUserId" });
            DropIndex("dbo.FooterSocials", "IX_FooterSocial_IsDeleted");
            DropIndex("dbo.FooterSocials", "IX_FooterSocial_IsActive");
            DropIndex("dbo.FooterSettings", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FooterSettings", new[] { "CreatedByUserId" });
            DropIndex("dbo.FooterSettings", "IX_FooterSettings_IsActive");
            DropIndex("dbo.FooterSettings", "IX_FooterSettings_ClinicId");
            DropIndex("dbo.FooterLinks", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FooterLinks", new[] { "CreatedByUserId" });
            DropIndex("dbo.FooterLinks", new[] { "DeletedByUserId" });
            DropIndex("dbo.FooterLinks", "IX_FooterLink_IsDeleted");
            DropIndex("dbo.FooterLinks", "IX_FooterLink_IsActive");
            DropIndex("dbo.FooterLinks", "IX_FooterLink_LinkType");
            DropIndex("dbo.FooterCertifications", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FooterCertifications", new[] { "CreatedByUserId" });
            DropIndex("dbo.FooterCertifications", new[] { "DeletedByUserId" });
            DropIndex("dbo.FooterCertifications", "IX_FooterCertification_IsDeleted");
            DropIndex("dbo.FooterCertifications", "IX_FooterCertification_IsActive");
            DropTable("dbo.FooterSocials",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_FooterSocial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.FooterSettings");
            DropTable("dbo.FooterLinks",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_FooterLink_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.FooterCertifications",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_FooterCertification_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
