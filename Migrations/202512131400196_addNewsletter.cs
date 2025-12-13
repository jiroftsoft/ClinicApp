namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class addNewsletter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NewsletterCampaignRecipients",
                c => new
                    {
                        NewsletterCampaignRecipientId = c.Int(nullable: false, identity: true),
                        NewsletterCampaignId = c.Int(nullable: false),
                        NewsletterSubscriptionId = c.Int(nullable: false),
                        Email = c.String(nullable: false, maxLength: 200),
                        Status = c.Byte(nullable: false),
                        SentAt = c.DateTime(),
                        OpenedAt = c.DateTime(),
                        ClickedAt = c.DateTime(),
                        ClickedUrl = c.String(maxLength: 1000),
                        ErrorMessage = c.String(maxLength: 1000),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.NewsletterCampaignRecipientId)
                .ForeignKey("dbo.NewsletterCampaigns", t => t.NewsletterCampaignId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.NewsletterSubscriptions", t => t.NewsletterSubscriptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.NewsletterCampaignId, t.Status }, name: "IX_NewsletterCampaignRecipient_CampaignId_Status")
                .Index(t => new { t.NewsletterSubscriptionId, t.NewsletterCampaignId }, name: "IX_NewsletterCampaignRecipient_SubscriptionId_CampaignId")
                .Index(t => t.Email, name: "IX_NewsletterCampaignRecipient_Email")
                .Index(t => t.Status, name: "IX_NewsletterCampaignRecipient_Status")
                .Index(t => t.SentAt, name: "IX_NewsletterCampaignRecipient_SentAt")
                .Index(t => t.OpenedAt, name: "IX_NewsletterCampaignRecipient_OpenedAt")
                .Index(t => t.ClickedAt, name: "IX_NewsletterCampaignRecipient_ClickedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.NewsletterCampaigns",
                c => new
                    {
                        NewsletterCampaignId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 300),
                        Subject = c.String(nullable: false, maxLength: 500),
                        Content = c.String(nullable: false, storeType: "ntext"),
                        NewsletterTemplateId = c.Int(),
                        Categories = c.String(storeType: "ntext"),
                        SendToAll = c.Boolean(nullable: false),
                        ScheduledAt = c.DateTime(),
                        SentAt = c.DateTime(),
                        Status = c.Byte(nullable: false),
                        TotalRecipients = c.Int(nullable: false),
                        SentCount = c.Int(nullable: false),
                        FailedCount = c.Int(nullable: false),
                        OpenedCount = c.Int(nullable: false),
                        ClickedCount = c.Int(nullable: false),
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
                    { "DynamicFilter_NewsletterCampaign_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.NewsletterCampaignId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.NewsletterTemplates", t => t.NewsletterTemplateId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_NewsletterCampaign_Title")
                .Index(t => t.Subject, name: "IX_NewsletterCampaign_Subject")
                .Index(t => t.NewsletterTemplateId)
                .Index(t => t.ScheduledAt, name: "IX_NewsletterCampaign_ScheduledAt")
                .Index(t => new { t.ScheduledAt, t.Status }, name: "IX_NewsletterCampaign_ScheduledAt_Status")
                .Index(t => t.SentAt, name: "IX_NewsletterCampaign_SentAt")
                .Index(t => t.Status, name: "IX_NewsletterCampaign_Status")
                .Index(t => new { t.Status, t.IsDeleted, t.CreatedAt }, name: "IX_NewsletterCampaign_Status_Deleted_CreatedAt")
                .Index(t => t.IsDeleted, name: "IX_NewsletterCampaign_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_NewsletterCampaign_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.NewsletterTemplates",
                c => new
                    {
                        NewsletterTemplateId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Subject = c.String(nullable: false, maxLength: 500),
                        Content = c.String(nullable: false, storeType: "ntext"),
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
                    { "DynamicFilter_NewsletterTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.NewsletterTemplateId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_NewsletterTemplate_Name")
                .Index(t => t.Subject, name: "IX_NewsletterTemplate_Subject")
                .Index(t => t.IsActive, name: "IX_NewsletterTemplate_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted, t.CreatedAt }, name: "IX_NewsletterTemplate_Active_Deleted_CreatedAt")
                .Index(t => t.IsDeleted, name: "IX_NewsletterTemplate_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_NewsletterTemplate_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.NewsletterSubscriptions",
                c => new
                    {
                        NewsletterSubscriptionId = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false, maxLength: 200),
                        FullName = c.String(maxLength: 200),
                        PhoneNumber = c.String(maxLength: 50),
                        Categories = c.String(storeType: "ntext"),
                        Source = c.Byte(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        VerificationToken = c.String(maxLength: 100),
                        VerifiedAt = c.DateTime(),
                        UnsubscribedAt = c.DateTime(),
                        UnsubscribeToken = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 500),
                        UserAgent = c.String(maxLength: 500),
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
                    { "DynamicFilter_NewsletterSubscription_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.NewsletterSubscriptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Email, unique: true, name: "IX_NewsletterSubscription_Email")
                .Index(t => t.FullName, name: "IX_NewsletterSubscription_FullName")
                .Index(t => t.Source, name: "IX_NewsletterSubscription_Source")
                .Index(t => new { t.IsActive, t.IsVerified, t.IsDeleted, t.Source }, name: "IX_NewsletterSubscription_Active_Verified_Deleted_Source")
                .Index(t => t.IsActive, name: "IX_NewsletterSubscription_IsActive")
                .Index(t => new { t.IsActive, t.IsVerified, t.CreatedAt }, name: "IX_NewsletterSubscription_Active_Verified_CreatedAt")
                .Index(t => t.IsVerified, name: "IX_NewsletterSubscription_IsVerified")
                .Index(t => t.VerificationToken, unique: true, name: "IX_NewsletterSubscription_VerificationToken")
                .Index(t => t.UnsubscribeToken, unique: true, name: "IX_NewsletterSubscription_UnsubscribeToken")
                .Index(t => t.IsDeleted, name: "IX_NewsletterSubscription_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_NewsletterSubscription_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NewsletterCampaignRecipients", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaignRecipients", "NewsletterSubscriptionId", "dbo.NewsletterSubscriptions");
            DropForeignKey("dbo.NewsletterSubscriptions", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterSubscriptions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterSubscriptions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaignRecipients", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaignRecipients", "NewsletterCampaignId", "dbo.NewsletterCampaigns");
            DropForeignKey("dbo.NewsletterCampaigns", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaigns", "NewsletterTemplateId", "dbo.NewsletterTemplates");
            DropForeignKey("dbo.NewsletterTemplates", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterTemplates", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterTemplates", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaigns", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NewsletterCampaigns", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.NewsletterSubscriptions", new[] { "UpdatedByUserId" });
            DropIndex("dbo.NewsletterSubscriptions", new[] { "CreatedByUserId" });
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_CreatedAt");
            DropIndex("dbo.NewsletterSubscriptions", new[] { "DeletedByUserId" });
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_IsDeleted");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_UnsubscribeToken");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_VerificationToken");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_IsVerified");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_Active_Verified_CreatedAt");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_IsActive");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_Active_Verified_Deleted_Source");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_Source");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_FullName");
            DropIndex("dbo.NewsletterSubscriptions", "IX_NewsletterSubscription_Email");
            DropIndex("dbo.NewsletterTemplates", new[] { "UpdatedByUserId" });
            DropIndex("dbo.NewsletterTemplates", new[] { "CreatedByUserId" });
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_CreatedAt");
            DropIndex("dbo.NewsletterTemplates", new[] { "DeletedByUserId" });
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_IsDeleted");
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_Active_Deleted_CreatedAt");
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_IsActive");
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_Subject");
            DropIndex("dbo.NewsletterTemplates", "IX_NewsletterTemplate_Name");
            DropIndex("dbo.NewsletterCampaigns", new[] { "UpdatedByUserId" });
            DropIndex("dbo.NewsletterCampaigns", new[] { "CreatedByUserId" });
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_CreatedAt");
            DropIndex("dbo.NewsletterCampaigns", new[] { "DeletedByUserId" });
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_IsDeleted");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_Status_Deleted_CreatedAt");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_Status");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_SentAt");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_ScheduledAt_Status");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_ScheduledAt");
            DropIndex("dbo.NewsletterCampaigns", new[] { "NewsletterTemplateId" });
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_Subject");
            DropIndex("dbo.NewsletterCampaigns", "IX_NewsletterCampaign_Title");
            DropIndex("dbo.NewsletterCampaignRecipients", new[] { "UpdatedByUserId" });
            DropIndex("dbo.NewsletterCampaignRecipients", new[] { "CreatedByUserId" });
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_ClickedAt");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_OpenedAt");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_SentAt");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_Status");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_Email");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_SubscriptionId_CampaignId");
            DropIndex("dbo.NewsletterCampaignRecipients", "IX_NewsletterCampaignRecipient_CampaignId_Status");
            DropTable("dbo.NewsletterSubscriptions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_NewsletterSubscription_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.NewsletterTemplates",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_NewsletterTemplate_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.NewsletterCampaigns",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_NewsletterCampaign_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.NewsletterCampaignRecipients");
        }
    }
}
