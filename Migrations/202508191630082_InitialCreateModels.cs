namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreateModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appointments",
                c => new
                    {
                        AppointmentId = c.Int(nullable: false, identity: true),
                        DoctorId = c.Int(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Appointment_DoctorId",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Appointment_DoctorId }")
                                },
                            }),
                        PatientId = c.Int(
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Appointment_PatientId",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Appointment_PatientId }")
                                },
                            }),
                        AppointmentDate = c.DateTime(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Appointment_Date",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Appointment_Date }")
                                },
                            }),
                        Status = c.Byte(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Appointment_Status",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Appointment_Status }")
                                },
                            }),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentTransactionId = c.String(maxLength: 100),
                        IsDeleted = c.Boolean(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Appointment_IsDeleted",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Appointment_IsDeleted }")
                                },
                            }),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                        DeletedById = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Appointment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.AppointmentId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .Index(t => t.DoctorId)
                .Index(t => t.PatientId);
            
            CreateTable(
                "dbo.Doctors",
                c => new
                    {
                        DoctorId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Specialization = c.String(maxLength: 250),
                        PhoneNumber = c.String(maxLength: 50),
                        ClinicId = c.Int(),
                        DepartmentId = c.Int(),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                        DeletedById = c.String(),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                        Bio = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Doctor_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.DoctorId)
                .ForeignKey("dbo.Clinics", t => t.ClinicId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .Index(t => t.ClinicId)
                .Index(t => t.DepartmentId)
                .Index(t => t.ApplicationUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        CreatedByUserId = c.String(),
                        UpdatedByUserId = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
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
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        PhoneNumber = c.String(nullable: false, maxLength: 20),
                        Message = c.String(nullable: false, maxLength: 500),
                        SentDate = c.DateTime(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_SentDate",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_SentDate }")
                                },
                            }),
                        IsSent = c.Boolean(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_IsSent",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_IsSent }")
                                },
                            }),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Patients",
                c => new
                    {
                        PatientId = c.Int(nullable: false, identity: true),
                        NationalCode = c.String(nullable: false, maxLength: 10,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_NationalCode",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_NationalCode, IsUnique: True }")
                                },
                            }),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        BirthDate = c.DateTime(),
                        Address = c.String(maxLength: 500),
                        PhoneNumber = c.String(maxLength: 50),
                        InsuranceId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                        LastLoginDate = c.DateTime(),
                        ApplicationUserId = c.String(nullable: false, maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Patient_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.PatientId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUserId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .Index(t => t.NationalCode, unique: true)
                .Index(t => t.InsuranceId)
                .Index(t => t.ApplicationUserId);
            
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
                    { "DynamicFilter_Insurance_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.InsuranceId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.DeletedByUserId, name: "IX_Insurance_DeletedByUserId")
                .Index(t => t.CreatedByUserId, name: "IX_Insurance_CreatedByUserId")
                .Index(t => t.UpdatedByUserId, name: "IX_Insurance_UpdatedByUserId");
            
            CreateTable(
                "dbo.Receptions",
                c => new
                    {
                        ReceptionId = c.Int(nullable: false, identity: true),
                        PatientId = c.Int(nullable: false),
                        DoctorId = c.Int(nullable: false),
                        InsuranceId = c.Int(nullable: false),
                        ReceptionDate = c.DateTime(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_ReceptionDate",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_ReceptionDate }")
                                },
                            }),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PatientCoPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InsurerShareAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Byte(nullable: false,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_Status",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_Status }")
                                },
                            }),
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
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .ForeignKey("dbo.Patients", t => t.PatientId)
                .ForeignKey("dbo.Doctors", t => t.DoctorId)
                .Index(t => t.PatientId)
                .Index(t => t.DoctorId)
                .Index(t => t.InsuranceId)
                .Index(t => t.DeletedByUserId, name: "IX_Reception_DeletedByUserId")
                .Index(t => t.CreatedByUserId, name: "IX_Reception_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_Reception_UpdatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.ReceiptPrints",
                c => new
                    {
                        ReceiptPrintId = c.Int(nullable: false, identity: true),
                        ReceptionId = c.Int(nullable: false),
                        ReceiptContent = c.String(nullable: false),
                        PrintDate = c.DateTime(nullable: false),
                        PrintedBy = c.String(maxLength: 250),
                        PrintedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                    })
                .PrimaryKey(t => t.ReceiptPrintId)
                .ForeignKey("dbo.AspNetUsers", t => t.PrintedByUserId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId, cascadeDelete: true)
                .Index(t => t.ReceptionId)
                .Index(t => t.PrintedByUserId);
            
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
                        CreatedAt = c.DateTime(),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceptionItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ReceptionItemId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.Receptions", t => t.ReceptionId, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ReceptionId)
                .Index(t => t.ServiceId)
                .Index(t => t.CreatedByUserId, name: "IX_ReceptionItem_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_ReceptionItem_UpdatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        ServiceId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 250),
                        ServiceCode = c.String(nullable: false, maxLength: 50,
                            annotations: new Dictionary<string, AnnotationValues>
                            {
                                { 
                                    "IX_ServiceCode",
                                    new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { Name: IX_ServiceCode, IsUnique: True }")
                                },
                            }),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(maxLength: 1000),
                        ServiceCategoryId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(),
                        CreatedByUserId = c.String(),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Service_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceId)
                .ForeignKey("dbo.ServiceCategories", t => t.ServiceCategoryId)
                .Index(t => t.ServiceCode, unique: true)
                .Index(t => t.ServiceCategoryId);
            
            CreateTable(
                "dbo.ServiceCategories",
                c => new
                    {
                        ServiceCategoryId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        DepartmentId = c.Int(nullable: false),
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
                    { "DynamicFilter_ServiceCategory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ServiceCategoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.DepartmentId)
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        ClinicId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Department_ActiveDepartments", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Department_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.DepartmentId)
                .ForeignKey("dbo.Clinics", t => t.ClinicId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ClinicId)
                .Index(t => t.CreatedByUserId, name: "IX_Department_CreatedByUserId")
                .Index(t => t.UpdatedByUserId, name: "IX_Department_UpdatedByUserId")
                .Index(t => t.DeletedByUserId, name: "IX_Department_DeletedByUserId");
            
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
                        DeletedByUserId = c.String(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                        CreatedById = c.String(),
                        UpdatedById = c.String(),
                        DeletedById = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Clinic_ActiveClinics", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Clinic_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.ClinicId);
            
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
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.InsuranceTariffId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.Insurances", t => t.InsuranceId)
                .ForeignKey("dbo.Services", t => t.ServiceId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => new { t.InsuranceId, t.ServiceId }, unique: true, name: "IX_Insurance_Service")
                .Index(t => t.CreatedByUserId, name: "IX_InsuranceTariff_CreatedByUserId")
                .Index(t => t.UpdatedByUserId, name: "IX_InsuranceTariff_UpdatedByUserId");
            
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
                .Index(t => t.PosTerminalId, name: "IX_PaymentTransaction_PosTerminalId")
                .Index(t => t.CashSessionId, name: "IX_PaymentTransaction_CashSessionId")
                .Index(t => t.DeletedByUserId, name: "IX_PaymentTransaction_DeletedByUserId")
                .Index(t => t.CreatedByUserId, name: "IX_PaymentTransaction_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PaymentTransaction_UpdatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
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
                .Index(t => t.DeletedByUserId, name: "IX_CashSession_DeletedByUserId")
                .Index(t => t.CreatedByUserId, name: "IX_CashSession_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_CashSession_UpdatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
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
                .Index(t => t.DeletedByUserId, name: "IX_PosTerminal_DeletedByUserId")
                .Index(t => t.CreatedByUserId, name: "IX_PosTerminal_CreatedByUserId")
                .Index(t => t.UpdatedAt, name: "IX_PosTerminal_UpdatedByUserId")
                .Index(t => t.UpdatedByUserId);
            
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
            DropForeignKey("dbo.Appointments", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Appointments", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.Receptions", "DoctorId", "dbo.Doctors");
            DropForeignKey("dbo.Doctors", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "PatientId", "dbo.Patients");
            DropForeignKey("dbo.Insurances", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "InsuranceId", "dbo.Insurances");
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
            DropForeignKey("dbo.ReceptionItems", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceptionItems", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.InsuranceTariffs", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.InsuranceTariffs", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.InsuranceTariffs", "InsuranceId", "dbo.Insurances");
            DropForeignKey("dbo.InsuranceTariffs", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "ServiceCategoryId", "dbo.ServiceCategories");
            DropForeignKey("dbo.ServiceCategories", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceCategories", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Departments", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Departments", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Departments", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Doctors", "ClinicId", "dbo.Clinics");
            DropForeignKey("dbo.Departments", "ClinicId", "dbo.Clinics");
            DropForeignKey("dbo.ServiceCategories", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceCategories", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceptionItems", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.ReceptionItems", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReceiptPrints", "ReceptionId", "dbo.Receptions");
            DropForeignKey("dbo.ReceiptPrints", "PrintedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Receptions", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Patients", "InsuranceId", "dbo.Insurances");
            DropForeignKey("dbo.Insurances", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Insurances", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Patients", "ApplicationUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.PosTerminals", new[] { "UpdatedByUserId" });
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_UpdatedByUserId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_CreatedByUserId");
            DropIndex("dbo.PosTerminals", "IX_PosTerminal_DeletedByUserId");
            DropIndex("dbo.CashSessions", new[] { "UpdatedByUserId" });
            DropIndex("dbo.CashSessions", "IX_CashSession_UpdatedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_CreatedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_DeletedByUserId");
            DropIndex("dbo.CashSessions", "IX_CashSession_UserId");
            DropIndex("dbo.PaymentTransactions", new[] { "UpdatedByUserId" });
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_UpdatedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CreatedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_DeletedByUserId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_CashSessionId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_PosTerminalId");
            DropIndex("dbo.PaymentTransactions", "IX_PaymentTransaction_ReceptionId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_UpdatedByUserId");
            DropIndex("dbo.InsuranceTariffs", "IX_InsuranceTariff_CreatedByUserId");
            DropIndex("dbo.InsuranceTariffs", "IX_Insurance_Service");
            DropIndex("dbo.Departments", "IX_Department_DeletedByUserId");
            DropIndex("dbo.Departments", "IX_Department_UpdatedByUserId");
            DropIndex("dbo.Departments", "IX_Department_CreatedByUserId");
            DropIndex("dbo.Departments", new[] { "ClinicId" });
            DropIndex("dbo.ServiceCategories", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ServiceCategories", new[] { "CreatedByUserId" });
            DropIndex("dbo.ServiceCategories", new[] { "DeletedByUserId" });
            DropIndex("dbo.ServiceCategories", new[] { "DepartmentId" });
            DropIndex("dbo.Services", new[] { "ServiceCategoryId" });
            DropIndex("dbo.Services", new[] { "ServiceCode" });
            DropIndex("dbo.ReceptionItems", new[] { "UpdatedByUserId" });
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_UpdatedByUserId");
            DropIndex("dbo.ReceptionItems", "IX_ReceptionItem_CreatedByUserId");
            DropIndex("dbo.ReceptionItems", new[] { "ServiceId" });
            DropIndex("dbo.ReceptionItems", new[] { "ReceptionId" });
            DropIndex("dbo.ReceiptPrints", new[] { "PrintedByUserId" });
            DropIndex("dbo.ReceiptPrints", new[] { "ReceptionId" });
            DropIndex("dbo.Receptions", new[] { "UpdatedByUserId" });
            DropIndex("dbo.Receptions", "IX_Reception_UpdatedByUserId");
            DropIndex("dbo.Receptions", "IX_Reception_CreatedByUserId");
            DropIndex("dbo.Receptions", "IX_Reception_DeletedByUserId");
            DropIndex("dbo.Receptions", new[] { "InsuranceId" });
            DropIndex("dbo.Receptions", new[] { "DoctorId" });
            DropIndex("dbo.Receptions", new[] { "PatientId" });
            DropIndex("dbo.Insurances", "IX_Insurance_UpdatedByUserId");
            DropIndex("dbo.Insurances", "IX_Insurance_CreatedByUserId");
            DropIndex("dbo.Insurances", "IX_Insurance_DeletedByUserId");
            DropIndex("dbo.Patients", new[] { "ApplicationUserId" });
            DropIndex("dbo.Patients", new[] { "InsuranceId" });
            DropIndex("dbo.Patients", new[] { "NationalCode" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Doctors", new[] { "ApplicationUserId" });
            DropIndex("dbo.Doctors", new[] { "DepartmentId" });
            DropIndex("dbo.Doctors", new[] { "ClinicId" });
            DropIndex("dbo.Appointments", new[] { "PatientId" });
            DropIndex("dbo.Appointments", new[] { "DoctorId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
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
            DropTable("dbo.InsuranceTariffs");
            DropTable("dbo.Clinics",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Clinic_ActiveClinics", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Clinic_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Departments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Department_ActiveDepartments", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                    { "DynamicFilter_Department_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ServiceCategories",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ServiceCategory_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Services",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Service_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                },
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "ServiceCode",
                        new Dictionary<string, object>
                        {
                            { "IX_ServiceCode", "IndexAnnotation: { Name: IX_ServiceCode, IsUnique: True }" },
                        }
                    },
                });
            DropTable("dbo.ReceptionItems",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_ReceptionItem_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.ReceiptPrints");
            DropTable("dbo.Receptions",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Reception_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                },
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "ReceptionDate",
                        new Dictionary<string, object>
                        {
                            { "IX_ReceptionDate", "IndexAnnotation: { Name: IX_ReceptionDate }" },
                        }
                    },
                    {
                        "Status",
                        new Dictionary<string, object>
                        {
                            { "IX_Status", "IndexAnnotation: { Name: IX_Status }" },
                        }
                    },
                });
            DropTable("dbo.Insurances",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Insurance_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Patients",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Patient_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                },
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "NationalCode",
                        new Dictionary<string, object>
                        {
                            { "IX_NationalCode", "IndexAnnotation: { Name: IX_NationalCode, IsUnique: True }" },
                        }
                    },
                });
            DropTable("dbo.Notifications",
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "IsSent",
                        new Dictionary<string, object>
                        {
                            { "IX_IsSent", "IndexAnnotation: { Name: IX_IsSent }" },
                        }
                    },
                    {
                        "SentDate",
                        new Dictionary<string, object>
                        {
                            { "IX_SentDate", "IndexAnnotation: { Name: IX_SentDate }" },
                        }
                    },
                });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Doctors",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Doctor_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
            DropTable("dbo.Appointments",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_Appointment_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                },
                removedColumnAnnotations: new Dictionary<string, IDictionary<string, object>>
                {
                    {
                        "AppointmentDate",
                        new Dictionary<string, object>
                        {
                            { "IX_Appointment_Date", "IndexAnnotation: { Name: IX_Appointment_Date }" },
                        }
                    },
                    {
                        "DoctorId",
                        new Dictionary<string, object>
                        {
                            { "IX_Appointment_DoctorId", "IndexAnnotation: { Name: IX_Appointment_DoctorId }" },
                        }
                    },
                    {
                        "IsDeleted",
                        new Dictionary<string, object>
                        {
                            { "IX_Appointment_IsDeleted", "IndexAnnotation: { Name: IX_Appointment_IsDeleted }" },
                        }
                    },
                    {
                        "PatientId",
                        new Dictionary<string, object>
                        {
                            { "IX_Appointment_PatientId", "IndexAnnotation: { Name: IX_Appointment_PatientId }" },
                        }
                    },
                    {
                        "Status",
                        new Dictionary<string, object>
                        {
                            { "IX_Appointment_Status", "IndexAnnotation: { Name: IX_Appointment_Status }" },
                        }
                    },
                });
        }
    }
}
