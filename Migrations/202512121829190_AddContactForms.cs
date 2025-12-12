namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddContactForms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContactForms",
                c => new
                    {
                        ContactFormId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 200),
                        Email = c.String(nullable: false, maxLength: 200),
                        PhoneNumber = c.String(nullable: false, maxLength: 50),
                        Subject = c.String(nullable: false, maxLength: 500),
                        Message = c.String(nullable: false),
                        Category = c.Byte(nullable: false),
                        Status = c.Byte(nullable: false),
                        ReplyMessage = c.String(),
                        RepliedAt = c.DateTime(),
                        RepliedByUserId = c.String(maxLength: 128),
                        IsRead = c.Boolean(nullable: false),
                        ReadAt = c.DateTime(),
                        ReadByUserId = c.String(maxLength: 128),
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
                    { "DynamicFilter_ContactForm_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ContactFormId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.ReadByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.RepliedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.FullName, name: "IX_ContactForm_FullName")
                .Index(t => t.Email, name: "IX_ContactForm_Email")
                .Index(t => t.PhoneNumber, name: "IX_ContactForm_PhoneNumber")
                .Index(t => t.Subject, name: "IX_ContactForm_Subject")
                .Index(t => t.Category, name: "IX_ContactForm_Category")
                .Index(t => new { t.Category, t.Status, t.IsDeleted }, name: "IX_ContactForm_Category_Status_Deleted")
                .Index(t => t.Status, name: "IX_ContactForm_Status")
                .Index(t => new { t.Status, t.IsDeleted, t.CreatedAt }, name: "IX_ContactForm_Status_Deleted_CreatedAt")
                .Index(t => new { t.IsRead, t.Status, t.IsDeleted }, name: "IX_ContactForm_IsRead_Status_Deleted")
                .Index(t => t.RepliedByUserId)
                .Index(t => t.IsRead, name: "IX_ContactForm_IsRead")
                .Index(t => t.ReadByUserId)
                .Index(t => t.IsDeleted, name: "IX_ContactForm_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_ContactForm_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ContactForms", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContactForms", "RepliedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContactForms", "ReadByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContactForms", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContactForms", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ContactForms", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ContactForms", new[] { "CreatedByUserId" });
            DropIndex("dbo.ContactForms", "IX_ContactForm_CreatedAt");
            DropIndex("dbo.ContactForms", new[] { "DeletedByUserId" });
            DropIndex("dbo.ContactForms", "IX_ContactForm_IsDeleted");
            DropIndex("dbo.ContactForms", new[] { "ReadByUserId" });
            DropIndex("dbo.ContactForms", "IX_ContactForm_IsRead");
            DropIndex("dbo.ContactForms", new[] { "RepliedByUserId" });
            DropIndex("dbo.ContactForms", "IX_ContactForm_IsRead_Status_Deleted");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Status_Deleted_CreatedAt");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Status");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Category_Status_Deleted");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Category");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Subject");
            DropIndex("dbo.ContactForms", "IX_ContactForm_PhoneNumber");
            DropIndex("dbo.ContactForms", "IX_ContactForm_Email");
            DropIndex("dbo.ContactForms", "IX_ContactForm_FullName");
            DropTable("dbo.ContactForms",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ContactForm_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
