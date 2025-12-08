namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixDoctorScheduleForeignKey : DbMigration
    {
        public override void Up()
        {
            // بررسی و حذف ستون Doctor_DoctorId در صورت وجود
            // این ستون ممکن است به دلیل Navigation Property تکراری ایجاد شده باشد
            Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.DoctorSchedules') 
                    AND name = 'Doctor_DoctorId'
                )
                BEGIN
                    -- حذف Foreign Key constraint در صورت وجود
                    IF object_id(N'[dbo].[FK_dbo.DoctorSchedules_dbo.Doctors_Doctor_DoctorId]', N'F') IS NOT NULL
                    BEGIN
                        ALTER TABLE [dbo].[DoctorSchedules] DROP CONSTRAINT [FK_dbo.DoctorSchedules_dbo.Doctors_Doctor_DoctorId]
                    END
                    
                    -- حذف Index در صورت وجود
                    IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_Doctor_DoctorId' AND object_id = object_id(N'[dbo].[DoctorSchedules]', N'U'))
                    BEGIN
                        DROP INDEX [IX_Doctor_DoctorId] ON [dbo].[DoctorSchedules]
                    END
                    
                    -- حذف Default Constraint در صورت وجود
                    DECLARE @var0 nvarchar(128)
                    SELECT @var0 = name
                    FROM sys.default_constraints
                    WHERE parent_object_id = object_id(N'dbo.DoctorSchedules')
                    AND col_name(parent_object_id, parent_column_id) = 'Doctor_DoctorId';
                    IF @var0 IS NOT NULL
                    BEGIN
                        EXECUTE('ALTER TABLE [dbo].[DoctorSchedules] DROP CONSTRAINT [' + @var0 + ']')
                    END
                    
                    -- حذف ستون
                    ALTER TABLE [dbo].[DoctorSchedules] DROP COLUMN [Doctor_DoctorId]
                END
            ");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DoctorSchedules", "Doctor_DoctorId", c => c.Int());
            CreateIndex("dbo.DoctorSchedules", "Doctor_DoctorId");
            AddForeignKey("dbo.DoctorSchedules", "Doctor_DoctorId", "dbo.Doctors", "DoctorId");
        }
    }
}
