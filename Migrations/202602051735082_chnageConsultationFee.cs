namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chnageConsultationFee : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DoctorSchedules", "ConsultationFee", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.DoctorSchedules", "CancellationFee", c => c.Decimal(nullable: false, precision: 18, scale: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DoctorSchedules", "CancellationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.DoctorSchedules", "ConsultationFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
