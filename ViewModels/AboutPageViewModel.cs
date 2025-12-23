using System.Collections.Generic;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای صفحه "درباره ما" - Production-Grade
    /// طراحی شده طبق استانداردهای کلینیک درمانی
    /// </summary>
    public class AboutPageViewModel
    {
        #region 1. معرفی کلینیک (Hero Section)

        public string ClinicName { get; set; }
        public string ClinicDescription { get; set; }
        public string EstablishedYear { get; set; }

        #endregion

        #region 2. مأموریت و رویکرد درمانی

        public List<MissionValueViewModel> MissionValues { get; set; } = new List<MissionValueViewModel>();

        #endregion

        #region 3. مجوزها و اعتبارها

        public List<LicenseViewModel> Licenses { get; set; } = new List<LicenseViewModel>();
        public string RegulatoryBody { get; set; }

        #endregion

        #region 4. کادر درمان و تخصص‌ها

        public int DoctorCount { get; set; }
        public List<SpecializationSummaryViewModel> Specializations { get; set; } = new List<SpecializationSummaryViewModel>();
        public string MedicalTeamDescription { get; set; }

        #endregion

        #region 5. تجهیزات و زیرساخت‌ها

        public int EquipmentCount { get; set; }
        public List<EquipmentCategoryViewModel> EquipmentCategories { get; set; } = new List<EquipmentCategoryViewModel>();
        public string InfrastructureDescription { get; set; }

        #endregion

        #region 6. تعهد به اخلاق پزشکی

        public List<EthicalCommitmentViewModel> EthicalCommitments { get; set; } = new List<EthicalCommitmentViewModel>();

        #endregion
    }

    #region Supporting ViewModels

    /// <summary>
    /// ViewModel برای مأموریت و ارزش‌ها
    /// </summary>
    public class MissionValueViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    /// <summary>
    /// ViewModel برای مجوزها
    /// </summary>
    public class LicenseViewModel
    {
        public string Title { get; set; }
        public string IssuingAuthority { get; set; }
        public string LicenseNumber { get; set; }
        public string ValidUntil { get; set; }
    }

    /// <summary>
    /// ViewModel برای خلاصه تخصص‌ها
    /// </summary>
    public class SpecializationSummaryViewModel
    {
        public string Name { get; set; }
        public int DoctorCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای دسته‌بندی تجهیزات
    /// </summary>
    public class EquipmentCategoryViewModel
    {
        public string CategoryName { get; set; }
        public int EquipmentCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای تعهدات اخلاقی
    /// </summary>
    public class EthicalCommitmentViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    #endregion
}
