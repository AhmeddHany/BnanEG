using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class ContractAdditionalVM 
    {
        [Range(5000000001, 5099999999, ErrorMessage = "error_Codestart50"), Required(ErrorMessage = "requiredFiled")]
        public string CrMasSupContractAdditionalCode { get; set; } = null!;
        [MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrMasSupContractAdditionalGroup { get; set; } = "50";
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractAdditionalArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractAdditionalEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractAdditionalAcceptImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractAdditionalRejectImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractAdditionalBlockImage { get; set; }
        public string? CrMasSupContractAdditionalStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractAdditionalReasons { get; set; }
        [MaxLength(1, ErrorMessage = "requiredFiled")]
        public string? CrMasSupContractAdditionalByDayContract { get; set; } = "1";

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupContractAdditionalNaqlCode { get; set; } = 0;

        public virtual CrMasSysGroup? CrMasSupContractAdditionalGroupNavigation { get; set; }

    }
}


