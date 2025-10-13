namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class FixTriageUserRelations : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.TriageProtocolTriageAssessment", newName: "TriageAssessmentProtocols");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_Patient_Time_IsDeleted");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_AssessorId");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_Level_Status_IsDeleted");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_Time_Level_IsDeleted");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_AssessmentTime");
            DropIndex("dbo.TriageProtocols", "IX_TriageProtocol_Type_Level_Active_IsDeleted");
            DropIndex("dbo.TriageProtocols", "IX_TriageProtocol_Validity_Active_IsDeleted");
            DropIndex("dbo.TriageProtocols", "IX_TriageProtocol_Priority_Mandatory_Active_IsDeleted");
            DropIndex("dbo.TriageQueues", "IX_TriageQueue_Patient_Status_IsDeleted");
            DropIndex("dbo.TriageQueues", "IX_TriageQueue_Status_Priority_IsDeleted");
            DropIndex("dbo.TriageQueues", "IX_TriageQueue_Time_Status_IsDeleted");
            DropIndex("dbo.TriageQueues", "IX_TriageQueue_Immediate_Status_IsDeleted");
            DropIndex("dbo.TriageVitalSigns", "IX_TriageVitalSigns_Assessment_Time_IsDeleted");
            DropIndex("dbo.TriageVitalSigns", "IX_TriageVitalSigns_Normal_Immediate_IsDeleted");
            RenameColumn(table: "dbo.TriageAssessmentProtocols", name: "TriageProtocol_TriageProtocolId", newName: "TriageProtocolId");
            RenameColumn(table: "dbo.TriageAssessmentProtocols", name: "TriageAssessment_TriageAssessmentId", newName: "TriageAssessmentId");
            RenameColumn(table: "dbo.TriageAssessments", name: "AssessorId", newName: "AssessorUserId");
            RenameIndex(table: "dbo.TriageAssessmentProtocols", name: "IX_TriageAssessment_TriageAssessmentId", newName: "IX_TriageAssessmentId");
            RenameIndex(table: "dbo.TriageAssessmentProtocols", name: "IX_TriageProtocol_TriageProtocolId", newName: "IX_TriageProtocolId");
            DropPrimaryKey("dbo.TriageAssessmentProtocols");
            CreateTable(
                "dbo.TriageReassessments",
                c => new
                    {
                        TriageReassessmentId = c.Int(nullable: false, identity: true),
                        TriageAssessmentId = c.Int(nullable: false),
                        ReassessmentAt = c.DateTime(nullable: false),
                        NewLevel = c.Int(),
                        PreviousLevel = c.Int(),
                        Changes = c.String(nullable: false, maxLength: 1000),
                        Actions = c.String(maxLength: 500),
                        Reason = c.Int(nullable: false),
                        AssessorUserId = c.String(nullable: false, maxLength: 128),
                        Notes = c.String(maxLength: 500),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_TriageReassessment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TriageReassessmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.AssessorUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.TriageAssessments", t => t.TriageAssessmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.TriageAssessmentId, name: "IX_TriageReassessment_TriageAssessmentId")
                .Index(t => t.ReassessmentAt, name: "IX_TriageReassessment_ReassessmentAt")
                .Index(t => t.NewLevel, name: "IX_TriageReassessment_NewLevel")
                .Index(t => t.Reason, name: "IX_TriageReassessment_Reason")
                .Index(t => t.AssessorUserId, name: "IX_TriageReassessment_AssessorUserId")
                .Index(t => t.IsDeleted, name: "IX_TriageReassessment_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedAt, name: "IX_TriageReassessment_CreatedAt")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            AddColumn("dbo.TriageAssessments", "EsiScore", c => c.Int());
            AddColumn("dbo.TriageAssessments", "News2Score", c => c.Int());
            AddColumn("dbo.TriageAssessments", "PewsScore", c => c.Int());
            AddColumn("dbo.TriageAssessments", "ChiefComplaintCode", c => c.String(maxLength: 20));
            AddColumn("dbo.TriageAssessments", "ArrivalAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.TriageAssessments", "TriageStartAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.TriageAssessments", "TriageEndAt", c => c.DateTime());
            AddColumn("dbo.TriageAssessments", "FirstPhysicianContactAt", c => c.DateTime());
            AddColumn("dbo.TriageAssessments", "IsOpen", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "RedFlag_Sepsis", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "RedFlag_Stroke", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "RedFlag_ACS", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "RedFlag_Trauma", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "IsPregnant", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageAssessments", "Isolation", c => c.Int());
            AddColumn("dbo.TriageAssessments", "EstimatedWaitTimeMinutes", c => c.Int());
            AddColumn("dbo.TriageAssessments", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.TriageProtocols", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.TriageQueues", "NextReassessmentDueAt", c => c.DateTime());
            AddColumn("dbo.TriageQueues", "ReassessmentCount", c => c.Int(nullable: false));
            AddColumn("dbo.TriageQueues", "LastReassessmentAt", c => c.DateTime());
            AddColumn("dbo.TriageQueues", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.TriageVitalSigns", "GcsE", c => c.Int());
            AddColumn("dbo.TriageVitalSigns", "GcsV", c => c.Int());
            AddColumn("dbo.TriageVitalSigns", "GcsM", c => c.Int());
            AddColumn("dbo.TriageVitalSigns", "OnOxygen", c => c.Boolean(nullable: false));
            AddColumn("dbo.TriageVitalSigns", "OxygenDevice", c => c.Int());
            AddColumn("dbo.TriageVitalSigns", "O2FlowLpm", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.TriageVitalSigns", "ReassessmentReason", c => c.Int());
            AddColumn("dbo.TriageVitalSigns", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AlterColumn("dbo.TriageAssessments", "AssessorUserId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.TriageAssessmentProtocols", new[] { "TriageAssessmentId", "TriageProtocolId" });
            CreateIndex("dbo.TriageAssessments", "AssessorUserId", name: "IX_TriageAssessment_AssessorUserId");
            CreateIndex("dbo.TriageAssessments", "TriageStartAt", name: "IX_TriageAssessment_TriageStartAt");
            CreateIndex("dbo.TriageAssessments", "IsOpen", name: "IX_TriageAssessment_IsOpen");
            CreateIndex("dbo.TriageQueues", "NextReassessmentDueAt", name: "IX_TriageQueue_NextReassessmentDueAt");
            DropColumn("dbo.TriageAssessments", "AssessmentTime");
            DropColumn("dbo.TriageAssessments", "EstimatedWaitTime");
            DropColumn("dbo.TriageVitalSigns", "ConsciousnessLevel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TriageVitalSigns", "ConsciousnessLevel", c => c.Int());
            AddColumn("dbo.TriageAssessments", "EstimatedWaitTime", c => c.Int());
            AddColumn("dbo.TriageAssessments", "AssessmentTime", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.TriageReassessments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TriageReassessments", "TriageAssessmentId", "dbo.TriageAssessments");
            DropForeignKey("dbo.TriageReassessments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TriageReassessments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TriageReassessments", "AssessorUserId", "dbo.AspNetUsers");
            DropIndex("dbo.TriageQueues", "IX_TriageQueue_NextReassessmentDueAt");
            DropIndex("dbo.TriageReassessments", new[] { "UpdatedByUserId" });
            DropIndex("dbo.TriageReassessments", new[] { "CreatedByUserId" });
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_CreatedAt");
            DropIndex("dbo.TriageReassessments", new[] { "DeletedByUserId" });
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_IsDeleted");
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_AssessorUserId");
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_Reason");
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_NewLevel");
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_ReassessmentAt");
            DropIndex("dbo.TriageReassessments", "IX_TriageReassessment_TriageAssessmentId");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_IsOpen");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_TriageStartAt");
            DropIndex("dbo.TriageAssessments", "IX_TriageAssessment_AssessorUserId");
            DropPrimaryKey("dbo.TriageAssessmentProtocols");
            AlterColumn("dbo.TriageAssessments", "AssessorUserId", c => c.Int(nullable: false));
            DropColumn("dbo.TriageVitalSigns", "RowVersion");
            DropColumn("dbo.TriageVitalSigns", "ReassessmentReason");
            DropColumn("dbo.TriageVitalSigns", "O2FlowLpm");
            DropColumn("dbo.TriageVitalSigns", "OxygenDevice");
            DropColumn("dbo.TriageVitalSigns", "OnOxygen");
            DropColumn("dbo.TriageVitalSigns", "GcsM");
            DropColumn("dbo.TriageVitalSigns", "GcsV");
            DropColumn("dbo.TriageVitalSigns", "GcsE");
            DropColumn("dbo.TriageQueues", "RowVersion");
            DropColumn("dbo.TriageQueues", "LastReassessmentAt");
            DropColumn("dbo.TriageQueues", "ReassessmentCount");
            DropColumn("dbo.TriageQueues", "NextReassessmentDueAt");
            DropColumn("dbo.TriageProtocols", "RowVersion");
            DropColumn("dbo.TriageAssessments", "RowVersion");
            DropColumn("dbo.TriageAssessments", "EstimatedWaitTimeMinutes");
            DropColumn("dbo.TriageAssessments", "Isolation");
            DropColumn("dbo.TriageAssessments", "IsPregnant");
            DropColumn("dbo.TriageAssessments", "RedFlag_Trauma");
            DropColumn("dbo.TriageAssessments", "RedFlag_ACS");
            DropColumn("dbo.TriageAssessments", "RedFlag_Stroke");
            DropColumn("dbo.TriageAssessments", "RedFlag_Sepsis");
            DropColumn("dbo.TriageAssessments", "IsOpen");
            DropColumn("dbo.TriageAssessments", "FirstPhysicianContactAt");
            DropColumn("dbo.TriageAssessments", "TriageEndAt");
            DropColumn("dbo.TriageAssessments", "TriageStartAt");
            DropColumn("dbo.TriageAssessments", "ArrivalAt");
            DropColumn("dbo.TriageAssessments", "ChiefComplaintCode");
            DropColumn("dbo.TriageAssessments", "PewsScore");
            DropColumn("dbo.TriageAssessments", "News2Score");
            DropColumn("dbo.TriageAssessments", "EsiScore");
            DropTable("dbo.TriageReassessments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_TriageReassessment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            AddPrimaryKey("dbo.TriageAssessmentProtocols", new[] { "TriageProtocol_TriageProtocolId", "TriageAssessment_TriageAssessmentId" });
            RenameIndex(table: "dbo.TriageAssessmentProtocols", name: "IX_TriageProtocolId", newName: "IX_TriageProtocol_TriageProtocolId");
            RenameIndex(table: "dbo.TriageAssessmentProtocols", name: "IX_TriageAssessmentId", newName: "IX_TriageAssessment_TriageAssessmentId");
            RenameColumn(table: "dbo.TriageAssessments", name: "AssessorUserId", newName: "AssessorId");
            RenameColumn(table: "dbo.TriageAssessmentProtocols", name: "TriageAssessmentId", newName: "TriageAssessment_TriageAssessmentId");
            RenameColumn(table: "dbo.TriageAssessmentProtocols", name: "TriageProtocolId", newName: "TriageProtocol_TriageProtocolId");
            CreateIndex("dbo.TriageVitalSigns", new[] { "IsNormal", "RequiresImmediateAttention", "IsDeleted" }, name: "IX_TriageVitalSigns_Normal_Immediate_IsDeleted");
            CreateIndex("dbo.TriageVitalSigns", new[] { "TriageAssessmentId", "MeasurementTime", "IsDeleted" }, name: "IX_TriageVitalSigns_Assessment_Time_IsDeleted");
            CreateIndex("dbo.TriageQueues", new[] { "RequiresImmediateCare", "Status", "IsDeleted" }, name: "IX_TriageQueue_Immediate_Status_IsDeleted");
            CreateIndex("dbo.TriageQueues", new[] { "QueueTime", "Status", "IsDeleted" }, name: "IX_TriageQueue_Time_Status_IsDeleted");
            CreateIndex("dbo.TriageQueues", new[] { "Status", "Priority", "IsDeleted" }, name: "IX_TriageQueue_Status_Priority_IsDeleted");
            CreateIndex("dbo.TriageQueues", new[] { "PatientId", "Status", "IsDeleted" }, name: "IX_TriageQueue_Patient_Status_IsDeleted");
            CreateIndex("dbo.TriageProtocols", new[] { "Priority", "IsMandatory", "IsActive", "IsDeleted" }, name: "IX_TriageProtocol_Priority_Mandatory_Active_IsDeleted");
            CreateIndex("dbo.TriageProtocols", new[] { "ValidFrom", "ValidTo", "IsActive", "IsDeleted" }, name: "IX_TriageProtocol_Validity_Active_IsDeleted");
            CreateIndex("dbo.TriageProtocols", new[] { "Type", "TargetLevel", "IsActive", "IsDeleted" }, name: "IX_TriageProtocol_Type_Level_Active_IsDeleted");
            CreateIndex("dbo.TriageAssessments", "AssessmentTime", name: "IX_TriageAssessment_AssessmentTime");
            CreateIndex("dbo.TriageAssessments", new[] { "AssessmentTime", "Level", "IsDeleted" }, name: "IX_TriageAssessment_Time_Level_IsDeleted");
            CreateIndex("dbo.TriageAssessments", new[] { "Level", "Status", "IsDeleted" }, name: "IX_TriageAssessment_Level_Status_IsDeleted");
            CreateIndex("dbo.TriageAssessments", "AssessorId", name: "IX_TriageAssessment_AssessorId");
            CreateIndex("dbo.TriageAssessments", new[] { "PatientId", "AssessmentTime", "IsDeleted" }, name: "IX_TriageAssessment_Patient_Time_IsDeleted");
            RenameTable(name: "dbo.TriageAssessmentProtocols", newName: "TriageProtocolTriageAssessment");
        }
    }
}
