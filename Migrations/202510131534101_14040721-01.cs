namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1404072101 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TriageAssessments", "Symptoms", c => c.String(maxLength: 500));
            AddColumn("dbo.TriageAssessments", "MedicalHistory", c => c.String(maxLength: 1000));
            AddColumn("dbo.TriageAssessments", "Allergies", c => c.String(maxLength: 500));
            AddColumn("dbo.TriageAssessments", "Medications", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TriageAssessments", "Medications");
            DropColumn("dbo.TriageAssessments", "Allergies");
            DropColumn("dbo.TriageAssessments", "MedicalHistory");
            DropColumn("dbo.TriageAssessments", "Symptoms");
        }
    }
}
