namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSnapshotJsonAndServiceEligibilityFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReceptionItems", "SnapshotJson", c => c.String());
            AddColumn("dbo.Services", "GroupCode", c => c.Int());
            AddColumn("dbo.Services", "AgeMin", c => c.Int());
            AddColumn("dbo.Services", "AgeMax", c => c.Int());
            AddColumn("dbo.Services", "GenderLimit", c => c.Byte());
            CreateIndex("dbo.Services", "GroupCode", name: "IX_Service_GroupCode");
            CreateIndex("dbo.Services", "AgeMin", name: "IX_Service_AgeMin");
            CreateIndex("dbo.Services", "AgeMax", name: "IX_Service_AgeMax");
            CreateIndex("dbo.Services", "GenderLimit", name: "IX_Service_GenderLimit");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Services", "IX_Service_GenderLimit");
            DropIndex("dbo.Services", "IX_Service_AgeMax");
            DropIndex("dbo.Services", "IX_Service_AgeMin");
            DropIndex("dbo.Services", "IX_Service_GroupCode");
            DropColumn("dbo.Services", "GenderLimit");
            DropColumn("dbo.Services", "AgeMax");
            DropColumn("dbo.Services", "AgeMin");
            DropColumn("dbo.Services", "GroupCode");
            DropColumn("dbo.ReceptionItems", "SnapshotJson");
        }
    }
}
