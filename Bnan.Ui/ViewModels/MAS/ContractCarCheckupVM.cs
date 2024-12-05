using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class ContractCarCheckupVM 
    {
        [Range(10, 99, ErrorMessage = "error_Codestart2"),Required(ErrorMessage = "requiredFiled")]
        public string CrMasSupContractCarCheckupCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractCarCheckupArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupContractCarCheckupEnName { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractCarCheckupAcceptImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractCarCheckupRejectImage { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractCarCheckupBlockImage { get; set; }
        public string? CrMasSupContractCarCheckupStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractCarCheckupReasons { get; set; }

    }
}


