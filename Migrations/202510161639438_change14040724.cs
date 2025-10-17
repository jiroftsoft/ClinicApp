namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change14040724 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Departments", "Code", c => c.String(maxLength: 20));
            AddColumn("dbo.Clinics", "Code", c => c.String(maxLength: 20));
            AddColumn("dbo.Doctors", "DoctorCode", c => c.String(maxLength: 20));
            // حذف رابطه تکراری Doctor_DoctorId - رابطه اصلی DoctorId قبلاً وجود دارد
        }
        
        public override void Down()
        {
            // حذف رابطه تکراری Doctor_DoctorId - رابطه اصلی DoctorId قبلاً وجود دارد
            DropColumn("dbo.Doctors", "DoctorCode");
            DropColumn("dbo.Clinics", "Code");
            DropColumn("dbo.Departments", "Code");
        }
    }
}
