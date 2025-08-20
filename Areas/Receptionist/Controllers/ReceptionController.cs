//using ClinicApp.Interfaces;
//using ClinicApp.Models; // For AppRoles
//using ClinicApp.Models.Entities;
//using ClinicApp.ViewModels;
//using Microsoft.AspNet.Identity;
//using Serilog;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace ClinicApp.Areas.Receptionist.Controllers
//{
//    [Authorize(Roles = AppRoles.Receptionist + "," + AppRoles.Admin)]
//    public class ReceptionController : Controller
//    {
//        private readonly IReceptionService _receptionService;
//        private readonly IPatientService _patientService;
//        private readonly IDoctorService _doctorService; // Assuming a service to get doctors
//        private readonly IServiceService _serviceService; // Assuming a service to get services
//        private readonly ILogger _log;

//        public ReceptionController(
//            IReceptionService receptionService,
//            IPatientService patientService,
//            IDoctorService doctorService,
//            IServiceService serviceService,
//            ILogger logger)
//        {
//            _receptionService = receptionService;
//            _patientService = patientService;
//            _doctorService = doctorService;
//            _serviceService = serviceService;
//            _log = logger.ForContext<ReceptionController>();
//        }

//        #region Reception List (Index)

//        // GET: Receptionist/Reception
//        public ActionResult Index()
//        {
//            // The main view container. Data will be loaded via AJAX.
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<PartialViewResult> LoadReceptions(ReceptionSearchViewModel searchModel)
//        {
//            // Set default pagination if not provided
//            searchModel.PageNumber = searchModel.PageNumber > 0 ? searchModel.PageNumber : 1;
//            searchModel.PageSize = searchModel.PageSize > 0 ? searchModel.PageSize : 10;

//            var result = await _receptionService.SearchReceptionsAsync(searchModel);

//            if (result.Success)
//            {
//                return PartialView("_ReceptionListPartial", result.Data);
//            }

//            // Return an empty partial to prevent UI errors
//            return PartialView("_ReceptionListPartial", new Helpers.PagedResult<ReceptionIndexViewModel>());
//        }

//        #endregion

//        #region Create Reception

//        // GET: Receptionist/Reception/Create
//        public async Task<ActionResult> Create()
//        {
//            var model = new ReceptionCreateViewModel();
//            await PopulateDropdowns(model);
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Create(ReceptionCreateViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                await PopulateDropdowns(model); // Repopulate dropdowns on error
//                return View(model);
//            }

//            var receptionistId = User.Identity.GetUserId();
//            var result = await _receptionService.CreateReceptionAsync(model, receptionistId);

//            if (result.Success)
//            {
//                TempData["SuccessMessage"] = result.Message;
//                // Redirect to a receipt/details page for the new reception
//                return RedirectToAction("Details", new { id = result.Data });
//            }

//            ModelState.AddModelError("", result.Message);
//            await PopulateDropdowns(model);
//            return View(model);
//        }

//        #endregion

//        #region Details

//        // GET: Receptionist/Reception/Details/5
//        public async Task<ActionResult> Details(int id)
//        {
//            var result = await _receptionService.GetReceptionDetailsAsync(id);
//            if (result.Success)
//            {
//                return View(result.Data);
//            }
//            return HttpNotFound();
//        }

//        #endregion

//        #region AJAX Helpers

//        // This action is called by JavaScript on the Create form to find a patient
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<JsonResult> FindPatient(string nationalCode)
//        {
//            if (string.IsNullOrWhiteSpace(nationalCode))
//            {
//                return Json(new { success = false, message = "Please enter a national code." });
//            }

//            var result = await _patientService.FindPatientByNationalCodeAsync(nationalCode);

//            if (result.Success)
//            {
//                // Return only the necessary data to the client
//                var patientData = new
//                {
//                    id = result.Data.PatientId,
//                    fullName = $"{result.Data.FirstName} {result.Data.LastName}",
//                    phoneNumber = result.Data.PhoneNumber
//                };
//                return Json(new { success = true, patient = patientData });
//            }

//            return Json(new { success = false, message = result.Message });
//        }

//        #endregion

//        #region Private Methods

//        // Helper to populate dropdowns for Create/Edit forms
//        private async Task PopulateDropdowns(ReceptionCreateViewModel model)
//        {
//            var doctors = await _doctorService.GetAllDoctorsAsync(); // Assumes this method exists
//            var services = await _serviceService.GetAllServicesAsync(); // Assumes this method exists

//            model.DoctorList = doctors.Select(d => new SelectListItem { Value = d.DoctorId.ToString(), Text = $"{d.FirstName} {d.LastName}" });
//            model.ServiceList = services.Select(s => new SelectListItem { Value = s.ServiceId.ToString(), Text = $"{s.Title} ({s.Price:N0})" });
//        }

//        #endregion
//    }
//}