using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای لود داده‌های Layout به صورت Strongly-Typed
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public static class LayoutDataHelper
    {
        /// <summary>
        /// دریافت Stories برای Layout (Strongly-Typed)
        /// </summary>
        public static List<StoryPublicViewModel> GetStories()
        {
            try
            {
                var storyService = DependencyResolver.Current.GetService<IStoryService>();
                if (storyService == null)
                {
                    return new List<StoryPublicViewModel>();
                }

                // لود Stories به صورت Async (اما باید Synchronous باشد)
                var storiesTask = Task.Run(async () => await storyService.GetActiveStoriesForPublicAsync());
                storiesTask.Wait(); // Wait for completion

                var storiesResult = storiesTask.Result;
                if (storiesResult.Success && storiesResult.Data != null && storiesResult.Data.Any())
                {
                    return storiesResult.Data;
                }

                return new List<StoryPublicViewModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in LayoutDataHelper.GetStories: {ex.Message}");
                return new List<StoryPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت Footer برای Layout (Strongly-Typed)
        /// </summary>
        public static FooterViewModel GetFooter()
        {
            try
            {
                var homePageService = DependencyResolver.Current.GetService<IHomePageService>();
                if (homePageService == null)
                {
                    return null;
                }

                // لود Footer به صورت Async (اما باید Synchronous باشد)
                var footerTask = Task.Run(async () => await homePageService.GetFooterDataAsync());
                footerTask.Wait(); // Wait for completion

                return footerTask.Result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in LayoutDataHelper.GetFooter: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// دریافت Emergency Contacts برای Layout (Strongly-Typed)
        /// </summary>
        public static List<EmergencyContactPublicViewModel> GetEmergencyContacts()
        {
            try
            {
                var emergencyContactService = DependencyResolver.Current.GetService<IEmergencyContactService>();
                if (emergencyContactService == null)
                {
                    return new List<EmergencyContactPublicViewModel>();
                }

                // لود Emergency Contacts به صورت Async (اما باید Synchronous باشد)
                var contactsTask = Task.Run(async () => await emergencyContactService.GetActiveContactsAsync());
                contactsTask.Wait(); // Wait for completion

                var contactsResult = contactsTask.Result;
                if (contactsResult != null && contactsResult.Success && contactsResult.Data != null && contactsResult.Data.Any())
                {
                    return contactsResult.Data;
                }

                return new List<EmergencyContactPublicViewModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in LayoutDataHelper.GetEmergencyContacts: {ex.Message}");
                return new List<EmergencyContactPublicViewModel>();
            }
        }

        /// <summary>
        /// دریافت LayoutViewModel کامل (Strongly-Typed)
        /// </summary>
        public static LayoutViewModel GetLayoutData()
        {
            return new LayoutViewModel
            {
                Stories = GetStories(),
                Footer = GetFooter(),
                EmergencyContacts = GetEmergencyContacts()
            };
        }
    }
}
