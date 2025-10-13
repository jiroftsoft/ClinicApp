-- =============================================
-- اصلاحات Production-Ready برای ماژول تریاژ
-- =============================================

-- 1. اضافه کردن Unique Index برای یک ارزیابی باز برای هر بیمار
CREATE UNIQUE INDEX IX_TriageAssessments_Patient_Open
ON dbo.TriageAssessments(PatientId)
WHERE IsOpen = 1 AND IsDeleted = 0;

-- 2. اضافه کردن Unique Index برای یک صف Waiting برای هر ارزیابی
CREATE UNIQUE INDEX IX_TriageQueues_Assessment_Waiting
ON dbo.TriageQueues(TriageAssessmentId)
WHERE Status = 0 AND IsDeleted = 0; -- Status = 0 = Waiting

-- 3. اضافه کردن Check Constraints برای توالی زمان‌ها در TriageAssessment
ALTER TABLE dbo.TriageAssessments WITH CHECK ADD
CONSTRAINT CK_TriageAssessment_Times
CHECK (ArrivalAt <= TriageStartAt AND
       (TriageEndAt IS NULL OR TriageStartAt <= TriageEndAt) AND
       (FirstPhysicianContactAt IS NULL OR
        (TriageEndAt IS NULL OR TriageEndAt <= FirstPhysicianContactAt)));

-- 4. اضافه کردن Check Constraints برای توالی زمان‌ها در TriageQueue
ALTER TABLE dbo.TriageQueues WITH CHECK ADD
CONSTRAINT CK_TriageQueue_Times
CHECK (QueueTime <= ISNULL(CalledTime, GETDATE()) AND
       (CompletedTime IS NULL OR
        ISNULL(CalledTime, QueueTime) <= CompletedTime));

-- 5. اضافه کردن ایندکس‌های ترکیبی برای Performance
-- TriageVitalSigns
CREATE INDEX IX_TriageVitalSigns_Assessment_Time_IsDeleted
ON dbo.TriageVitalSigns(TriageAssessmentId, MeasurementTime, IsDeleted);

CREATE INDEX IX_TriageVitalSigns_Normal_Immediate_IsDeleted
ON dbo.TriageVitalSigns(IsNormal, RequiresImmediateAttention, IsDeleted);

-- TriageProtocol
CREATE INDEX IX_TriageProtocol_Type_Level_Active_IsDeleted
ON dbo.TriageProtocols(Type, TargetLevel, IsActive, IsDeleted);

CREATE INDEX IX_TriageProtocol_Validity_Active_IsDeleted
ON dbo.TriageProtocols(ValidFrom, ValidTo, IsActive, IsDeleted);

CREATE INDEX IX_TriageProtocol_Priority_Mandatory_Active_IsDeleted
ON dbo.TriageProtocols(Priority, IsMandatory, IsActive, IsDeleted);

-- TriageQueue
CREATE INDEX IX_TriageQueue_Status_Priority_IsDeleted
ON dbo.TriageQueues(Status, Priority, IsDeleted);

CREATE INDEX IX_TriageQueue_Time_Status_IsDeleted
ON dbo.TriageQueues(QueueTime, Status, IsDeleted);

CREATE INDEX IX_TriageQueue_Patient_Status_IsDeleted
ON dbo.TriageQueues(PatientId, Status, IsDeleted);

CREATE INDEX IX_TriageQueue_Immediate_Status_IsDeleted
ON dbo.TriageQueues(RequiresImmediateCare, Status, IsDeleted);

-- TriageReassessment
CREATE INDEX IX_TriageReassessment_Assessment_Time_IsDeleted
ON dbo.TriageReassessments(TriageAssessmentId, At, IsDeleted);

CREATE INDEX IX_TriageReassessment_Level_Reason_IsDeleted
ON dbo.TriageReassessments(NewLevel, ReassessmentReason, IsDeleted);

-- 6. اضافه کردن ایندکس برای NextReassessmentDueAt
CREATE INDEX IX_TriageQueues_NextReassessmentDueAt
ON dbo.TriageQueues(NextReassessmentDueAt)
WHERE NextReassessmentDueAt IS NOT NULL;

-- 7. اضافه کردن ایندکس برای ChiefComplaintCode
CREATE INDEX IX_TriageAssessments_ChiefComplaintCode
ON dbo.TriageAssessments(ChiefComplaintCode)
WHERE ChiefComplaintCode IS NOT NULL;

-- 8. اضافه کردن ایندکس برای Isolation
CREATE INDEX IX_TriageAssessments_Isolation
ON dbo.TriageAssessments(Isolation)
WHERE Isolation IS NOT NULL;

-- 9. اضافه کردن ایندکس برای Red Flags
CREATE INDEX IX_TriageAssessments_RedFlags
ON dbo.TriageAssessments(RedFlag_Sepsis, RedFlag_Stroke, RedFlag_ACS, RedFlag_Trauma, IsPregnant)
WHERE RedFlag_Sepsis = 1 OR RedFlag_Stroke = 1 OR RedFlag_ACS = 1 OR RedFlag_Trauma = 1 OR IsPregnant = 1;

-- 10. اضافه کردن ایندکس برای GCS
CREATE INDEX IX_TriageVitalSigns_GCS
ON dbo.TriageVitalSigns(GcsE, GcsV, GcsM)
WHERE GcsE IS NOT NULL OR GcsV IS NOT NULL OR GcsM IS NOT NULL;

-- 11. اضافه کردن ایندکس برای اکسیژن
CREATE INDEX IX_TriageVitalSigns_Oxygen
ON dbo.TriageVitalSigns(OnOxygen, OxygenDevice, O2FlowLpm)
WHERE OnOxygen = 1 OR OxygenDevice IS NOT NULL OR O2FlowLpm IS NOT NULL;

-- 12. اضافه کردن ایندکس برای NEWS2/PEWS
CREATE INDEX IX_TriageAssessments_Scoring
ON dbo.TriageAssessments(EsiScore, News2Score, PewsScore)
WHERE EsiScore IS NOT NULL OR News2Score IS NOT NULL OR PewsScore IS NOT NULL;

PRINT 'تمامی اصلاحات Production-Ready برای ماژول تریاژ با موفقیت اعمال شد.';
