namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOnlineVisitModoule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OnlineConsultationRooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        AppointmentId = c.Int(nullable: false),
                        RoomName = c.String(nullable: false, maxLength: 256),
                        StartedAt = c.DateTime(),
                        EndedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.RoomId)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.AppointmentId, unique: true, name: "IX_OnlineConsultationRoom_AppointmentId")
                .Index(t => t.RoomName, unique: true, name: "IX_OnlineConsultationRoom_RoomName")
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            AddColumn("dbo.Appointments", "IsOnlineConsultation", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OnlineConsultationRooms", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlineConsultationRooms", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OnlineConsultationRooms", "AppointmentId", "dbo.Appointments");
            DropIndex("dbo.OnlineConsultationRooms", new[] { "UpdatedByUserId" });
            DropIndex("dbo.OnlineConsultationRooms", new[] { "CreatedByUserId" });
            DropIndex("dbo.OnlineConsultationRooms", "IX_OnlineConsultationRoom_RoomName");
            DropIndex("dbo.OnlineConsultationRooms", "IX_OnlineConsultationRoom_AppointmentId");
            DropColumn("dbo.Appointments", "IsOnlineConsultation");
            DropTable("dbo.OnlineConsultationRooms");
        }
    }
}
