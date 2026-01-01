namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOtpStatesTable1011 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OtpStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SessionId = c.String(nullable: false, maxLength: 88),
                        NationalCode = c.String(nullable: false, maxLength: 10),
                        PhoneNumber = c.String(nullable: false, maxLength: 11),
                        OtpHash = c.String(nullable: false, maxLength: 255),
                        ExpiryUtc = c.DateTime(nullable: false),
                        IpAddress = c.String(maxLength: 45),
                        UserAgent = c.String(maxLength: 500),
                        AttemptCount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.SessionId, t.ExpiryUtc }, name: "IX_OtpState_SessionId_Expiry")
                .Index(t => new { t.NationalCode, t.ExpiryUtc }, name: "IX_OtpState_NationalCode_Expiry")
                .Index(t => t.ExpiryUtc, name: "IX_OtpState_Expiry");
            
            AddColumn("dbo.UserLoginHistories", "IdempotencyKey", c => c.String(maxLength: 50));
            
            // ✅ Filtered Unique Index - فقط برای مقادیر NOT NULL
            // این اجازه می‌دهد رکوردهای قدیمی (NULL) بدون مشکل باقی بمانند
            Sql(@"CREATE UNIQUE INDEX [IX_UserLoginHistory_IdempotencyKey] 
                  ON [dbo].[UserLoginHistories]([IdempotencyKey]) 
                  WHERE [IdempotencyKey] IS NOT NULL");
        }
        
        public override void Down()
        {
            // ✅ حذف Filtered Index با SQL مستقیم
            Sql("DROP INDEX [IX_UserLoginHistory_IdempotencyKey] ON [dbo].[UserLoginHistories]");
            DropIndex("dbo.OtpStates", "IX_OtpState_Expiry");
            DropIndex("dbo.OtpStates", "IX_OtpState_NationalCode_Expiry");
            DropIndex("dbo.OtpStates", "IX_OtpState_SessionId_Expiry");
            DropColumn("dbo.UserLoginHistories", "IdempotencyKey");
            DropTable("dbo.OtpStates");
        }
    }
}
