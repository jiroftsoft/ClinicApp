namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MedicalRecordStandardFieldsAndTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MedicalHistoryLabResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MedicalHistoryId = c.Int(nullable: false),
                        LabName = c.String(nullable: false, maxLength: 100),
                        Value = c.String(maxLength: 50),
                        Unit = c.String(maxLength: 50),
                        LabDate = c.DateTime(nullable: false, storeType: "date"),
                        ReferenceRange = c.String(maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.MedicalHistories", t => t.MedicalHistoryId, cascadeDelete: true)
                .Index(t => t.MedicalHistoryId, name: "IX_MedicalHistoryLabResult_MedicalHistoryId")
                .Index(t => new { t.MedicalHistoryId, t.LabDate }, name: "IX_MedicalHistoryLabResult_HistoryId_Date")
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.MedicalHistoryMedications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MedicalHistoryId = c.Int(nullable: false),
                        DrugName = c.String(nullable: false, maxLength: 200),
                        Dosage = c.String(maxLength: 100),
                        DosageUnit = c.String(maxLength: 50),
                        Frequency = c.String(maxLength: 100),
                        Route = c.String(maxLength: 50),
                        StartDate = c.DateTime(storeType: "date"),
                        EndDate = c.DateTime(storeType: "date"),
                        Indication = c.String(maxLength: 300),
                        PrescribingDoctor = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.MedicalHistories", t => t.MedicalHistoryId, cascadeDelete: true)
                .Index(t => t.MedicalHistoryId, name: "IX_MedicalHistoryMedication_MedicalHistoryId")
                .Index(t => new { t.MedicalHistoryId, t.IsActive }, name: "IX_MedicalHistoryMedication_HistoryId_Active")
                .Index(t => t.CreatedByUserId);
            
            AddColumn("dbo.Patients", "MaritalStatus", c => c.String(maxLength: 20));
            AddColumn("dbo.Patients", "GuardianName", c => c.String(maxLength: 100));
            AddColumn("dbo.Patients", "GuardianPhone", c => c.String(maxLength: 50));
            AddColumn("dbo.Receptions", "Diagnosis", c => c.String(maxLength: 500));
            AddColumn("dbo.Receptions", "DiagnosisCode", c => c.String(maxLength: 20));
            AddColumn("dbo.Receptions", "TreatmentPlan", c => c.String(maxLength: 2000));
            AddColumn("dbo.MedicalHistories", "IsCritical", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MedicalHistoryMedications", "MedicalHistoryId", "dbo.MedicalHistories");
            DropForeignKey("dbo.MedicalHistoryMedications", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MedicalHistoryLabResults", "MedicalHistoryId", "dbo.MedicalHistories");
            DropForeignKey("dbo.MedicalHistoryLabResults", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.MedicalHistoryMedications", new[] { "CreatedByUserId" });
            DropIndex("dbo.MedicalHistoryMedications", "IX_MedicalHistoryMedication_HistoryId_Active");
            DropIndex("dbo.MedicalHistoryMedications", "IX_MedicalHistoryMedication_MedicalHistoryId");
            DropIndex("dbo.MedicalHistoryLabResults", new[] { "CreatedByUserId" });
            DropIndex("dbo.MedicalHistoryLabResults", "IX_MedicalHistoryLabResult_HistoryId_Date");
            DropIndex("dbo.MedicalHistoryLabResults", "IX_MedicalHistoryLabResult_MedicalHistoryId");
            DropColumn("dbo.MedicalHistories", "IsCritical");
            DropColumn("dbo.Receptions", "TreatmentPlan");
            DropColumn("dbo.Receptions", "DiagnosisCode");
            DropColumn("dbo.Receptions", "Diagnosis");
            DropColumn("dbo.Patients", "GuardianPhone");
            DropColumn("dbo.Patients", "GuardianName");
            DropColumn("dbo.Patients", "MaritalStatus");
            DropTable("dbo.MedicalHistoryMedications");
            DropTable("dbo.MedicalHistoryLabResults");
        }
    }
}
