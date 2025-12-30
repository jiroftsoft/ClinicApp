namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ADDUserLoginHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserLoginHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginTime = c.DateTime(nullable: false),
                        LogoutTime = c.DateTime(),
                        IpAddress = c.String(maxLength: 50),
                        UserAgent = c.String(maxLength: 500),
                        DeviceType = c.String(maxLength: 50),
                        BrowserName = c.String(maxLength: 50),
                        BrowserVersion = c.String(maxLength: 20),
                        OSName = c.String(maxLength: 50),
                        OSVersion = c.String(maxLength: 20),
                        Location = c.String(maxLength: 100),
                        IsSuccessful = c.Boolean(nullable: false),
                        FailureReason = c.String(maxLength: 200),
                        SessionId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId, name: "IX_UserLoginHistory_UserId")
                .Index(t => new { t.UserId, t.LoginTime }, name: "IX_UserLoginHistory_UserId_LoginTime")
                .Index(t => t.LoginTime, name: "IX_UserLoginHistory_LoginTime")
                .Index(t => t.IpAddress, name: "IX_UserLoginHistory_IpAddress")
                .Index(t => t.IsSuccessful, name: "IX_UserLoginHistory_IsSuccessful");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserLoginHistories", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserLoginHistories", "IX_UserLoginHistory_IsSuccessful");
            DropIndex("dbo.UserLoginHistories", "IX_UserLoginHistory_IpAddress");
            DropIndex("dbo.UserLoginHistories", "IX_UserLoginHistory_LoginTime");
            DropIndex("dbo.UserLoginHistories", "IX_UserLoginHistory_UserId_LoginTime");
            DropIndex("dbo.UserLoginHistories", "IX_UserLoginHistory_UserId");
            DropTable("dbo.UserLoginHistories");
        }
    }
}
