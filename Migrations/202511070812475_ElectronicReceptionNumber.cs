namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ElectronicReceptionNumber : DbMigration
    {
        public override void Up()
        {
            // 🏥 MEDICAL: حذف Index قبلی
            DropIndex("dbo.Receptions", "IX_Reception_ReceptionNo");
            
            // 🏥 MEDICAL: اضافه کردن فیلد جدید
            AddColumn("dbo.Receptions", "ElectronicReceptionNumber", c => c.String(maxLength: 30));
            
            // 🏥 MEDICAL: تبدیل تمام NULL ها به مقادیر منحصر به فرد قبل از ایجاد Unique Index
            // استفاده از ReceptionId برای ایجاد شماره موقت منحصر به فرد
            // توجه: تمام رکوردها (حتی حذف شده) را در نظر می‌گیریم تا Unique Index ایجاد شود
            Sql(@"
                UPDATE [dbo].[Receptions]
                SET [ReceptionNo] = 'LEGACY-' + CAST([ReceptionId] AS NVARCHAR(10))
                WHERE [ReceptionNo] IS NULL
            ");
            
            // 🏥 MEDICAL: ایجاد Unique Index (اکنون هیچ NULL وجود ندارد)
            CreateIndex("dbo.Receptions", "ReceptionNo", unique: true, name: "IX_Reception_ReceptionNo");
            
            // 🏥 MEDICAL: ایجاد Index برای ElectronicReceptionNumber
            CreateIndex("dbo.Receptions", "ElectronicReceptionNumber", name: "IX_Reception_ElectronicReceptionNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Receptions", "IX_Reception_ElectronicReceptionNumber");
            DropIndex("dbo.Receptions", "IX_Reception_ReceptionNo");
            DropColumn("dbo.Receptions", "ElectronicReceptionNumber");
            CreateIndex("dbo.Receptions", "ReceptionNo", name: "IX_Reception_ReceptionNo");
        }
    }
}
