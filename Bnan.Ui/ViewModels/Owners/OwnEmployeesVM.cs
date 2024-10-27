using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.Owners
{
    public class OwnEmployeesVM
    {

        public string CrMasUserInformationCode { get; set; } = null!;
        public string? CrMasUserInformationLessor { get; set; }
        public string? CrMasUserInformationDefaultBranch { get; set; }
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
        public string? CrMasUserInformationTasksArName { get; set; }
        public string? CrMasUserInformationTasksEnName { get; set; }
        public decimal? CrMasUserInformationReservedBalance { get; set; }
        public decimal? CrMasUserInformationTotalBalance { get; set; }
        public decimal? CrMasUserInformationAvailableBalance { get; set; }
        public decimal? CrMasUserInformationCreditLimit { get; set; }
        public DateTime? CrMasUserInformationEntryLastDate { get; set; }
        public TimeSpan? CrMasUserInformationEntryLastTime { get; set; }
        public string? EntryLastDateString { get; set; }
        public string? EntryLastTimeString { get; set; }
        public DateTime? CrMasUserInformationExitLastDate { get; set; }
        public TimeSpan? CrMasUserInformationExitLastTime { get; set; }
        public DateTime? CrMasUserInformationLastActionDate { get; set; }
        public bool CrMasUserInformationOperationStatus { get; set; }
        public string? CrMasUserInformationPicture { get; set; }
        public string? CrMasUserInformationStatus { get; set; }

        public int? ClosedContract { get; set; }
        public int? ContractsCount { get; set; }
        public int? ActiveContract { get; set; }
        public int? ExpireTomorrowContracts { get; set; }
        public int? ExpiredContracts { get; set; }
        public int? ExpireTodayContracts { get; set; }
        public int? ExpireLaterContracts { get; set; }
        public bool? CreditLimitExceeded { get; set; }
        public bool? HaveCustodyNotAccepted { get; set; }
        public bool? OnlineOrOflline { get; set; }

        public List<OwnPaymentMethodLessorVM>? OwnUsersPaymentMethods;
        public List<CrCasRenterContractAlert>? AlertContracts;



    }
}
