-- =============================================
-- Enterprise Notification System - NotificationQueue
-- ClinicApp - ASP.NET MVC5 + EF6
-- Run after deploying new code (or use EF Migrations)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationQueue')
BEGIN
    CREATE TABLE [dbo].[NotificationQueue] (
        [Id]                BIGINT IDENTITY(1,1) NOT NULL,
        [UserId]            NVARCHAR(128) NULL,
        [PatientId]         INT NULL,
        [AppointmentId]     INT NULL,
        [NotificationType]  INT NOT NULL,
        [Title]             NVARCHAR(200) NOT NULL,
        [Message]           NVARCHAR(2000) NOT NULL,
        [Channel]           INT NOT NULL,
        [Status]            INT NOT NULL,
        [RetryCount]        INT NOT NULL DEFAULT 0,
        [MaxRetries]        INT NOT NULL DEFAULT 3,
        [ScheduledTime]     DATETIME2 NULL,
        [SentTime]          DATETIME2 NULL,
        [ErrorLog]          NVARCHAR(2000) NULL,
        [IdempotencyKey]    NVARCHAR(256) NOT NULL,
        [Recipient]         NVARCHAR(100) NOT NULL,
        [Subject]           NVARCHAR(500) NULL,
        [CreatedAt]         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_NotificationQueue] PRIMARY KEY CLUSTERED ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationQueue_Status] ON [dbo].[NotificationQueue]([Status]);
    CREATE NONCLUSTERED INDEX [IX_NotificationQueue_ScheduledTime] ON [dbo].[NotificationQueue]([ScheduledTime]);
    CREATE NONCLUSTERED INDEX [IX_NotificationQueue_IdempotencyKey] ON [dbo].[NotificationQueue]([IdempotencyKey]);
    CREATE NONCLUSTERED INDEX [IX_NotificationQueue_Appointment_Type_Channel] ON [dbo].[NotificationQueue]([AppointmentId], [NotificationType], [Channel]);

    PRINT 'Table NotificationQueue created.';
END
ELSE
    PRINT 'Table NotificationQueue already exists.';
GO
