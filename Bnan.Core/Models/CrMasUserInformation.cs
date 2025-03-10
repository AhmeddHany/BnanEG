﻿using Microsoft.AspNetCore.Identity;

namespace Bnan.Core.Models
{
    public partial class CrMasUserInformation : IdentityUser
    {
        public CrMasUserInformation()
        {
            CrCasAccountContractTaxOweds = new HashSet<CrCasAccountContractTaxOwed>();
            CrCasAccountInvoices = new HashSet<CrCasAccountInvoice>();
            CrCasAccountReceipts = new HashSet<CrCasAccountReceipt>();
            CrCasLessorWhatsupConnectCrCasLessorWhatsupConnectUserLoginNavigations = new HashSet<CrCasLessorWhatsupConnect>();
            CrCasLessorWhatsupConnectCrCasLessorWhatsupConnectUserLogoutNavigations = new HashSet<CrCasLessorWhatsupConnect>();
            CrCasRenterContractEvaluations = new HashSet<CrCasRenterContractEvaluation>();
            CrCasRenterContractStatisticCrCasRenterContractStatisticsUserCloseNavigations = new HashSet<CrCasRenterContractStatistic>();
            CrCasRenterContractStatisticCrCasRenterContractStatisticsUserOpenNavigations = new HashSet<CrCasRenterContractStatistic>();
            CrCasSysAdministrativeProcedures = new HashSet<CrCasSysAdministrativeProcedure>();
            CrMasLessorMessages = new HashSet<CrMasLessorMessage>();
            CrMasUserBranchValidities = new HashSet<CrMasUserBranchValidity>();
            CrMasUserLogins = new HashSet<CrMasUserLogin>();
            CrMasUserMainValidations = new HashSet<CrMasUserMainValidation>();
            CrMasUserMessageCrMasUserMessageUserReceiverNavigations = new HashSet<CrMasUserMessage>();
            CrMasUserMessageCrMasUserMessageUserSenderNavigations = new HashSet<CrMasUserMessage>();
            CrMasUserProceduresValidations = new HashSet<CrMasUserProceduresValidation>();
            CrMasUserSubValidations = new HashSet<CrMasUserSubValidation>();
        }

        public string CrMasUserInformationCode { get; set; } = null!;
        public string? CrMasUserInformationPassWord { get; set; }
        public string? CrMasUserInformationRemindMe { get; set; }
        public string? CrMasUserInformationLessor { get; set; }
        public string? CrMasUserInformationId { get; set; }
        public string? CrMasUserInformationDefaultBranch { get; set; }
        public string? CrMasUserInformationDefaultLanguage { get; set; }
        public bool? CrMasUserInformationAuthorizationBnan { get; set; }
        public bool? CrMasUserInformationAuthorizationAdmin { get; set; }
        public bool? CrMasUserInformationAuthorizationBranch { get; set; }
        public bool? CrMasUserInformationAuthorizationOwner { get; set; }
        public bool? CrMasUserInformationAuthorizationFoolwUp { get; set; }
        public string? CrMasUserInformationArName { get; set; }
        public string? CrMasUserInformationEnName { get; set; }
        public string? CrMasUserInformationTasksArName { get; set; }
        public string? CrMasUserInformationTasksEnName { get; set; }
        public decimal? CrMasUserInformationReservedBalance { get; set; }
        public decimal? CrMasUserInformationTotalBalance { get; set; }
        public decimal? CrMasUserInformationAvailableBalance { get; set; }
        public decimal? CrMasUserInformationCreditLimit { get; set; }
        public string? CrMasUserInformationCallingKey { get; set; }
        public string? CrMasUserInformationMobileNo { get; set; }
        public string? CrMasUserInformationEmail { get; set; }
        public DateTime? CrMasUserInformationChangePassWordLastDate { get; set; }
        public bool? CrMasUserInformationAlertCreditLimit { get; set; }
        public int? CrMasUserInformationAlertDaysPassWord { get; set; }
        public int? CrMasUserInformationCreditDaysLimit { get; set; }
        public DateTime? CrMasUserInformationEntryLastDate { get; set; }
        public TimeSpan? CrMasUserInformationEntryLastTime { get; set; }
        public DateTime? CrMasUserInformationExitLastDate { get; set; }
        public TimeSpan? CrMasUserInformationExitLastTime { get; set; }
        public int? CrMasUserInformationExitTimer { get; set; }
        public DateTime? CrMasUserInformationLastActionDate { get; set; }
        public string? CrMasUserInformationPicture { get; set; }
        public string? CrMasUserInformationSignature { get; set; }
        public bool? CrMasUserInformationOperationStatus { get; set; }
        public string? CrMasUserInformationStatus { get; set; }
        public string? CrMasUserInformationReasons { get; set; }

        public virtual CrMasLessorInformation? CrMasUserInformationLessorNavigation { get; set; }
        public virtual CrMasUserContractValidity CrMasUserContractValidity { get; set; } = null!;
        public virtual ICollection<CrCasAccountContractTaxOwed> CrCasAccountContractTaxOweds { get; set; }
        public virtual ICollection<CrCasAccountInvoice> CrCasAccountInvoices { get; set; }
        public virtual ICollection<CrCasAccountReceipt> CrCasAccountReceipts { get; set; }
        public virtual ICollection<CrCasLessorWhatsupConnect> CrCasLessorWhatsupConnectCrCasLessorWhatsupConnectUserLoginNavigations { get; set; }
        public virtual ICollection<CrCasLessorWhatsupConnect> CrCasLessorWhatsupConnectCrCasLessorWhatsupConnectUserLogoutNavigations { get; set; }
        public virtual ICollection<CrCasRenterContractEvaluation> CrCasRenterContractEvaluations { get; set; }
        public virtual ICollection<CrCasRenterContractStatistic> CrCasRenterContractStatisticCrCasRenterContractStatisticsUserCloseNavigations { get; set; }
        public virtual ICollection<CrCasRenterContractStatistic> CrCasRenterContractStatisticCrCasRenterContractStatisticsUserOpenNavigations { get; set; }
        public virtual ICollection<CrCasSysAdministrativeProcedure> CrCasSysAdministrativeProcedures { get; set; }
        public virtual ICollection<CrMasLessorMessage> CrMasLessorMessages { get; set; }
        public virtual ICollection<CrMasUserBranchValidity> CrMasUserBranchValidities { get; set; }
        public virtual ICollection<CrMasUserLogin> CrMasUserLogins { get; set; }
        public virtual ICollection<CrMasUserMainValidation> CrMasUserMainValidations { get; set; }
        public virtual ICollection<CrMasUserMessage> CrMasUserMessageCrMasUserMessageUserReceiverNavigations { get; set; }
        public virtual ICollection<CrMasUserMessage> CrMasUserMessageCrMasUserMessageUserSenderNavigations { get; set; }
        public virtual ICollection<CrMasUserProceduresValidation> CrMasUserProceduresValidations { get; set; }
        public virtual ICollection<CrMasUserSubValidation> CrMasUserSubValidations { get; set; }
    }
}
