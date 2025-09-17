namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeValueRange : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_Value");
            AlterColumn("dbo.FactorSettings", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.FactorSettings", "Value", name: "IX_FactorSetting_Value");
        }
        
        public override void Down()
        {
            DropIndex("dbo.FactorSettings", "IX_FactorSetting_Value");
            AlterColumn("dbo.FactorSettings", "Value", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            CreateIndex("dbo.FactorSettings", "Value", name: "IX_FactorSetting_Value");
        }
    }
}
