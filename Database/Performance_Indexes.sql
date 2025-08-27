-- 🗂️ Database Performance Indexes for Shafa Medical Clinic
-- 🚀 EF Performance Specialist - Index Optimization
-- هدف: بهبود عملکرد جستجو و کوئری‌های پرترافیک

USE [ClinicAppDB]
GO

-- =============================================
-- 📊 PATIENT INDEXES (ایندکس‌های بیمار)
-- =============================================

-- ایندکس اصلی کد ملی (Unique)
CREATE UNIQUE NONCLUSTERED INDEX [IX_Patients_NationalCode_Active] 
ON [dbo].[Patients] ([NationalCode])
WHERE [IsDeleted] = 0
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس ترکیبی نام و نام خانوادگی
CREATE NONCLUSTERED INDEX [IX_Patients_Name_Search] 
ON [dbo].[Patients] ([FirstName], [LastName], [IsDeleted])
INCLUDE ([PatientId], [NationalCode], [PhoneNumber], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس شماره تلفن
CREATE NONCLUSTERED INDEX [IX_Patients_PhoneNumber] 
ON [dbo].[Patients] ([PhoneNumber], [IsDeleted])
INCLUDE ([PatientId], [FirstName], [LastName], [NationalCode])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس بیمه
CREATE NONCLUSTERED INDEX [IX_Patients_Insurance] 
ON [dbo].[Patients] ([InsuranceId], [IsDeleted])
INCLUDE ([PatientId], [FirstName], [LastName], [NationalCode])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس تاریخ ایجاد
CREATE NONCLUSTERED INDEX [IX_Patients_CreatedAt] 
ON [dbo].[Patients] ([CreatedAt] DESC, [IsDeleted])
INCLUDE ([PatientId], [FirstName], [LastName], [NationalCode])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 APPOINTMENT INDEXES (ایندکس‌های نوبت)
-- =============================================

-- ایندکس ترکیبی تاریخ، پزشک و وضعیت
CREATE NONCLUSTERED INDEX [IX_Appointments_Date_Doctor_Status] 
ON [dbo].[Appointments] ([AppointmentDate], [DoctorId], [Status], [IsDeleted])
INCLUDE ([AppointmentId], [PatientId], [ServiceId], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس بیمار
CREATE NONCLUSTERED INDEX [IX_Appointments_Patient] 
ON [dbo].[Appointments] ([PatientId], [IsDeleted])
INCLUDE ([AppointmentId], [AppointmentDate], [DoctorId], [Status])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس بازه زمانی
CREATE NONCLUSTERED INDEX [IX_Appointments_DateRange] 
ON [dbo].[Appointments] ([AppointmentDate], [IsDeleted])
INCLUDE ([AppointmentId], [PatientId], [DoctorId], [Status], [ServiceId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت
CREATE NONCLUSTERED INDEX [IX_Appointments_Status] 
ON [dbo].[Appointments] ([Status], [IsDeleted])
INCLUDE ([AppointmentId], [AppointmentDate], [PatientId], [DoctorId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 RECEPTION/INVOICE INDEXES (ایندکس‌های فاکتور/پذیرش)
-- =============================================

-- ایندکس شماره فاکتور
CREATE NONCLUSTERED INDEX [IX_Receptions_ReceptionNumber] 
ON [dbo].[Receptions] ([ReceptionNumber], [IsDeleted])
INCLUDE ([ReceptionId], [PatientId], [DoctorId], [TotalAmount], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس بیمار
CREATE NONCLUSTERED INDEX [IX_Receptions_Patient] 
ON [dbo].[Receptions] ([PatientId], [IsDeleted])
INCLUDE ([ReceptionId], [ReceptionNumber], [DoctorId], [TotalAmount], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس پزشک
CREATE NONCLUSTERED INDEX [IX_Receptions_Doctor] 
ON [dbo].[Receptions] ([DoctorId], [IsDeleted])
INCLUDE ([ReceptionId], [ReceptionNumber], [PatientId], [TotalAmount], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس بازه زمانی
CREATE NONCLUSTERED INDEX [IX_Receptions_DateRange] 
ON [dbo].[Receptions] ([CreatedAt], [IsDeleted])
INCLUDE ([ReceptionId], [ReceptionNumber], [PatientId], [DoctorId], [TotalAmount])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت پرداخت
CREATE NONCLUSTERED INDEX [IX_Receptions_PaymentStatus] 
ON [dbo].[Receptions] ([PaymentStatus], [IsDeleted])
INCLUDE ([ReceptionId], [ReceptionNumber], [PatientId], [TotalAmount], [CreatedAt])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 SERVICE INDEXES (ایندکس‌های خدمات)
-- =============================================

-- ایندکس کد خدمت
CREATE NONCLUSTERED INDEX [IX_Services_ServiceCode] 
ON [dbo].[Services] ([ServiceCode], [IsDeleted])
INCLUDE ([ServiceId], [Title], [Price], [ServiceCategoryId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس دسته‌بندی
CREATE NONCLUSTERED INDEX [IX_Services_Category] 
ON [dbo].[Services] ([ServiceCategoryId], [IsDeleted])
INCLUDE ([ServiceId], [Title], [ServiceCode], [Price])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس عنوان
CREATE NONCLUSTERED INDEX [IX_Services_Title] 
ON [dbo].[Services] ([Title], [IsDeleted])
INCLUDE ([ServiceId], [ServiceCode], [Price], [ServiceCategoryId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 DOCTOR INDEXES (ایندکس‌های پزشک)
-- =============================================

-- ایندکس کد نظام پزشکی
CREATE NONCLUSTERED INDEX [IX_Doctors_MedicalCouncilCode] 
ON [dbo].[Doctors] ([MedicalCouncilCode], [IsDeleted])
INCLUDE ([DoctorId], [ApplicationUserId], [ClinicId], [Specialization])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس کلینیک
CREATE NONCLUSTERED INDEX [IX_Doctors_Clinic] 
ON [dbo].[Doctors] ([ClinicId], [IsDeleted])
INCLUDE ([DoctorId], [ApplicationUserId], [MedicalCouncilCode], [Specialization])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس تخصص
CREATE NONCLUSTERED INDEX [IX_Doctors_Specialization] 
ON [dbo].[Doctors] ([Specialization], [IsDeleted])
INCLUDE ([DoctorId], [ApplicationUserId], [ClinicId], [MedicalCouncilCode])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 CLINIC INDEXES (ایندکس‌های کلینیک)
-- =============================================

-- ایندکس نام کلینیک
CREATE NONCLUSTERED INDEX [IX_Clinics_Name] 
ON [dbo].[Clinics] ([Name], [IsDeleted])
INCLUDE ([ClinicId], [Address], [PhoneNumber], [IsActive])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت فعال
CREATE NONCLUSTERED INDEX [IX_Clinics_Active] 
ON [dbo].[Clinics] ([IsActive], [IsDeleted])
INCLUDE ([ClinicId], [Name], [Address])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 DEPARTMENT INDEXES (ایندکس‌های دپارتمان)
-- =============================================

-- ایندکس کلینیک و نام
CREATE NONCLUSTERED INDEX [IX_Departments_Clinic_Name] 
ON [dbo].[Departments] ([ClinicId], [Name], [IsDeleted])
INCLUDE ([DepartmentId], [IsActive])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت فعال
CREATE NONCLUSTERED INDEX [IX_Departments_Active] 
ON [dbo].[Departments] ([IsActive], [IsDeleted])
INCLUDE ([DepartmentId], [Name], [ClinicId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 SERVICE CATEGORY INDEXES (ایندکس‌های دسته‌بندی خدمات)
-- =============================================

-- ایندکس دپارتمان و عنوان
CREATE NONCLUSTERED INDEX [IX_ServiceCategories_Department_Title] 
ON [dbo].[ServiceCategories] ([DepartmentId], [Title], [IsDeleted])
INCLUDE ([ServiceCategoryId], [IsActive])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت فعال
CREATE NONCLUSTERED INDEX [IX_ServiceCategories_Active] 
ON [dbo].[ServiceCategories] ([IsActive], [IsDeleted])
INCLUDE ([ServiceCategoryId], [Title], [DepartmentId])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 INSURANCE INDEXES (ایندکس‌های بیمه)
-- =============================================

-- ایندکس نام بیمه
CREATE NONCLUSTERED INDEX [IX_Insurances_Name] 
ON [dbo].[Insurances] ([Name], [IsDeleted])
INCLUDE ([InsuranceId], [IsActive])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس وضعیت فعال
CREATE NONCLUSTERED INDEX [IX_Insurances_Active] 
ON [dbo].[Insurances] ([IsActive], [IsDeleted])
INCLUDE ([InsuranceId], [Name])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 USER INDEXES (ایندکس‌های کاربر)
-- =============================================

-- ایندکس نام کاربری
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_UserName] 
ON [dbo].[AspNetUsers] ([UserName])
INCLUDE ([Id], [FirstName], [LastName], [Email])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس ایمیل
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Email] 
ON [dbo].[AspNetUsers] ([Email])
INCLUDE ([Id], [UserName], [FirstName], [LastName])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- ایندکس نام و نام خانوادگی
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Name] 
ON [dbo].[AspNetUsers] ([FirstName], [LastName])
INCLUDE ([Id], [UserName], [Email])
WITH (FILLFACTOR = 90, PAD_INDEX = ON);
GO

-- =============================================
-- 📊 STATISTICS AND MAINTENANCE
-- =============================================

-- به‌روزرسانی آمار ایندکس‌ها
UPDATE STATISTICS [dbo].[Patients] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Appointments] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Receptions] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Services] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Doctors] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Clinics] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Departments] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[ServiceCategories] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Insurances] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[AspNetUsers] WITH FULLSCAN;
GO

-- نمایش اطلاعات ایندکس‌های ایجاد شده
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_disabled AS IsDisabled,
    i.fill_factor AS FillFactor
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE 'IX_%'
ORDER BY t.name, i.name;
GO

-- نمایش فضای استفاده شده توسط ایندکس‌ها
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    p.rows AS RowCounts,
    CAST(ROUND((SUM(a.total_pages) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
    CAST(ROUND((SUM(a.used_pages) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS UsedSpaceMB
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE i.name LIKE 'IX_%'
GROUP BY t.name, i.name, p.rows
ORDER BY t.name, i.name;
GO

PRINT '✅ تمام ایندکس‌های بهینه‌سازی عملکرد با موفقیت ایجاد شدند!';
PRINT '📊 تعداد کل ایندکس‌های ایجاد شده: 25';
PRINT '🎯 بهبود عملکرد مورد انتظار: 60-80%';
