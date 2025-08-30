-- =============================================
-- Database Performance Optimization Scripts
-- Doctor Assignment Management System
-- =============================================

-- Indexes for Doctor table
CREATE NONCLUSTERED INDEX [IX_Doctor_IsActive_IsDeleted] ON [dbo].[Doctors]
(
    [IsActive] ASC,
    [IsDeleted] ASC
)
INCLUDE ([DoctorId], [FirstName], [LastName], [NationalCode], [Education], [CreatedAt], [UpdatedAt], [UpdatedByUserId]);

CREATE NONCLUSTERED INDEX [IX_Doctor_NationalCode] ON [dbo].[Doctors]
(
    [NationalCode] ASC
)
WHERE ([IsDeleted] = 0);

CREATE NONCLUSTERED INDEX [IX_Doctor_Name_Search] ON [dbo].[Doctors]
(
    [FirstName] ASC,
    [LastName] ASC
)
INCLUDE ([DoctorId], [NationalCode], [IsActive])
WHERE ([IsDeleted] = 0);

-- Indexes for DoctorDepartment table
CREATE NONCLUSTERED INDEX [IX_DoctorDepartment_DoctorId_IsActive] ON [dbo].[DoctorDepartments]
(
    [DoctorId] ASC,
    [IsActive] ASC
)
INCLUDE ([DepartmentId], [Role], [StartDate], [CreatedAt]);

CREATE NONCLUSTERED INDEX [IX_DoctorDepartment_DepartmentId_IsActive] ON [dbo].[DoctorDepartments]
(
    [DepartmentId] ASC,
    [IsActive] ASC
)
INCLUDE ([DoctorId], [Role], [StartDate]);

-- Indexes for DoctorServiceCategory table
CREATE NONCLUSTERED INDEX [IX_DoctorServiceCategory_DoctorId_IsActive] ON [dbo].[DoctorServiceCategories]
(
    [DoctorId] ASC,
    [IsActive] ASC
)
INCLUDE ([ServiceCategoryId], [AuthorizationLevel], [GrantedDate], [CertificateNumber]);

CREATE NONCLUSTERED INDEX [IX_DoctorServiceCategory_ServiceCategoryId_IsActive] ON [dbo].[DoctorServiceCategories]
(
    [ServiceCategoryId] ASC,
    [IsActive] ASC
)
INCLUDE ([DoctorId], [AuthorizationLevel]);

-- Indexes for Department table
CREATE NONCLUSTERED INDEX [IX_Department_IsActive_IsDeleted] ON [dbo].[Departments]
(
    [IsActive] ASC,
    [IsDeleted] ASC
)
INCLUDE ([DepartmentId], [Name], [ClinicId]);

CREATE NONCLUSTERED INDEX [IX_Department_Name] ON [dbo].[Departments]
(
    [Name] ASC
)
WHERE ([IsDeleted] = 0);

-- Indexes for ServiceCategory table
CREATE NONCLUSTERED INDEX [IX_ServiceCategory_IsActive_IsDeleted] ON [dbo].[ServiceCategories]
(
    [IsActive] ASC,
    [IsDeleted] ASC
)
INCLUDE ([ServiceCategoryId], [Title], [DepartmentId]);

CREATE NONCLUSTERED INDEX [IX_ServiceCategory_DepartmentId_IsActive] ON [dbo].[ServiceCategories]
(
    [DepartmentId] ASC,
    [IsActive] ASC
)
INCLUDE ([ServiceCategoryId], [Title])
WHERE ([IsDeleted] = 0);

-- Composite indexes for complex queries
CREATE NONCLUSTERED INDEX [IX_Doctor_Assignment_Stats] ON [dbo].[Doctors]
(
    [IsActive] ASC,
    [IsDeleted] ASC,
    [CreatedAt] ASC
)
INCLUDE ([DoctorId], [FirstName], [LastName], [NationalCode]);

-- Index for audit trail queries
CREATE NONCLUSTERED INDEX [IX_Doctor_Audit_Trail] ON [dbo].[Doctors]
(
    [UpdatedAt] ASC,
    [UpdatedByUserId] ASC
)
INCLUDE ([DoctorId], [FirstName], [LastName])
WHERE ([IsDeleted] = 0);

-- Statistics for query optimization
UPDATE STATISTICS [dbo].[Doctors] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[DoctorDepartments] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[DoctorServiceCategories] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Departments] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[ServiceCategories] WITH FULLSCAN;
