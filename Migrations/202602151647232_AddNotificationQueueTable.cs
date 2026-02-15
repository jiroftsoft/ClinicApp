namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationQueueTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationQueue",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        PatientId = c.Int(),
                        AppointmentId = c.Int(),
                        NotificationType = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Message = c.String(nullable: false, maxLength: 2000),
                        Channel = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        RetryCount = c.Int(nullable: false),
                        MaxRetries = c.Int(nullable: false),
                        ScheduledTime = c.DateTime(),
                        SentTime = c.DateTime(),
                        ErrorLog = c.String(maxLength: 2000),
                        IdempotencyKey = c.String(nullable: false, maxLength: 256),
                        Recipient = c.String(nullable: false, maxLength: 100),
                        Subject = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.AppointmentId, t.NotificationType, t.Channel }, name: "IX_NotificationQueue_Appointment_Type_Channel")
                .Index(t => t.Status, name: "IX_NotificationQueue_Status")
                .Index(t => t.ScheduledTime, name: "IX_NotificationQueue_ScheduledTime")
                .Index(t => t.IdempotencyKey, name: "IX_NotificationQueue_IdempotencyKey");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.NotificationQueue", "IX_NotificationQueue_IdempotencyKey");
            DropIndex("dbo.NotificationQueue", "IX_NotificationQueue_ScheduledTime");
            DropIndex("dbo.NotificationQueue", "IX_NotificationQueue_Status");
            DropIndex("dbo.NotificationQueue", "IX_NotificationQueue_Appointment_Type_Channel");
            DropTable("dbo.NotificationQueue");
        }
    }
}
