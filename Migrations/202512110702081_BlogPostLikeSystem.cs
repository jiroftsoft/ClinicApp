namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class BlogPostLikeSystem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogPostComments",
                c => new
                    {
                        BlogPostCommentId = c.Int(nullable: false, identity: true),
                        BlogPostId = c.Int(nullable: false),
                        CommentText = c.String(nullable: false, maxLength: 2000),
                        AuthorName = c.String(maxLength: 200),
                        AuthorEmail = c.String(maxLength: 500),
                        AuthorPhone = c.String(maxLength: 50),
                        AuthorUserId = c.String(maxLength: 128),
                        IsApproved = c.Boolean(nullable: false),
                        IsSpam = c.Boolean(nullable: false),
                        IsReported = c.Boolean(nullable: false),
                        ParentCommentId = c.Int(),
                        IpAddress = c.String(maxLength: 50),
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
                    { "DynamicFilter_BlogPostComment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.BlogPostCommentId)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorUserId)
                .ForeignKey("dbo.BlogPosts", t => t.BlogPostId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.BlogPostComments", t => t.ParentCommentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.BlogPostId, name: "IX_BlogPostComment_BlogPostId")
                .Index(t => new { t.BlogPostId, t.IsApproved, t.IsDeleted, t.CreatedAt }, name: "IX_BlogPostComment_BlogPost_Approved_Deleted_Date")
                .Index(t => t.AuthorUserId)
                .Index(t => t.IsApproved, name: "IX_BlogPostComment_IsApproved")
                .Index(t => t.ParentCommentId, name: "IX_BlogPostComment_ParentCommentId")
                .Index(t => t.IsDeleted, name: "IX_BlogPostComment_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_BlogPostComment_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.BlogPostLikes",
                c => new
                    {
                        BlogPostLikeId = c.Int(nullable: false, identity: true),
                        BlogPostId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        GuestIdentifier = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 50),
                        UserAgent = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.BlogPostLikeId)
                .ForeignKey("dbo.BlogPosts", t => t.BlogPostId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.BlogPostId, name: "IX_BlogPostLike_BlogPostId")
                .Index(t => new { t.BlogPostId, t.UserId }, name: "IX_BlogPostLike_BlogPost_User")
                .Index(t => new { t.BlogPostId, t.GuestIdentifier }, name: "IX_BlogPostLike_BlogPost_Guest")
                .Index(t => t.UserId, name: "IX_BlogPostLike_UserId")
                .Index(t => t.GuestIdentifier, name: "IX_BlogPostLike_GuestIdentifier")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BlogPostLikes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostLikes", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostLikes", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostLikes", "BlogPostId", "dbo.BlogPosts");
            DropForeignKey("dbo.BlogPostComments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostComments", "ParentCommentId", "dbo.BlogPostComments");
            DropForeignKey("dbo.BlogPostComments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostComments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.BlogPostComments", "BlogPostId", "dbo.BlogPosts");
            DropForeignKey("dbo.BlogPostComments", "AuthorUserId", "dbo.AspNetUsers");
            DropIndex("dbo.BlogPostLikes", new[] { "UpdatedByUserId" });
            DropIndex("dbo.BlogPostLikes", new[] { "CreatedByUserId" });
            DropIndex("dbo.BlogPostLikes", "IX_BlogPostLike_GuestIdentifier");
            DropIndex("dbo.BlogPostLikes", "IX_BlogPostLike_UserId");
            DropIndex("dbo.BlogPostLikes", "IX_BlogPostLike_BlogPost_Guest");
            DropIndex("dbo.BlogPostLikes", "IX_BlogPostLike_BlogPost_User");
            DropIndex("dbo.BlogPostLikes", "IX_BlogPostLike_BlogPostId");
            DropIndex("dbo.BlogPostComments", new[] { "UpdatedByUserId" });
            DropIndex("dbo.BlogPostComments", new[] { "CreatedByUserId" });
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_CreatedAt");
            DropIndex("dbo.BlogPostComments", new[] { "DeletedByUserId" });
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_IsDeleted");
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_ParentCommentId");
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_IsApproved");
            DropIndex("dbo.BlogPostComments", new[] { "AuthorUserId" });
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_BlogPost_Approved_Deleted_Date");
            DropIndex("dbo.BlogPostComments", "IX_BlogPostComment_BlogPostId");
            DropTable("dbo.BlogPostLikes");
            DropTable("dbo.BlogPostComments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_BlogPostComment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
