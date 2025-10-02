namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ChangeTableInsurancetariif : DbMigration
    {
        public override void Up()
        {
            // ------------------------------------------------------------
            // 0) (اختیاری) اگر مبالغ قبلاً به تومان ذخیره می‌شدند: تومان → ریال (×۱۰)
            //    در صورت نیاز این بلاک را فعال کن.
            // ------------------------------------------------------------
            // Sql(@"
            // UPDATE dbo.InsuranceTariffs SET TariffPrice = ROUND(TariffPrice * 10, 0) WHERE TariffPrice IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET PatientShare = ROUND(PatientShare * 10, 0) WHERE PatientShare IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET InsurerShare = ROUND(InsurerShare * 10, 0) WHERE InsurerShare IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET SupplementaryMaxPayment = ROUND(SupplementaryMaxPayment * 10, 0) WHERE SupplementaryMaxPayment IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET SupplementaryDeductible = ROUND(SupplementaryDeductible * 10, 0) WHERE SupplementaryDeductible IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET MinPatientCopay = ROUND(MinPatientCopay * 10, 0) WHERE MinPatientCopay IS NOT NULL;
            // ");

            // ------------------------------------------------------------
            // 1) Drop همهٔ ایندکس‌هایی که به هر یک از ستون‌های مبلغی وابسته‌اند (Key یا Include)
            //    این کار خطای "index is dependent on column" را رفع می‌کند.
            // ------------------------------------------------------------
            Sql(@"
DECLARE @tbl sysname = N'dbo.InsuranceTariffs';
IF OBJECT_ID(@tbl, 'U') IS NOT NULL
BEGIN
    DECLARE @cols TABLE(col sysname);
    INSERT INTO @cols(col) VALUES
        (N'TariffPrice'),
        (N'PatientShare'),
        (N'InsurerShare'),
        (N'SupplementaryMaxPayment'),
        (N'SupplementaryDeductible'),
        (N'MinPatientCopay');

    ;WITH idx AS (
        SELECT DISTINCT i.name AS idx_name
        FROM sys.indexes i
        JOIN sys.index_columns ic
            ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        WHERE i.object_id = OBJECT_ID(@tbl)
          AND i.is_hypothetical = 0
          AND i.name IS NOT NULL
          AND (
                COL_NAME(ic.object_id, ic.column_id) IN (SELECT col FROM @cols)
              )
    )
    SELECT idx_name INTO #to_drop FROM idx;

    DECLARE @n sysname;
    WHILE EXISTS (SELECT 1 FROM #to_drop)
    BEGIN
        SELECT TOP(1) @n = idx_name FROM #to_drop;
        EXEC(N'DROP INDEX [' + @n + N'] ON ' + @tbl + N';');
        DELETE FROM #to_drop WHERE idx_name = @n;
    END
END
");

            // ------------------------------------------------------------
            // 2) تغییر precision همهٔ مبالغ به decimal(18,0)  (ذخیره‌سازی «ریالِ صحیح»)
            // ------------------------------------------------------------
            AlterColumn("dbo.InsuranceTariffs", "TariffPrice", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceTariffs", "PatientShare", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceTariffs", "InsurerShare", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceTariffs", "SupplementaryDeductible", c => c.Decimal(precision: 18, scale: 0));
            AlterColumn("dbo.InsuranceTariffs", "MinPatientCopay", c => c.Decimal(precision: 18, scale: 0));

            // ------------------------------------------------------------
            // 3) ایجاد مجدد ایندکس‌هایی که واقعاً لازم داریم
            //    (ایندکس‌های سفارشیِ دیگر را در صورت نیاز خودت دوباره بساز)
            // ------------------------------------------------------------
            CreateIndex("dbo.InsuranceTariffs", "TariffPrice", name: "IX_InsuranceTariff_TariffPrice");
            CreateIndex("dbo.InsuranceTariffs", "PatientShare", name: "IX_InsuranceTariff_PatientShare");
            CreateIndex("dbo.InsuranceTariffs", "InsurerShare", name: "IX_InsuranceTariff_InsurerShare");
            CreateIndex("dbo.InsuranceTariffs", "SupplementaryMaxPayment", name: "IX_InsuranceTariff_SupplementaryMaxPayment");
            CreateIndex("dbo.InsuranceTariffs", "SupplementaryDeductible", name: "IX_InsuranceTariff_SupplementaryDeductible");
            CreateIndex("dbo.InsuranceTariffs", "MinPatientCopay", name: "IX_InsuranceTariff_MinPatientCopay");

            // ------------------------------------------------------------
            // 4) (اختیاری) قیود غیرمنفی برای کیفیت داده
            // ------------------------------------------------------------
            Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_TariffPrice_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_TariffPrice_NonNegative CHECK (TariffPrice IS NULL OR TariffPrice >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_PatientShare_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_PatientShare_NonNegative CHECK (PatientShare IS NULL OR PatientShare >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_InsurerShare_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_InsurerShare_NonNegative CHECK (InsurerShare IS NULL OR InsurerShare >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_SuppMax_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_SuppMax_NonNegative CHECK (SupplementaryMaxPayment IS NULL OR SupplementaryMaxPayment >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_SuppDed_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_SuppDed_NonNegative CHECK (SupplementaryDeductible IS NULL OR SupplementaryDeductible >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_MinCopay_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs ADD CONSTRAINT CK_InsuranceTariffs_MinCopay_NonNegative CHECK (MinPatientCopay IS NULL OR MinPatientCopay >= 0);
");
        }

        public override void Down()
        {
            // حذف قیود غیرمنفی (اگر در Up ایجاد شده‌اند)
            Sql(@"
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_MinCopay_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_MinCopay_NonNegative;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_SuppDed_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_SuppDed_NonNegative;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_SuppMax_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_SuppMax_NonNegative;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_InsurerShare_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_InsurerShare_NonNegative;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_PatientShare_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_PatientShare_NonNegative;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_InsuranceTariffs_TariffPrice_NonNegative')
    ALTER TABLE dbo.InsuranceTariffs DROP CONSTRAINT CK_InsuranceTariffs_TariffPrice_NonNegative;
");

            // Drop ایندکس‌های ساخته‌شده در Up
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_MinPatientCopay");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryDeductible");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_SupplementaryMaxPayment");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsurerShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_PatientShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_TariffPrice");

            // برگرداندن precision به decimal(18,2)
            AlterColumn("dbo.InsuranceTariffs", "MinPatientCopay", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "SupplementaryDeductible", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "SupplementaryMaxPayment", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "InsurerShare", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "PatientShare", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InsuranceTariffs", "TariffPrice", c => c.Decimal(precision: 18, scale: 2));

            // ساخت مجدد ایندکس‌های ساده (مطابق حالت اولیه)
            CreateIndex("dbo.InsuranceTariffs", "SupplementaryMaxPayment", name: "IX_InsuranceTariff_SupplementaryMaxPayment");
            CreateIndex("dbo.InsuranceTariffs", "InsurerShare", name: "IX_InsuranceTariff_InsurerShare");
            CreateIndex("dbo.InsuranceTariffs", "PatientShare", name: "IX_InsuranceTariff_PatientShare");
            CreateIndex("dbo.InsuranceTariffs", "TariffPrice", name: "IX_InsuranceTariff_TariffPrice");

            // (اختیاری) اگر در Up تبدیل تومان→ریال انجام دادی، اینجا معکوس کن:
            // Sql(@"
            // UPDATE dbo.InsuranceTariffs SET TariffPrice = TariffPrice / 10.0 WHERE TariffPrice IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET PatientShare = PatientShare / 10.0 WHERE PatientShare IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET InsurerShare = InsurerShare / 10.0 WHERE InsurerShare IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET SupplementaryMaxPayment = SupplementaryMaxPayment / 10.0 WHERE SupplementaryMaxPayment IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET SupplementaryDeductible = SupplementaryDeductible / 10.0 WHERE SupplementaryDeductible IS NOT NULL;
            // UPDATE dbo.InsuranceTariffs SET MinPatientCopay = MinPatientCopay / 10.0 WHERE MinPatientCopay IS NOT NULL;
            // ");
        }
    }
}
