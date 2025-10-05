namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChageDb14040178 : DbMigration
    {
        public override void Up()
        {
            // مرحله 1: ایجاد InsuranceProvider پیش‌فرض
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [dbo].[InsuranceProviders] WHERE [InsuranceProviderId] = 1)
                BEGIN
                    SET IDENTITY_INSERT [dbo].[InsuranceProviders] ON
                    INSERT INTO [dbo].[InsuranceProviders] ([InsuranceProviderId], [Name], [Code], [IsActive], [IsDeleted], [CreatedAt]) 
                    VALUES (1, 'بیمه پیش‌فرض', 'DEFAULT', 1, 0, GETDATE())
                    SET IDENTITY_INSERT [dbo].[InsuranceProviders] OFF
                END
            ");
            
            // مرحله 2: اضافه کردن ستون‌ها (ابتدا بدون Foreign Key)
            AddColumn("dbo.PatientInsurances", "SupplementaryPolicyNumber", c => c.String(maxLength: 100));
            AddColumn("dbo.PatientInsurances", "InsuranceProviderId", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.PatientInsurances", "SupplementaryInsuranceProviderId", c => c.Int());
            AddColumn("dbo.PatientInsurances", "SupplementaryInsurancePlanId", c => c.Int());
            
            // مرحله 3: به‌روزرسانی تمام رکوردهای موجود
            Sql("UPDATE [dbo].[PatientInsurances] SET [InsuranceProviderId] = 1 WHERE [InsuranceProviderId] = 0");
            
            // مرحله 4: ایجاد ایندکس‌ها
            CreateIndex("dbo.PatientInsurances", "SupplementaryPolicyNumber", name: "IX_PatientInsurance_SupplementaryPolicyNumber");
            CreateIndex("dbo.PatientInsurances", "InsuranceProviderId");
            CreateIndex("dbo.PatientInsurances", "SupplementaryInsuranceProviderId");
            CreateIndex("dbo.PatientInsurances", "SupplementaryInsurancePlanId");
            
            // مرحله 5: اضافه کردن Foreign Key ها
            AddForeignKey("dbo.PatientInsurances", "InsuranceProviderId", "dbo.InsuranceProviders", "InsuranceProviderId");
            AddForeignKey("dbo.PatientInsurances", "SupplementaryInsuranceProviderId", "dbo.InsuranceProviders", "InsuranceProviderId");
            AddForeignKey("dbo.PatientInsurances", "SupplementaryInsurancePlanId", "dbo.InsurancePlans", "InsurancePlanId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PatientInsurances", "SupplementaryInsurancePlanId", "dbo.InsurancePlans");
            DropForeignKey("dbo.PatientInsurances", "SupplementaryInsuranceProviderId", "dbo.InsuranceProviders");
            DropForeignKey("dbo.PatientInsurances", "InsuranceProviderId", "dbo.InsuranceProviders");
            DropIndex("dbo.PatientInsurances", new[] { "SupplementaryInsurancePlanId" });
            DropIndex("dbo.PatientInsurances", new[] { "SupplementaryInsuranceProviderId" });
            DropIndex("dbo.PatientInsurances", new[] { "InsuranceProviderId" });
            DropIndex("dbo.PatientInsurances", "IX_PatientInsurance_SupplementaryPolicyNumber");
            DropColumn("dbo.PatientInsurances", "SupplementaryInsurancePlanId");
            DropColumn("dbo.PatientInsurances", "SupplementaryInsuranceProviderId");
            DropColumn("dbo.PatientInsurances", "InsuranceProviderId");
            DropColumn("dbo.PatientInsurances", "SupplementaryPolicyNumber");
        }
    }
}
