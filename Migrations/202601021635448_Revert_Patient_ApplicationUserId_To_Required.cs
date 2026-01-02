namespace ClinicApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Revert_Patient_ApplicationUserId_To_Required : DbMigration
    {
        public override void Up()
        {
            // ⚠️ CRITICAL: این Migration برای 7,107 بیمار Legacy که User ندارند، User ایجاد می‌کند
            
            // STEP 1: شناسایی بیماران بدون User
            Sql(@"
                PRINT '🔍 شناسایی بیماران بدون User account...';
                
                DECLARE @NullCount INT;
                SELECT @NullCount = COUNT(*) 
                FROM Patients 
                WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
                
                PRINT '📊 تعداد بیماران بدون User: ' + CAST(@NullCount AS NVARCHAR(10));
            ");
            
            // STEP 2: دریافت RoleId برای نقش Patient
            Sql(@"
                DECLARE @PatientRoleId NVARCHAR(128);
                SELECT @PatientRoleId = Id FROM AspNetRoles WHERE Name = 'Patient';
                
                IF @PatientRoleId IS NULL
                BEGIN
                    RAISERROR('❌ نقش Patient یافت نشد. لطفاً ابتدا نقش Patient را ایجاد کنید.', 16, 1);
                    RETURN;
                END
                
                PRINT '✅ نقش Patient شناسایی شد: ' + @PatientRoleId;
            ");
            
            // STEP 3: ایجاد User برای بیماران بدون User
            Sql(@"
                PRINT '🚀 شروع ایجاد User برای بیماران...';
                
                DECLARE @PatientRoleId NVARCHAR(128);
                SELECT @PatientRoleId = Id FROM AspNetRoles WHERE Name = 'Patient';
                
                DECLARE @Counter INT = 0;
                DECLARE @PatientId INT;
                DECLARE @NationalCode NVARCHAR(10);
                DECLARE @UniqueNationalCode NVARCHAR(10);
                DECLARE @PhoneNumber NVARCHAR(20);
                DECLARE @UniquePhoneNumber NVARCHAR(20);
                DECLARE @FirstName NVARCHAR(100);
                DECLARE @LastName NVARCHAR(100);
                DECLARE @NewUserId NVARCHAR(128);
                DECLARE @PhoneExists INT;
                DECLARE @NationalCodeExists INT;
                
                -- Cursor برای پردازش هر بیمار
                DECLARE patient_cursor CURSOR FOR
                SELECT PatientId, NationalCode, PhoneNumber, FirstName, LastName
                FROM Patients
                WHERE ApplicationUserId IS NULL AND IsDeleted = 0
                ORDER BY PatientId;
                
                OPEN patient_cursor;
                FETCH NEXT FROM patient_cursor INTO @PatientId, @NationalCode, @PhoneNumber, @FirstName, @LastName;
                
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    -- تولید UserId جدید
                    SET @NewUserId = LOWER(NEWID());
                    
                    -- چک کردن NationalCode تکراری
                    SET @UniqueNationalCode = @NationalCode;
                    
                    IF @NationalCode IS NOT NULL AND @NationalCode != ''
                    BEGIN
                        SELECT @NationalCodeExists = COUNT(*) 
                        FROM AspNetUsers 
                        WHERE NationalCode = @NationalCode OR UserName = @NationalCode;
                        
                        -- اگر NationalCode تکراری است، یک NationalCode منحصربه‌فرد بسازیم
                        IF @NationalCodeExists > 0
                        BEGIN
                            -- فرمت: LEG-{PatientId} (10 رقمی)
                            SET @UniqueNationalCode = 'LEG' + RIGHT('0000000' + CAST(@PatientId AS NVARCHAR(7)), 7);
                        END
                    END
                    ELSE
                    BEGIN
                        -- NationalCode خالی است، یک مقدار جعلی بسازیم
                        SET @UniqueNationalCode = 'LEG' + RIGHT('0000000' + CAST(@PatientId AS NVARCHAR(7)), 7);
                    END
                    
                    -- چک کردن اینکه آیا PhoneNumber تکراری است یا NULL
                    SET @UniquePhoneNumber = @PhoneNumber;
                    
                    IF @PhoneNumber IS NULL OR @PhoneNumber = '' OR LEN(@PhoneNumber) < 10
                    BEGIN
                        -- شماره جعلی از روی PatientId (فرمت: 09999XXXXXXX)
                        SET @UniquePhoneNumber = '09999' + RIGHT('0000000' + CAST(@PatientId AS NVARCHAR(7)), 7);
                    END
                    ELSE
                    BEGIN
                        SELECT @PhoneExists = COUNT(*) 
                        FROM AspNetUsers 
                        WHERE PhoneNumber = @PhoneNumber;
                        
                        -- اگر PhoneNumber تکراری است، شماره جعلی بسازیم
                        IF @PhoneExists > 0
                        BEGIN
                            SET @UniquePhoneNumber = '09999' + RIGHT('0000000' + CAST(@PatientId AS NVARCHAR(7)), 7);
                        END
                    END
                    
                    -- ایجاد User جدید
                    BEGIN TRY
                        INSERT INTO AspNetUsers (
                            Id, 
                            UserName,
                            FirstName,
                            LastName,
                            NationalCode,
                            PhoneNumber, 
                            PhoneNumberConfirmed,
                            Email,
                            EmailConfirmed,
                            PasswordHash,
                            SecurityStamp,
                            TwoFactorEnabled,
                            LockoutEnabled,
                            AccessFailedCount,
                            IsDeleted,
                            IsActive,
                            Gender,
                            CreatedAt,
                            CreatedByUserId
                        )
                        VALUES (
                            @NewUserId,
                            @UniqueNationalCode, -- UserName (منحصربه‌فرد)
                            ISNULL(@FirstName, 'بیمار'), -- FirstName (required)
                            ISNULL(@LastName, 'Legacy'), -- LastName (required)
                            @UniqueNationalCode, -- NationalCode (منحصربه‌فرد)
                            @UniquePhoneNumber, -- PhoneNumber (منحصربه‌فرد)
                            CASE WHEN @PhoneNumber IS NOT NULL AND LEN(@PhoneNumber) >= 10 THEN 1 ELSE 0 END, -- PhoneNumberConfirmed
                            NULL, -- Email
                            0, -- EmailConfirmed
                            NULL, -- PasswordHash (بیمار باید اولین بار رمز تنظیم کند)
                            CAST(NEWID() AS NVARCHAR(128)), -- SecurityStamp
                            0, -- TwoFactorEnabled
                            0, -- LockoutEnabled
                            0, -- AccessFailedCount
                            0, -- IsDeleted
                            1, -- IsActive
                            0, -- Gender (0 = نامشخص)
                            GETDATE(), -- CreatedAt
                            NULL -- CreatedByUserId
                        );
                        
                        -- اختصاص نقش Patient
                        INSERT INTO AspNetUserRoles (UserId, RoleId)
                        VALUES (@NewUserId, @PatientRoleId);
                        
                        -- لینک User به Patient
                        UPDATE Patients
                        SET ApplicationUserId = @NewUserId
                        WHERE PatientId = @PatientId;
                        
                        SET @Counter = @Counter + 1;
                        
                        -- گزارش پیشرفت هر 500 بیمار
                        IF @Counter % 500 = 0
                            PRINT '⏳ پردازش شده: ' + CAST(@Counter AS NVARCHAR(10)) + ' بیمار';
                    END TRY
                    BEGIN CATCH
                        PRINT '❌ خطا در پردازش بیمار ' + CAST(@PatientId AS NVARCHAR(10)) + ': ' + ERROR_MESSAGE();
                    END CATCH
                    
                    FETCH NEXT FROM patient_cursor INTO @PatientId, @NationalCode, @PhoneNumber, @FirstName, @LastName;
                END
                
                CLOSE patient_cursor;
                DEALLOCATE patient_cursor;
                
                PRINT '✅ تکمیل شد! تعداد User ایجاد شده: ' + CAST(@Counter AS NVARCHAR(10));
            ");
            
            // STEP 4: تأیید اینکه همه بیماران User دارند
            Sql(@"
                DECLARE @RemainingNull INT;
                SELECT @RemainingNull = COUNT(*) 
                FROM Patients 
                WHERE ApplicationUserId IS NULL AND IsDeleted = 0;
                
                IF @RemainingNull > 0
                BEGIN
                    RAISERROR('❌ هنوز %d بیمار بدون User باقی مانده است!', 16, 1, @RemainingNull);
                    RETURN;
                END
                
                PRINT '✅ همه بیماران User دارند. ادامه به NOT NULL کردن...';
            ");
            
            // STEP 5: حالا می‌توانیم ApplicationUserId را NOT NULL کنیم
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            AlterColumn("dbo.Patients", "ApplicationUserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Patients", "ApplicationUserId");
            
            Sql(@"PRINT '🎉 Migration با موفقیت اجرا شد!';");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            AlterColumn("dbo.Patients", "ApplicationUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Patients", "ApplicationUserId");
        }
    }
}
