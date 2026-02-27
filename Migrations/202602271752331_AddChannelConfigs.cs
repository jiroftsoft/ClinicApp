namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChannelConfigs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChannelConfigs",
                c => new
                    {
                        ChannelConfigId = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false, maxLength: 100),
                        SettingKey = c.String(nullable: false, maxLength: 100),
                        SettingValue = c.String(),
                        UpdatedAt = c.DateTime(nullable: false),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ChannelConfigId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ChannelConfigs");
        }
    }
}
