namespace Bnan.Core.Models
{
    public partial class CrMasLessorImage
    {
        public string CrMasLessorImageCode { get; set; } = null!;
        public string? CrMasLessorImageLogo { get; set; }
        public string? CrMasLessorImageStamp { get; set; }
        public string? CrMasLessorImageLoaderLogo { get; set; }
        public string? CrMasLessorImageWhatsupLogo { get; set; }
        public string? CrMasLessorImageQrCodeSite { get; set; }
        public string? CrMasLessorImageAuthenticatedElectronically { get; set; }
        public string? CrMasLessorImageQrCodeAuthenticated { get; set; }
        public string? CrMasLessorImageMangerSignture { get; set; }
        public string? CrMasLessorImageContractCard { get; set; }
        public string? CrMasLessorImagePerformInvoice { get; set; }
        public string? CrMasLessorImageTaxInvoice { get; set; }
        public string? CrMasLessorImageReceipt { get; set; }
        public string? CrMasLessorImageExchange { get; set; }
        public string? CrMasLessorImageContractPage1 { get; set; }
        public string? CrMasLessorImageContractPage2 { get; set; }
        public string? CrMasLessorImageContractPage3 { get; set; }
        public string? CrMasLessorImageContractPage4 { get; set; }
        public string? CrMasLessorImageContractPage5 { get; set; }
        public string? CrMasLessorImageContractPage6 { get; set; }
        public string? CrMasLessorImageContractPage7 { get; set; }
        public string? CrMasLessorImageContractPage8 { get; set; }
        public string? CrMasLessorImageContractPage9 { get; set; }
        public string? CrMasLessorImageContractPage10 { get; set; }
        public string? CrMasLessorImageContractPage11 { get; set; }
        public string? CrMasLessorImageContractPage12 { get; set; }
        public string? CrMasLessorImageArConditionPage1 { get; set; }
        public string? CrMasLessorImageArConditionPage2 { get; set; }
        public string? CrMasLessorImageArConditionPage3 { get; set; }
        public string? CrMasLessorImageEnConditionPage1 { get; set; }
        public string? CrMasLessorImageEnConditionPage2 { get; set; }
        public string? CrMasLessorImageEnConditionPage3 { get; set; }
        public string? CrMasLessorImageArDailyReport { get; set; }
        public string? CrMasLessorImageEnDailyReport { get; set; }

        public virtual CrMasLessorInformation CrMasLessorImageCodeNavigation { get; set; } = null!;
    }
}
