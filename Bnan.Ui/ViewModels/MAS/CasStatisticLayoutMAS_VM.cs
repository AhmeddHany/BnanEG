using Bnan.Core.Models;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bnan.Ui.ViewModels.MAS
{
    public class CasStatisticLayoutMAS_VM
    {
        public List<CrCasBranchInformation>? CrCasBranchInformations = new List<CrCasBranchInformation>();

        public decimal? CreditorTotal { get; set; }
        public decimal? DebitTotal { get; set; }
        public decimal? TransaferTotal { get; set; }
        [Required(ErrorMessage = "requiredFiled")]

        public List<ChartBranchDataVM>? ChartBranchDataVM = new List<ChartBranchDataVM>();
        public List<ChartBranchDataVM>? ChartBranchDataVM_2ForAll = new List<ChartBranchDataVM>();
        public List<ChartBranchDataVM>? PaymentMethodsUser = new List<ChartBranchDataVM>();
        public List<CrMasLessorInformation>? All_Lessors = new List<CrMasLessorInformation>();
        public CrCasBranchInformation? CrCasBranchInformation  = new CrCasBranchInformation();
        public CrMasUserBranchValidity? CrMasUserBranchValidity = new CrMasUserBranchValidity();

        public string? SelectedBranch { get; set; }


    }
}
