-- ========================================
-- ایندکس‌های ترکیبی و قیدهای یکتایی برای ماژول تریاژ
-- ========================================

-- ========== ایندکس‌های ترکیبی TriageAssessment ==========
CREATE INDEX IX_TriageAssessment_Level_Status_IsDeleted
ON dbo.TriageAssessments(Level, Status, IsDeleted);

CREATE INDEX IX_TriageAssessment_Time_Level_IsDeleted
ON dbo.TriageAssessments(TriageStartAt, Level, IsDeleted);

CREATE INDEX IX_TriageAssessment_Patient_Time_IsDeleted
ON dbo.TriageAssessments(PatientId, TriageStartAt, IsDeleted);

CREATE INDEX IX_TriageAssessment_IsOpen_PatientId
ON dbo.TriageAssessments(IsOpen, PatientId);

-- ========== ایندکس‌های ترکیبی TriageVitalSigns ==========
CREATE INDEX IX_TriageVitalSigns_Assessment_Time_IsDeleted
ON dbo.TriageVitalSigns(TriageAssessmentId, MeasurementTime, IsDeleted);

CREATE INDEX IX_TriageVitalSigns_Normal_Immediate_IsDeleted
ON dbo.TriageVitalSigns(IsNormal, RequiresImmediateAttention, IsDeleted);

CREATE INDEX IX_TriageVitalSigns_MeasurementTime_IsDeleted
ON dbo.TriageVitalSigns(MeasurementTime, IsDeleted);

-- ========== ایندکس‌های ترکیبی TriageQueue ==========
CREATE INDEX IX_TriageQueue_Status_Priority_IsDeleted
ON dbo.TriageQueues(Status, Priority, IsDeleted);

CREATE INDEX IX_TriageQueue_Time_Status_IsDeleted
ON dbo.TriageQueues(QueueTime, Status, IsDeleted);

CREATE INDEX IX_TriageQueue_Patient_Status_IsDeleted
ON dbo.TriageQueues(PatientId, Status, IsDeleted);

CREATE INDEX IX_TriageQueue_Immediate_Status_IsDeleted
ON dbo.TriageQueues(RequiresImmediateCare, Status, IsDeleted);

CREATE INDEX IX_TriageQueue_NextReassessment_Status
ON dbo.TriageQueues(NextReassessmentDueAt, Status);

-- ========== ایندکس‌های ترکیبی TriageProtocol ==========
CREATE INDEX IX_TriageProtocol_Type_Level_Active_IsDeleted
ON dbo.TriageProtocols(Type, TargetLevel, IsActive, IsDeleted);

CREATE INDEX IX_TriageProtocol_Validity_Active_IsDeleted
ON dbo.TriageProtocols(ValidFrom, ValidTo, IsActive, IsDeleted);

CREATE INDEX IX_TriageProtocol_Priority_Mandatory_Active_IsDeleted
ON dbo.TriageProtocols(Priority, IsMandatory, IsActive, IsDeleted);

CREATE INDEX IX_TriageProtocol_IsActive_IsDeleted
ON dbo.TriageProtocols(IsActive, IsDeleted);

-- ========== ایندکس‌های ترکیبی TriageReassessment ==========
CREATE INDEX IX_TriageReassessment_Assessment_Time_IsDeleted
ON dbo.TriageReassessments(TriageAssessmentId, ReassessmentAt, IsDeleted);

CREATE INDEX IX_TriageReassessment_Level_Reason_IsDeleted
ON dbo.TriageReassessments(NewLevel, Reason, IsDeleted);

CREATE INDEX IX_TriageReassessment_ReassessmentAt_IsDeleted
ON dbo.TriageReassessments(ReassessmentAt, IsDeleted);

-- ========== قیدهای یکتایی ==========

-- یک تریاژ باز برای هر بیمار
CREATE UNIQUE INDEX IX_TriageAssessment_Patient_Open
ON dbo.TriageAssessments(PatientId)
WHERE IsOpen = 1 AND IsDeleted = 0;

-- یک صف Waiting برای هر ارزیابی
CREATE UNIQUE INDEX IX_TriageQueue_Assessment_Waiting
ON dbo.TriageQueues(TriageAssessmentId)
WHERE Status = 0 AND IsDeleted = 0; -- 0 = Waiting

-- ========== قیدهای CHECK برای صحت داده‌ها ==========

-- قید زمان‌ها در TriageAssessment
ALTER TABLE dbo.TriageAssessments
ADD CONSTRAINT CK_TriageAssessment_Times
CHECK (ArrivalAt <= TriageStartAt AND TriageStartAt <= TriageEndAt);

