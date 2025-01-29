using Bnan.Inferastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.CAS
{
 
    
    public class CASContractSourceVM
    {
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string CrCasOwnersCode { get; set; } = null!;
        [Required(ErrorMessage = "requiredFiled"), MaxLength(4, ErrorMessage = "requiredFiled")]
        public string CrCasOwnersLessorCode { get; set; } = null!;
        public string? CrCasOwnersCommercialNo { get; set; }
        public string? CrCasOwnersSector { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(10, ErrorMessage = "requiredNoLengthFiled10")]
        public string? CrCasOwnersCountryKey { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrCasOwnersMobile { get; set; }
        public bool? CrCasOwnersSendContractByWhatsUp { get; set; }
        public bool? CrCasOwnersSendContractByEmail { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasOwnersArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasOwnersEnName { get; set; }
        public string? CrCasOwnersStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrCasOwnersReasons { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        //[EmailAddress(ErrorMessage = "requiredFiledEmail")]
        public string? CrCasOwnersEmail { get; set; }


        public List<CrCasOwner> list_crCasOwner = new List<CrCasOwner>();
        public List<CrMasSysCallingKey> keys = new List<CrMasSysCallingKey>();

        public List<TResult2> all_CarsCount = new List<TResult2>();

        //[Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredFiled")]
        //public string? mob { get; set; }
        //[Required(ErrorMessage = "requiredFiled"), MaxLength(10, ErrorMessage = "requiredFiled")]
        //public string? key { get; set; }
        //public string? key2 { get; set; }

    }
}
