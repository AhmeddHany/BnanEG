﻿using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
    public class DriverContractVM
    {

        public string CrCasRenterContractBasicNo { get; set; } = null!;
        public int CrCasRenterContractBasicCopy { get; set; }
        public string? CrCasRenterContractBasicYear { get; set; }
        public string? CrCasRenterContractBasicSector { get; set; }
        public string? CrCasRenterContractBasicProcedures { get; set; }
        public string? CrCasRenterContractBasicLessor { get; set; }
        public string? CrCasRenterContractBasicBranch { get; set; }
        public DateTime? CrCasRenterContractBasicIssuedDate { get; set; }
        public DateTime? CrCasRenterContractBasicAllowCanceled { get; set; }
        public DateTime? CrCasRenterContractBasicExpectedStartDate { get; set; }
        public DateTime? CrCasRenterContractBasicExpectedEndDate { get; set; }
        public int? CrCasRenterContractBasicExpectedRentalDays { get; set; }
        public string? CrCasRenterContractBasicRenterId { get; set; }
        public string? CrCasRenterContractBasicDriverId { get; set; }
        public string? CrCasRenterContractBasicAdditionalDriverId { get; set; }
        public string? CrCasRenterContractBasicPrivateDriverId { get; set; }
        public string? CrCasRenterContractBasicCarSerailNo { get; set; }
        public int? CrCasRenterContractBasicFreeHours { get; set; }
        public int? CrCasRenterContractBasicUserFreeHours { get; set; }
        public int? CrCasRenterContractBasicTotalFreeHours { get; set; }
        public int? CrCasRenterContractBasicHourMax { get; set; }
        public decimal? CrCasRenterContractBasicHourValue { get; set; }
        public int? CrCasRenterContractBasicDailyFreeKm { get; set; }
        public int? CrCasRenterContractBasicDailyFreeKmUser { get; set; }
        public int? CrCasRenterContractBasicTotalDailyFreeKm { get; set; }
        public decimal? CrCasRenterContractBasicKmValue { get; set; }
        public decimal? CrCasRenterContractBasicDailyRent { get; set; }
        public decimal? CrCasRenterContractBasicWeeklyRent { get; set; }
        public decimal? CrCasRenterContractBasicMonthlyRent { get; set; }
        public decimal? CrCasRenterContractBasicYearlyRent { get; set; }
        public decimal? CrCasRenterContractBasicAdditionalDriverValue { get; set; }
        public decimal? CrCasRenterContractBasicPrivateDriverValue { get; set; }
        public decimal? CrCasRenterContractBasicAuthorizationValue { get; set; }
        public decimal? CrCasRenterContractBasicTaxRate { get; set; }
        public decimal? CrCasRenterContractBasicUserDiscountRate { get; set; }
        public int? CrCasRenterContractBasicCurrentReadingMeter { get; set; }
        public decimal? CrCasRenterContractBasicExpectedRentValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedOptionsValue { get; set; }
        public decimal? CrCasRenterContractBasicAdditionalValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedPrivateDriverValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedValueBeforDiscount { get; set; }
        public decimal? CrCasRenterContractBasicExpectedDiscountValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedValueAfterDiscount { get; set; }
        public decimal? CrCasRenterContractBasicExpectedTaxValue { get; set; }
        public decimal? CrCasRenterContractBasicExpectedTotal { get; set; }
        public decimal? CrCasRenterContractBasicPreviousBalance { get; set; }
        public decimal? CrCasRenterContractBasicAmountRequired { get; set; }
        public decimal? CrCasRenterContractBasicAmountPaidAdvance { get; set; }
        public string? CrCasRenterContractBasicPdfFile { get; set; }
        public string? CrCasRenterContractBasicPdfTga { get; set; }
        public string? CrCasRenterContractPriceReference { get; set; }
        public string? CrCasRenterContractOffersReference { get; set; }
        public string? CrCasRenterContractUserReference { get; set; }
        public string? CrCasRenterContractBasicUserInsert { get; set; }
        public string? CrCasRenterContractBasicStatus { get; set; }
        public string? CrCasRenterContractBasicReasons { get; set; }

        public virtual CrCasBranchInformation? CrCasRenterContractBasic1 { get; set; }
        public virtual CrCasRenterLessor? CrCasRenterContractBasic2 { get; set; }
        public virtual CrCasRenterPrivateDriverInformation? CrCasRenterContractBasic3 { get; set; }
        public virtual CrCasRenterLessor? CrCasRenterContractBasic4 { get; set; }
        public virtual CrCasCarInformation? CrCasRenterContractBasicCarSerailNoNavigation { get; set; }
        public virtual CrCasRenterLessor? CrCasRenterContractBasicNavigation { get; set; }
        public virtual CrMasSupRenterSector? CrCasRenterContractBasicSectorNavigation { get; set; }

        public List<CrCasAccountInvoice>? CrCasAccountInvoice_308 = new List<CrCasAccountInvoice>();

        public List<CrCasAccountInvoice>? CrCasAccountInvoice_309 = new List<CrCasAccountInvoice>();

        public List<CrCasRenterContractBasic>? CrCasRenterContractBasic = new List<CrCasRenterContractBasic>();
    }
}
