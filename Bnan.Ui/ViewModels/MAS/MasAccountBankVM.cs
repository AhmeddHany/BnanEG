using Bnan.Inferastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Ui.ViewModels.MAS
{
 
    
    public class MasAccountBankVM
    {
        public string CrMasSupAccountBankCode { get; set; } = null!;

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupAccountBankArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(50, ErrorMessage = "requiredNoLengthFiled50")]
        public string? CrMasSupAccountBankEnName { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupAccountBankReasons { get; set; }

        public string? CrMasSupAccountBankStatus { get; set; }

        public List<CrMasSupAccountBank> crMasSupAccountBank = new List<CrMasSupAccountBank>();

        public List<TResult2>? Banks_count = new List<TResult2>();


    }
}
