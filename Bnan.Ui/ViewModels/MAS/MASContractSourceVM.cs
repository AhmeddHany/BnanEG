using Bnan.Inferastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.MAS
{
 
    
    public class MASContractSourceVM
    {
        public string CrMasSupContractSourceCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupContractSourceArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(30, ErrorMessage = "requiredNoLengthFiled30")]
        public string? CrMasSupContractSourceEnName { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractSourceReasons { get; set; }

        public string? CrMasSupContractSourceStatus { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupContractSourceMobile { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        //[EmailAddress(ErrorMessage = "requiredFiledEmail")]
        public string? CrMasSupContractSourceEmail { get; set; }

        public List<CrMasSupContractSource> crMasSupContractSource = new List<CrMasSupContractSource>();
        public List<CrMasSysCallingKey> keys = new List<CrMasSysCallingKey>();

        [Required(ErrorMessage = "requiredFiled"), MaxLength(13, ErrorMessage = "requiredFiled")]
        public string? mob { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(6, ErrorMessage = "requiredFiled")]
        public string? key { get; set; }
        public string? key2 { get; set; }

    }
}
