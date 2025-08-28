namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreateDateBase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        AppointmentId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        PatientId = c.Int(),
                        AppointmentDate = c.DateTime(nullable: false),
                        Status = c.Byte(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentTransactionId = c.Int(),
                        Description = c.String(maxLength: 500),
                        IsNewPatient = c.Boolean(nullable: false),
                        PatientName = c.String(maxLength: 200),
                        PatientPhone = c.String(maxLength: 20),
                        ServiceCategoryId = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Appointment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.PaymentTransactions", t => t.PaymentTransactionId)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.DoctorId, name: "IX_Appointment_DoctorId")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.Status }, name: "IX_Appointment_DoctorId_Date_Status")
                .Index(t => t.PatientId, name: "IX_Appointment_PatientId")
                .Index(t => new { t.PatientId, t.Status, t.AppointmentDate }, name: "IX_Appointment_PatientId_Status_Date")
                .Index(t => t.AppointmentDate, name: "IX_Appointment_AppointmentDate")
                .Index(t => t.Status, name: "IX_Appointment_Status")
                .Index(t => t.PaymentTransactionId, name: "IX_Appointment_PaymentTransactionId")
                .Index(t => t.ServiceCategoryId)
                .Index(t => t.IsDeleted, name: "IX_Appointment_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Appointment_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Appointment_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Appointment_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Appointment_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Appointment_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Appointment_UpdatedByUserId");
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        NationalCode = c.String(nullable: false, maxLength: 10),
                        PhoneNumber = c.String(nullable: false, maxLength: 20),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        LastLoginDate = c.DateTime(),
                        Gender = c.Byte(nullable: false),
                        Address = c.String(maxLength: 500),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ApplicationUser_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.FirstName, name: "IX_ApplicationUser_FirstName")
                .Index(t => t.LastName, name: "IX_ApplicationUser_LastName")
                .Index(t => t.NationalCode, unique: true)
                .Index(t => t.PhoneNumber, unique: true, name: "IX_ApplicationUser_PhoneNumber")
                .Index(t => t.IsActive, name: "IX_ApplicationUser_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted }, name: "IX_ApplicationUser_IsActive_IsDeleted")
                .Index(t => t.IsDeleted, name: "IX_ApplicationUser_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ApplicationUser_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ApplicationUser_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ApplicationUser_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ApplicationUser_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ApplicationUser_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ApplicationUser_UpdatedByUserId")
                .Index(t => t.Gender, name: "IX_Patient_Gender")
                .Index(t => t.Email, name: "IX_ApplicationUser_Email")
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Doctors",
                c => new
                    {
                        DoctorId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        Degree = c.Byte(nullable: false),
                        GraduationYear = c.Int(),
                        University = c.String(maxLength: 200),
                        Gender = c.Byte(nullable: false),
                        DateOfBirth = c.DateTime(),
                        HomeAddress = c.String(maxLength: 500),
                        OfficeAddress = c.String(maxLength: 500),
                        ExperienceYears = c.Int(),
                        ProfileImageUrl = c.String(maxLength: 500),
                        PhoneNumber = c.String(maxLength: 50),
                        NationalCode = c.String(maxLength: 10),
                        MedicalCouncilCode = c.String(maxLength: 20),
                        Email = c.String(maxLength: 100),
                        ClinicId = c.Int(),
                        Bio = c.String(maxLength: 2000),
                        Address = c.String(maxLength: 500),
                        EmergencyContact = c.String(maxLength: 50),
                        SecurityLevel = c.String(maxLength: 20),
                        Education = c.String(maxLength: 100),
                        LicenseNumber = c.String(maxLength: 50),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Doctor_ActiveDoctors", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Doctor_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .ForeignKey("dbo.Clinics", t => t.ClinicId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.FirstName, name: "IX_Doctor_FirstName")
                .Index(t => new { t.LastName, t.FirstName }, name: "IX_Doctor_LastName_FirstName")
                .Index(t => t.LastName, name: "IX_Doctor_LastName")
                .Index(t => t.IsActive, name: "IX_Doctor_IsActive")
                .Index(t => new { t.ClinicId, t.IsActive, t.IsDeleted }, name: "IX_Doctor_ClinicId_IsActive_IsDeleted")
                .Index(t => new { t.University, t.IsActive, t.IsDeleted }, name: "IX_Doctor_University_IsActive_IsDeleted")
                .Index(t => t.Degree, name: "IX_Doctor_Degree")
                .Index(t => t.GraduationYear, name: "IX_Doctor_GraduationYear")
                .Index(t => t.University, name: "IX_Doctor_University")
                .Index(t => t.Gender, name: "IX_Doctor_Gender")
                .Index(t => t.DateOfBirth, name: "IX_Doctor_DateOfBirth")
                .Index(t => t.ExperienceYears, name: "IX_Doctor_ExperienceYears")
                .Index(t => t.PhoneNumber, name: "IX_Doctor_PhoneNumber")
                .Index(t => t.NationalCode, name: "IX_Doctor_NationalCode")
                .Index(t => t.MedicalCouncilCode, name: "IX_Doctor_MedicalCouncilCode")
                .Index(t => t.Email, name: "IX_Doctor_Email")
                .Index(t => t.ClinicId, name: "IX_Doctor_ClinicId")
                .Index(t => t.IsDeleted, name: "IX_Doctor_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Doctor_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Doctor_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Doctor_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Doctor_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Doctor_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Doctor_UpdatedByUserId")
                .Index(t => t.ApplicationUserId, name: "IX_Doctor_ApplicationUserId");
            
            CreateTable(
                "dbo.Clinics",
                c => new
                    {
                        ClinicId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Address = c.String(maxLength: 500),
                        PhoneNumber = c.String(maxLength: 50),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Clinic_ActiveClinics", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Clinic_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ClinicId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_Clinic_Name")
                .Index(t => t.PhoneNumber, name: "IX_Clinic_PhoneNumber")
                .Index(t => t.IsDeleted, name: "IX_Clinic_IsDeleted")
                .Index(t => new { t.IsDeleted, t.CreatedAt }, name: "IX_Clinic_IsDeleted_CreatedAt")
                .Index(t => t.DeletedAt, name: "IX_Clinic_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Clinic_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Clinic_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Clinic_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Clinic_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Clinic_UpdatedByUserId");
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        ClinicId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        Description = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Department_ActiveDepartments", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Department_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Clinics", t => t.ClinicId)
                .Index(t => t.Name, name: "IX_Department_Name")
                .Index(t => t.ClinicId, name: "IX_Department_ClinicId")
                .Index(t => new { t.ClinicId, t.IsDeleted }, name: "IX_Department_ClinicId_IsDeleted")
                .Index(t => new { t.ClinicId, t.IsActive, t.IsDeleted }, name: "IX_Department_ClinicId_IsActive_IsDeleted")
                .Index(t => t.IsActive, name: "IX_Department_IsActive")
                .Index(t => t.IsDeleted, name: "IX_Department_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Department_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Department_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Department_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Department_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Department_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Department_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorDepartments",
                c => new
                    {
                        DoctorId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        Role = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.DoctorId, t.DepartmentId })
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.DoctorId, name: "IX_DoctorDepartment_DoctorId")
                .Index(t => new { t.DoctorId, t.IsActive }, name: "IX_DoctorDepartment_DoctorId_IsActive")
                .Index(t => new { t.DoctorId, t.DepartmentId, t.IsActive }, name: "IX_DoctorDepartment_DoctorId_DepartmentId_IsActive")
                .Index(t => t.DepartmentId, name: "IX_DoctorDepartment_DepartmentId")
                .Index(t => new { t.DepartmentId, t.IsActive }, name: "IX_DoctorDepartment_DepartmentId_IsActive")
                .Index(t => t.Role, name: "IX_DoctorDepartment_Role")
                .Index(t => t.IsActive, name: "IX_DoctorDepartment_IsActive")
                .Index(t => t.StartDate, name: "IX_DoctorDepartment_StartDate")
                .Index(t => new { t.StartDate, t.EndDate }, name: "IX_DoctorDepartment_StartDate_EndDate")
                .Index(t => t.EndDate, name: "IX_DoctorDepartment_EndDate")
                .Index(t => t.CreatedAt, name: "IX_DoctorDepartment_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorDepartment_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorDepartment_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorDepartment_UpdatedByUserId")
                .Index(t => t.DeletedByUserId);
            
            CreateTable(
                "dbo.ServiceCategories",
                c => new
                    {
                        ServiceCategoryId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        DepartmentId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        Description = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceCategory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.Title, name: "IX_ServiceCategory_Title")
                .Index(t => t.DepartmentId, name: "IX_ServiceCategory_DepartmentId")
                .Index(t => new { t.DepartmentId, t.IsActive, t.IsDeleted }, name: "IX_ServiceCategory_DepartmentId_IsActive_IsDeleted")
                .Index(t => t.IsActive, name: "IX_ServiceCategory_IsActive")
                .Index(t => t.IsDeleted, name: "IX_ServiceCategory_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ServiceCategory_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ServiceCategory_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ServiceCategory_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ServiceCategory_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ServiceCategory_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ServiceCategory_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorServiceCategories",
                c => new
                    {
                        DoctorId = c.Int(nullable: false),
                        ServiceCategoryId = c.Int(nullable: false),
                        AuthorizationLevel = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        GrantedDate = c.DateTime(),
                        ExpiryDate = c.DateTime(),
                        CertificateNumber = c.String(maxLength: 100),
                        Notes = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.DoctorId, t.ServiceCategoryId })
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .Index(t => t.DoctorId, name: "IX_DoctorServiceCategory_DoctorId")
                .Index(t => new { t.DoctorId, t.IsActive }, name: "IX_DoctorServiceCategory_DoctorId_IsActive")
                .Index(t => new { t.DoctorId, t.ServiceCategoryId, t.IsActive }, name: "IX_DoctorServiceCategory_DoctorId_ServiceCategoryId_IsActive")
                .Index(t => t.ServiceCategoryId, name: "IX_DoctorServiceCategory_ServiceCategoryId")
                .Index(t => new { t.ServiceCategoryId, t.IsActive }, name: "IX_DoctorServiceCategory_ServiceCategoryId_IsActive")
                .Index(t => t.AuthorizationLevel, name: "IX_DoctorServiceCategory_AuthorizationLevel")
                .Index(t => new { t.AuthorizationLevel, t.IsActive }, name: "IX_DoctorServiceCategory_AuthorizationLevel_IsActive")
                .Index(t => t.IsActive, name: "IX_DoctorServiceCategory_IsActive")
                .Index(t => new { t.ExpiryDate, t.IsActive }, name: "IX_DoctorServiceCategory_ExpiryDate_IsActive")
                .Index(t => t.GrantedDate, name: "IX_DoctorServiceCategory_GrantedDate")
                .Index(t => t.ExpiryDate, name: "IX_DoctorServiceCategory_ExpiryDate")
                .Index(t => t.CertificateNumber, name: "IX_DoctorServiceCategory_CertificateNumber")
                .Index(t => t.CreatedAt, name: "IX_DoctorServiceCategory_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorServiceCategory_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorServiceCategory_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorServiceCategory_UpdatedByUserId");
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        ServiceId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 250),
                        ServiceCode = c.String(nullable: false, maxLength: 50),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 1000),
                        ServiceCategoryId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                        Notes = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Service_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_Service_Title")
                .Index(t => t.ServiceCode, unique: true, name: "IX_Service_ServiceCode")
                .Index(t => t.Price, name: "IX_Service_Price")
                .Index(t => t.ServiceCategoryId, name: "IX_Service_ServiceCategoryId")
                .Index(t => new { t.ServiceCategoryId, t.IsDeleted }, name: "IX_Service_ServiceCategoryId_IsDeleted")
                .Index(t => t.IsDeleted, name: "IX_Service_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Service_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Service_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Service_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Service_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Service_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Service_UpdatedByUserId");
            
            CreateTable(
                "dbo.ReceptionItems",
                c => new
                    {
                        ReceptionItemId = c.Int(nullable: false, identity: true),
                        ReceptionId = c.Int(nullable: false),
                        ServiceId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PatientShareAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InsurerShareAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceptionItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ReceptionItemId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ReceptionId, name: "IX_ReceptionItem_ReceptionId")
                .Index(t => new { t.ReceptionId, t.ServiceId }, name: "IX_ReceptionItem_ReceptionId_ServiceId")
                .Index(t => t.ServiceId, name: "IX_ReceptionItem_ServiceId")
                .Index(t => new { t.ServiceId, t.CreatedAt }, name: "IX_ReceptionItem_ServiceId_CreatedAt")
                .Index(t => t.Quantity, name: "IX_ReceptionItem_Quantity")
                .Index(t => t.UnitPrice, name: "IX_ReceptionItem_UnitPrice")
                .Index(t => t.PatientShareAmount, name: "IX_ReceptionItem_PatientShareAmount")
                .Index(t => t.InsurerShareAmount, name: "IX_ReceptionItem_InsurerShareAmount")
                .Index(t => t.IsDeleted, name: "IX_ReceptionItem_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ReceptionItem_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ReceptionItem_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ReceptionItem_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ReceptionItem_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ReceptionItem_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ReceptionItem_UpdatedByUserId");
            
            CreateTable(
                "dbo.Receptions",
                c => new
                    {
                        ReceptionId = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                        InsuranceId = c.Int(nullable: false),
                        ReceptionDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PatientCoPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InsurerShareAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Byte(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Reception_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ReceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .Index(t => t.PatientId, name: "IX_Reception_PatientId")
                .Index(t => new { t.PatientId, t.ReceptionDate, t.Status }, name: "IX_Reception_PatientId_Date_Status")
                .Index(t => t.DoctorId, name: "IX_Reception_DoctorId")
                .Index(t => new { t.DoctorId, t.ReceptionDate, t.Status }, name: "IX_Reception_DoctorId_Date_Status")
                .Index(t => t.InsuranceId, name: "IX_Reception_InsuranceId")
                .Index(t => new { t.InsuranceId, t.ReceptionDate, t.Status }, name: "IX_Reception_InsuranceId_Date_Status")
                .Index(t => t.ReceptionDate, name: "IX_Reception_ReceptionDate")
                .Index(t => t.Status, name: "IX_Reception_Status")
                .Index(t => t.IsDeleted, name: "IX_Reception_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Reception_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Reception_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Reception_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Reception_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Reception_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Reception_UpdatedByUserId");
            
            CreateTable(
                "dbo.Insurances",
                c => new
                    {
                        InsuranceId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 250),
                        Description = c.String(maxLength: 1000),
                        DefaultPatientShare = c.Decimal(nullable: false, precision: 5, scale: 2),
                        DefaultInsurerShare = c.Decimal(nullable: false, precision: 5, scale: 2),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(nullable: false, maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Insurance_ActiveInsurances", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Insurance_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.InsuranceId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_Insurance_Name")
                .Index(t => t.IsActive, name: "IX_Insurance_IsActive")
                .Index(t => new { t.IsActive, t.IsDeleted }, name: "IX_Insurance_IsActive_IsDeleted")
                .Index(t => t.IsDeleted, name: "IX_Insurance_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Insurance_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Insurance_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Insurance_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Insurance_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Insurance_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Insurance_UpdatedByUserId");
            
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        NationalCode = c.String(nullable: false, maxLength: 10),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        BirthDate = c.DateTime(),
                        Address = c.String(maxLength: 500),
                        PhoneNumber = c.String(maxLength: 50),
                        Gender = c.Byte(nullable: false),
                        InsuranceId = c.Int(nullable: false),
                        LastLoginDate = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Patient_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PatientId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .Index(t => t.NationalCode, unique: true, name: "IX_Patient_NationalCode")
                .Index(t => t.FirstName, name: "IX_Patient_FirstName")
                .Index(t => new { t.LastName, t.FirstName }, name: "IX_Patient_LastName_FirstName")
                .Index(t => t.LastName, name: "IX_Patient_LastName")
                .Index(t => t.BirthDate, name: "IX_Patient_BirthDate")
                .Index(t => t.PhoneNumber, name: "IX_Patient_PhoneNumber")
                .Index(t => t.InsuranceId, name: "IX_Patient_InsuranceId")
                .Index(t => new { t.InsuranceId, t.LastLoginDate }, name: "IX_Patient_InsuranceId_LastLoginDate")
                .Index(t => t.LastLoginDate, name: "IX_Patient_LastLoginDate")
                .Index(t => t.IsDeleted, name: "IX_Patient_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Patient_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Patient_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Patient_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Patient_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Patient_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Patient_UpdatedByUserId")
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.InsuranceTariffs",
                c => new
                    {
                        InsuranceTariffId = c.Int(nullable: false, identity: true),
                        InsuranceId = c.Int(nullable: false),
                        ServiceId = c.Int(nullable: false),
                        TariffPrice = c.Decimal(precision: 18, scale: 2),
                        PatientShare = c.Decimal(precision: 5, scale: 2),
                        InsurerShare = c.Decimal(precision: 5, scale: 2),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_InsuranceTariff_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.InsuranceTariffId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .Index(t => t.InsuranceId, name: "IX_InsuranceTariff_InsuranceId")
                .Index(t => new { t.InsuranceId, t.ServiceId }, unique: true, name: "IX_InsuranceTariff_Insurance_Service_Unique")
                .Index(t => t.ServiceId, name: "IX_InsuranceTariff_ServiceId")
                .Index(t => t.TariffPrice, name: "IX_InsuranceTariff_TariffPrice")
                .Index(t => t.PatientShare, name: "IX_InsuranceTariff_PatientShare")
                .Index(t => t.InsurerShare, name: "IX_InsuranceTariff_InsurerShare")
                .Index(t => t.IsDeleted, name: "IX_InsuranceTariff_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_InsuranceTariff_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_InsuranceTariff_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_InsuranceTariff_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_InsuranceTariff_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_InsuranceTariff_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_InsuranceTariff_UpdatedByUserId");
            
            CreateTable(
                "dbo.ReceiptPrints",
                c => new
                    {
                        ReceiptPrintId = c.Int(nullable: false, identity: true),
                        ReceptionId = c.Int(nullable: false),
                        ReceiptContent = c.String(nullable: false, maxLength: 1000),
                        ReceiptHash = c.String(nullable: false, maxLength: 64),
                        PrintDate = c.DateTime(nullable: false),
                        PrintedBy = c.String(maxLength: 250),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        PrintedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceiptPrint_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ReceiptPrintId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.PrintedByUserId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.ReceptionId, t.PrintDate }, name: "IX_ReceiptPrint_ReceptionId_PrintDate")
                .Index(t => t.ReceiptContent, name: "IX_ReceiptPrint_ReceiptContent")
                .Index(t => t.ReceiptHash, unique: true, name: "IX_ReceiptPrint_ReceiptHash")
                .Index(t => t.PrintDate, name: "IX_ReceiptPrint_PrintDate")
                .Index(t => new { t.PrintDate, t.IsDeleted }, name: "IX_ReceiptPrint_PrintDate_IsDeleted")
                .Index(t => t.PrintedBy, name: "IX_ReceiptPrint_PrintedBy")
                .Index(t => t.IsDeleted, name: "IX_ReceiptPrint_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_ReceiptPrint_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_ReceiptPrint_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_ReceiptPrint_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_ReceiptPrint_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_ReceiptPrint_CreatedByUserId")
                .Index(t => t.PrintedByUserId)
                .Index(t => t.UpdatedAt, name: "IX_ReceiptPrint_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_ReceiptPrint_UpdatedByUserId");
            
            CreateTable(
                "dbo.PaymentTransactions",
                c => new
                    {
                        PaymentTransactionId = c.Int(nullable: false, identity: true),
                        ReceptionId = c.Int(nullable: false),
                        PosTerminalId = c.Int(),
                        CashSessionId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Method = c.Byte(nullable: false),
                        Status = c.Int(nullable: false),
                        TransactionId = c.String(maxLength: 100),
                        ReferenceCode = c.String(maxLength: 100),
                        ReceiptNo = c.String(maxLength: 50),
                        Description = c.String(maxLength: 500),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PaymentTransaction_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PaymentTransactionId)
                .ForeignKey("dbo.CashSessions", t => t.CashSessionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.PosTerminals", t => t.PosTerminalId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ReceptionId, name: "IX_PaymentTransaction_ReceptionId")
                .Index(t => new { t.ReceptionId, t.Status }, name: "IX_PaymentTransaction_ReceptionId_Status")
                .Index(t => t.PosTerminalId, name: "IX_PaymentTransaction_PosTerminalId")
                .Index(t => t.CashSessionId, name: "IX_PaymentTransaction_CashSessionId")
                .Index(t => new { t.CashSessionId, t.Status, t.CreatedAt }, name: "IX_PaymentTransaction_CashSessionId_Status_CreatedAt")
                .Index(t => t.Amount, name: "IX_PaymentTransaction_Amount")
                .Index(t => t.Method, name: "IX_PaymentTransaction_Method")
                .Index(t => t.Status, name: "IX_PaymentTransaction_Status")
                .Index(t => t.TransactionId, name: "IX_PaymentTransaction_TransactionId")
                .Index(t => t.ReferenceCode, name: "IX_PaymentTransaction_ReferenceCode")
                .Index(t => t.ReceiptNo, name: "IX_PaymentTransaction_ReceiptNo")
                .Index(t => t.IsDeleted, name: "IX_PaymentTransaction_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_PaymentTransaction_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_PaymentTransaction_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_PaymentTransaction_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_PaymentTransaction_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PaymentTransaction_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_PaymentTransaction_UpdatedByUserId");
            
            CreateTable(
                "dbo.CashSessions",
                c => new
                    {
                        CashSessionId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        OpenedAt = c.DateTime(nullable: false),
                        ClosedAt = c.DateTime(),
                        OpeningBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CashBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PosBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_CashSession_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.CashSessionId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId, name: "IX_CashSession_UserId")
                .Index(t => new { t.UserId, t.Status, t.OpenedAt }, name: "IX_CashSession_UserId_Status_OpenedAt")
                .Index(t => t.OpenedAt, name: "IX_CashSession_OpenedAt")
                .Index(t => t.ClosedAt, name: "IX_CashSession_ClosedAt")
                .Index(t => t.Status, name: "IX_CashSession_Status")
                .Index(t => t.IsDeleted, name: "IX_CashSession_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_CashSession_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_CashSession_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_CashSession_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_CashSession_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_CashSession_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_CashSession_UpdatedByUserId");
            
            CreateTable(
                "dbo.PosTerminals",
                c => new
                    {
                        PosTerminalId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        TerminalId = c.String(nullable: false, maxLength: 50),
                        MerchantId = c.String(nullable: false, maxLength: 50),
                        SerialNumber = c.String(maxLength: 100),
                        IpAddress = c.String(maxLength: 50),
                        MacAddress = c.String(maxLength: 50),
                        Provider = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Protocol = c.Int(nullable: false),
                        Port = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PosTerminal_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PosTerminalId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Title, name: "IX_PosTerminal_Title")
                .Index(t => t.TerminalId, name: "IX_PosTerminal_TerminalId")
                .Index(t => t.MerchantId, name: "IX_PosTerminal_MerchantId")
                .Index(t => t.Provider, name: "IX_PosTerminal_Provider")
                .Index(t => new { t.IsActive, t.Provider }, name: "IX_PosTerminal_IsActive_Provider")
                .Index(t => t.IsActive, name: "IX_PosTerminal_IsActive")
                .Index(t => t.IsDeleted, name: "IX_PosTerminal_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_PosTerminal_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_PosTerminal_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_PosTerminal_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_PosTerminal_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PosTerminal_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_PosTerminal_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorSpecializations",
                c => new
                    {
                        SpecializationId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                        Specialization_SpecializationId = c.Int(),
                        Doctor_DoctorId = c.Int(),
                    })
                .PrimaryKey(t => new { t.SpecializationId, t.DoctorId })
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.Specializations", t => t.Specialization_SpecializationId)
                .ForeignKey("dbo.Specializations", t => t.SpecializationId)
                .ForeignKey("dbo.Doctors", t => t.Doctor_DoctorId)
                .Index(t => t.SpecializationId)
                .Index(t => t.DoctorId)
                .Index(t => t.Specialization_SpecializationId)
                .Index(t => t.Doctor_DoctorId);
            
            CreateTable(
                "dbo.Specializations",
                c => new
                    {
                        SpecializationId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Specialization_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.SpecializationId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.Name, name: "IX_Specialization_Name")
                .Index(t => t.IsActive, name: "IX_Specialization_IsActive")
                .Index(t => t.DisplayOrder, name: "IX_Specialization_DisplayOrder")
                .Index(t => t.IsDeleted, name: "IX_Specialization_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_Specialization_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_Specialization_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_Specialization_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_Specialization_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Specialization_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_Specialization_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorSchedules",
                c => new
                    {
                        ScheduleId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        AppointmentDuration = c.Int(nullable: false),
                        DefaultStartTime = c.Time(precision: 7),
                        DefaultEndTime = c.Time(precision: 7),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorSchedule_ActiveDoctorSchedules", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorSchedule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ScheduleId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ScheduleId, name: "IX_DoctorSchedule_ScheduleId")
                .Index(t => t.DoctorId, name: "IX_DoctorSchedule_DoctorId")
                .Index(t => new { t.DoctorId, t.IsActive }, name: "IX_DoctorSchedule_DoctorId_IsActive")
                .Index(t => new { t.DoctorId, t.IsDeleted }, name: "IX_DoctorSchedule_DoctorId_IsDeleted")
                .Index(t => new { t.DoctorId, t.IsActive, t.IsDeleted }, unique: true, name: "IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique")
                .Index(t => t.AppointmentDuration, name: "IX_DoctorSchedule_AppointmentDuration")
                .Index(t => t.IsActive, name: "IX_DoctorSchedule_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorSchedule_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorSchedule_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorSchedule_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorSchedule_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorSchedule_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorSchedule_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorSchedule_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorSchedule_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorWorkDays",
                c => new
                    {
                        WorkDayId = c.Int(nullable: false, identity: true),
                        ScheduleId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorWorkDay_ActiveDoctorWorkDays", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorWorkDay_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.WorkDayId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorSchedules", t => t.ScheduleId, cascadeDelete: true)
                .Index(t => t.WorkDayId, name: "IX_DoctorWorkDay_WorkDayId")
                .Index(t => t.ScheduleId, name: "IX_DoctorWorkDay_ScheduleId")
                .Index(t => new { t.ScheduleId, t.DayOfWeek }, name: "IX_DoctorWorkDay_ScheduleId_DayOfWeek")
                .Index(t => new { t.ScheduleId, t.IsActive }, name: "IX_DoctorWorkDay_ScheduleId_IsActive")
                .Index(t => new { t.ScheduleId, t.IsDeleted }, name: "IX_DoctorWorkDay_ScheduleId_IsDeleted")
                .Index(t => new { t.ScheduleId, t.DayOfWeek, t.IsDeleted }, unique: true, name: "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique")
                .Index(t => t.DayOfWeek, name: "IX_DoctorWorkDay_DayOfWeek")
                .Index(t => t.IsActive, name: "IX_DoctorWorkDay_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorWorkDay_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorWorkDay_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorWorkDay_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorWorkDay_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorWorkDay_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorWorkDay_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorWorkDay_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorWorkDay_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorTimeRanges",
                c => new
                    {
                        TimeRangeId = c.Int(nullable: false, identity: true),
                        WorkDayId = c.Int(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        IsActive = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeRange_ActiveDoctorTimeRanges", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorTimeRange_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TimeRangeId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.DoctorWorkDays", t => t.WorkDayId, cascadeDelete: true)
                .Index(t => t.TimeRangeId, name: "IX_DoctorTimeRange_TimeRangeId")
                .Index(t => t.WorkDayId, name: "IX_DoctorTimeRange_WorkDayId")
                .Index(t => new { t.WorkDayId, t.StartTime }, name: "IX_DoctorTimeRange_WorkDayId_StartTime")
                .Index(t => new { t.WorkDayId, t.IsActive }, name: "IX_DoctorTimeRange_WorkDayId_IsActive")
                .Index(t => new { t.WorkDayId, t.IsDeleted }, name: "IX_DoctorTimeRange_WorkDayId_IsDeleted")
                .Index(t => new { t.WorkDayId, t.StartTime, t.EndTime }, name: "IX_DoctorTimeRange_WorkDayId_StartTime_EndTime")
                .Index(t => t.StartTime, name: "IX_DoctorTimeRange_StartTime")
                .Index(t => t.EndTime, name: "IX_DoctorTimeRange_EndTime")
                .Index(t => t.IsActive, name: "IX_DoctorTimeRange_IsActive")
                .Index(t => t.IsDeleted, name: "IX_DoctorTimeRange_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorTimeRange_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorTimeRange_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorTimeRange_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorTimeRange_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorTimeRange_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorTimeRange_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorTimeRange_UpdatedByUserId");
            
            CreateTable(
                "dbo.DoctorTimeSlots",
                c => new
                    {
                        TimeSlotId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Duration = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        AppointmentId = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.TimeSlotId)
                .ForeignKey("dbo.Appointments", t => t.AppointmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.TimeSlotId, name: "IX_DoctorTimeSlot_TimeSlotId")
                .Index(t => t.DoctorId, name: "IX_DoctorTimeSlot_DoctorId")
                .Index(t => new { t.DoctorId, t.AppointmentDate }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.Status }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status")
                .Index(t => new { t.DoctorId, t.IsDeleted }, name: "IX_DoctorTimeSlot_DoctorId_IsDeleted")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.StartTime, t.Status }, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status")
                .Index(t => new { t.DoctorId, t.AppointmentDate, t.StartTime, t.EndTime, t.IsDeleted }, unique: true, name: "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique")
                .Index(t => t.AppointmentDate, name: "IX_DoctorTimeSlot_AppointmentDate")
                .Index(t => new { t.AppointmentDate, t.Status, t.IsDeleted }, name: "IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted")
                .Index(t => t.StartTime, name: "IX_DoctorTimeSlot_StartTime")
                .Index(t => t.EndTime, name: "IX_DoctorTimeSlot_EndTime")
                .Index(t => t.Duration, name: "IX_DoctorTimeSlot_Duration")
                .Index(t => t.Status, name: "IX_DoctorTimeSlot_Status")
                .Index(t => t.AppointmentId, name: "IX_DoctorTimeSlot_AppointmentId")
                .Index(t => t.IsDeleted, name: "IX_DoctorTimeSlot_IsDeleted")
                .Index(t => new { t.CreatedAt, t.IsDeleted }, name: "IX_DoctorTimeSlot_CreatedAt_IsDeleted")
                .Index(t => t.DeletedAt, name: "IX_DoctorTimeSlot_DeletedAt")
                .Index(t => t.DeletedByUserId, name: "IX_DoctorTimeSlot_DeletedByUserId")
                .Index(t => t.CreatedAt, name: "IX_DoctorTimeSlot_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_DoctorTimeSlot_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_DoctorTimeSlot_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_DoctorTimeSlot_UpdatedByUserId");
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.NotificationHistories",
                c => new
                    {
                        HistoryId = c.Guid(nullable: false),
                        NotificationId = c.Guid(nullable: false),
                        ChannelType = c.Int(nullable: false),
                        Recipient = c.String(nullable: false, maxLength: 50),
                        Subject = c.String(maxLength: 200),
                        Message = c.String(nullable: false, maxLength: 1000),
                        SentAt = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusDescription = c.String(maxLength: 500),
                        SenderUserId = c.String(nullable: false, maxLength: 128),
                        RelatedEntityId = c.String(maxLength: 50),
                        RelatedEntityType = c.String(maxLength: 100),
                        AttemptCount = c.Int(nullable: false),
                        ExternalMessageId = c.String(maxLength: 100),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_NotificationHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.HistoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.SenderUserId)
                .Index(t => t.Recipient, name: "IX_NotificationHistory_Recipient")
                .Index(t => new { t.Recipient, t.Status, t.SentAt }, name: "IX_NotificationHistory_Recipient_Status_SentAt")
                .Index(t => t.SentAt, name: "IX_NotificationHistory_SentAt")
                .Index(t => new { t.SentAt, t.Status, t.IsDeleted }, name: "IX_NotificationHistory_SentAt_Status_IsDeleted")
                .Index(t => t.Status, name: "IX_NotificationHistory_Status")
                .Index(t => t.SenderUserId)
                .Index(t => t.RelatedEntityType, name: "IX_NotificationHistory_RelatedEntityType")
                .Index(t => t.IsDeleted, name: "IX_NotificationHistory_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId, name: "IX_NotificationHistory_CreatedByUserId")
                .Index(t => t.UpdatedByUserId, name: "IX_NotificationHistory_UpdatedByUserId");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.DatabaseVersion",
                c => new
                    {
                        VersionId = c.Int(nullable: false, identity: true),
                        VersionNumber = c.String(),
                        Description = c.String(),
                        AppliedDate = c.DateTime(nullable: false),
                        AppliedByUserId = c.String(),
                        MigrationScript = c.String(),
                    })
                .PrimaryKey(t => t.VersionId);
            
            CreateTable(
                "dbo.NotificationTemplates",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 50),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        ChannelType = c.Int(nullable: false),
                        PersianTemplate = c.String(nullable: false, maxLength: 1000),
                        EnglishTemplate = c.String(maxLength: 1000),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ChannelType, name: "IX_NotificationTemplate_ChannelType")
                .Index(t => t.IsActive, name: "IX_NotificationTemplate_IsActive")
                .Index(t => t.CreatedByUserId, name: "IX_NotificationTemplate_CreatedByUserId")
                .Index(t => t.UpdatedByUserId, name: "IX_NotificationTemplate_UpdatedByUserId");
            
            CreateTable(
                "dbo.OtpRequests",
                c => new
                    {
                        OtpRequestId = c.Int(nullable: false, identity: true),
                        PhoneNumber = c.String(nullable: false, maxLength: 20),
                        OtpCodeHash = c.String(nullable: false),
                        RequestTime = c.DateTime(nullable: false),
                        AttemptCount = c.Int(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.OtpRequestId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.PhoneNumber, name: "IX_OtpRequest_PhoneNumber")
                .Index(t => new { t.PhoneNumber, t.IsVerified, t.RequestTime }, name: "IX_OtpRequest_PhoneNumber_IsVerified_RequestTime")
                .Index(t => t.RequestTime, name: "IX_OtpRequest_RequestTime")
                .Index(t => t.IsVerified, name: "IX_OtpRequest_IsVerified")
                .Index(t => t.CreatedAt, name: "IX_OtpRequest_CreatedAt")
                .Index(t => t.CreatedByUserId, name: "IX_OtpRequest_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_OtpRequest_UpdatedAt")
                .Index(t => t.UpdatedByUserId, name: "IX_OtpRequest_UpdatedByUserId");
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.OtpRequests", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OtpRequests", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationTemplates", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationTemplates", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Appointments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Appointments", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.Appointments", "PaymentTransactionId", "dbo.PaymentTransactions");
            DropForeignKey("dbo.Appointments", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Appointments", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.Appointments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Appointments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationHistories", "SenderUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationHistories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationHistories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.NotificationHistories", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorTimeSlots", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeSlots", "AppointmentId", "dbo.Appointments");
            DropForeignKey("dbo.DoctorWorkDays", "ScheduleId", "dbo.DoctorSchedules");
            DropForeignKey("dbo.DoctorWorkDays", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "WorkDayId", "dbo.DoctorWorkDays");
            DropForeignKey("dbo.DoctorTimeRanges", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorTimeRanges", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorWorkDays", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorWorkDays", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorSchedules", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSchedules", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorSpecializations", "Doctor_DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorSpecializations", "SpecializationId", "dbo.Specializations");
            DropForeignKey("dbo.Specializations", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSpecializations", "Specialization_SpecializationId", "dbo.Specializations");
            DropForeignKey("dbo.Specializations", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Specializations", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorSpecializations", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorServiceCategories", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.Doctors", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Clinics", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "ClinicId", "dbo.Clinics");
            DropForeignKey("dbo.Departments", "ClinicId", "dbo.Clinics");
            DropForeignKey("dbo.Departments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceCategories", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.ServiceCategories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.ReceptionItems", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceptionItems", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ReceptionItems", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.Receptions", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactions", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactions", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.PaymentTransactions", "PosTerminalId", "dbo.PosTerminals");
            DropForeignKey("dbo.PosTerminals", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PosTerminals", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PosTerminals", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashSessions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashSessions", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactions", "CashSessionId", "dbo.CashSessions");
            DropForeignKey("dbo.CashSessions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CashSessions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceiptPrints", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceiptPrints", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.ReceiptPrints", "PrintedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceiptPrints", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceiptPrints", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Insurances", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceTariffs", "InsuranceId", "dbo.Insurances");
            DropForeignKey("dbo.InsuranceTariffs", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceTariffs", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.InsuranceTariffs", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceTariffs", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "InsuranceId", "dbo.Insurances");
            DropForeignKey("dbo.Patients", "InsuranceId", "dbo.Insurances");
            DropForeignKey("dbo.Patients", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Patients", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Patients", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Patients", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Insurances", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Insurances", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceptionItems", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceptionItems", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorServiceCategories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorServiceCategories", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.DoctorServiceCategories", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceCategories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceCategories", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorDepartments", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.DoctorDepartments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorDepartments", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.DoctorDepartments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DoctorDepartments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Departments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Departments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Clinics", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Clinics", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_UpdatedByUserId");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_UpdatedAt");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_CreatedByUserId");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_CreatedAt");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_IsVerified");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_RequestTime");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_PhoneNumber_IsVerified_RequestTime");
            DropIndex("dbo.OtpRequests", "IX_OtpRequest_PhoneNumber");
            DropIndex("dbo.NotificationTemplates", "IX_NotificationTemplate_UpdatedByUserId");
            DropIndex("dbo.NotificationTemplates", "IX_NotificationTemplate_CreatedByUserId");
            DropIndex("dbo.NotificationTemplates", "IX_NotificationTemplate_IsActive");
            DropIndex("dbo.NotificationTemplates", "IX_NotificationTemplate_ChannelType");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_UpdatedByUserId");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_CreatedByUserId");
            DropIndex("dbo.NotificationHistories", new[] { "DeletedByUserId" });
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_IsDeleted");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_RelatedEntityType");
            DropIndex("dbo.NotificationHistories", new[] { "SenderUserId" });
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_Status");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_SentAt_Status_IsDeleted");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_SentAt");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_Recipient_Status_SentAt");
            DropIndex("dbo.NotificationHistories", "IX_NotificationHistory_Recipient");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_UpdatedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_UpdatedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DeletedByUserId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DeletedAt");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_Duration");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_EndTime");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_StartTime");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_AppointmentDate");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_IsDeleted");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId_AppointmentDate");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_DoctorId");
            DropIndex("dbo.DoctorTimeSlots", "IX_DoctorTimeSlot_TimeSlotId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_UpdatedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_UpdatedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_DeletedByUserId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_DeletedAt");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_IsActive");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_EndTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_StartTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_StartTime_EndTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_IsDeleted");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_IsActive");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId_StartTime");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_WorkDayId");
            DropIndex("dbo.DoctorTimeRanges", "IX_DoctorTimeRange_TimeRangeId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_UpdatedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_UpdatedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DeletedByUserId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DeletedAt");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_IsActive");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_DayOfWeek");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_IsDeleted");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_IsActive");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId_DayOfWeek");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_ScheduleId");
            DropIndex("dbo.DoctorWorkDays", "IX_DoctorWorkDay_WorkDayId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_UpdatedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_UpdatedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DeletedByUserId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DeletedAt");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_CreatedAt_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_IsActive");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_AppointmentDuration");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsDeleted");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId_IsActive");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_DoctorId");
            DropIndex("dbo.DoctorSchedules", "IX_DoctorSchedule_ScheduleId");
            DropIndex("dbo.Specializations", "IX_Specialization_UpdatedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_UpdatedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_CreatedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_CreatedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_DeletedByUserId");
            DropIndex("dbo.Specializations", "IX_Specialization_DeletedAt");
            DropIndex("dbo.Specializations", "IX_Specialization_IsDeleted");
            DropIndex("dbo.Specializations", "IX_Specialization_DisplayOrder");
            DropIndex("dbo.Specializations", "IX_Specialization_IsActive");
            DropIndex("dbo.Specializations", "IX_Specialization_Name");
            DropIndex("dbo.DoctorSpecializations", new[] { "Doctor_DoctorId" });
            DropIndex("dbo.DoctorSpecializations", new[] { "Specialization_SpecializationId" });
            DropIndex("dbo.DoctorSpecializations", new[] { "DoctorId" });
            DropIndex("dbo.DoctorSpecializations", new[] { "SpecializationId" });
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_UpdatedByUserId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_UpdatedAt");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_CreatedByUserId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_CreatedAt");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_DeletedByUserId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_DeletedAt");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_IsDeleted");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_IsActive");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_IsActive_Provider");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_Provider");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_MerchantId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_TerminalId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_Title");
            DropIndex("dbo.CashSessions", "IX_CashSession_UpdatedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_UpdatedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_CreatedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_CreatedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_DeletedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_DeletedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_IsDeleted");
            DropIndex("dbo.CashSessions", "IX_CashSession_Status");
            DropIndex("dbo.CashSessions", "IX_CashSession_ClosedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_OpenedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_UserId_Status_OpenedAt");
            DropIndex("dbo.CashSessions", "IX_CashSession_UserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_UpdatedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_UpdatedAt");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CreatedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CreatedAt");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_DeletedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_DeletedAt");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_IsDeleted");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_ReceiptNo");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_ReferenceCode");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_TransactionId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_Status");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_Method");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_Amount");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CashSessionId_Status_CreatedAt");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CashSessionId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_PosTerminalId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_ReceptionId_Status");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_ReceptionId");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_UpdatedByUserId");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_UpdatedAt");
            DropIndex("dbo.ReceiptPrints", new[] { "PrintedByUserId" });
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_CreatedByUserId");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_CreatedAt");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_DeletedByUserId");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_DeletedAt");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_CreatedAt_IsDeleted");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_IsDeleted");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_PrintedBy");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_PrintDate_IsDeleted");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_PrintDate");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_ReceiptHash");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_ReceiptContent");
            DropIndex("dbo.ReceiptPrints", "IX_ReceiptPrint_ReceptionId_PrintDate");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_UpdatedByUserId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_UpdatedAt");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_CreatedByUserId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_CreatedAt");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_DeletedByUserId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_DeletedAt");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_IsDeleted");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsurerShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_PatientShare");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_TariffPrice");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_ServiceId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_Insurance_Service_Unique");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_InsuranceId");
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            DropIndex("dbo.Patients", "IX_Patient_UpdatedByUserId");
            DropIndex("dbo.Patients", "IX_Patient_UpdatedAt");
            DropIndex("dbo.Patients", "IX_Patient_CreatedByUserId");
            DropIndex("dbo.Patients", "IX_Patient_CreatedAt");
            DropIndex("dbo.Patients", "IX_Patient_DeletedByUserId");
            DropIndex("dbo.Patients", "IX_Patient_DeletedAt");
            DropIndex("dbo.Patients", "IX_Patient_IsDeleted");
            DropIndex("dbo.Patients", "IX_Patient_LastLoginDate");
            DropIndex("dbo.Patients", "IX_Patient_InsuranceId_LastLoginDate");
            DropIndex("dbo.Patients", "IX_Patient_InsuranceId");
            DropIndex("dbo.Patients", "IX_Patient_PhoneNumber");
            DropIndex("dbo.Patients", "IX_Patient_BirthDate");
            DropIndex("dbo.Patients", "IX_Patient_LastName");
            DropIndex("dbo.Patients", "IX_Patient_LastName_FirstName");
            DropIndex("dbo.Patients", "IX_Patient_FirstName");
            DropIndex("dbo.Patients", "IX_Patient_NationalCode");
            DropIndex("dbo.Insurances", "IX_Insurance_UpdatedByUserId");
            DropIndex("dbo.Insurances", "IX_Insurance_UpdatedAt");
            DropIndex("dbo.Insurances", "IX_Insurance_CreatedByUserId");
            DropIndex("dbo.Insurances", "IX_Insurance_CreatedAt");
            DropIndex("dbo.Insurances", "IX_Insurance_DeletedByUserId");
            DropIndex("dbo.Insurances", "IX_Insurance_DeletedAt");
            DropIndex("dbo.Insurances", "IX_Insurance_IsDeleted");
            DropIndex("dbo.Insurances", "IX_Insurance_IsActive_IsDeleted");
            DropIndex("dbo.Insurances", "IX_Insurance_IsActive");
            DropIndex("dbo.Insurances", "IX_Insurance_Name");
            DropIndex("dbo.Receptions", "IX_Reception_UpdatedByUserId");
            DropIndex("dbo.Receptions", "IX_Reception_UpdatedAt");
            DropIndex("dbo.Receptions", "IX_Reception_CreatedByUserId");
            DropIndex("dbo.Receptions", "IX_Reception_CreatedAt");
            DropIndex("dbo.Receptions", "IX_Reception_DeletedByUserId");
            DropIndex("dbo.Receptions", "IX_Reception_DeletedAt");
            DropIndex("dbo.Receptions", "IX_Reception_IsDeleted");
            DropIndex("dbo.Receptions", "IX_Reception_Status");
            DropIndex("dbo.Receptions", "IX_Reception_ReceptionDate");
            DropIndex("dbo.Receptions", "IX_Reception_InsuranceId_Date_Status");
            DropIndex("dbo.Receptions", "IX_Reception_InsuranceId");
            DropIndex("dbo.Receptions", "IX_Reception_DoctorId_Date_Status");
            DropIndex("dbo.Receptions", "IX_Reception_DoctorId");
            DropIndex("dbo.Receptions", "IX_Reception_PatientId_Date_Status");
            DropIndex("dbo.Receptions", "IX_Reception_PatientId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UpdatedByUserId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UpdatedAt");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_CreatedByUserId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_CreatedAt");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_DeletedByUserId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_DeletedAt");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_IsDeleted");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_InsurerShareAmount");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_PatientShareAmount");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UnitPrice");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_Quantity");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_ServiceId_CreatedAt");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_ServiceId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_ReceptionId_ServiceId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_ReceptionId");
            DropIndex("dbo.Services", "IX_Service_UpdatedByUserId");
            DropIndex("dbo.Services", "IX_Service_UpdatedAt");
            DropIndex("dbo.Services", "IX_Service_CreatedByUserId");
            DropIndex("dbo.Services", "IX_Service_CreatedAt");
            DropIndex("dbo.Services", "IX_Service_DeletedByUserId");
            DropIndex("dbo.Services", "IX_Service_DeletedAt");
            DropIndex("dbo.Services", "IX_Service_IsDeleted");
            DropIndex("dbo.Services", "IX_Service_ServiceCategoryId_IsDeleted");
            DropIndex("dbo.Services", "IX_Service_ServiceCategoryId");
            DropIndex("dbo.Services", "IX_Service_Price");
            DropIndex("dbo.Services", "IX_Service_ServiceCode");
            DropIndex("dbo.Services", "IX_Service_Title");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_UpdatedByUserId");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_UpdatedAt");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_CreatedByUserId");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_CreatedAt");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_CertificateNumber");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_ExpiryDate");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_GrantedDate");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_ExpiryDate_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_AuthorizationLevel_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_AuthorizationLevel");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_ServiceCategoryId_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_ServiceCategoryId");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DoctorId_ServiceCategoryId_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DoctorId_IsActive");
            DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DoctorId");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_UpdatedByUserId");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_UpdatedAt");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_CreatedByUserId");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_CreatedAt");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_DeletedByUserId");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_DeletedAt");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_IsDeleted");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_IsActive");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_DepartmentId_IsActive_IsDeleted");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_DepartmentId");
            DropIndex("dbo.ServiceCategories", "IX_ServiceCategory_Title");
            DropIndex("dbo.DoctorDepartments", new[] { "DeletedByUserId" });
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_UpdatedByUserId");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_UpdatedAt");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_CreatedByUserId");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_CreatedAt");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_EndDate");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_StartDate_EndDate");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_StartDate");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_IsActive");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_Role");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_DepartmentId_IsActive");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_DepartmentId");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_DoctorId_DepartmentId_IsActive");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_DoctorId_IsActive");
            DropIndex("dbo.DoctorDepartments", "IX_DoctorDepartment_DoctorId");
            DropIndex("dbo.Departments", "IX_Department_UpdatedByUserId");
            DropIndex("dbo.Departments", "IX_Department_UpdatedAt");
            DropIndex("dbo.Departments", "IX_Department_CreatedByUserId");
            DropIndex("dbo.Departments", "IX_Department_CreatedAt");
            DropIndex("dbo.Departments", "IX_Department_DeletedByUserId");
            DropIndex("dbo.Departments", "IX_Department_DeletedAt");
            DropIndex("dbo.Departments", "IX_Department_IsDeleted");
            DropIndex("dbo.Departments", "IX_Department_IsActive");
            DropIndex("dbo.Departments", "IX_Department_ClinicId_IsActive_IsDeleted");
            DropIndex("dbo.Departments", "IX_Department_ClinicId_IsDeleted");
            DropIndex("dbo.Departments", "IX_Department_ClinicId");
            DropIndex("dbo.Departments", "IX_Department_Name");
            DropIndex("dbo.Clinics", "IX_Clinic_UpdatedByUserId");
            DropIndex("dbo.Clinics", "IX_Clinic_UpdatedAt");
            DropIndex("dbo.Clinics", "IX_Clinic_CreatedByUserId");
            DropIndex("dbo.Clinics", "IX_Clinic_CreatedAt");
            DropIndex("dbo.Clinics", "IX_Clinic_DeletedByUserId");
            DropIndex("dbo.Clinics", "IX_Clinic_DeletedAt");
            DropIndex("dbo.Clinics", "IX_Clinic_IsDeleted_CreatedAt");
            DropIndex("dbo.Clinics", "IX_Clinic_IsDeleted");
            DropIndex("dbo.Clinics", "IX_Clinic_PhoneNumber");
            DropIndex("dbo.Clinics", "IX_Clinic_Name");
            DropIndex("dbo.Doctors", "IX_Doctor_ApplicationUserId");
            DropIndex("dbo.Doctors", "IX_Doctor_UpdatedByUserId");
            DropIndex("dbo.Doctors", "IX_Doctor_UpdatedAt");
            DropIndex("dbo.Doctors", "IX_Doctor_CreatedByUserId");
            DropIndex("dbo.Doctors", "IX_Doctor_CreatedAt");
            DropIndex("dbo.Doctors", "IX_Doctor_DeletedByUserId");
            DropIndex("dbo.Doctors", "IX_Doctor_DeletedAt");
            DropIndex("dbo.Doctors", "IX_Doctor_IsDeleted");
            DropIndex("dbo.Doctors", "IX_Doctor_ClinicId");
            DropIndex("dbo.Doctors", "IX_Doctor_Email");
            DropIndex("dbo.Doctors", "IX_Doctor_MedicalCouncilCode");
            DropIndex("dbo.Doctors", "IX_Doctor_NationalCode");
            DropIndex("dbo.Doctors", "IX_Doctor_PhoneNumber");
            DropIndex("dbo.Doctors", "IX_Doctor_ExperienceYears");
            DropIndex("dbo.Doctors", "IX_Doctor_DateOfBirth");
            DropIndex("dbo.Doctors", "IX_Doctor_Gender");
            DropIndex("dbo.Doctors", "IX_Doctor_University");
            DropIndex("dbo.Doctors", "IX_Doctor_GraduationYear");
            DropIndex("dbo.Doctors", "IX_Doctor_Degree");
            DropIndex("dbo.Doctors", "IX_Doctor_University_IsActive_IsDeleted");
            DropIndex("dbo.Doctors", "IX_Doctor_ClinicId_IsActive_IsDeleted");
            DropIndex("dbo.Doctors", "IX_Doctor_IsActive");
            DropIndex("dbo.Doctors", "IX_Doctor_LastName");
            DropIndex("dbo.Doctors", "IX_Doctor_LastName_FirstName");
            DropIndex("dbo.Doctors", "IX_Doctor_FirstName");
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_Email");
            DropIndex("dbo.AspNetUsers", "IX_Patient_Gender");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_UpdatedByUserId");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_UpdatedAt");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_CreatedByUserId");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_CreatedAt");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_DeletedByUserId");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_DeletedAt");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_IsDeleted");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_IsActive_IsDeleted");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_IsActive");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_PhoneNumber");
            DropIndex("dbo.AspNetUsers", new[] { "NationalCode" });
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_LastName");
            DropIndex("dbo.AspNetUsers", "IX_ApplicationUser_FirstName");
            DropIndex("dbo.Appointments", "IX_Appointment_UpdatedByUserId");
            DropIndex("dbo.Appointments", "IX_Appointment_UpdatedAt");
            DropIndex("dbo.Appointments", "IX_Appointment_CreatedByUserId");
            DropIndex("dbo.Appointments", "IX_Appointment_CreatedAt");
            DropIndex("dbo.Appointments", "IX_Appointment_DeletedByUserId");
            DropIndex("dbo.Appointments", "IX_Appointment_DeletedAt");
            DropIndex("dbo.Appointments", "IX_Appointment_IsDeleted");
            DropIndex("dbo.Appointments", new[] { "ServiceCategoryId" });
            DropIndex("dbo.Appointments", "IX_Appointment_PaymentTransactionId");
            DropIndex("dbo.Appointments", "IX_Appointment_Status");
            DropIndex("dbo.Appointments", "IX_Appointment_AppointmentDate");
            DropIndex("dbo.Appointments", "IX_Appointment_PatientId_Status_Date");
            DropIndex("dbo.Appointments", "IX_Appointment_PatientId");
            DropIndex("dbo.Appointments", "IX_Appointment_DoctorId_Date_Status");
            DropIndex("dbo.Appointments", "IX_Appointment_DoctorId");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.OtpRequests");
            DropTable("dbo.NotificationTemplates");
            DropTable("dbo.DatabaseVersion");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.NotificationHistories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_NotificationHistory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.DoctorTimeSlots",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeSlot_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorTimeRanges",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorTimeRange_ActiveDoctorTimeRanges", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorTimeRange_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorWorkDays",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorWorkDay_ActiveDoctorWorkDays", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorWorkDay_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorSchedules",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_DoctorSchedule_ActiveDoctorSchedules", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_DoctorSchedule_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Specializations",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Specialization_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorSpecializations");
            DropTable("dbo.PosTerminals",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PosTerminal_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.CashSessions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_CashSession_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.PaymentTransactions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_PaymentTransaction_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ReceiptPrints",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceiptPrint_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.InsuranceTariffs",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_InsuranceTariff_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Patients",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Patient_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Insurances",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Insurance_ActiveInsurances", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Insurance_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Receptions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Reception_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ReceptionItems",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceptionItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Services",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Service_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorServiceCategories");
            DropTable("dbo.ServiceCategories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceCategory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.DoctorDepartments");
            DropTable("dbo.Departments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Department_ActiveDepartments", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Department_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Clinics",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Clinic_ActiveClinics", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Clinic_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Doctors",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Doctor_ActiveDoctors", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Doctor_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ApplicationUser_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Appointments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Appointment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
