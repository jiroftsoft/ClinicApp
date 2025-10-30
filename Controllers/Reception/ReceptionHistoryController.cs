using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Models;
using System.Data.Entity;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Controllers.Reception
{
    [Authorize]
    [NoCache]
    public class ReceptionHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceptionHistoryController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult> Index(string nationalCode = null, int page = 1, int pageSize = 20)
        {
            var q = _context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.ReceptionDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nationalCode))
            {
                q = q.Where(r => r.Patient.NationalCode == nationalCode);
            }

            var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new ReceptionHistoryItemVM
                {
                    ReceptionId = r.ReceptionId,
                    ReceptionDate = r.ReceptionDate,
                    PatientName = r.Patient.FirstName + " " + r.Patient.LastName,
                    NationalCode = r.Patient.NationalCode,
                    DoctorName = r.Doctor.FirstName + " " + r.Doctor.LastName,
                    TotalAmount = r.TotalAmount
                }).ToListAsync();

            return View(items);
        }
    }

    public class ReceptionHistoryItemVM
    {
        public int ReceptionId { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string PatientName { get; set; }
        public string NationalCode { get; set; }
        public string DoctorName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}



