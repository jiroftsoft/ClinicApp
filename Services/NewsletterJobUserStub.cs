using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Services
{
    /// <summary>
    /// استاب ICurrentUserService برای Job ارسال کمپین خبرنامه (بدون HttpContext).
    /// </summary>
    internal sealed class NewsletterJobUserStub : ICurrentUserService
    {
        public string UserId => null;
        public string UserName => "System";
        public bool IsAuthenticated => false;
        public bool IsAdmin => false;
        public bool IsDoctor => false;
        public bool IsReceptionist => false;
        public bool IsPatient => false;
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        public ClaimsPrincipal ClaimsPrincipal => null;
        public IEnumerable<string> Roles => Array.Empty<string>();

        public bool IsDevelopmentEnvironment() => false;
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
        public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class => false;
        public Task<bool> HasAccessToServiceAsync(int serviceId) => Task.FromResult(false);
        public Task<bool> HasAccessToInsuranceAsync(int insuranceId) => Task.FromResult(false);
        public Task<bool> HasAccessToDepartmentAsync(int departmentId) => Task.FromResult(false);
        public Task<Doctor> GetDoctorInfoAsync() => Task.FromResult<Doctor>(null);
        public Task<Models.Entities.Patient.Patient> GetPatientInfoAsync() => Task.FromResult<Models.Entities.Patient.Patient>(null);
        public string GetSystemUserId() => null;
        public Task<List<Department>> GetDoctorActiveDepartmentsAsync() => Task.FromResult(new List<Department>());
        public Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync() => Task.FromResult(new List<ServiceCategory>());
        public Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId) => Task.FromResult(false);
        public Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId) => Task.FromResult(false);
        public Task<string> GetDoctorRoleInDepartmentAsync(int departmentId) => Task.FromResult<string>(null);
        public string[] GetUserRoles() => Array.Empty<string>();
        public string GetCurrentUserId() => null;
        public string GetCurrentUserName() => "System";
    }
}
