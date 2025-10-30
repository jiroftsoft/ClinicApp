using System.Collections.Generic;

namespace ClinicApp.Dtos.Reception
{
    public class UpdateDraftRequest
    {
        public int ReceptionId { get; set; }
        public int? ClinicId { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
    }
}


