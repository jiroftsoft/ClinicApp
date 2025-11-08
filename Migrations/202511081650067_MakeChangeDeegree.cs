namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeChangeDeegree : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Doctors", "IX_Doctor_Degree");
            AlterColumn("dbo.Doctors", "Degree", c => c.Byte());
            CreateIndex("dbo.Doctors", "Degree", name: "IX_Doctor_Degree");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Doctors", "IX_Doctor_Degree");
            AlterColumn("dbo.Doctors", "Degree", c => c.Byte(nullable: false));
            CreateIndex("dbo.Doctors", "Degree", name: "IX_Doctor_Degree");
        }
    }
}
