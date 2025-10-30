using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models;
using Serilog;
using PatientEntity = ClinicApp.Models.Entities.Patient.Patient;

namespace ClinicApp.Repositories.Patient
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PatientRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<PatientRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PatientEntity> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted);
        }

        public async Task<PatientEntity> GetPatientByNationalCodeAsync(string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode)) return null;
            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.NationalCode == nationalCode && !p.IsDeleted);
        }

        public async Task<List<PatientEntity>> SearchPatientsAsync(string keyword, int pageNumber, int pageSize)
        {
            keyword = (keyword ?? "").Trim();
            var query = _context.Patients.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.FirstName.Contains(keyword) ||
                                         p.LastName.Contains(keyword) ||
                                         p.NationalCode.Contains(keyword) ||
                                         p.PhoneNumber.Contains(keyword));
            }
            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(Math.Max(0, (pageNumber - 1) * pageSize))
                .Take(Math.Max(1, pageSize))
                .ToListAsync();
        }

        public async Task<PatientEntity> CreatePatientAsync(PatientEntity patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<PatientEntity> UpdatePatientAsync(PatientEntity patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<bool> PatientExistsByNationalCodeAsync(string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode)) return false;
            return await _context.Patients.AnyAsync(p => p.NationalCode == nationalCode && !p.IsDeleted);
        }

        public async Task<int> GetPatientCountAsync(string keyword = null)
        {
            keyword = (keyword ?? "").Trim();
            var query = _context.Patients.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.FirstName.Contains(keyword) ||
                                         p.LastName.Contains(keyword) ||
                                         p.NationalCode.Contains(keyword) ||
                                         p.PhoneNumber.Contains(keyword));
            }
            return await query.CountAsync();
        }
    }
}


