using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class PostBranch : IPostBranch
    {
        private readonly IUnitOfWork _unitOfWork;
        public PostBranch(IUnitOfWork unitOfWork, BnanSCContext context)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPostBranch(CrCasBranchPost CrCasBranchPost, CrMasSupPostCity City)
        {
            var concatenatedArAddress = "";
            var concatenatedEnAddress = "";
            var concatenatedArAddressShort = "";
            var concatenatedEnAddressShort = "";
            var buildingInfoAr = "";
            var unitInfoAr = "";
            var zipCodeAr = "";
            var additionalNoAr = "";
            var buildingInfoEn = "";
            var unitInfoEn = "";
            var zipCodeEn = "";
            var additionalNoEn = "";




            buildingInfoAr = CrCasBranchPost.CrCasBranchPostBuilding != null ? $"مبنى ({CrCasBranchPost.CrCasBranchPostBuilding}) " : string.Empty;
            unitInfoAr = CrCasBranchPost.CrCasBranchPostUnitNo != null ? $"وحدة ({CrCasBranchPost.CrCasBranchPostUnitNo}) " : string.Empty;
            zipCodeAr = CrCasBranchPost.CrCasBranchPostZipCode != null ? $"الرمز البريدي ({CrCasBranchPost.CrCasBranchPostZipCode}) " : string.Empty;
            additionalNoAr = CrCasBranchPost.CrCasBranchPostAdditionalNumbers != null ? $"الرقم الاضافي ({CrCasBranchPost.CrCasBranchPostAdditionalNumbers}) " : string.Empty;

            buildingInfoEn = CrCasBranchPost.CrCasBranchPostBuilding != null ? $"Building ({CrCasBranchPost.CrCasBranchPostBuilding}) " : string.Empty;
            unitInfoEn = CrCasBranchPost.CrCasBranchPostUnitNo != null ? $"Unit ({CrCasBranchPost.CrCasBranchPostUnitNo}) " : string.Empty;
            zipCodeEn = CrCasBranchPost.CrCasBranchPostZipCode != null ? $"ZipCode ({CrCasBranchPost.CrCasBranchPostZipCode}) " : string.Empty;
            additionalNoEn = CrCasBranchPost.CrCasBranchPostAdditionalNumbers != null ? $"additionalNo ({CrCasBranchPost.CrCasBranchPostAdditionalNumbers}) " : string.Empty;

            concatenatedArAddress = string.Join(" - ", City.CrMasSupPostCityConcatenateArName, CrCasBranchPost.CrCasBranchPostArDistrict,
                                                       CrCasBranchPost.CrCasBranchPostArStreet, buildingInfoAr, unitInfoAr, zipCodeAr, additionalNoAr);

            concatenatedEnAddress = string.Join(" - ", City.CrMasSupPostCityConcatenateEnName, CrCasBranchPost.CrCasBranchPostEnDistrict,
                                                       CrCasBranchPost.CrCasBranchPostEnStreet, buildingInfoEn, unitInfoEn, zipCodeEn, additionalNoEn);

            concatenatedArAddressShort = string.Join(" - ", City.CrMasSupPostCityConcatenateArName, CrCasBranchPost.CrCasBranchPostArDistrict,
                                                       CrCasBranchPost.CrCasBranchPostArStreet);

            concatenatedEnAddressShort = string.Join(" - ", City.CrMasSupPostCityConcatenateEnName, CrCasBranchPost.CrCasBranchPostEnDistrict,
                                                       CrCasBranchPost.CrCasBranchPostEnStreet);
            var BranchPost = new CrCasBranchPost
            {
                CrCasBranchPostLessor = CrCasBranchPost.CrCasBranchPostLessor,
                CrCasBranchPostBranch = CrCasBranchPost.CrCasBranchPostBranch,
                CrCasBranchPostRegions = City.CrMasSupPostCityRegionsCode,
                CrCasBranchPostCity = City.CrMasSupPostCityCode,
                CrCasBranchPostArDistrict = CrCasBranchPost.CrCasBranchPostArDistrict,
                CrCasBranchPostEnDistrict = CrCasBranchPost.CrCasBranchPostEnDistrict,
                CrCasBranchPostArStreet = CrCasBranchPost.CrCasBranchPostArStreet,
                CrCasBranchPostEnStreet = CrCasBranchPost.CrCasBranchPostEnStreet,
                CrCasBranchPostBuilding = CrCasBranchPost.CrCasBranchPostBuilding,
                CrCasBranchPostUnitNo = CrCasBranchPost.CrCasBranchPostUnitNo,
                CrCasBranchPostZipCode = CrCasBranchPost.CrCasBranchPostZipCode,
                CrCasBranchPostAdditionalNumbers = CrCasBranchPost.CrCasBranchPostAdditionalNumbers,
                CrCasBranchPostArConcatenate = concatenatedArAddress,
                CrCasBranchPostEnConcatenate = concatenatedEnAddress,
                CrCasBranchPostArShortConcatenate = concatenatedArAddressShort,
                CrCasBranchPostEnShortConcatenate = concatenatedEnAddressShort,
                CrCasBranchPostUpDateMail = DateTime.Now,
                CrCasBranchPostStatus = "A"

            };
            await _unitOfWork.CrCasBranchPost.AddAsync(BranchPost);
            return true;
        }

        public async Task<bool> AddPostBranchDefault(string LessorCode, CrCasBranchPost crCasBranchPost, CrMasSupPostCity City)
        {
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(LessorCode);
            var BranchPost = new CrCasBranchPost
            {
                CrCasBranchPostLessor = lessor.CrMasLessorInformationCode,
                CrCasBranchPostBranch = "100",
                CrCasBranchPostRegions = City.CrMasSupPostCityRegionsCode,
                CrCasBranchPostCity = City.CrMasSupPostCityCode,
                CrCasBranchPostArDistrict = crCasBranchPost.CrCasBranchPostArDistrict,
                CrCasBranchPostEnDistrict = crCasBranchPost.CrCasBranchPostEnDistrict,
                CrCasBranchPostArStreet = crCasBranchPost.CrCasBranchPostArStreet,
                CrCasBranchPostEnStreet = crCasBranchPost.CrCasBranchPostEnStreet,
                CrCasBranchPostBuilding = crCasBranchPost.CrCasBranchPostBuilding,
                CrCasBranchPostUnitNo = crCasBranchPost.CrCasBranchPostUnitNo,
                CrCasBranchPostZipCode = crCasBranchPost.CrCasBranchPostZipCode,
                CrCasBranchPostAdditionalNumbers = crCasBranchPost.CrCasBranchPostAdditionalNumbers,
                CrCasBranchPostArConcatenate =
                                                $"{(string.IsNullOrEmpty(City.CrMasSupPostCityConcatenateArName) ? "" : City.CrMasSupPostCityConcatenateArName + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostArDistrict) ? "" : crCasBranchPost.CrCasBranchPostArDistrict + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostArStreet) ? "" : crCasBranchPost.CrCasBranchPostArStreet + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostBuilding) ? "" : crCasBranchPost.CrCasBranchPostBuilding + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostUnitNo) ? "" : crCasBranchPost.CrCasBranchPostUnitNo + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostZipCode) ? "" : crCasBranchPost.CrCasBranchPostZipCode + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostAdditionalNumbers) ? "" : crCasBranchPost.CrCasBranchPostAdditionalNumbers)}",

                CrCasBranchPostEnConcatenate =
                                                $"{(string.IsNullOrEmpty(City.CrMasSupPostCityConcatenateEnName) ? "" : City.CrMasSupPostCityConcatenateEnName + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostEnDistrict) ? "" : crCasBranchPost.CrCasBranchPostEnDistrict + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostEnStreet) ? "" : crCasBranchPost.CrCasBranchPostEnStreet + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostBuilding) ? "" : crCasBranchPost.CrCasBranchPostBuilding + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostUnitNo) ? "" : crCasBranchPost.CrCasBranchPostUnitNo + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostZipCode) ? "" : crCasBranchPost.CrCasBranchPostZipCode + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostAdditionalNumbers) ? "" : crCasBranchPost.CrCasBranchPostAdditionalNumbers)}",

                CrCasBranchPostArShortConcatenate =
                                                $"{(string.IsNullOrEmpty(City.CrMasSupPostCityConcatenateArName) ? "" : City.CrMasSupPostCityConcatenateArName + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostArDistrict) ? "" : crCasBranchPost.CrCasBranchPostArDistrict + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostArStreet) ? "" : crCasBranchPost.CrCasBranchPostArStreet)}",

                CrCasBranchPostEnShortConcatenate =
                                                $"{(string.IsNullOrEmpty(City.CrMasSupPostCityConcatenateEnName) ? "" : City.CrMasSupPostCityConcatenateEnName + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostEnDistrict) ? "" : crCasBranchPost.CrCasBranchPostEnDistrict + " - ")}" +
                                                $"{(string.IsNullOrEmpty(crCasBranchPost.CrCasBranchPostEnStreet) ? "" : crCasBranchPost.CrCasBranchPostEnStreet)}",
                CrCasBranchPostUpDateMail = DateTime.Now,
                CrCasBranchPostStatus = "A"

            };
            await _unitOfWork.CrCasBranchPost.AddAsync(BranchPost);
            return true;
        }

        public List<CrCasBranchPost> GetAllByLessor(string LessorCode)
        {
            var branches = _unitOfWork.CrCasBranchPost.FindAll(l => l.CrCasBranchPostLessor == LessorCode, (new[] { "CrCasBranchPostCityNavigation", "CrCasBranchPostNavigation" })).ToList();
            return branches;
        }

        public bool UpdatePostBranch(CrCasBranchPost CrCasBranchPost, CrMasSupPostCity City)
        {
            var BranchPost = _unitOfWork.CrCasBranchPost.Find(x => x.CrCasBranchPostLessor == CrCasBranchPost.CrCasBranchPostLessor && x.CrCasBranchPostBranch == CrCasBranchPost.CrCasBranchPostBranch);
            var Branch = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationLessor == CrCasBranchPost.CrCasBranchPostLessor && x.CrCasBranchInformationCode == CrCasBranchPost.CrCasBranchPostBranch);
            var concatenatedArAddress = "";
            var concatenatedEnAddress = "";
            var concatenatedArAddressShort = "";
            var concatenatedEnAddressShort = "";
            var buildingInfoAr = "";
            var unitInfoAr = "";
            var zipCodeAr = "";
            var additionalNoAr = "";
            var buildingInfoEn = "";
            var unitInfoEn = "";
            var zipCodeEn = "";
            var additionalNoEn = "";




            buildingInfoAr = CrCasBranchPost.CrCasBranchPostBuilding != null ? $"مبنى ({CrCasBranchPost.CrCasBranchPostBuilding}) " : string.Empty;
            unitInfoAr = CrCasBranchPost.CrCasBranchPostUnitNo != null ? $"وحدة ({CrCasBranchPost.CrCasBranchPostUnitNo}) " : string.Empty;
            zipCodeAr = CrCasBranchPost.CrCasBranchPostZipCode != null ? $"الرمز البريدي ({CrCasBranchPost.CrCasBranchPostZipCode}) " : string.Empty;
            additionalNoAr = CrCasBranchPost.CrCasBranchPostAdditionalNumbers != null ? $"الرقم الاضافي ({CrCasBranchPost.CrCasBranchPostAdditionalNumbers}) " : string.Empty;

            buildingInfoEn = CrCasBranchPost.CrCasBranchPostBuilding != null ? $"Building ({CrCasBranchPost.CrCasBranchPostBuilding}) " : string.Empty;
            unitInfoEn = CrCasBranchPost.CrCasBranchPostUnitNo != null ? $"Unit ({CrCasBranchPost.CrCasBranchPostUnitNo}) " : string.Empty;
            zipCodeEn = CrCasBranchPost.CrCasBranchPostZipCode != null ? $"ZipCode ({CrCasBranchPost.CrCasBranchPostZipCode}) " : string.Empty;
            additionalNoEn = CrCasBranchPost.CrCasBranchPostAdditionalNumbers != null ? $"additionalNo ({CrCasBranchPost.CrCasBranchPostAdditionalNumbers}) " : string.Empty;

            concatenatedArAddress = string.Join(" - ", City.CrMasSupPostCityConcatenateArName, CrCasBranchPost.CrCasBranchPostArDistrict,
                                                       CrCasBranchPost.CrCasBranchPostArStreet, buildingInfoAr, unitInfoAr, zipCodeAr, additionalNoAr);

            concatenatedEnAddress = string.Join(" - ", City.CrMasSupPostCityConcatenateEnName, CrCasBranchPost.CrCasBranchPostEnDistrict,
                                                       CrCasBranchPost.CrCasBranchPostEnStreet, buildingInfoEn, unitInfoEn, zipCodeEn, additionalNoEn);

            concatenatedArAddressShort = string.Join(" - ", City.CrMasSupPostCityConcatenateArName, CrCasBranchPost.CrCasBranchPostArDistrict,
                                                       CrCasBranchPost.CrCasBranchPostArStreet);

            concatenatedEnAddressShort = string.Join(" - ", City.CrMasSupPostCityConcatenateEnName, CrCasBranchPost.CrCasBranchPostEnDistrict,
                                                       CrCasBranchPost.CrCasBranchPostEnStreet);

            if (BranchPost == null) return false;
            BranchPost.CrCasBranchPostCity = City.CrMasSupPostCityCode;
            BranchPost.CrCasBranchPostRegions = City.CrMasSupPostCityRegionsCode;
            BranchPost.CrCasBranchPostArDistrict = CrCasBranchPost.CrCasBranchPostArDistrict;
            BranchPost.CrCasBranchPostEnDistrict = CrCasBranchPost.CrCasBranchPostEnDistrict;
            BranchPost.CrCasBranchPostArStreet = CrCasBranchPost.CrCasBranchPostArStreet;
            BranchPost.CrCasBranchPostEnStreet = CrCasBranchPost.CrCasBranchPostEnStreet;
            BranchPost.CrCasBranchPostBuilding = CrCasBranchPost.CrCasBranchPostBuilding;
            BranchPost.CrCasBranchPostUnitNo = CrCasBranchPost.CrCasBranchPostUnitNo;
            BranchPost.CrCasBranchPostZipCode = CrCasBranchPost.CrCasBranchPostZipCode;
            BranchPost.CrCasBranchPostAdditionalNumbers = CrCasBranchPost.CrCasBranchPostAdditionalNumbers;
            BranchPost.CrCasBranchPostArConcatenate = concatenatedArAddress;
            BranchPost.CrCasBranchPostEnConcatenate = concatenatedEnAddress;
            BranchPost.CrCasBranchPostArShortConcatenate = concatenatedArAddressShort;
            BranchPost.CrCasBranchPostEnShortConcatenate = concatenatedEnAddressShort;
            BranchPost.CrCasBranchPostUpDateMail = DateTime.Now;
            BranchPost.CrCasBranchPostStatus = Branch.CrCasBranchInformationStatus;

            _unitOfWork.CrCasBranchPost.Update(BranchPost);
            return true;
        }
    }
}
