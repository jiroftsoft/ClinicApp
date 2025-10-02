namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanServiceConfig01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlanServices", "PatientSharePercent", c => c.Decimal(precision: 5, scale: 2));
            DropColumn("dbo.PlanServices", "Copay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlanServices", "Copay", c => c.Decimal(precision: 5, scale: 2));
            DropColumn("dbo.PlanServices", "PatientSharePercent");
        }
    }
}
