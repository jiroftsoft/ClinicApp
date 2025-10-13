using System;
using System.Collections.Generic;
using System.Linq;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// مدل نمایش گزارش یکپارچه تریاژ و پذیرش
    /// </summary>
    public class IntegratedReportViewModel
    {
        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// آمار تریاژ
        /// </summary>
        public List<TriageAssessment> TriageStats { get; set; } = new List<TriageAssessment>();

        /// <summary>
        /// آمار پذیرش
        /// </summary>
        public List<ReceptionIndexViewModel> ReceptionStats { get; set; } = new List<ReceptionIndexViewModel>();

        /// <summary>
        /// آمار یکپارچه
        /// </summary>
        public IntegratedStatsViewModel IntegratedStats { get; set; } = new IntegratedStatsViewModel();

        /// <summary>
        /// محاسبه آمار یکپارچه
        /// </summary>
        public void CalculateIntegratedStats()
        {
            IntegratedStats = new IntegratedStatsViewModel
            {
                TotalTriageAssessments = TriageStats.Count,
                TotalReceptions = ReceptionStats.Count,
                EmergencyTriageCount = TriageStats.Count(t => t.Level == TriageLevel.ESI1 || t.Level == TriageLevel.ESI2),
                EmergencyReceptionCount = ReceptionStats.Count(r => r.IsEmergency),
                AverageTriageTime = CalculateAverageTriageTime(),
                AverageReceptionTime = CalculateAverageReceptionTime(),
                ConversionRate = CalculateConversionRate(),
                TopComplaints = GetTopComplaints(),
                DepartmentDistribution = GetDepartmentDistribution(),
                TimeDistribution = GetTimeDistribution()
            };
        }

        /// <summary>
        /// محاسبه میانگین زمان تریاژ
        /// </summary>
        private double CalculateAverageTriageTime()
        {
            var validAssessments = TriageStats.Where(t => t.TriageStartAt != null && t.TriageEndAt != null);
            if (!validAssessments.Any()) return 0;

            var totalMinutes = validAssessments.Sum(t => (t.TriageEndAt - t.TriageStartAt).Value.TotalMinutes);
            return totalMinutes / validAssessments.Count();
        }

        /// <summary>
        /// محاسبه میانگین زمان پذیرش
        /// </summary>
        private double CalculateAverageReceptionTime()
        {
            var validReceptions = ReceptionStats.Where(r => !string.IsNullOrEmpty(r.ReceptionDate));
            if (!validReceptions.Any()) return 0;

            var totalMinutes = validReceptions.Sum(r => (DateTime.UtcNow - DateTime.Parse(r.ReceptionDate)).TotalMinutes);
            return totalMinutes / validReceptions.Count();
        }

        /// <summary>
        /// محاسبه نرخ تبدیل تریاژ به پذیرش
        /// </summary>
        private double CalculateConversionRate()
        {
            if (TriageStats.Count == 0) return 0;
            return (double)ReceptionStats.Count / TriageStats.Count * 100;
        }

        /// <summary>
        /// دریافت شکایت‌های برتر
        /// </summary>
        private List<ComplaintStats> GetTopComplaints()
        {
            return TriageStats
                .GroupBy(t => t.ChiefComplaint)
                .Select(g => new ComplaintStats
                {
                    Complaint = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / TriageStats.Count * 100
                })
                .OrderByDescending(c => c.Count)
                .Take(10)
                .ToList();
        }

        /// <summary>
        /// دریافت توزیع بخش‌ها
        /// </summary>
        private List<DepartmentStats> GetDepartmentDistribution()
        {
            return ReceptionStats
                .GroupBy(r => r.DepartmentName)
                .Select(g => new DepartmentStats
                {
                    DepartmentName = g.Key,
                    Count = g.Count(),
                    Percentage = (double)g.Count() / ReceptionStats.Count * 100
                })
                .OrderByDescending(d => d.Count)
                .ToList();
        }

        /// <summary>
        /// دریافت توزیع زمانی
        /// </summary>
        private List<TimeStats> GetTimeDistribution()
        {
            var hourlyStats = new List<TimeStats>();
            
            for (int hour = 0; hour < 24; hour++)
            {
                var hourReceptions = ReceptionStats.Count(r => !string.IsNullOrEmpty(r.ReceptionDate) && DateTime.Parse(r.ReceptionDate).Hour == hour);
                var hourTriage = TriageStats.Count(t => t.TriageStartAt != null && t.TriageStartAt.Hour == hour);
                
                hourlyStats.Add(new TimeStats
                {
                    Hour = hour,
                    ReceptionCount = hourReceptions,
                    TriageCount = hourTriage,
                    TotalCount = hourReceptions + hourTriage
                });
            }

            return hourlyStats.OrderByDescending(t => t.TotalCount).ToList();
        }
    }

    /// <summary>
    /// آمار یکپارچه
    /// </summary>
    public class IntegratedStatsViewModel
    {
        /// <summary>
        /// تعداد کل ارزیابی‌های تریاژ
        /// </summary>
        public int TotalTriageAssessments { get; set; }

        /// <summary>
        /// تعداد کل پذیرش‌ها
        /// </summary>
        public int TotalReceptions { get; set; }

        /// <summary>
        /// تعداد تریاژ اورژانس
        /// </summary>
        public int EmergencyTriageCount { get; set; }

        /// <summary>
        /// تعداد پذیرش اورژانس
        /// </summary>
        public int EmergencyReceptionCount { get; set; }

        /// <summary>
        /// میانگین زمان تریاژ (دقیقه)
        /// </summary>
        public double AverageTriageTime { get; set; }

        /// <summary>
        /// میانگین زمان پذیرش (دقیقه)
        /// </summary>
        public double AverageReceptionTime { get; set; }

        /// <summary>
        /// نرخ تبدیل تریاژ به پذیرش (%)
        /// </summary>
        public double ConversionRate { get; set; }

        /// <summary>
        /// شکایت‌های برتر
        /// </summary>
        public List<ComplaintStats> TopComplaints { get; set; } = new List<ComplaintStats>();

        /// <summary>
        /// توزیع بخش‌ها
        /// </summary>
        public List<DepartmentStats> DepartmentDistribution { get; set; } = new List<DepartmentStats>();

        /// <summary>
        /// توزیع زمانی
        /// </summary>
        public List<TimeStats> TimeDistribution { get; set; } = new List<TimeStats>();
    }

    /// <summary>
    /// آمار شکایت‌ها
    /// </summary>
    public class ComplaintStats
    {
        /// <summary>
        /// شکایت
        /// </summary>
        public string Complaint { get; set; }

        /// <summary>
        /// تعداد
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// درصد
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// آمار بخش‌ها
    /// </summary>
    public class DepartmentStats
    {
        /// <summary>
        /// نام بخش
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// تعداد
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// درصد
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// آمار زمانی
    /// </summary>
    public class TimeStats
    {
        /// <summary>
        /// ساعت
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// تعداد پذیرش
        /// </summary>
        public int ReceptionCount { get; set; }

        /// <summary>
        /// تعداد تریاژ
        /// </summary>
        public int TriageCount { get; set; }

        /// <summary>
        /// تعداد کل
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// ساعت به صورت متن
        /// </summary>
        public string HourText => $"{Hour:00}:00";
    }
}
