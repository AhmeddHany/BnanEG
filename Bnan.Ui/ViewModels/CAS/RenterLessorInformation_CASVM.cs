using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{

    public class RenterLessorInformation_CASVM
    {
        public List<RenterLessorInformation_SingleVM>? all_RentersData = new List<RenterLessorInformation_SingleVM>();
        public RenterLessorInformation_Single_Edit_VM? This_RentersData_edit = new RenterLessorInformation_Single_Edit_VM();
        public List<CrMasSysEvaluation>? all_Rates = new List<CrMasSysEvaluation>();

        public string? CrCasRenterLessorId { get; set; }
        public string? CrCasRenterLessorCode { get; set; }
        public string? CrCasRenterLessorDealingMechanism { get; set; }
        public string? CrCasRenterLessorStatus { get; set; }
        public string? CrCasRenterLessorReasons { get; set; }
        public string? RenterNameAr { get; set; }
        public string? RenterNameEn { get; set; }

        //public List<CrCasRenterLessor> all_RentersData = new List<CrCasRenterLessor>();
        
        public string? start_Date { get; set; }
        public string? end_Date { get; set; }

    }

    public class RenterLessorInformation_SingleVM
    {
        public string? Email { get; set; }
        public string? addressAr { get; set; }
        public string? addressEn { get; set; }
        public string? RateAr { get; set; }
        public string? RateEn { get; set; }
        public string? jobAr { get; set; }
        public string? jobEn { get; set; }
        public string? nationalityAr { get; set; }
        public string? nationalityEn { get; set; }
        public string? RenterNameAr { get; set; }
        public string? RenterNameEn { get; set; }

        public string? WorkPlaceAr { get; set; }
        public string? WorkPlaceEn { get; set; }

        public string? CrCasRenterLessorId { get; set; }
        public string? CrCasRenterLessorCode { get; set; }
        //public string? CrCasRenterLessorSector { get; set; }
        public int? CrCasRenterLessorCopyId { get; set; }
        public string? CrCasRenterLessorIdtrype { get; set; }
        //public string? CrCasRenterLessorMembership { get; set; }
        public DateTime? CrCasRenterLessorDateFirstInteraction { get; set; }
        public DateTime? CrCasRenterLessorDateLastContractual { get; set; }
        public int? CrCasRenterLessorContractCount { get; set; }
        //public int? CrCasRenterLessorContractExtension { get; set; }
        //public int? CrCasRenterLessorContractDays { get; set; }
        //public int? CrCasRenterLessorContractKm { get; set; }
        //public decimal? CrCasRenterLessorContractTradedAmount { get; set; }
        //public decimal? CrCasRenterLessorBalance { get; set; }
        //public decimal? CrCasRenterLessorAvailableBalance { get; set; }
        //public decimal? CrCasRenterLessorReservedBalance { get; set; }
        public int? CrCasRenterLessorEvaluationNumber { get; set; }
        public int? CrCasRenterLessorEvaluationTotal { get; set; }
        public decimal? CrCasRenterLessorEvaluationValue { get; set; }
        //public string? CrCasRenterLessorStatisticsNationalities { get; set; }
        //public string? CrCasRenterLessorStatisticsGender { get; set; }
        //public string? CrCasRenterLessorStatisticsJobs { get; set; }
        //public string? CrCasRenterLessorStatisticsRegions { get; set; }
        //public string? CrCasRenterLessorStatisticsCity { get; set; }
        public string? CrCasRenterLessorStatisticsAge { get; set; }
        public string? CrCasRenterLessorStatisticsTraded { get; set; }
        public string? CrCasRenterLessorDealingMechanism { get; set; }
        public string? CrCasRenterLessorStatus { get; set; }
        public string? CrCasRenterLessorReasons { get; set; }
        /// </summary>

    }

    public class RenterLessorInformation_Single_Edit_VM
    {
        public string? Email { get; set; }
        public string? addressAr { get; set; }
        public string? addressEn { get; set; }
        public string? RateAr { get; set; }
        public string? RateEn { get; set; }
        public string? jobAr { get; set; }
        public string? jobEn { get; set; }
        public string? nationalityAr { get; set; }
        public string? nationalityEn { get; set; }
        public string? RenterNameAr { get; set; }
        public string? RenterNameEn { get; set; }

        //// <summary>
        public string? IdTypeAr { get; set; }
        public string? IdTypeEn { get; set; }
        public string? WorkPlaceAr { get; set; }
        public string? WorkPlaceEn { get; set; }
        public string? License_Ar { get; set; }
        public string? License_En { get; set; }
        public string? MempershipAr { get; set; }
        public string? MempershipEn { get; set; }
        public string? GenderAr { get; set; }
        public string? GenderEn { get; set; }
        public string? BankAr { get; set; }
        public string? BankEn { get; set; }
        public string? SignatureImage { get; set; }
        public string? Bank_Iban { get; set; }
        public string? License_No { get; set; }
        public DateTime? birthDate { get; set; }
        public DateTime? License_ExpiryDate { get; set; }

        //// </summary>
        public string? CrCasRenterLessorId { get; set; }
        public string? CrCasRenterLessorCode { get; set; }
        //public string? CrCasRenterLessorSector { get; set; }
        public int? CrCasRenterLessorCopyId { get; set; }
        public string? CrCasRenterLessorIdtrype { get; set; }
        //public string? CrCasRenterLessorMembership { get; set; }
        public DateTime? CrCasRenterLessorDateFirstInteraction { get; set; }
        public DateTime? CrCasRenterLessorDateLastContractual { get; set; }
        public int? CrCasRenterLessorContractCount { get; set; }
        //public int? CrCasRenterLessorContractExtension { get; set; }
        //public int? CrCasRenterLessorContractDays { get; set; }
        //public int? CrCasRenterLessorContractKm { get; set; }
        //public decimal? CrCasRenterLessorContractTradedAmount { get; set; }
        //public decimal? CrCasRenterLessorBalance { get; set; }
        //public decimal? CrCasRenterLessorAvailableBalance { get; set; }
        //public decimal? CrCasRenterLessorReservedBalance { get; set; }
        public int? CrCasRenterLessorEvaluationNumber { get; set; }
        public int? CrCasRenterLessorEvaluationTotal { get; set; }
        public decimal? CrCasRenterLessorEvaluationValue { get; set; }
        //public string? CrCasRenterLessorStatisticsNationalities { get; set; }
        //public string? CrCasRenterLessorStatisticsGender { get; set; }
        //public string? CrCasRenterLessorStatisticsJobs { get; set; }
        //public string? CrCasRenterLessorStatisticsRegions { get; set; }
        //public string? CrCasRenterLessorStatisticsCity { get; set; }
        public string? CrCasRenterLessorStatisticsAge { get; set; }
        public string? CrCasRenterLessorStatisticsTraded { get; set; }
        public string? CrCasRenterLessorDealingMechanism { get; set; }
        public string? CrCasRenterLessorStatus { get; set; }
        public string? CrCasRenterLessorReasons { get; set; }
        /// </summary>

    }
}
