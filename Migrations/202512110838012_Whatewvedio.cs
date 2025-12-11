namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Whatewvedio : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Videos", "IX_Video_VideoType");
            AlterColumn("dbo.Videos", "VideoType", c => c.Byte(nullable: false));
            CreateIndex("dbo.Videos", "VideoType", name: "IX_Video_VideoType");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Videos", "IX_Video_VideoType");
            AlterColumn("dbo.Videos", "VideoType", c => c.Int(nullable: false));
            CreateIndex("dbo.Videos", "VideoType", name: "IX_Video_VideoType");
        }
    }
}
