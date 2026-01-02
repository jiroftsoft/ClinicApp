/*
 * Migration: Make Patient.ApplicationUserId Nullable
 * تاریخ: 2026-01-02
 * نویسنده: AI Assistant
 * 
 * هدف:
 * - تبدیل ستون ApplicationUserId در جدول Patients به NULLABLE
 * - جدا کردن 7,107 بیمار از User نادرست که در Seed Data به آنها متصل شده بودند
 * 
 * دلیل:
 * - بیماران قدیمی که از database قبلی import شده‌اند، User account ندارند
 * - وقتی منشی بیمار جدید پذیرش می‌کند، نباید User account برایش ایجاد شود
 * - وقتی بیمار از سایت ثبت‌نام می‌کند، User به Patient موجود (با NationalCode matching) متصل می‌شود
 * 
 * توجه:
 * - این Migration قبلاً به صورت دستی اجرا شده است
 * - این فایل فقط برای مستندسازی و rollback است
 */

SET NOCOUNT ON;

PRINT N'========================================';
PRINT N'Migration: Make Patient.ApplicationUserId Nullable';
PRINT N'Date: 2026-01-02';
PRINT N'========================================';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ✅ Step 1: Make ApplicationUserId column NULLABLE
    PRINT N'Step 1: Making ApplicationUserId column NULLABLE...';
    
    ALTER TABLE Patients 
    ALTER COLUMN ApplicationUserId NVARCHAR(128) NULL;
    
    PRINT N'✅ Column altered successfully';
    PRINT '';

    -- ✅ Step 2: Unlink patients from incorrect User (ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2)
    PRINT N'Step 2: Unlinking patients from incorrect User...';
    
    UPDATE Patients 
    SET ApplicationUserId = NULL
    WHERE ApplicationUserId = 'ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2' 
      AND NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS != '3020094925';
    
    DECLARE @RowsUpdated INT = @@ROWCOUNT;
    PRINT N'✅ Rows Updated: ' + CAST(@RowsUpdated AS NVARCHAR);
    PRINT '';

    -- ✅ Step 3: Verification
    PRINT N'Step 3: Verifying the migration...';
    
    DECLARE @TotalPatients INT;
    DECLARE @PatientsWithUser INT;
    DECLARE @PatientsWithoutUser INT;
    DECLARE @CorrectMappings INT;
    DECLARE @IncorrectMappings INT;

    SELECT @TotalPatients = COUNT(*) FROM Patients WHERE IsDeleted = 0;
    SELECT @PatientsWithUser = COUNT(*) FROM Patients WHERE ApplicationUserId IS NOT NULL AND IsDeleted = 0;
    SELECT @PatientsWithoutUser = COUNT(*) FROM Patients WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
    
    SELECT @CorrectMappings = COUNT(*)
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0 
      AND u.IsDeleted = 0 
      AND p.NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS = u.UserName COLLATE SQL_Latin1_General_CP1_CI_AS;
    
    SELECT @IncorrectMappings = COUNT(*)
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0 
      AND u.IsDeleted = 0 
      AND p.NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS != u.UserName COLLATE SQL_Latin1_General_CP1_CI_AS;

    PRINT N'  Total Patients: ' + CAST(@TotalPatients AS NVARCHAR);
    PRINT N'  Patients WITH User: ' + CAST(@PatientsWithUser AS NVARCHAR);
    PRINT N'  Patients WITHOUT User: ' + CAST(@PatientsWithoutUser AS NVARCHAR);
    PRINT N'  Correct Mappings: ' + CAST(@CorrectMappings AS NVARCHAR);
    PRINT N'  Incorrect Mappings: ' + CAST(@IncorrectMappings AS NVARCHAR);
    PRINT '';

    IF @IncorrectMappings > 0
    BEGIN
        PRINT N'⚠️ WARNING: Still ' + CAST(@IncorrectMappings AS NVARCHAR) + N' incorrect mappings found!';
        ROLLBACK TRANSACTION;
        RAISERROR(N'Migration verification failed - incorrect mappings still exist', 16, 1);
        RETURN;
    END

    -- ✅ Commit
    COMMIT TRANSACTION;
    
    PRINT N'';
    PRINT N'========================================';
    PRINT N'✅ Migration completed successfully!';
    PRINT N'========================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    PRINT N'';
    PRINT N'========================================';
    PRINT N'❌ Migration FAILED!';
    PRINT N'========================================';
    PRINT N'Error: ' + @ErrorMessage;
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;

GO

/*
 * ========================================
 * ROLLBACK SCRIPT (در صورت نیاز)
 * ========================================
 * 
 * ⚠️ هشدار: این rollback تمام Patients بدون User را به User پیش‌فرض متصل می‌کند
 * فقط در صورت بروز مشکل جدی استفاده کنید!
 * 
 * -- Step 1: Re-link patients to default User
 * UPDATE Patients 
 * SET ApplicationUserId = 'ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2'
 * WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
 * 
 * -- Step 2: Make ApplicationUserId NOT NULL again
 * ALTER TABLE Patients 
 * ALTER COLUMN ApplicationUserId NVARCHAR(128) NOT NULL;
 * 
 * ⚠️ توصیه نمی‌شود rollback کنید! منطق جدید صحیح‌تر است.
 */

