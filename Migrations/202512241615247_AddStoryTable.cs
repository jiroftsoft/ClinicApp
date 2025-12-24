namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddStoryTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Stories",
                c => new
                    {
                        StoryId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        VideoUrl = c.String(maxLength: 1000),
                        VideoType = c.String(maxLength: 50),
                        ThumbnailUrl = c.String(nullable: false, maxLength: 500),
                        LinkUrl = c.String(maxLength: 500),
                        ButtonText = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        ViewCount = c.Int(nullable: false),
                        Duration = c.Int(),
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
                    { "DynamicFilter_Story_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.StoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_Story_Title")
                .Index(t => t.VideoType, name: "IX_Story_VideoType")
                .Index(t => t.IsActive, name: "IX_Story_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted, t.DisplayOrder, t.StartDate, t.EndDate }, name: "IX_Story_Active_Deleted_Order_Dates")
                .Index(t => t.DisplayOrder, name: "IX_Story_DisplayOrder")
                .Index(t => t.ViewCount, name: "IX_Story_ViewCount")
                .Index(t => t.IsDeleted, name: "IX_Story_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Stories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Stories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Stories", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Stories", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Stories", new[] { "CreatedByUserId" });
            DropIndex("dbo.Stories", new[] { "DeletedByUserId" });
            DropIndex("dbo.Stories", "IX_Story_IsDeleted");
            DropIndex("dbo.Stories", "IX_Story_ViewCount");
            DropIndex("dbo.Stories", "IX_Story_DisplayOrder");
            DropIndex("dbo.Stories", "IX_Story_Active_Deleted_Order_Dates");
            DropIndex("dbo.Stories", "IX_Story_IsActive");
            DropIndex("dbo.Stories", "IX_Story_VideoType");
            DropIndex("dbo.Stories", "IX_Story_Title");
            DropTable("dbo.Stories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Story_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
