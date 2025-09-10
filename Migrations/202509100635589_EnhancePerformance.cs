namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnhancePerformance : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Patients", new[] { "NationalCode", "IsDeleted" }, name: "IX_Patient_NationalCode_IsDeleted");
            CreateIndex("dbo.Patients", new[] { "FirstName", "LastName", "IsDeleted" }, name: "IX_Patient_FirstName_LastName_IsDeleted");
            CreateIndex("dbo.Patients", new[] { "PhoneNumber", "IsDeleted" }, name: "IX_Patient_PhoneNumber_IsDeleted");
            CreateIndex("dbo.Patients", new[] { "IsDeleted", "CreatedAt" }, name: "IX_Patient_IsDeleted_CreatedAt");
            CreateIndex("dbo.PatientInsurances", new[] { "PatientId", "IsActive", "Priority", "IsDeleted" }, name: "IX_PatientInsurance_PatientId_IsActive_Priority_IsDeleted");
            CreateIndex("dbo.PatientInsurances", new[] { "StartDate", "EndDate", "IsActive", "IsDeleted" }, name: "IX_PatientInsurance_StartDate_EndDate_IsActive_IsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PatientInsurances", "IX_PatientInsurance_StartDate_EndDate_IsActive_IsDeleted");
            DropIndex("dbo.PatientInsurances", "IX_PatientInsurance_PatientId_IsActive_Priority_IsDeleted");
            DropIndex("dbo.Patients", "IX_Patient_IsDeleted_CreatedAt");
            DropIndex("dbo.Patients", "IX_Patient_PhoneNumber_IsDeleted");
            DropIndex("dbo.Patients", "IX_Patient_FirstName_LastName_IsDeleted");
            DropIndex("dbo.Patients", "IX_Patient_NationalCode_IsDeleted");
        }
    }
}
