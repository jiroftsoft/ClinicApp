namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChnagTypeOfBirthDate : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Patients", "IX_Patient_BirthDate");
            AlterColumn("dbo.Patients", "BirthDate", c => c.DateTime(storeType: "date"));
            CreateIndex("dbo.Patients", "BirthDate", name: "IX_Patient_BirthDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Patients", "IX_Patient_BirthDate");
            AlterColumn("dbo.Patients", "BirthDate", c => c.DateTime());
            CreateIndex("dbo.Patients", "BirthDate", name: "IX_Patient_BirthDate");
        }
    }
}
