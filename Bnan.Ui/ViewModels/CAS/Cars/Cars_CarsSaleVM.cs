using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bnan.Ui.ViewModels.CAS.Cars
{
    public class Cars_CarsSaleVM
    {
        public string CrCasCarInformationSerailNo { get; set; } = null!;
        public string? CrCasCarInformationLessor { get; set; }
        public string? CrCasCarInformationBranch { get; set; }
        public string? CrCasCarInformationNo { get; set; }
        public string? CrCasCarInformationConcatenateArName { get; set; }
        public string? CrCasCarInformationConcatenateEnName { get; set; }
        public DateTime? CrCasCarInformationJoinedFleetDate { get; set; }
        public int? CrCasCarInformationCurrentMeter { get; set; }
        public int? CrCasCarInformationConractCount { get; set; }
        public int? CrCasCarInformationConractDaysNo { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasCarInformationOfferedSaleDate { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        [Column(TypeName = "decimal(13,2)")]
        public decimal CrCasCarInformationOfferValueSale { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public string? OfferValueSaleString { get; set; }
        public string? CrCasCarInformationLastPictures { get; set; }
        [Required(ErrorMessage = "requiredFiled")]
        public DateTime? CrCasCarInformationSoldDate { get; set; }
        public string? CrCasCarInformationPriceNo { get; set; }
        public string? CrCasCarInformationBranchStatus { get; set; }
        public string? CrCasCarInformationOwnerStatus { get; set; }
        public string? CrCasCarInformationForSaleStatus { get; set; }
        public string? CrCasCarInformationStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasCarInformationReasons { get; set; }

        public virtual CrCasBranchInformation? CrCasCarInformation1 { get; set; }
        public virtual CrMasSupCarDistribution? CrCasCarInformationDistributionNavigation { get; set; }
        public virtual ICollection<CrCasCarDocumentsMaintenance>? CrCasCarDocumentsMaintenances { get; set; }
    }
}
