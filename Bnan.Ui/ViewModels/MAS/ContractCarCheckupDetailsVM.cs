using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class ContractCarCheckupDetailsVM
    {

        [MaxLength(2, ErrorMessage = "error_Codestart2"), Required(ErrorMessage = "requiredFiled")]
        public string CrMasSupContractCarCheckupDetailsCode { get; set; } = null!;
        [MaxLength(2, ErrorMessage = "error_Codestart9"), Required(ErrorMessage = "requiredFiled")]
        public string CrMasSupContractCarCheckupDetailsNo { get; set; } 
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupContractCarCheckupDetailsArName { get; set; }
        [Required(ErrorMessage = "requiredFiled"), MaxLength(20, ErrorMessage = "requiredNoLengthFiled20")]
        public string? CrMasSupContractCarCheckupDetailsEnName { get; set; }
        public string? CrMasSupContractCarCheckupDetailsStatus { get; set; }
        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupContractCarCheckupDetailsReasons { get; set; }

        public virtual CrMasSupContractCarCheckup CrMasSupContractCarCheckupDetailsCodeNavigation { get; set; } = null!;

        public CrMasSupContractCarCheckup? singleCheckup { get; set; } = new CrMasSupContractCarCheckup();

        public List<CrMasSupContractCarCheckup>? crMasSupContractCarCheckup = new List<CrMasSupContractCarCheckup>();
        public List<CrMasSupContractCarCheckup>? Checkup_activated = new List<CrMasSupContractCarCheckup>();
        public List<CrMasSupContractCarCheckupDetail>? crMasSupContractCarCheckupDetails = new List<CrMasSupContractCarCheckupDetail>();
        // {"CrCasRenterContractCarCheckups"}
    }
}


