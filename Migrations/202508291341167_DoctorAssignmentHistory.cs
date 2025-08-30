namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class DoctorAssignmentHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DoctorAssignmentHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        ActionType = c.String(nullable: false, maxLength: 50),
                        ActionTitle = c.String(nullable: false, maxLength: 200),
                        ActionDescription = c.String(maxLength: 1000),
                        ActionDate = c.DateTime(nullable: false),
                        DepartmentId = c.Int(),
                        DepartmentName = c.String(maxLength: 200),
                        ServiceCategories = c.String(maxLength: 2000),
                        PerformedByUserId = c.String(nullable: false, maxLength: 128),
                        PerformedByUserName = c.String(maxLength: 200),
                        Notes = c.String(maxLength: 1000),
                        PreviousData = c.String(maxLength: 4000),
                        NewData = c.String(maxLength: 4000),
                        Importance = c.Int(nullable: false),
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
                    { "DynamicFilter_DoctorAssignmentHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.PerformedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.DoctorId, t.ActionDate }, name: "IX_DoctorAssignmentHistory_DoctorId_ActionDate")
                .Index(t => t.ActionType, name: "IX_DoctorAssignmentHistory_ActionType")
                .Index(t => new { t.ActionType, t.ActionDate }, name: "IX_DoctorAssignmentHistory_ActionType_ActionDate")
                .Index(t => t.ActionDate, name: "IX_DoctorAssignmentHistory_ActionDate")
                .Index(t => new { t.DepartmentId, t.ActionDate }, name: "IX_DoctorAssignmentHistory_DepartmentId_ActionDate")
                .Index(t => new { t.PerformedByUserId, t.ActionDate }, name: "IX_DoctorAssignmentHistory_PerformedByUserId_ActionDate")
                .Index(t => new { t.Importance, t.ActionDate }, name: "IX_DoctorAssignmentHistory_Importance_ActionDate")
                .Index(t => new { t.IsDeleted, t.ActionDate }, name: "IX_DoctorAssignmentHistory_IsDeleted_ActionDate")
                .Index(t => t.PerformedByUserId, name: "IX_DoctorAssignmentHistory_PerformedByUserId")
                .Index(t => t.Importance, name: "IX_DoctorAssignmentHistory_Importance")
                .Index(t => t.IsDeleted, name: "IX_DoctorAssignmentHistory_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorAssignmentHistory_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorAssignmentHistory_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorAssignmentHistory_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorAssignmentHistory_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorAssignmentHistory_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorAssignmentHistory_UpdatedByUserId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DoctorAssignmentHistories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorAssignmentHistories", "PerformedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorAssignmentHistories", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorAssignmentHistories", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.DoctorAssignmentHistories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorAssignmentHistories", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_UpdatedByUserId");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_UpdatedAt");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_CreatedByUserId");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_CreatedAt");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_DeletedByUserId");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_DeletedAt");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_IsDeleted");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_Importance");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_PerformedByUserId");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_IsDeleted_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_Importance_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_PerformedByUserId_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_DepartmentId_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_ActionType_ActionDate");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_ActionType");
            DropIndex("dbo.DoctorAssignmentHistories", "IX_DoctorAssignmentHistory_DoctorId_ActionDate");
            DropTable("dbo.DoctorAssignmentHistories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorAssignmentHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
