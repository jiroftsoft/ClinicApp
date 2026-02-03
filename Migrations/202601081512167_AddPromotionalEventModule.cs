namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddPromotionalEventModule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PromotionalEvents",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        DiscountType = c.Byte(nullable: false),
                        DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 0),
                        TotalSlots = c.Int(),
                        UsedSlots = c.Int(nullable: false),
                        IsDoctorSpecific = c.Boolean(nullable: false),
                        DoctorIds = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(nullable: false, maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PromotionalEvent_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_PromotionalEvent_Title")
                .Index(t => t.StartDate, name: "IX_PromotionalEvent_StartDate")
                .Index(t => new { t.StartDate, t.EndDate, t.IsActive }, name: "IX_PromotionalEvent_StartDate_EndDate_IsActive")
                .Index(t => t.EndDate, name: "IX_PromotionalEvent_EndDate")
                .Index(t => t.IsActive, name: "IX_PromotionalEvent_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted }, name: "IX_PromotionalEvent_IsActive_IsDeleted")
                .Index(t => t.IsDeleted, name: "IX_PromotionalEvent_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_PromotionalEvent_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_PromotionalEvent_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_PromotionalEvent_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_PromotionalEvent_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PromotionalEvent_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_PromotionalEvent_UpdatedByUserId");
            
            AddColumn("dbo.Appointments", "PromotionalEventId", c => c.Int());
            AddColumn("dbo.Appointments", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.Appointments", "PromotionalEventId", name: "IX_Appointment_PromotionalEventId");
            AddForeignKey("dbo.Appointments", "PromotionalEventId", "dbo.PromotionalEvents", "EventId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PromotionalEvents", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PromotionalEvents", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PromotionalEvents", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Appointments", "PromotionalEventId", "dbo.PromotionalEvents");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_UpdatedByUserId");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_UpdatedAt");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_CreatedByUserId");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_CreatedAt");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_DeletedByUserId");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_DeletedAt");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_IsDeleted");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_IsActive_IsDeleted");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_IsActive");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_EndDate");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_StartDate_EndDate_IsActive");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_StartDate");
            DropIndex("dbo.PromotionalEvents", "IX_PromotionalEvent_Title");
            DropIndex("dbo.Appointments", "IX_Appointment_PromotionalEventId");
            DropColumn("dbo.Appointments", "DiscountAmount");
            DropColumn("dbo.Appointments", "PromotionalEventId");
            DropTable("dbo.PromotionalEvents",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PromotionalEvent_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
