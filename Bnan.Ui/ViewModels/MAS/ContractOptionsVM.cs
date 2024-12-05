using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class ContractOptionsVM 
    {
        [Range(5100000001, 5199999999, ErrorMessage = "error_Codestart51"),Required(ErrorMessage = "requiredFiled")]
        public string CrMasSupContractOptionsCode { get; set; } = null!;
        [MaxLength(2, ErrorMessage = "requiredFiled")]
        public string? CrMasSupContractOptionsGroup { get; set; } = "51";
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractOptionsArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractOptionsEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractOptionsAcceptImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractOptionsRejectImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractOptionsBlockImage { get; set; }
        public string? CrMasSupContractOptionsStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractOptionsReasons { get; set; }
        [MaxLength(1, ErrorMessage = "requiredFiled")]
        public string? CrMasSupContractOptionsByDayContract { get; set; } = "1";

        [Range(0, 9999999999, ErrorMessage = "requiredNoLengthFiled10")]
        public int? CrMasSupContractOptionsNaqlCode { get; set; } = 0;

        public virtual CrMasSysGroup? CrMasSupContractOptionsGroupNavigation { get; set; }

    }
}


