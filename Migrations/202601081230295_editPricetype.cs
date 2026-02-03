namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editPricetype : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Appointments", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            AlterColumn("dbo.AppointmentSlots", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AppointmentSlots", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AlterColumn("dbo.Appointments", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
    }
}