-- قید زمان‌ها در TriageQueue
ALTER TABLE dbo.TriageQueues
ADD CONSTRAINT CK_TriageQueue_Times
CHECK (QueueTime <= ISNULL(CalledTime, QueueTime) AND 
       ISNULL(CalledTime, QueueTime) <= ISNULL(CompletedTime, ISNULL(CalledTime, QueueTime)));

-- قید محدوده‌های حیاتی
ALTER TABLE dbo.TriageVitalSigns
ADD CONSTRAINT CK_TriageVitalSigns_VitalRanges
CHECK (
    (BloodPressureSystolic IS NULL OR (BloodPressureSystolic >= 50 AND BloodPressureSystolic <= 300)) AND
    (BloodPressureDiastolic IS NULL OR (BloodPressureDiastolic >= 30 AND BloodPressureDiastolic <= 200)) AND
    (HeartRate IS NULL OR (HeartRate >= 30 AND HeartRate <= 300)) AND
    (RespiratoryRate IS NULL OR (RespiratoryRate >= 5 AND RespiratoryRate <= 60)) AND
    (Temperature IS NULL OR (Temperature >= 30.0 AND Temperature <= 45.0)) AND
    (OxygenSaturation IS NULL OR (OxygenSaturation >= 50 AND OxygenSaturation <= 100)) AND
    (PainLevel IS NULL OR (PainLevel >= 0 AND PainLevel <= 10)) AND
    (GlucoseLevel IS NULL OR (GlucoseLevel >= 20 AND GlucoseLevel <= 1000)) AND
    (Weight IS NULL OR (Weight >= 0.5 AND Weight <= 500)) AND
    (Height IS NULL OR (Height >= 30 AND Height <= 250)) AND
    (GcsE IS NULL OR (GcsE >= 1 AND GcsE <= 4)) AND
    (GcsV IS NULL OR (GcsV >= 1 AND GcsV <= 5)) AND
    (GcsM IS NULL OR (GcsM >= 1 AND GcsM <= 6))
);

-- قید محدوده‌های پروتکل
ALTER TABLE dbo.TriageProtocols
ADD CONSTRAINT CK_TriageProtocol_Validity
CHECK (ValidFrom <= ISNULL(ValidTo, '9999-12-31') AND 
       (ValidTo IS NULL OR ValidTo >= ValidFrom));

-- ========== ایندکس‌های فیلتر شده برای عملکرد ==========

-- ایندکس فیلتر شده برای پروتکل‌های فعال
CREATE INDEX IX_TriageProtocol_Active_Filtered
ON dbo.TriageProtocols(ProtocolCode, Type, TargetLevel)
WHERE IsActive = 1 AND IsDeleted = 0;

-- ایندکس فیلتر شده برای ارزیابی‌های باز
CREATE INDEX IX_TriageAssessment_Open_Filtered
ON dbo.TriageAssessments(PatientId, Level, Status)
WHERE IsOpen = 1 AND IsDeleted = 0;

-- ایندکس فیلتر شده برای صف‌های در انتظار
CREATE INDEX IX_TriageQueue_Waiting_Filtered
ON dbo.TriageQueues(PatientId, Priority, QueueTime)
WHERE Status = 0 AND IsDeleted = 0; -- 0 = Waiting

-- ========== ایندکس‌های پوششی برای گزارش‌گیری ==========

-- ایندکس پوششی برای آمار روزانه تریاژ
CREATE INDEX IX_TriageAssessment_DailyStats_Covering
ON dbo.TriageAssessments(TriageStartAt, Level, Status, IsDeleted)
INCLUDE (PatientId, AssessorUserId, TriageStartAt, TriageEndAt);

-- ایندکس پوششی برای آمار صف
CREATE INDEX IX_TriageQueue_QueueStats_Covering
ON dbo.TriageQueues(QueueTime, Status, Priority, IsDeleted)
INCLUDE (PatientId, TriageAssessmentId, CalledTime, CompletedTime, WaitTimeMinutes);

-- ========== ایندکس‌های پارتیشن‌بندی شده (اختیاری) ==========

-- برای جداول بزرگ، می‌توان از پارتیشن‌بندی بر اساس تاریخ استفاده کرد
-- این ایندکس‌ها برای جداول با حجم بالا مفید هستند

-- ایندکس پارتیشن‌بندی شده برای ارزیابی‌ها
CREATE INDEX IX_TriageAssessment_Partitioned_Date
ON dbo.TriageAssessments(TriageStartAt, Level, Status)
WHERE TriageStartAt >= '2024-01-01' AND TriageStartAt < '2025-01-01';

-- ایندکس پارتیشن‌بندی شده برای صف‌ها
CREATE INDEX IX_TriageQueue_Partitioned_Date
ON dbo.TriageQueues(QueueTime, Status, Priority)
WHERE QueueTime >= '2024-01-01' AND QueueTime < '2025-01-01';
