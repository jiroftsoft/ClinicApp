using System.ComponentModel;

namespace ClinicApp.Models.Entities;

using System.ComponentModel;

public enum AppointmentType
{
    [Description("ویزیت عمومی")]
    GeneralVisit = 1,

    [Description("ویزیت تخصصی")]
    SpecialistVisit = 2,

    [Description("ویزیت فوق‌تخصصی")]
    SubSpecialistVisit = 3,

    [Description("معاینه اولیه")]
    InitialExamination = 4,

    [Description("معاینه پیگیری")]
    FollowUp = 5,

    [Description("اقدامات درمانی")]
    MedicalProcedure = 6,

    [Description("اورژانس")]
    Emergency = 7,

    [Description("تزریق")]
    Injection = 8,

    [Description("واکسیناسیون")]
    Vaccination = 9,

    [Description("آزمایش")]
    Laboratory = 10,

    [Description("تصویربرداری")]
    Imaging = 11,

    [Description("مشاوره")]
    Consultation = 12,

    [Description("کنسلی")]
    Cancellation = 99
}