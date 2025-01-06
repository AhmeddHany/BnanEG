using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS.Employees
{
    public class EmployeesValiditesVM
    {
        public string? CrMasUserInformationCode { get; set; }
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
        public List<CrMasSysMainTask>? MainTasks { get; set; }
        public List<CrMasUserMainValidation>? MainValidation { get; set; }

        public List<CrMasSysSubTask>? SubTasks { get; set; }
        public List<CrMasUserSubValidation>? SubValidation { get; set; }

        public List<CrMasSysProceduresTask>? ProceduresTasks { get; set; }
        public List<CrMasUserProceduresValidation>? ProceduresValidation { get; set; }


    }
}
