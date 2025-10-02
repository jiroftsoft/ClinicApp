namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class M01_ServicePrice_To_Rial_Decimal18_0 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Services", "IX_Service_Price");
            AlterColumn("dbo.Services", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 0));
            CreateIndex("dbo.Services", "Price", name: "IX_Service_Price");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Services", "IX_Service_Price");
            AlterColumn("dbo.Services", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.Services", "Price", name: "IX_Service_Price");
        }
    }
}
