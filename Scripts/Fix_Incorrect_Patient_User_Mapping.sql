/*
 * Fix Incorrect Patient-User Mapping
 * تاریخ: 2026-01-02
 * هدف: تصحیح ارتباط نادرست بین Patients و Users
 *
 * مشکل:
 * - 7,108 بیمار به یک User نادرست متصل شده‌اند (UserId: ba1140f4-e1f0-43af-8387-ab4ea7e9f9c2)
 * - این بیماران در Seed Data به یک ApplicationUserId default متصل شده‌اند
 *
 * راه‌حل:
 * - ApplicationUserId را NULL می‌کنیم برای بیمارانی که User نادرست دارند
 * - فقط بیمارانی که NationalCode = UserName باید User account داشته باشند
 */

SET NOCOUNT ON;

PRINT N'🔍 شروع تصحیح ارتباط Patient-User...';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1️⃣ بررسی وضعیت فعلی
    PRINT N'📊 وضعیت فعلی:';
    PRINT N'------------------------------';

    DECLARE @TotalPatients INT;
    DECLARE @IncorrectMappings INT;
    
    SELECT @TotalPatients = COUNT(*)
    FROM Patients
    WHERE IsDeleted = 0;
    
    SELECT @IncorrectMappings = COUNT(*)
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0
      AND u.IsDeleted = 0
      AND p.NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS != u.UserName COLLATE SQL_Latin1_General_CP1_CI_AS;

    PRINT N'  Total Patients: ' + CAST(@TotalPatients AS NVARCHAR);
    PRINT N'  Incorrect Mappings: ' + CAST(@IncorrectMappings AS NVARCHAR);
    PRINT '';

    -- 2️⃣ شناسایی User نادرست
    PRINT N'🔍 User با بیشترین Patient (نادرست):';
    PRINT N'------------------------------';
    
    SELECT TOP 1
        @TotalPatients = COUNT(*),
        @IncorrectMappings = u.UserName
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0
    GROUP BY u.UserName, u.Id
    ORDER BY COUNT(*) DESC;

    PRINT N'  UserName: ' + CAST(@IncorrectMappings AS NVARCHAR);
    PRINT N'  Patient Count: ' + CAST(@TotalPatients AS NVARCHAR);
    PRINT '';

    -- 3️⃣ تصحیح: Set ApplicationUserId = NULL برای mappings نادرست
    PRINT N'🛠️ در حال تصحیح...';
    PRINT N'------------------------------';

    -- ⚠️ IMPORTANT: فقط Patients که NationalCode != UserName را NULL می‌کنیم
    UPDATE p
    SET ApplicationUserId = NULL
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0
      AND u.IsDeleted = 0
      AND p.NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS != u.UserName COLLATE SQL_Latin1_General_CP1_CI_AS;

    DECLARE @RowsAffected INT = @@ROWCOUNT;

    PRINT N'  ✅ Rows Updated: ' + CAST(@RowsAffected AS NVARCHAR);
    PRINT '';

    -- 4️⃣ بررسی وضعیت بعد از تصحیح
    PRINT N'📊 وضعیت بعد از تصحیح:';
    PRINT N'------------------------------';

    SELECT @IncorrectMappings = COUNT(*)
    FROM Patients p
    INNER JOIN AspNetUsers u ON p.ApplicationUserId = u.Id
    WHERE p.IsDeleted = 0
      AND u.IsDeleted = 0
      AND p.NationalCode COLLATE SQL_Latin1_General_CP1_CI_AS != u.UserName COLLATE SQL_Latin1_General_CP1_CI_AS;

    PRINT N'  Remaining Incorrect Mappings: ' + CAST(@IncorrectMappings AS NVARCHAR);
    PRINT '';

    -- 5️⃣ گزارش نهایی
    PRINT N'✅ تصحیح با موفقیت انجام شد!';
    PRINT '';
    PRINT N'📋 خلاصه:';
    PRINT N'  - ' + CAST(@RowsAffected AS NVARCHAR) + N' Patient record تصحیح شد';
    PRINT N'  - ApplicationUserId نادرست به NULL تغییر کرد';
    PRINT N'  - این بیماران می‌توانند بعداً با ثبت‌نام، User account بسازند';
    PRINT '';

    COMMIT TRANSACTION;
    PRINT N'✅ Transaction با موفقیت Commit شد.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    PRINT N'❌ خطا در تصحیح: ' + @ErrorMessage;
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;

PRINT '';
PRINT N'پایان اسکریپت.';

GO

-- 📊 گزارش نهایی
PRINT '';
PRINT N'📊 گزارش نهایی User-Patient Mapping:';
PRINT N'=====================================';

SELECT 
    u.UserName,
    COUNT(p.PatientId) AS PatientCount,
    CASE 
        WHEN COUNT(p.PatientId) = 0 THEN N'⚠️ No Patients'
        WHEN COUNT(p.PatientId) = 1 THEN N'✅ OK'
        ELSE N'❌ Multiple Patients (CHECK!)'
    END AS Status
FROM AspNetUsers u
LEFT JOIN Patients p ON u.Id = p.ApplicationUserId AND p.IsDeleted = 0
WHERE u.IsDeleted = 0
GROUP BY u.UserName
ORDER BY COUNT(p.PatientId) DESC;

