-- =============================================
-- اسکریپت اضافه کردن Unique Constraints برای ماژول تریاژ
-- =============================================

-- 1. Unique Index برای "یک ارزیابی باز برای هر بیمار"
CREATE UNIQUE INDEX IX_TriageAssessments_Patient_Open
ON dbo.TriageAssessments(PatientId)
WHERE IsOpen = 1 AND IsDeleted = 0;

-- 2. ایندکس‌های ترکیبی برای Performance
CREATE INDEX IX_TriageAssessments_Level_Status_IsDeleted
ON dbo.TriageAssessments(Level, Status, IsDeleted);

CREATE INDEX IX_TriageAssessments_Time_Level_IsDeleted
ON dbo.TriageAssessments(TriageStartAt, Level, IsDeleted);

CREATE INDEX IX_TriageAssessments_Patient_Time_IsDeleted
ON dbo.TriageAssessments(PatientId, TriageStartAt, IsDeleted);

-- 3. ایندکس برای پایش مجدد در صف
CREATE INDEX IX_TriageQueue_Status_NextReassessment
ON dbo.TriageQueues(Status, NextReassessmentDueAt)
WHERE IsDeleted = 0 AND NextReassessmentDueAt IS NOT NULL;

-- 4. ایندکس برای داشبورد real-time
CREATE INDEX IX_TriageQueue_Status_Priority_QueueTime
ON dbo.TriageQueues(Status, Priority, QueueTime)
WHERE IsDeleted = 0;

-- 5. ایندکس برای ارزیابی‌های مجدد
CREATE INDEX IX_TriageReassessments_Assessment_Time
ON dbo.TriageReassessments(TriageAssessmentId, ReassessmentAt)
WHERE IsDeleted = 0;

-- 6. ایندکس برای پروتکل‌های فعال
CREATE INDEX IX_TriageProtocols_Active_Valid
ON dbo.TriageProtocols(IsActive, ValidFrom, ValidTo)
WHERE IsDeleted = 0;

-- 7. ایندکس برای علائم حیاتی
CREATE INDEX IX_TriageVitalSigns_Assessment_Time
ON dbo.TriageVitalSigns(TriageAssessmentId, MeasuredAt)
WHERE IsDeleted = 0;

-- 8. Constraint برای محدود کردن Temperature به decimal(4,1)
ALTER TABLE dbo.TriageVitalSigns
ALTER COLUMN Temperature DECIMAL(4,1);

-- 9. Constraint برای محدود کردن O2FlowLpm
ALTER TABLE dbo.TriageVitalSigns
ALTER COLUMN O2FlowLpm DECIMAL(4,1);

-- 10. Constraint برای محدود کردن Weight و Height
ALTER TABLE dbo.TriageVitalSigns
ALTER COLUMN Weight DECIMAL(5,2);

ALTER TABLE dbo.TriageVitalSigns
ALTER COLUMN Height DECIMAL(5,2);
