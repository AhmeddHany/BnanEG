using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class RenterEmployerVM
    {      
        public string CrMasSupRenterEmployerCode { get; set; } = null!;

        public string? CrMasSupRenterEmployerGroupCode = "18";
        public string? CrMasSupRenterEmployerSectorCode { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterEmployerArName { get; set; }

        [Required(ErrorMessage = "requiredFiled"), MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterEmployerEnName { get; set; }
        public int? CrMasSupRenterEmployerCounter { get; set; }
        public string? CrMasSupRenterEmployerStatus { get; set; }

        [MaxLength(100, ErrorMessage = "requiredNoLengthFiled100")]
        public string? CrMasSupRenterEmployerReasons { get; set; }
        public int? RentersHave_withType_Count { get; set; }

        public List<CrMasSupRenterSector> All_Sectors = new List<CrMasSupRenterSector>();
        public virtual CrMasSysGroup? CrMasSupRenterEmployerGroupCodeNavigation { get; set; }
        public virtual CrMasSupRenterSector? CrMasSupRenterEmployerSectorCodeNavigation { get; set; }

    }
}
