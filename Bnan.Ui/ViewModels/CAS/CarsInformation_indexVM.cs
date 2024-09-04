using Bnan.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.CAS
{
    public class Car_Spasific_Info
    {
        public string CrCasCarInformationSerailNo { get; set; }
        public string? CrCasCarInformationStatus { get; set; }
        public string? CrCasCarInformationForSaleStatus { get; set; }
        public string? CrCasCarInformationBranchStatus { get; set; }
        public string? CrCasCarInformationOwnerStatus { get; set; }
        public bool? CrCasCarInformationPriceStatus { get; set; }
        public string? CrCasBranchInformationArShortName { get; set; }
        public string? CrCasBranchInformationEnShortName { get; set; }
        public string? CrCasCarInformationConcatenateArName { get; set; }
        public string? CrCasCarInformationConcatenateEnName { get; set; }
        public int? CrCasCarInformationCurrentMeter { get; set; }
        public string? CrMasSupCarRegistrationArName { get; set; }
        public string? CrMasSupCarRegistrationEnName { get; set; }
        public string? CrMasSupCarFuelArName { get; set; }
        public string? CrMasSupCarFuelEnName { get; set; }
        public string? CrMasSupCarCvtArName { get; set; }
        public string? CrMasSupCarCvtEnName { get; set; }
    }
    public class CarsInformation_indexVM
    {
        public List<Car_Spasific_Info>? AllCars_list = new List<Car_Spasific_Info>();

        public IEnumerable<CrCasCarInformation>? AllCars { get; set; }
    
        public List<CrCasCarAdvantage>? AdvantagesAll = new List<CrCasCarAdvantage>();

    }
}
