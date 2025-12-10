namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class CMSTables14040919 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Announcements",
                c => new
                    {
                        AnnouncementId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 300),
                        Content = c.String(maxLength: 2000),
                        ImageUrl = c.String(maxLength: 500),
                        LinkUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsImportant = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Type = c.String(maxLength: 50),
                        TargetAudience = c.String(maxLength: 100),
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
                    { "DynamicFilter_Announcement_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.AnnouncementId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_Announcement_Title")
                .Index(t => t.IsActive, name: "IX_Announcement_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder, t.StartDate, t.EndDate }, name: "IX_Announcement_Active_Deleted_Order_Date")
                .Index(t => t.IsImportant, name: "IX_Announcement_IsImportant")
                .Index(t => t.DisplayOrder, name: "IX_Announcement_DisplayOrder")
                .Index(t => t.Type, name: "IX_Announcement_Type")
                .Index(t => t.IsDeleted, name: "IX_Announcement_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.BlogPosts",
                c => new
                    {
                        BlogPostId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500),
                        Summary = c.String(maxLength: 1000),
                        Content = c.String(nullable: false, storeType: "ntext"),
                        ImageUrl = c.String(maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 200),
                        AuthorName = c.String(maxLength: 100),
                        CategoryName = c.String(maxLength: 50),
                        PublishedAt = c.DateTime(),
                        IsPublished = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        DisplayOrder = c.Int(),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
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
                    { "DynamicFilter_BlogPost_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.BlogPostId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_BlogPost_Title")
                .Index(t => t.PublishedAt, name: "IX_BlogPost_PublishedAt")
                .Index(t => new { t.IsPublished, t.IsDeleted, t.PublishedAt }, name: "IX_BlogPost_Published_Deleted_Date")
                .Index(t => t.IsPublished, name: "IX_BlogPost_IsPublished")
                .Index(t => t.IsFeatured, name: "IX_BlogPost_IsFeatured")
                .Index(t => t.Slug, unique: true, name: "IX_BlogPost_Slug")
                .Index(t => t.IsDeleted, name: "IX_BlogPost_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.ClinicWorkingHours",
                c => new
                    {
                        ClinicWorkingHoursId = c.Int(nullable: false, identity: true),
                        ClinicId = c.Int(),
                        DayOfWeek = c.Int(nullable: false),
                        DayName = c.String(nullable: false, maxLength: 20),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        IsOpen = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
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
                    { "DynamicFilter_ClinicWorkingHours_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ClinicWorkingHoursId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ClinicId, name: "IX_ClinicWorkingHours_ClinicId")
                .Index(t => new { t.ClinicId, t.DayOfWeek, t.IsActive, t.IsDeleted }, name: "IX_ClinicWorkingHours_ClinicId_DayOfWeek_Active_Deleted")
                .Index(t => t.DayOfWeek, name: "IX_ClinicWorkingHours_DayOfWeek")
                .Index(t => t.IsOpen, name: "IX_ClinicWorkingHours_IsOpen")
                .Index(t => t.IsActive, name: "IX_ClinicWorkingHours_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder }, name: "IX_ClinicWorkingHours_Active_Deleted_Order")
                .Index(t => t.IsDeleted, name: "IX_ClinicWorkingHours_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.EmergencyContacts",
                c => new
                    {
                        EmergencyContactId = c.Int(nullable: false, identity: true),
                        ContactType = c.String(nullable: false, maxLength: 50),
                        Title = c.String(nullable: false, maxLength: 200),
                        PhoneNumber = c.String(nullable: false, maxLength: 50),
                        SecondaryPhoneNumber = c.String(maxLength: 50),
                        Address = c.String(maxLength: 500),
                        Instructions = c.String(maxLength: 2000),
                        MapUrl = c.String(maxLength: 500),
                        WhatsAppUrl = c.String(maxLength: 500),
                        TelegramUrl = c.String(maxLength: 500),
                        Email = c.String(maxLength: 500),
                        WebsiteUrl = c.String(maxLength: 500),
                        IconUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsAlwaysVisible = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        ShortDescription = c.String(maxLength: 500),
                        Slug = c.String(maxLength: 200),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
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
                    { "DynamicFilter_EmergencyContact_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.EmergencyContactId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ContactType, name: "IX_EmergencyContact_ContactType")
                .Index(t => new { t.IsActive, t.IsDeleted, t.ContactType }, name: "IX_EmergencyContact_Active_Deleted_Type")
                .Index(t => t.IsActive, name: "IX_EmergencyContact_IsActive")
                .Index(t => new { t.IsAlwaysVisible, t.IsActive, t.DisplayOrder }, name: "IX_EmergencyContact_AlwaysVisible_Active_Order")
                .Index(t => t.IsAlwaysVisible, name: "IX_EmergencyContact_IsAlwaysVisible")
                .Index(t => t.DisplayOrder, name: "IX_EmergencyContact_DisplayOrder")
                .Index(t => t.Slug, unique: true, name: "IX_EmergencyContact_Slug")
                .Index(t => t.IsDeleted, name: "IX_EmergencyContact_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.FAQs",
                c => new
                    {
                        FAQId = c.Int(nullable: false, identity: true),
                        Question = c.String(nullable: false, maxLength: 500),
                        Answer = c.String(nullable: false, storeType: "ntext"),
                        Category = c.String(maxLength: 100),
                        Tags = c.String(maxLength: 500),
                        RelatedLinkUrl = c.String(maxLength: 500),
                        ViewCount = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
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
                    { "DynamicFilter_FAQ_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.FAQId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Question, name: "IX_FAQ_Question")
                .Index(t => t.Category, name: "IX_FAQ_Category")
                .Index(t => new { t.IsActive, t.IsDeleted, t.Category }, name: "IX_FAQ_Active_Deleted_Category")
                .Index(t => t.ViewCount, name: "IX_FAQ_ViewCount")
                .Index(t => t.IsActive, name: "IX_FAQ_IsActive")
                .Index(t => new { t.IsFeatured, t.IsActive, t.DisplayOrder }, name: "IX_FAQ_Featured_Active_Order")
                .Index(t => t.IsFeatured, name: "IX_FAQ_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_FAQ_DisplayOrder")
                .Index(t => t.Slug, unique: true, name: "IX_FAQ_Slug")
                .Index(t => t.IsDeleted, name: "IX_FAQ_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.GalleryItems",
                c => new
                    {
                        GalleryItemId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        ImageUrl = c.String(nullable: false, maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        Category = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
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
                    { "DynamicFilter_GalleryItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.GalleryItemId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_GalleryItem_Title")
                .Index(t => t.Category, name: "IX_GalleryItem_Category")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder, t.Category }, name: "IX_GalleryItem_Active_Deleted_Order_Category")
                .Index(t => t.IsActive, name: "IX_GalleryItem_IsActive")
                .Index(t => t.DisplayOrder, name: "IX_GalleryItem_DisplayOrder")
                .Index(t => t.IsDeleted, name: "IX_GalleryItem_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.HealthTips",
                c => new
                    {
                        HealthTipId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 300),
                        Summary = c.String(maxLength: 500),
                        Content = c.String(nullable: false, storeType: "ntext"),
                        ImageUrl = c.String(maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        Category = c.String(maxLength: 100),
                        Tags = c.String(maxLength: 500),
                        PublishedAt = c.DateTime(),
                        ExpiryDate = c.DateTime(),
                        IsPublished = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        ShareCount = c.Int(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
                        RelatedLinkUrl = c.String(maxLength: 500),
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
                    { "DynamicFilter_HealthTip_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.HealthTipId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_HealthTip_Title")
                .Index(t => t.Category, name: "IX_HealthTip_Category")
                .Index(t => new { t.Category, t.IsPublished, t.IsDeleted }, name: "IX_HealthTip_Category_Published_Deleted")
                .Index(t => t.PublishedAt, name: "IX_HealthTip_PublishedAt")
                .Index(t => new { t.IsPublished, t.IsDeleted, t.PublishedAt, t.ExpiryDate }, name: "IX_HealthTip_Published_Deleted_Dates")
                .Index(t => t.ExpiryDate, name: "IX_HealthTip_ExpiryDate")
                .Index(t => t.IsPublished, name: "IX_HealthTip_IsPublished")
                .Index(t => new { t.IsFeatured, t.IsPublished, t.DisplayOrder }, name: "IX_HealthTip_Featured_Published_Order")
                .Index(t => t.IsFeatured, name: "IX_HealthTip_IsFeatured")
                .Index(t => t.ViewCount, name: "IX_HealthTip_ViewCount")
                .Index(t => t.DisplayOrder, name: "IX_HealthTip_DisplayOrder")
                .Index(t => t.Slug, unique: true, name: "IX_HealthTip_Slug")
                .Index(t => t.IsDeleted, name: "IX_HealthTip_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.InsuranceInfos",
                c => new
                    {
                        InsuranceInfoId = c.Int(nullable: false, identity: true),
                        InsuranceName = c.String(nullable: false, maxLength: 200),
                        InsuranceType = c.String(maxLength: 100),
                        Description = c.String(maxLength: 500),
                        FullDescription = c.String(storeType: "ntext"),
                        LogoUrl = c.String(maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        ContactPhone = c.String(maxLength: 200),
                        WebsiteUrl = c.String(maxLength: 200),
                        Address = c.String(maxLength: 500),
                        CoveragePercentage = c.Decimal(precision: 18, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
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
                    { "DynamicFilter_InsuranceInfo_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.InsuranceInfoId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.InsuranceName, name: "IX_InsuranceInfo_Name")
                .Index(t => t.InsuranceType, name: "IX_InsuranceInfo_Type")
                .Index(t => new { t.IsActive, t.IsDeleted, t.InsuranceType }, name: "IX_InsuranceInfo_Active_Deleted_Type")
                .Index(t => t.IsActive, name: "IX_InsuranceInfo_IsActive")
                .Index(t => new { t.IsFeatured, t.IsActive, t.DisplayOrder }, name: "IX_InsuranceInfo_Featured_Active_Order")
                .Index(t => t.IsFeatured, name: "IX_InsuranceInfo_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_InsuranceInfo_DisplayOrder")
                .Index(t => t.ViewCount, name: "IX_InsuranceInfo_ViewCount")
                .Index(t => t.Slug, unique: true, name: "IX_InsuranceInfo_Slug")
                .Index(t => t.IsDeleted, name: "IX_InsuranceInfo_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.MedicalEquipments",
                c => new
                    {
                        MedicalEquipmentId = c.Int(nullable: false, identity: true),
                        EquipmentName = c.String(nullable: false, maxLength: 200),
                        Model = c.String(maxLength: 100),
                        Manufacturer = c.String(maxLength: 200),
                        Category = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 2000),
                        TechnicalSpecifications = c.String(),
                        ImageUrl = c.String(maxLength: 500),
                        ImageUrls = c.String(maxLength: 2000),
                        VideoUrl = c.String(maxLength: 500),
                        PurchaseDate = c.DateTime(),
                        InstallationDate = c.DateTime(),
                        WarrantyExpiryDate = c.DateTime(),
                        Status = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Features = c.String(maxLength: 2000),
                        ShortDescription = c.String(maxLength: 500),
                        Slug = c.String(maxLength: 200),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        ViewCount = c.Int(nullable: false),
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
                    { "DynamicFilter_MedicalEquipment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.MedicalEquipmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.EquipmentName, name: "IX_MedicalEquipment_EquipmentName")
                .Index(t => t.Category, name: "IX_MedicalEquipment_Category")
                .Index(t => new { t.IsActive, t.IsDeleted, t.Category }, name: "IX_MedicalEquipment_Active_Deleted_Category")
                .Index(t => t.IsActive, name: "IX_MedicalEquipment_IsActive")
                .Index(t => new { t.IsFeatured, t.IsActive, t.DisplayOrder }, name: "IX_MedicalEquipment_Featured_Active_Order")
                .Index(t => t.IsFeatured, name: "IX_MedicalEquipment_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_MedicalEquipment_DisplayOrder")
                .Index(t => t.Slug, unique: true, name: "IX_MedicalEquipment_Slug")
                .Index(t => t.IsDeleted, name: "IX_MedicalEquipment_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.MedicalServiceInfos",
                c => new
                    {
                        MedicalServiceInfoId = c.Int(nullable: false, identity: true),
                        ServiceId = c.Int(nullable: false),
                        Description = c.String(maxLength: 500),
                        FullDescription = c.String(storeType: "ntext"),
                        Features = c.String(maxLength: 2000),
                        ImageUrl = c.String(maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        VideoUrl = c.String(maxLength: 500),
                        Price = c.Decimal(precision: 18, scale: 2),
                        InsuranceCoverage = c.String(maxLength: 2000),
                        EstimatedDuration = c.String(maxLength: 500),
                        RequiredDocuments = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
                        RelatedLinkUrl = c.String(maxLength: 500),
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
                    { "DynamicFilter_MedicalServiceInfo_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.MedicalServiceInfoId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ServiceId, name: "IX_MedicalServiceInfo_ServiceId")
                .Index(t => new { t.IsActive, t.IsDeleted, t.ServiceId }, name: "IX_MedicalServiceInfo_Active_Deleted_ServiceId")
                .Index(t => t.IsActive, name: "IX_MedicalServiceInfo_IsActive")
                .Index(t => new { t.IsFeatured, t.IsActive, t.DisplayOrder }, name: "IX_MedicalServiceInfo_Featured_Active_Order")
                .Index(t => t.IsFeatured, name: "IX_MedicalServiceInfo_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_MedicalServiceInfo_DisplayOrder")
                .Index(t => t.ViewCount, name: "IX_MedicalServiceInfo_ViewCount")
                .Index(t => t.Slug, unique: true, name: "IX_MedicalServiceInfo_Slug")
                .Index(t => t.IsDeleted, name: "IX_MedicalServiceInfo_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.Sliders",
                c => new
                    {
                        SliderId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        ImageUrl = c.String(nullable: false, maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        LinkUrl = c.String(maxLength: 500),
                        ButtonText = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Position = c.String(maxLength: 50),
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
                    { "DynamicFilter_Slider_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.SliderId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_Slider_Title")
                .Index(t => t.IsActive, name: "IX_Slider_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder }, name: "IX_Slider_Active_Deleted_Order")
                .Index(t => t.DisplayOrder, name: "IX_Slider_DisplayOrder")
                .Index(t => t.Position, name: "IX_Slider_Position")
                .Index(t => t.IsDeleted, name: "IX_Slider_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.Testimonials",
                c => new
                    {
                        TestimonialId = c.Int(nullable: false, identity: true),
                        PatientName = c.String(nullable: false, maxLength: 200),
                        PatientInitials = c.String(maxLength: 10),
                        Comment = c.String(nullable: false, maxLength: 2000),
                        Rating = c.Decimal(nullable: false, precision: 3, scale: 2),
                        DoctorName = c.String(maxLength: 200),
                        PhotoUrl = c.String(maxLength: 500),
                        VideoUrl = c.String(maxLength: 500),
                        IsApproved = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        ApprovedAt = c.DateTime(),
                        PatientId = c.Int(),
                        DoctorId = c.Int(),
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
                    { "DynamicFilter_Testimonial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TestimonialId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.PatientName, name: "IX_Testimonial_PatientName")
                .Index(t => t.Rating, name: "IX_Testimonial_Rating")
                .Index(t => t.IsApproved, name: "IX_Testimonial_IsApproved")
                .Index(t => new { t.IsApproved, t.IsFeatured, t.IsDeleted, t.DisplayOrder }, name: "IX_Testimonial_Approved_Featured_Deleted_Order")
                .Index(t => t.IsFeatured, name: "IX_Testimonial_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_Testimonial_DisplayOrder")
                .Index(t => t.IsDeleted, name: "IX_Testimonial_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Testimonials", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Testimonials", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Testimonials", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sliders", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sliders", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sliders", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalServiceInfos", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalServiceInfos", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.MedicalServiceInfos", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalServiceInfos", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalEquipments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalEquipments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalEquipments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceInfos", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceInfos", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceInfos", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.HealthTips", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.HealthTips", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.HealthTips", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryItems", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryItems", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.GalleryItems", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FAQs", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FAQs", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FAQs", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EmergencyContacts", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EmergencyContacts", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EmergencyContacts", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicWorkingHours", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicWorkingHours", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ClinicWorkingHours", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPosts", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPosts", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPosts", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Announcements", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Announcements", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Announcements", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Testimonials", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Testimonials", new[] { "CreatedByUserId" });
            DropIndex("dbo.Testimonials", new[] { "DeletedByUserId" });
            DropIndex("dbo.Testimonials", "IX_Testimonial_IsDeleted");
            DropIndex("dbo.Testimonials", "IX_Testimonial_DisplayOrder");
            DropIndex("dbo.Testimonials", "IX_Testimonial_IsFeatured");
            DropIndex("dbo.Testimonials", "IX_Testimonial_Approved_Featured_Deleted_Order");
            DropIndex("dbo.Testimonials", "IX_Testimonial_IsApproved");
            DropIndex("dbo.Testimonials", "IX_Testimonial_Rating");
            DropIndex("dbo.Testimonials", "IX_Testimonial_PatientName");
            DropIndex("dbo.Sliders", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Sliders", new[] { "CreatedByUserId" });
            DropIndex("dbo.Sliders", new[] { "DeletedByUserId" });
            DropIndex("dbo.Sliders", "IX_Slider_IsDeleted");
            DropIndex("dbo.Sliders", "IX_Slider_Position");
            DropIndex("dbo.Sliders", "IX_Slider_DisplayOrder");
            DropIndex("dbo.Sliders", "IX_Slider_Active_Deleted_Order");
            DropIndex("dbo.Sliders", "IX_Slider_IsActive");
            DropIndex("dbo.Sliders", "IX_Slider_Title");
            DropIndex("dbo.MedicalServiceInfos", new[] { "UpdatedByUserId" });
            DropIndex("dbo.MedicalServiceInfos", new[] { "CreatedByUserId" });
            DropIndex("dbo.MedicalServiceInfos", new[] { "DeletedByUserId" });
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_IsDeleted");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_Slug");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_ViewCount");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_DisplayOrder");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_IsFeatured");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_Featured_Active_Order");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_IsActive");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_Active_Deleted_ServiceId");
            DropIndex("dbo.MedicalServiceInfos", "IX_MedicalServiceInfo_ServiceId");
            DropIndex("dbo.MedicalEquipments", new[] { "UpdatedByUserId" });
            DropIndex("dbo.MedicalEquipments", new[] { "CreatedByUserId" });
            DropIndex("dbo.MedicalEquipments", new[] { "DeletedByUserId" });
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_IsDeleted");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_Slug");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_DisplayOrder");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_IsFeatured");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_Featured_Active_Order");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_IsActive");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_Active_Deleted_Category");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_Category");
            DropIndex("dbo.MedicalEquipments", "IX_MedicalEquipment_EquipmentName");
            DropIndex("dbo.InsuranceInfos", new[] { "UpdatedByUserId" });
            DropIndex("dbo.InsuranceInfos", new[] { "CreatedByUserId" });
            DropIndex("dbo.InsuranceInfos", new[] { "DeletedByUserId" });
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_IsDeleted");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_Slug");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_ViewCount");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_DisplayOrder");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_IsFeatured");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_Featured_Active_Order");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_IsActive");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_Active_Deleted_Type");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_Type");
            DropIndex("dbo.InsuranceInfos", "IX_InsuranceInfo_Name");
            DropIndex("dbo.HealthTips", new[] { "UpdatedByUserId" });
            DropIndex("dbo.HealthTips", new[] { "CreatedByUserId" });
            DropIndex("dbo.HealthTips", new[] { "DeletedByUserId" });
            DropIndex("dbo.HealthTips", "IX_HealthTip_IsDeleted");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Slug");
            DropIndex("dbo.HealthTips", "IX_HealthTip_DisplayOrder");
            DropIndex("dbo.HealthTips", "IX_HealthTip_ViewCount");
            DropIndex("dbo.HealthTips", "IX_HealthTip_IsFeatured");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Featured_Published_Order");
            DropIndex("dbo.HealthTips", "IX_HealthTip_IsPublished");
            DropIndex("dbo.HealthTips", "IX_HealthTip_ExpiryDate");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Published_Deleted_Dates");
            DropIndex("dbo.HealthTips", "IX_HealthTip_PublishedAt");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Category_Published_Deleted");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Category");
            DropIndex("dbo.HealthTips", "IX_HealthTip_Title");
            DropIndex("dbo.GalleryItems", new[] { "UpdatedByUserId" });
            DropIndex("dbo.GalleryItems", new[] { "CreatedByUserId" });
            DropIndex("dbo.GalleryItems", new[] { "DeletedByUserId" });
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_IsDeleted");
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_DisplayOrder");
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_IsActive");
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_Active_Deleted_Order_Category");
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_Category");
            DropIndex("dbo.GalleryItems", "IX_GalleryItem_Title");
            DropIndex("dbo.FAQs", new[] { "UpdatedByUserId" });
            DropIndex("dbo.FAQs", new[] { "CreatedByUserId" });
            DropIndex("dbo.FAQs", new[] { "DeletedByUserId" });
            DropIndex("dbo.FAQs", "IX_FAQ_IsDeleted");
            DropIndex("dbo.FAQs", "IX_FAQ_Slug");
            DropIndex("dbo.FAQs", "IX_FAQ_DisplayOrder");
            DropIndex("dbo.FAQs", "IX_FAQ_IsFeatured");
            DropIndex("dbo.FAQs", "IX_FAQ_Featured_Active_Order");
            DropIndex("dbo.FAQs", "IX_FAQ_IsActive");
            DropIndex("dbo.FAQs", "IX_FAQ_ViewCount");
            DropIndex("dbo.FAQs", "IX_FAQ_Active_Deleted_Category");
            DropIndex("dbo.FAQs", "IX_FAQ_Category");
            DropIndex("dbo.FAQs", "IX_FAQ_Question");
            DropIndex("dbo.EmergencyContacts", new[] { "UpdatedByUserId" });
            DropIndex("dbo.EmergencyContacts", new[] { "CreatedByUserId" });
            DropIndex("dbo.EmergencyContacts", new[] { "DeletedByUserId" });
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_IsDeleted");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_Slug");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_DisplayOrder");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_IsAlwaysVisible");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_AlwaysVisible_Active_Order");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_IsActive");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_Active_Deleted_Type");
            DropIndex("dbo.EmergencyContacts", "IX_EmergencyContact_ContactType");
            DropIndex("dbo.ClinicWorkingHours", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ClinicWorkingHours", new[] { "CreatedByUserId" });
            DropIndex("dbo.ClinicWorkingHours", new[] { "DeletedByUserId" });
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_IsDeleted");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_Active_Deleted_Order");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_IsActive");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_IsOpen");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_DayOfWeek");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_ClinicId_DayOfWeek_Active_Deleted");
            DropIndex("dbo.ClinicWorkingHours", "IX_ClinicWorkingHours_ClinicId");
            DropIndex("dbo.BlogPosts", new[] { "UpdatedByUserId" });
            DropIndex("dbo.BlogPosts", new[] { "CreatedByUserId" });
            DropIndex("dbo.BlogPosts", new[] { "DeletedByUserId" });
            DropIndex("dbo.BlogPosts", "IX_BlogPost_IsDeleted");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_Slug");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_IsFeatured");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_IsPublished");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_Published_Deleted_Date");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_PublishedAt");
            DropIndex("dbo.BlogPosts", "IX_BlogPost_Title");
            DropIndex("dbo.Announcements", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Announcements", new[] { "CreatedByUserId" });
            DropIndex("dbo.Announcements", new[] { "DeletedByUserId" });
            DropIndex("dbo.Announcements", "IX_Announcement_IsDeleted");
            DropIndex("dbo.Announcements", "IX_Announcement_Type");
            DropIndex("dbo.Announcements", "IX_Announcement_DisplayOrder");
            DropIndex("dbo.Announcements", "IX_Announcement_IsImportant");
            DropIndex("dbo.Announcements", "IX_Announcement_Active_Deleted_Order_Date");
            DropIndex("dbo.Announcements", "IX_Announcement_IsActive");
            DropIndex("dbo.Announcements", "IX_Announcement_Title");
            DropTable("dbo.Testimonials",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Testimonial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Sliders",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Slider_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.MedicalServiceInfos",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_MedicalServiceInfo_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.MedicalEquipments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_MedicalEquipment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.InsuranceInfos",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_InsuranceInfo_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.HealthTips",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_HealthTip_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.GalleryItems",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_GalleryItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.FAQs",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_FAQ_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.EmergencyContacts",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_EmergencyContact_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ClinicWorkingHours",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ClinicWorkingHours_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.BlogPosts",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_BlogPost_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Announcements",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Announcement_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
