namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class addPatientEducationMaterial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PatientEducationMaterials",
                c => new
                    {
                        PatientEducationMaterialId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 300),
                        Description = c.String(nullable: false, maxLength: 1000),
                        Content = c.String(nullable: false, storeType: "ntext"),
                        FileUrl = c.String(maxLength: 500),
                        FileName = c.String(maxLength: 100),
                        FileType = c.String(maxLength: 50),
                        FileSizeInBytes = c.Long(),
                        VideoUrl = c.String(maxLength: 500),
                        ImageUrl = c.String(maxLength: 500),
                        ThumbnailUrl = c.String(maxLength: 500),
                        Category = c.Byte(nullable: false),
                        Tags = c.String(maxLength: 500),
                        PublishedAt = c.DateTime(),
                        IsPublished = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        DownloadCount = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
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
                    { "DynamicFilter_PatientEducationMaterial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PatientEducationMaterialId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_PatientEducationMaterial_Title")
                .Index(t => t.Category, name: "IX_PatientEducationMaterial_Category")
                .Index(t => new { t.Category, t.IsPublished, t.IsDeleted }, name: "IX_PatientEducationMaterial_Category_Published_Deleted")
                .Index(t => t.IsPublished, name: "IX_PatientEducationMaterial_IsPublished")
                .Index(t => new { t.IsPublished, t.IsDeleted, t.CreatedAt }, name: "IX_PatientEducationMaterial_Published_Deleted_CreatedAt")
                .Index(t => new { t.IsFeatured, t.IsPublished, t.DisplayOrder }, name: "IX_PatientEducationMaterial_Featured_Published_Order")
                .Index(t => t.IsFeatured, name: "IX_PatientEducationMaterial_IsFeatured")
                .Index(t => t.DisplayOrder, name: "IX_PatientEducationMaterial_DisplayOrder")
                .Index(t => t.Slug, unique: true, name: "IX_PatientEducationMaterial_Slug")
                .Index(t => t.IsDeleted, name: "IX_PatientEducationMaterial_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_PatientEducationMaterial_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PatientEducationMaterials", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PatientEducationMaterials", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PatientEducationMaterials", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.PatientEducationMaterials", new[] { "UpdatedByUserId" });
            DropIndex("dbo.PatientEducationMaterials", new[] { "CreatedByUserId" });
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_CreatedAt");
            DropIndex("dbo.PatientEducationMaterials", new[] { "DeletedByUserId" });
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_IsDeleted");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Slug");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_DisplayOrder");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_IsFeatured");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Featured_Published_Order");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Published_Deleted_CreatedAt");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_IsPublished");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Category_Published_Deleted");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Category");
            DropIndex("dbo.PatientEducationMaterials", "IX_PatientEducationMaterial_Title");
            DropTable("dbo.PatientEducationMaterials",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PatientEducationMaterial_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
