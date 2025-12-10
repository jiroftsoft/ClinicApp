using System;
using System.Collections.Generic;
using System.Linq;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Helpers
{
    /// <summary>
    /// Helper Class برای تولید اسلات‌های زمانی
    /// 
    /// مسئولیت (SRP):
    /// - تولید اسلات‌های زمانی
    /// - تقسیم بازه زمانی به اسلات‌ها
    /// - مدیریت فاصله بین اسلات‌ها
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط تولید اسلات‌ها
    /// - Static Methods: بدون state، thread-safe
    /// </summary>
    public static class TimeSlotGenerator
    {
        /// <summary>
        /// تولید اسلات‌های زمانی برای یک بازه زمانی
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="appointmentDuration">مدت زمان هر نوبت (دقیقه)</param>
        /// <param name="doctorName">نام پزشک</param>
        /// <returns>لیست اسلات‌های زمانی</returns>
        public static List<TimeSlotViewModel> GenerateTimeSlots(
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            int appointmentDuration, 
            string doctorName = "نامشخص")
        {
            var slots = new List<TimeSlotViewModel>();

            if (startTime >= endTime || appointmentDuration <= 0)
            {
                return slots;
            }

            var currentTime = startTime;
            var slotId = 0;

            while (currentTime < endTime)
            {
                var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(appointmentDuration));

                if (slotEndTime <= endTime)
                {
                    slots.Add(new TimeSlotViewModel
                    {
                        SlotId = slotId++,
                        SlotDate = date,
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        Duration = appointmentDuration,
                        Price = 0, // در حال حاضر ثابت
                        Status = "Available",
                        IsAvailable = true,
                        IsEmergencySlot = false,
                        IsWalkInAllowed = false,
                        Priority = "عادی",
                        DoctorName = doctorName,
                        Specialization = "نامشخص",
                        ClinicName = "نامشخص",
                        ClinicAddress = "نامشخص"
                    });
                }

                currentTime = slotEndTime;
            }

            return slots;
        }

        /// <summary>
        /// تولید اسلات‌های زمانی با در نظر گیری زمان استراحت
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="appointmentDuration">مدت زمان هر نوبت (دقیقه)</param>
        /// <param name="breakTimes">لیست زمان‌های استراحت</param>
        /// <param name="doctorName">نام پزشک</param>
        /// <returns>لیست اسلات‌های زمانی</returns>
        public static List<TimeSlotViewModel> GenerateTimeSlotsWithBreaks(
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            int appointmentDuration,
            List<BreakTimeSlot> breakTimes,
            string doctorName = "نامشخص")
        {
            var slots = new List<TimeSlotViewModel>();

            if (startTime >= endTime || appointmentDuration <= 0)
            {
                return slots;
            }

            var currentTime = startTime;
            var slotId = 0;

            while (currentTime < endTime)
            {
                // بررسی اینکه آیا در زمان استراحت هستیم یا نه
                var isBreakTime = breakTimes?.Any(bt => 
                    currentTime >= bt.StartTime && currentTime < bt.EndTime) ?? false;

                if (isBreakTime)
                {
                    // پیدا کردن زمان پایان استراحت
                    var breakTime = breakTimes.FirstOrDefault(bt => 
                        currentTime >= bt.StartTime && currentTime < bt.EndTime);
                    
                    if (breakTime != null)
                    {
                        currentTime = breakTime.EndTime;
                        continue;
                    }
                }

                var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(appointmentDuration));

                if (slotEndTime <= endTime)
                {
                    // بررسی اینکه آیا اسلات با زمان استراحت تداخل دارد
                    var conflictsWithBreak = breakTimes?.Any(bt => 
                        (currentTime < bt.EndTime && slotEndTime > bt.StartTime)) ?? false;

                    if (!conflictsWithBreak)
                    {
                        slots.Add(new TimeSlotViewModel
                        {
                            SlotId = slotId++,
                            SlotDate = date,
                            StartTime = currentTime,
                            EndTime = slotEndTime,
                            Duration = appointmentDuration,
                            Price = 0,
                            Status = "Available",
                            IsAvailable = true,
                            IsEmergencySlot = false,
                            IsWalkInAllowed = false,
                            Priority = "عادی",
                            DoctorName = doctorName,
                            Specialization = "نامشخص",
                            ClinicName = "نامشخص",
                            ClinicAddress = "نامشخص"
                        });
                    }
                }

                currentTime = slotEndTime;
            }

            return slots;
        }

        /// <summary>
        /// تقسیم بازه زمانی به بخش‌های مساوی
        /// </summary>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="numberOfSlots">تعداد اسلات</param>
        /// <returns>لیست زمان‌های شروع اسلات</returns>
        public static List<TimeSpan> DivideTimeRange(TimeSpan startTime, TimeSpan endTime, int numberOfSlots)
        {
            var times = new List<TimeSpan>();

            if (startTime >= endTime || numberOfSlots <= 0)
            {
                return times;
            }

            var totalMinutes = (endTime - startTime).TotalMinutes;
            var slotDuration = totalMinutes / numberOfSlots;

            for (int i = 0; i < numberOfSlots; i++)
            {
                var slotStart = startTime.Add(TimeSpan.FromMinutes(i * slotDuration));
                times.Add(slotStart);
            }

            return times;
        }
    }
}

