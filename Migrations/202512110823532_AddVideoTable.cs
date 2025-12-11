namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddVideoTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        VideoId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 2000),
                        VideoUrl = c.String(nullable: false, maxLength: 1000),
                        VideoType = c.Int(nullable: false),
                        ThumbnailUrl = c.String(maxLength: 500),
                        Category = c.String(maxLength: 100),
                        Duration = c.Int(),
                        ViewCount = c.Int(nullable: false),
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
                    { "DynamicFilter_Video_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.VideoId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_Video_Title")
                .Index(t => t.VideoType, name: "IX_Video_VideoType")
                .Index(t => t.Category, name: "IX_Video_Category")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder, t.Category }, name: "IX_Video_Active_Deleted_Order_Category")
                .Index(t => t.ViewCount, name: "IX_Video_ViewCount")
                .Index(t => t.IsActive, name: "IX_Video_IsActive")
                .Index(t => t.DisplayOrder, name: "IX_Video_DisplayOrder")
                .Index(t => t.IsDeleted, name: "IX_Video_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Videos", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Videos", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Videos", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Videos", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Videos", new[] { "CreatedByUserId" });
            DropIndex("dbo.Videos", new[] { "DeletedByUserId" });
            DropIndex("dbo.Videos", "IX_Video_IsDeleted");
            DropIndex("dbo.Videos", "IX_Video_DisplayOrder");
            DropIndex("dbo.Videos", "IX_Video_IsActive");
            DropIndex("dbo.Videos", "IX_Video_ViewCount");
            DropIndex("dbo.Videos", "IX_Video_Active_Deleted_Order_Category");
            DropIndex("dbo.Videos", "IX_Video_Category");
            DropIndex("dbo.Videos", "IX_Video_VideoType");
            DropIndex("dbo.Videos", "IX_Video_Title");
            DropTable("dbo.Videos",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Video_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
