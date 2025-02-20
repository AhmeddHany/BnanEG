using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Date_Report_AdminstrativeProcedures_CasVM
    {
        public DateTime? dates { get; set; }
    }
    public class listReport_AdminstrativeProcedures_CasVM
    {
        public List<Report_AdminstrativeProcedures_CasVM> all_Adminstrative_procedures = new List<Report_AdminstrativeProcedures_CasVM>();
        //public List<CrMasLessorInformation> all_lessors = new List<CrMasLessorInformation>();
        //public List<CrMasRenterInformation> all_RentersMas = new List<CrMasRenterInformation>();
        public List<cas_list_String_4> all_Invoices = new List<cas_list_String_4>();
        
        public string? start_Date { get; set; }
        public string? end_Date { get; set; }

    }

    public class Report_AdminstrativeProcedures_CasVM
    {
        //public string? branchAr { get; set; }
        //public string? branchEn { get; set; }

        public string CrCasSysAdministrativeProceduresNo { get; set; } = null!;
        //public string? CrCasSysAdministrativeProceduresYear { get; set; }
        //public string? CrCasSysAdministrativeProceduresSector { get; set; }
        public string? CrCasSysAdministrativeProceduresCode { get; set; }
        public string? CrCasSysAdministrativeProceduresClassification { get; set; }
        public string? CrCasSysAdministrativeProceduresLessor { get; set; }
        //public string? CrCasSysAdministrativeProceduresBranch { get; set; }
        public DateTime? CrCasSysAdministrativeProceduresDate { get; set; }
        public TimeSpan? CrCasSysAdministrativeProceduresTime { get; set; }
        public string? CrCasSysAdministrativeProceduresTargeted { get; set; }
        //public decimal? CrCasSysAdministrativeProceduresDebit { get; set; }
        //public decimal? CrCasSysAdministrativeProceduresCreditor { get; set; }
        public string? CrCasSysAdministrativeProceduresDocNo { get; set; }
        //public DateTime? CrCasSysAdministrativeProceduresDocDate { get; set; }
        //public DateTime? CrCasSysAdministrativeProceduresDocStartDate { get; set; }
        //public DateTime? CrCasSysAdministrativeProceduresDocEndDate { get; set; }
        public string? CrCasSysAdministrativeProceduresCarFrom { get; set; }
        public string? CrCasSysAdministrativeProceduresCarTo { get; set; }
        public string? CrCasSysAdministrativeProceduresUserInsert { get; set; }
        public string? CrCasSysAdministrativeProceduresArDescription { get; set; }
        public string? CrCasSysAdministrativeProceduresEnDescription { get; set; }
        public string? CrCasSysAdministrativeProceduresStatus { get; set; }
        public string? CrCasSysAdministrativeProceduresReasons { get; set; }


        ////////////////
        public string? UserArName { get; set; }
        public string? UserEnName { get; set; }
        public string? procedure_ArName { get; set; }
        public string? procedure_EnName { get; set; }
        public string? Target_ArName { get; set; }
        public string? Target_EnName { get; set; }

        //public string? LessorArName { get; set; }
        //public string? LessorEnName { get; set; }
        //public string? ProffessionArName { get; set; }
        //public string? ProffessionEnName { get; set; }
        //public string? WorkPlaceArName { get; set; }
        //public string? WorkPlaceEnName { get; set; }
        //public string? DrivingLicesnseArName { get; set; }
        //public string? DrivingLicesnseEnName { get; set; }
        //public string? BankArName { get; set; }
        //public string? BankEnName { get; set; }
        //public string? IbanArName { get; set; }
        //public string? IbanEnName { get; set; }
        //public string? addressArName { get; set; }
        //public string? addressEnName { get; set; }
        /// <summary>
        /// ////////////
        /// </summary>

    }
}
