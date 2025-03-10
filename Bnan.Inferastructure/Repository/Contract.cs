﻿using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using System.Globalization;

namespace Bnan.Inferastructure.Repository
{
    public class Contract : IContract
    {
        private IUnitOfWork _unitOfWork;

        public Contract(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // For New System
        public async Task<CrMasRenterInformation> AddRenterToMASRenterInformation(CrMasRenterInformation model, string EmployerName, string day, string month, string year)
        {

            if (model == null) return null;

            var firstChar = model.CrMasRenterInformationId.Substring(0, 1);
            var sectorCode = GetSector(firstChar, "");
            var birthDate = GetBirthDateMiladiOrHijri(day, month, year, model?.CrMasRenterInformationId);

            model.CrMasRenterInformationBirthDate = birthDate;
            model.CrMasRenterInformationStatus = "A";
            model.CrMasRenterInformationCommunicationLanguage = "1";
            model.CrMasRenterInformationSector = "1";
            model.CrMasRenterInformationEmployer = GetEmployer(firstChar, EmployerName);
            if (string.IsNullOrEmpty(model.CrMasRenterInformationDrivingLicenseType)) model.CrMasRenterInformationDrivingLicenseType = null;
            if (string.IsNullOrEmpty(model.CrMasRenterInformationDrivingLicenseNo)) model.CrMasRenterInformationDrivingLicenseNo = null;
            if (string.IsNullOrEmpty(model.CrMasRenterInformationProfession)) model.CrMasRenterInformationProfession = "1400000002";
            if (string.IsNullOrEmpty(model.CrMasRenterInformationGender)) model.CrMasRenterInformationGender = "1100000002";
            model.CrMasRenterInformationUpDateLicenseData = DateTime.Now.Date;
            model.CrMasRenterInformationUpDatePersonalData = DateTime.Now.Date;
            model.CrMasRenterInformationUpDateWorkplaceData = DateTime.Now.Date;
            model.CrMasRenterInformationReasons = null;
            if (await _unitOfWork.CrMasRenterInformation.AddAsync(model) == null) return null;
            return model;
        }

        public async Task<CrMasRenterPost> AddRenterForMASRenterPost(string RenterId, string cityCode, string arAdress, string enAdress)
        {
            if (RenterId != null && cityCode != null)
            {
                var region = _unitOfWork.CrMasSupPostCity.Find(x => x.CrMasSupPostCityCode == cityCode);

                var concatenatedArAddress = "";
                var concatenatedEnAddress = "";

                var CityRegionAr = !string.IsNullOrEmpty(region.CrMasSupPostCityConcatenateArName) ? region.CrMasSupPostCityConcatenateArName : string.Empty;
                var CityRegionEn = !string.IsNullOrEmpty(region.CrMasSupPostCityConcatenateEnName) ? region.CrMasSupPostCityConcatenateEnName : string.Empty;

                arAdress = !string.IsNullOrEmpty(arAdress) ? arAdress : string.Empty;
                enAdress = !string.IsNullOrEmpty(enAdress) ? enAdress : string.Empty;

                // Concatenate only non-empty values, removing the need for the separator if either is null or empty
                concatenatedArAddress = string.Join(" - ", new[] { CityRegionAr, arAdress }.Where(s => !string.IsNullOrEmpty(s)));
                concatenatedEnAddress = string.Join(" - ", new[] { CityRegionEn, enAdress }.Where(s => !string.IsNullOrEmpty(s)));
                CrMasRenterPost crMasRenterPost = new CrMasRenterPost
                {
                    CrMasRenterPostCode = RenterId,
                    CrMasRenterPostCity = cityCode,
                    CrMasRenterPostRegions = region.CrMasSupPostCityRegionsCode,
                    CrMasRenterPostArDistrict = arAdress,
                    CrMasRenterPostEnDistrict = enAdress,
                    CrMasRenterPostArConcatenate = concatenatedArAddress,
                    CrMasRenterPostEnConcatenate = concatenatedEnAddress,
                    CrMasRenterPostStatus = Status.Active,
                    CrMasRenterPostUpDatePost = DateTime.Now.Date,
                    CrMasRenterPostReasons = ""
                };
                if (await _unitOfWork.CrMasRenterPost.AddAsync(crMasRenterPost) == null) return null;
                return crMasRenterPost;
            }
            return null;
        }

        public async Task<CrCasRenterLessor> AddRenterToCasRenterLessor(string LessorCode, CrMasRenterInformation crMasRenterInformation, CrMasRenterPost crMasRenterPost)
        {
            var ageCode = "";
            var RegionCode = "";
            var CityCode = "";
            if (crMasRenterPost != null)
            {
                RegionCode = crMasRenterPost.CrMasRenterPostRegions;
                CityCode = crMasRenterPost.CrMasRenterPostCity;
            }
            else
            {
                RegionCode = "11";
                CityCode = "1700000002";
            }

            if (crMasRenterInformation != null) ageCode = GetAge(crMasRenterInformation.CrMasRenterInformationBirthDate?.ToString("yyyy/MM/dd"));
            CrCasRenterLessor casRenterLessor = new CrCasRenterLessor
            {
                CrCasRenterLessorId = crMasRenterInformation.CrMasRenterInformationId,
                CrCasRenterLessorCode = LessorCode,
                CrCasRenterLessorSector = crMasRenterInformation?.CrMasRenterInformationSector,
                CrCasRenterLessorCopyId = crMasRenterInformation?.CrMasRenterInformationCopyId,
                CrCasRenterLessorIdtrype = crMasRenterInformation?.CrMasRenterInformationIdtype,
                CrCasRenterLessorMembership = Membership.Mutual, // This The first memeber ship for every new renter 1600000006
                CrCasRenterLessorDateFirstInteraction = DateTime.Now.Date,
                CrCasRenterLessorDateLastContractual = null,
                CrCasRenterLessorContractCount = 0,
                CrCasRenterLessorContractDays = 0,
                CrCasRenterLessorContractKm = 0,
                CrCasRenterLessorContractTradedAmount = 0,
                CrCasRenterLessorContractExtension = 0,
                CrCasRenterLessorEvaluationNumber = 0,
                CrCasRenterLessorEvaluationTotal = 0,
                CrCasRenterLessorEvaluationValue = 0,
                CrCasRenterLessorBalance = 0,
                CrCasRenterLessorAvailableBalance = 0,
                CrCasRenterLessorReservedBalance = 0,
                CrCasRenterLessorStatisticsNationalities = crMasRenterInformation?.CrMasRenterInformationNationality,
                CrCasRenterLessorStatisticsGender = crMasRenterInformation?.CrMasRenterInformationGender,
                CrCasRenterLessorStatisticsJobs = crMasRenterInformation?.CrMasRenterInformationProfession,
                CrCasRenterLessorStatisticsRegions = RegionCode,
                CrCasRenterLessorStatisticsCity = CityCode,
                CrCasRenterLessorStatisticsAge = ageCode,
                CrCasRenterLessorStatisticsTraded = "1",
                CrCasRenterLessorDealingMechanism = "10", //I Dont Know why put this 10 
                CrCasRenterLessorStatus = Status.Active,
                CrCasRenterLessorReasons = "",
            };
            if (await _unitOfWork.CrCasRenterLessor.AddAsync(casRenterLessor) == null) return null;
            return casRenterLessor;
        }

        public async Task<CrMasRenterInformation> UpdateRenterToMASRenterInformation(CrMasRenterInformation model, string EmployerName, string personalType)
        {
            var masRenter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == model.CrMasRenterInformationId);
            if (masRenter != null)
            {
                var employerCode = "";
                var sectorCode = "";

                if (model != null)
                {
                    var firstChar = model.CrMasRenterInformationId.Substring(0, 1);
                    sectorCode = GetSector(firstChar, "");
                    if (string.IsNullOrEmpty(model.CrMasRenterInformationEmployer)) employerCode = GetEmployer(firstChar, EmployerName);
                    else employerCode = model.CrMasRenterInformationEmployer;
                }

                masRenter.CrMasRenterInformationIdtype = model.CrMasRenterInformationIdtype;
                masRenter.CrMasRenterInformationArName = model.CrMasRenterInformationArName;
                masRenter.CrMasRenterInformationEnName = model.CrMasRenterInformationEnName;
                masRenter.CrMasRenterInformationNationality = model.CrMasRenterInformationNationality;
                if (string.IsNullOrEmpty(model.CrMasRenterInformationProfession)) masRenter.CrMasRenterInformationProfession = "1400000002";
                if (string.IsNullOrEmpty(model.CrMasRenterInformationGender)) masRenter.CrMasRenterInformationGender = "1100000002";
                masRenter.CrMasRenterInformationEmployer = employerCode;
                masRenter.CrMasRenterInformationCountreyKey = model.CrMasRenterInformationCountreyKey;
                masRenter.CrMasRenterInformationMobile = model.CrMasRenterInformationMobile;
                masRenter.CrMasRenterInformationEmail = model.CrMasRenterInformationEmail;
                //masRenter.CrMasRenterInformationSector = sectorCode;
                if (personalType != PersonalType.Renter)
                {
                    masRenter.CrMasRenterInformationDrivingLicenseNo = model.CrMasRenterInformationDrivingLicenseNo ?? null;
                    masRenter.CrMasRenterInformationDrivingLicenseType = model.CrMasRenterInformationDrivingLicenseType ?? null;
                    masRenter.CrMasRenterInformationExpiryDrivingLicenseDate = model.CrMasRenterInformationExpiryDrivingLicenseDate ?? null;
                }

                if (_unitOfWork.CrMasRenterInformation.Update(masRenter) == null) return null;
            }
            return masRenter;
        }

        public async Task<CrMasRenterPost> UpdateRenterForMASRenterPost(string RenterId, string cityCode, string arAdress, string enAdress)
        {
            if (RenterId != null && cityCode != null)
            {
                var renterPost = await _unitOfWork.CrMasRenterPost.FindAsync(x => x.CrMasRenterPostCode == RenterId);
                var region = _unitOfWork.CrMasSupPostCity.Find(x => x.CrMasSupPostCityCode == cityCode);


                var concatenatedArAddress = "";
                var concatenatedEnAddress = "";

                var CityRegionAr = !string.IsNullOrEmpty(region.CrMasSupPostCityConcatenateArName) ? region.CrMasSupPostCityConcatenateArName : string.Empty;
                var CityRegionEn = !string.IsNullOrEmpty(region.CrMasSupPostCityConcatenateEnName) ? region.CrMasSupPostCityConcatenateEnName : string.Empty;

                arAdress = !string.IsNullOrEmpty(arAdress) ? arAdress : string.Empty;
                enAdress = !string.IsNullOrEmpty(enAdress) ? enAdress : string.Empty;

                // Concatenate only non-empty values, removing the need for the separator if either is null or empty
                concatenatedArAddress = string.Join(" - ", new[] { CityRegionAr, arAdress }.Where(s => !string.IsNullOrEmpty(s)));
                concatenatedEnAddress = string.Join(" - ", new[] { CityRegionEn, enAdress }.Where(s => !string.IsNullOrEmpty(s)));

                if (renterPost != null && region != null)
                {
                    renterPost.CrMasRenterPostCity = cityCode;
                    renterPost.CrMasRenterPostRegions = region.CrMasSupPostCityRegionsCode;
                    renterPost.CrMasRenterPostArDistrict = arAdress;
                    renterPost.CrMasRenterPostEnDistrict = enAdress;
                    renterPost.CrMasRenterPostArConcatenate = concatenatedArAddress;
                    renterPost.CrMasRenterPostEnConcatenate = concatenatedEnAddress;
                    renterPost.CrMasRenterPostUpDatePost = DateTime.Now.Date;
                }
                if (_unitOfWork.CrMasRenterPost.Update(renterPost) != null) return renterPost;
            }
            return null;
        }

        public async Task<CrCasRenterLessor> UpdateRenterToCasRenterLessor(string LessorCode, CrMasRenterInformation crMasRenterInformation, CrMasRenterPost crMasRenterPost)
        {
            var renterLessor = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == crMasRenterInformation.CrMasRenterInformationId && x.CrCasRenterLessorCode == LessorCode);
            if (renterLessor != null)
            {
                var ageCode = "";
                var RegionCode = "";
                var CityCode = "";
                if (crMasRenterPost != null)
                {
                    RegionCode = crMasRenterPost.CrMasRenterPostRegions;
                    CityCode = crMasRenterPost.CrMasRenterPostCity;
                }
                else
                {
                    RegionCode = "11";
                    CityCode = "1700000002";
                }

                if (crMasRenterInformation != null) ageCode = GetAge(crMasRenterInformation.CrMasRenterInformationBirthDate?.ToString("yyyy/MM/dd"));

                renterLessor.CrCasRenterLessorIdtrype = crMasRenterInformation?.CrMasRenterInformationIdtype;
                renterLessor.CrCasRenterLessorStatisticsNationalities = crMasRenterInformation?.CrMasRenterInformationNationality;
                renterLessor.CrCasRenterLessorStatisticsGender = crMasRenterInformation?.CrMasRenterInformationGender;
                renterLessor.CrCasRenterLessorStatisticsJobs = crMasRenterInformation?.CrMasRenterInformationProfession;
                renterLessor.CrCasRenterLessorStatisticsRegions = RegionCode;
                renterLessor.CrCasRenterLessorStatisticsCity = CityCode;
                renterLessor.CrCasRenterLessorStatisticsAge = ageCode;
                if (_unitOfWork.CrCasRenterLessor.Update(renterLessor) != null) return renterLessor;
            }
            return null;
        }
        // For New System




        public async Task<bool> AddRenterContractChoice(string LessorCode, string ContractNo, string SerialNo, string PriceNo, string Choice)
        {
            CrCasRenterContractChoice renterContractChoice = new CrCasRenterContractChoice();

            var carInfo = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo);
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == PriceNo);
            if (carInfo != null && carPrice != null)
            {
                var carChoice = _unitOfWork.CrCasPriceCarOption.Find(x => x.CrCasPriceCarOptionsNo == carPrice.CrCasPriceCarBasicNo && x.CrCasPriceCarOptionsCode == Choice);
                if (carChoice != null)
                {
                    renterContractChoice.CrCasRenterContractChoiceNo = ContractNo;
                    renterContractChoice.CrCasRenterContractChoiceCode = carChoice.CrCasPriceCarOptionsCode;
                    renterContractChoice.CrCasContractChoiceValue = carChoice.CrCasPriceCarOptionsValue;
                }
            }
            if (renterContractChoice != null && _unitOfWork.CrCasRenterContractChoice.Add(renterContractChoice) != null) return true;
            return false;
        }
        public async Task<bool> AddRenterContractAdditional(string LessorCode, string ContractNo, string SerialNo, string PriceNo, string Additional)
        {

            CrCasRenterContractAdditional renterContractAdditional = new CrCasRenterContractAdditional();

            var carInfo = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo);
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == PriceNo);
            if (carInfo != null && carPrice != null)
            {
                var carAdditional = _unitOfWork.CrCasPriceCarAdditional.Find(x => x.CrCasPriceCarAdditionalNo == carPrice.CrCasPriceCarBasicNo && x.CrCasPriceCarAdditionalCode == Additional);
                if (carAdditional != null)
                {
                    renterContractAdditional.CrCasRenterContractAdditionalNo = ContractNo;
                    renterContractAdditional.CrCasRenterContractAdditionalCode = carAdditional.CrCasPriceCarAdditionalCode;
                    renterContractAdditional.CrCasContractAdditionalValue = carAdditional.CrCasPriceCarAdditionalValue;
                }
            }
            if (renterContractAdditional != null && _unitOfWork.CrCasRenterContractAdditional.Add(renterContractAdditional) != null) return true;
            return false;
        }
        public async Task<bool> AddRenterContractAdvantages(CrCasPriceCarAdvantage crCasPriceCarAdvantage, string ContractNo)
        {
            CrCasRenterContractAdvantage crCasRenterContractAdvantage = new CrCasRenterContractAdvantage();
            if (crCasPriceCarAdvantage != null)
            {
                crCasRenterContractAdvantage.CrCasRenterContractAdvantagesNo = ContractNo;
                crCasRenterContractAdvantage.CrCasRenterContractAdvantagesCode = crCasPriceCarAdvantage.CrCasPriceCarAdvantagesCode;
                crCasRenterContractAdvantage.CrCasContractAdvantagesValue = crCasPriceCarAdvantage.CrCasPriceCarAdvantagesValue;
                if (await _unitOfWork.CrCasRenterContractAdvantage.AddAsync(crCasRenterContractAdvantage) != null) return true;
            }
            return false;
        }
        public async Task<bool> AddRenterContractCheckUp(string LessorCode, string ContractNo, string SerialNo, string PriceNo, string CheckUpCode, string ReasonCheckCode, bool Status, string Reasons)
        {
            CrCasRenterContractCarCheckup renterContractCarCheckup = new CrCasRenterContractCarCheckup();

            var carInfo = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo);
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == PriceNo);
            if (carInfo != null && carPrice != null)
            {
                var carCheckUp = _unitOfWork.CrMasSupContractCarCheckup.Find(x => x.CrMasSupContractCarCheckupCode == CheckUpCode);
                if (carCheckUp != null)
                {
                    renterContractCarCheckup.CrCasRenterContractCarCheckupNo = ContractNo;
                    renterContractCarCheckup.CrCasRenterContractCarCheckupCode = carCheckUp.CrMasSupContractCarCheckupCode;
                    renterContractCarCheckup.CrCasRenterContractCarCheckupType = "1";
                    renterContractCarCheckup.CrCasRenterContractCarCheckupCheck = ReasonCheckCode;
                    renterContractCarCheckup.CrCasRenterContractCarCheckupStatus = Status;
                    renterContractCarCheckup.CrCasRenterContractCarCheckupReasons = Reasons;
                }
            }

            if (renterContractCarCheckup != null && _unitOfWork.CrCasRenterContractCarCheckup.Add(renterContractCarCheckup) != null) return true;
            return false;
        }
        public async Task<bool> AddRenterContractAuthrization(string ContractNo, string LessorCode, string AuthrizationType, string AuthrizationValue)
        {
            DateTime now = DateTime.Now;
            CrCasRenterContractAuthorization authorization = new CrCasRenterContractAuthorization();

            authorization.CrCasRenterContractAuthorizationContractNo = ContractNo;
            authorization.CrCasRenterContractAuthorizationLessor = LessorCode;
            if (AuthrizationType == "true") authorization.CrCasRenterContractAuthorizationType = true;
            else authorization.CrCasRenterContractAuthorizationType = false;
            authorization.CrCasRenterContractAuthorizationStartDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            authorization.CrCasRenterContractAuthorizationDaysNo = 365;
            authorization.CrCasRenterContractAuthorizationValue = decimal.Parse(AuthrizationValue, CultureInfo.InvariantCulture);
            //authorization.CrCasRenterContractAuthorizationValue = AuthrizationValue;
            authorization.CrCasRenterContractAuthorizationEndDate = authorization.CrCasRenterContractAuthorizationStartDate?.AddDays(Convert.ToDouble(authorization.CrCasRenterContractAuthorizationDaysNo));
            authorization.CrCasRenterContractAuthorizationStatus = Status.Active;
            //authorization.CrCasRenterContractAuthorizationAction = Status.Active;
            authorization.CrCasRenterContractAuthorizationAction = true;


            if (await _unitOfWork.CrCasRenterContractAuthorization.AddAsync(authorization) != null) return true;
            return false;
        }
        public async Task<CrCasRenterContractBasic> AddRenterContractBasic(string LessorCode, string BranchCode, string BranchReceivingCode, string ContractNo, string RenterId, string sectorCodeForRenter, string DriverId, string PrivateDriver,
                                                       string AdditionalDriver, string SerialNo, string PriceNo, string DaysNo, string UserFreeHour, string UserFreeKm,
                                                       string CurrentMeter, string OptionsTotal, string AdditionalTotal, string ContractValueAfterDiscount,
                                                       string DiscountValue, string ContractValueBeforeDiscount, string TaxValue, string TotalAmount, string UserInsert,
                                                       string Authrization, string UserDiscount, string AmountPayed, string ContractPdf, string PolicyCode, string SourceCode, string ContractType, string Reasons)
        {
            DateTime now = DateTime.Now;
            CrCasRenterContractBasic renterContractBasic = new CrCasRenterContractBasic();
            var masRenterInfo = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == RenterId);
            var renterlessorInfo = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == RenterId);
            var carInfo = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo);
            var carPrice = _unitOfWork.CrCasPriceCarBasic.Find(x => x.CrCasPriceCarBasicNo == PriceNo);
            var lessor = await _unitOfWork.CrMasLessorInformation.FindAsync(x => x.CrMasLessorInformationCode == LessorCode);


            renterContractBasic.CrCasRenterContractBasicNo = ContractNo;
            renterContractBasic.CrCasRenterContractBasicCopy = 0;
            renterContractBasic.CrCasRenterContractBasicYear = DateTime.Now.ToString("yy");
            renterContractBasic.CrCasRenterContractBasicSector = sectorCodeForRenter;
            renterContractBasic.CrCasRenterContractBasicProcedures = "401";
            renterContractBasic.CrCasRenterContractBasicLessor = LessorCode;
            renterContractBasic.CrCasRenterContractBasicBranch = BranchCode;
            renterContractBasic.CrCasRenterContractBasicBranchRecevied = string.IsNullOrEmpty(BranchReceivingCode) ? BranchCode : BranchReceivingCode;
            renterContractBasic.CrCasRenterContractBasicPolicy = int.Parse(PolicyCode);
            renterContractBasic.CrCasRenterContractBasicSource = SourceCode;
            renterContractBasic.CrCasRenterContractBasicTgaType = int.Parse(ContractType);
            renterContractBasic.CrCasRenterContractBasicIssuedDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            renterContractBasic.CrCasRenterContractBasicExpectedStartDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            renterContractBasic.CrCasRenterContractBasicExpectedRentalDays = int.Parse(DaysNo);
            renterContractBasic.CrCasRenterContractBasicExpectedEndDate = renterContractBasic.CrCasRenterContractBasicExpectedStartDate?.AddDays(int.Parse(DaysNo));
            renterContractBasic.CrCasRenterContractBasicActualCloseDateTime = renterContractBasic.CrCasRenterContractBasicExpectedEndDate;
            renterContractBasic.CrCasRenterContractBasicAllowCanceled = renterContractBasic.CrCasRenterContractBasicExpectedStartDate?.AddHours(Convert.ToDouble(carPrice.CrCasPriceCarBasicCancelHour));
            renterContractBasic.CrCasRenterContractBasicRenterId = RenterId;
            renterContractBasic.CrCasRenterContractBasicCarSerailNo = SerialNo;
            renterContractBasic.CrCasRenterContractBasicFuelValue = carInfo.CrCasCarInformationFuelValue;
            renterContractBasic.CrCasRenterContractBasicOwner = carInfo.CrCasCarInformationOwner;
            if (RenterId != DriverId)
            {
                if (!string.IsNullOrEmpty(DriverId)) renterContractBasic.CrCasRenterContractBasicDriverId = DriverId;
                else renterContractBasic.CrCasRenterContractBasicDriverId = null;
            }
            else
            {
                renterContractBasic.CrCasRenterContractBasicDriverId = null;
            }


            if (!string.IsNullOrEmpty(AdditionalDriver))
            {
                renterContractBasic.CrCasRenterContractBasicAdditionalDriverId = AdditionalDriver;
                renterContractBasic.CrCasRenterContractBasicAdditionalDriverValue = carPrice.CrCasPriceCarBasicAdditionalDriverValue;
            }
            else
            {
                renterContractBasic.CrCasRenterContractBasicAdditionalDriverId = null;
                renterContractBasic.CrCasRenterContractBasicAdditionalDriverValue = 0;

            }
            ;
            if (!string.IsNullOrEmpty(PrivateDriver))
            {
                renterContractBasic.CrCasRenterContractBasicPrivateDriverValue = carPrice.CrCasPriceCarBasicPrivateDriverValue ?? 0;
                renterContractBasic.CrCasRenterContractBasicPrivateDriverId = PrivateDriver;
                renterContractBasic.CrCasRenterContractBasicExpectedPrivateDriverValue = renterContractBasic.CrCasRenterContractBasicPrivateDriverValue * int.Parse(DaysNo);
            }
            else
            {
                renterContractBasic.CrCasRenterContractBasicPrivateDriverId = null;
                renterContractBasic.CrCasRenterContractBasicExpectedPrivateDriverValue = 0;
                renterContractBasic.CrCasRenterContractBasicPrivateDriverValue = 0;
            }
            ;
            //Get Data From Car Price Info

            // Hours
            renterContractBasic.CrCasRenterContractBasicFreeHours = carPrice.CrCasPriceCarBasicFreeAdditionalHours;
            renterContractBasic.CrCasRenterContractBasicUserFreeHours = int.Parse(UserFreeHour);
            renterContractBasic.CrCasRenterContractBasicTotalFreeHours = renterContractBasic.CrCasRenterContractBasicFreeHours + renterContractBasic.CrCasRenterContractBasicUserFreeHours;
            renterContractBasic.CrCasRenterContractBasicHourMax = carPrice.CrCasPriceCarBasicHourMax;
            renterContractBasic.CrCasRenterContractBasicHourValue = carPrice.CrCasPriceCarBasicExtraHourValue;

            // Km
            renterContractBasic.CrCasRenterContractBasicDailyFreeKm = carPrice.CrCasPriceCarBasicNoDailyFreeKm;
            renterContractBasic.CrCasRenterContractBasicDailyFreeKmUser = int.Parse(UserFreeKm);
            renterContractBasic.CrCasRenterContractBasicTotalDailyFreeKm = renterContractBasic.CrCasRenterContractBasicDailyFreeKm + renterContractBasic.CrCasRenterContractBasicDailyFreeKmUser;
            renterContractBasic.CrCasRenterContractBasicKmValue = carPrice.CrCasPriceCarBasicAdditionalKmValue;
            //Rental Value
            renterContractBasic.CrCasRenterContractBasicDailyRent = carPrice.CrCasPriceCarBasicDailyRent;
            renterContractBasic.CrCasRenterContractBasicWeeklyRent = carPrice.CrCasPriceCarBasicWeeklyRent;
            renterContractBasic.CrCasRenterContractBasicMonthlyRent = carPrice.CrCasPriceCarBasicMonthlyRent;
            renterContractBasic.CrCasRenterContractBasicYearlyRent = carPrice.CrCasPriceCarBasicYearlyRent;
            // Additionals
            if (Authrization == "true") renterContractBasic.CrCasRenterContractBasicAuthorizationValue = carPrice.CrCasCarPriceBasicOutFeesTamm;
            else renterContractBasic.CrCasRenterContractBasicAuthorizationValue = carPrice.CrCasCarPriceBasicInFeesTamm;
            renterContractBasic.CrCasRenterContractBasicTaxRate = carPrice.CrCasPriceCarBasicRentalTaxRate;
            renterContractBasic.CrCasRenterContractBasicUserDiscountRate = decimal.Parse(UserDiscount, CultureInfo.InvariantCulture);
            //Info From Payment Tab From Create Contract 
            renterContractBasic.CrCasRenterContractBasicCurrentReadingMeter = int.Parse(CurrentMeter);
            renterContractBasic.CrCasRenterContractBasicExpectedRentValue = carPrice.CrCasPriceCarBasicDailyRent * int.Parse(DaysNo);
            renterContractBasic.CrCasRenterContractBasicExpectedOptionsValue = decimal.Parse(OptionsTotal, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicAdditionalValue = decimal.Parse(AdditionalTotal, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicExpectedValueBeforDiscount = decimal.Parse(ContractValueBeforeDiscount, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicExpectedDiscountValue = decimal.Parse(DiscountValue, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicExpectedValueAfterDiscount = decimal.Parse(ContractValueAfterDiscount, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicExpectedTaxValue = decimal.Parse(TaxValue, CultureInfo.InvariantCulture);
            renterContractBasic.CrCasRenterContractBasicExpectedTotal = decimal.Parse(ContractValueAfterDiscount, CultureInfo.InvariantCulture) + decimal.Parse(TaxValue, CultureInfo.InvariantCulture);
            if (renterlessorInfo != null)
            {
                renterContractBasic.CrCasRenterContractBasicPreviousBalance = renterlessorInfo.CrCasRenterLessorAvailableBalance ?? 0;
                renterContractBasic.CrCasRenterContractBasicAmountRequired = renterContractBasic.CrCasRenterContractBasicExpectedTotal + (renterlessorInfo.CrCasRenterLessorBalance ?? 0);
            }
            else
            {
                renterContractBasic.CrCasRenterContractBasicPreviousBalance = 0;
                renterContractBasic.CrCasRenterContractBasicAmountRequired = 0;
            }




            if (!string.IsNullOrEmpty(AmountPayed)) renterContractBasic.CrCasRenterContractBasicAmountPaidAdvance = decimal.Parse(AmountPayed, CultureInfo.InvariantCulture);
            else renterContractBasic.CrCasRenterContractBasicAmountPaidAdvance = 0;




            renterContractBasic.CrCasRenterContractPriceReference = carPrice.CrCasPriceCarBasicNo;
            renterContractBasic.CrCasRenterContractBasicUserInsert = UserInsert;
            renterContractBasic.CrCasRenterContractBasicStatus = Status.Active;
            renterContractBasic.CrCasRenterContractBasicPdfFile = ContractPdf;
            renterContractBasic.CrCasRenterContractBasicReasons = Reasons;







            if (_unitOfWork.CrCasRenterContractBasic.Add(renterContractBasic) != null) return renterContractBasic;
            else return null;
        }


        public async Task<bool> UpdateCarInformation(string SerialNo, string LessorCode, string BranchCode, DateTime LastContract, int DaysNo, int CurrentMeter, string ExpireMaintainceCount)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo && x.CrCasCarInformationLessor == LessorCode && x.CrCasCarInformationBranch == BranchCode);


            if (car != null)
            {
                //if (car.CrCasCarInformationConractDaysNo != null) car.CrCasCarInformationConractDaysNo += DaysNo;
                //else car.CrCasCarInformationConractDaysNo = DaysNo;
                if (car.CrCasCarInformationConractCount != null) car.CrCasCarInformationConractCount += 1;
                else car.CrCasCarInformationConractCount = 1;
                car.CrCasCarInformationStatus = Status.Rented;
                car.CrCasCarInformationLastContractDate = LastContract;
                car.CrCasCarInformationCurrentMeter = CurrentMeter;
                if (ExpireMaintainceCount != null && ExpireMaintainceCount != "0") car.CrCasCarInformationMaintenanceStatus = false;
                if (_unitOfWork.CrCasCarInformation.Update(car) != null) return true;
            }
            return false;
        }

        public async Task<string> UpdateCarDocMaintainance(string SerialNo, string LessorCode, string BranchCode, int CurrentMeter)
        {
            var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == SerialNo && x.CrCasCarInformationLessor == LessorCode && x.CrCasCarInformationBranch == BranchCode);
            var CarDocMaintainances = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(x => x.CrCasCarDocumentsMaintenanceSerailNo == car.CrCasCarInformationSerailNo && x.CrCasCarDocumentsMaintenanceLessor == LessorCode &&
                                                                                            x.CrCasCarDocumentsMaintenanceBranch == BranchCode && x.CrCasCarDocumentsMaintenanceStatus == Status.Active).ToList();

            if (CarDocMaintainances != null)
            {
                foreach (var item in CarDocMaintainances.Where(x => x.CrCasCarDocumentsMaintenanceProceduresClassification == "12"))
                {
                    item.CrCasCarDocumentsMaintenanceCarStatus = Status.Rented;
                    _unitOfWork.CrCasCarDocumentsMaintenance.Update(item);
                }
                foreach (var item in CarDocMaintainances.Where(x => x.CrCasCarDocumentsMaintenanceProceduresClassification == "13"))
                {
                    item.CrCasCarDocumentsMaintenanceCurrentMeter = CurrentMeter;
                    item.CrCasCarDocumentsMaintenanceCarStatus = Status.Rented;
                    if (CurrentMeter >= item.CrCasCarDocumentsMaintenanceKmEndsAt) item.CrCasCarDocumentsMaintenanceStatus = Status.Expire;
                    if (CurrentMeter >= item.CrCasCarDocumentsMaintenanceKmAboutToFinish && CurrentMeter < item.CrCasCarDocumentsMaintenanceKmEndsAt) item.CrCasCarDocumentsMaintenanceStatus = Status.AboutToExpire;
                    _unitOfWork.CrCasCarDocumentsMaintenance.Update(item);
                }
                return CarDocMaintainances.Where(x => x.CrCasCarDocumentsMaintenanceProceduresClassification == "13").Count().ToString();
            }
            return null;
        }

        public async Task<bool> UpdateRenterLessor(string RenterId, string LessorCode, DateTime LastContract, decimal TotalPayed, decimal RequiredValue, string RenterReasons)
        {
            var RenterLessor = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == RenterId && x.CrCasRenterLessorCode == LessorCode);
            if (RenterLessor != null)
            {
                RenterLessor.CrCasRenterLessorDateLastContractual = LastContract;
                if (RenterLessor.CrCasRenterLessorContractCount != null) RenterLessor.CrCasRenterLessorContractCount += 1;
                else RenterLessor.CrCasRenterLessorContractCount = 1;

                RenterLessor.CrCasRenterLessorBalance += TotalPayed;
                RenterLessor.CrCasRenterLessorReservedBalance += RequiredValue;
                RenterLessor.CrCasRenterLessorAvailableBalance = RenterLessor.CrCasRenterLessorBalance - RenterLessor.CrCasRenterLessorReservedBalance;

                RenterLessor.CrCasRenterLessorStatus = Status.Rented;
                RenterLessor.CrCasRenterLessorReasons = RenterReasons;
                RenterLessor.CrCasRenterLessorContractExtension = 0;
                if (_unitOfWork.CrCasRenterLessor.Update(RenterLessor) != null) return true;
            }
            return false;
        }
        public async Task<bool> UpdateMasRenter(string RenterId)
        {
            var Renter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == RenterId);
            if (Renter != null)
            {
                Renter.CrMasRenterInformationStatus = Status.Rented;
                if (_unitOfWork.CrMasRenterInformation.Update(Renter) != null) return true;
            }
            return false;
        }
        public async Task<bool> UpdateDriverStatus(string DriverId, string LessorCode, string Reasons)
        {
            var Driver = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == DriverId && x.CrCasRenterLessorCode == LessorCode);

            if (Driver != null)
            {
                //Driver.CrCasRenterLessorStatus = Status.Rented;
                //Driver.CrCasRenterLessorDateLastContractual = DateTime.Now;
                Driver.CrCasRenterLessorReasons = Reasons;
                if (_unitOfWork.CrCasRenterLessor.Update(Driver) != null) return true;
            }
            return false;
        }
        public async Task<bool> UpdatePrivateDriverStatus(string PrivateDriverId, string LessorCode)
        {
            var PrivateDriver = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAsync(x => x.CrCasRenterPrivateDriverInformationId == PrivateDriverId && x.CrCasRenterPrivateDriverInformationLessor == LessorCode);

            if (PrivateDriver != null)
            {
                PrivateDriver.CrCasRenterPrivateDriverInformationStatus = Status.Rented;
                if (_unitOfWork.CrCasRenterPrivateDriverInformation.Update(PrivateDriver) != null) return true;
            }
            return false;
        }


        public async Task<bool> UpdateBranchBalance(string BranchCode, string LessorCode, decimal AmountPaid)
        {
            var Branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationCode == BranchCode && x.CrCasBranchInformationLessor == LessorCode);
            if (Branch != null)
            {
                if (Branch.CrCasBranchInformationAvailableBalance != null) Branch.CrCasBranchInformationAvailableBalance += AmountPaid;
                else Branch.CrCasBranchInformationAvailableBalance = AmountPaid;
                if (Branch.CrCasBranchInformationTotalBalance != null) Branch.CrCasBranchInformationTotalBalance += AmountPaid;
                else Branch.CrCasBranchInformationTotalBalance = AmountPaid;

                if (_unitOfWork.CrCasBranchInformation.Update(Branch) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateSalesPointBalance(string BranchCode, string LessorCode, string SalesPointCode, decimal AmountPaid)
        {
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == SalesPointCode &&
                                                                                     x.CrCasAccountSalesPointLessor == LessorCode &&
                                                                                     x.CrCasAccountSalesPointBrn == BranchCode);
            if (SalesPoint != null)
            {
                if (SalesPoint.CrCasAccountSalesPointTotalAvailable != null) SalesPoint.CrCasAccountSalesPointTotalAvailable += AmountPaid;
                else SalesPoint.CrCasAccountSalesPointTotalAvailable = AmountPaid;
                if (SalesPoint.CrCasAccountSalesPointTotalBalance != null) SalesPoint.CrCasAccountSalesPointTotalBalance += AmountPaid;
                else SalesPoint.CrCasAccountSalesPointTotalBalance = AmountPaid;
                if (_unitOfWork.CrCasAccountSalesPoint.Update(SalesPoint) != null) return true;
            }
            return false;
        }

        public async Task<CrCasAccountReceipt> AddAccountReceipt(string ContractNo, string LessorCode, string BranchCode, string PaymentMethod, string Account, string SerialNo, string SalesPointNo, decimal TotalPayed,
                                                                                                                        string RenterId, string sector, string UserId, string PassingType, string Reasons, string pdfPath)
        {
            CrCasAccountReceipt crCasAccountReceipt = new CrCasAccountReceipt();
            var User = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == UserId && x.CrMasUserInformationLessor == LessorCode);
            var Renter = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == RenterId && x.CrCasRenterLessorCode == LessorCode);
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == SalesPointNo && x.CrCasAccountSalesPointLessor == LessorCode && x.CrCasAccountSalesPointBrn == BranchCode,
                                                                                new[] { "CrCasAccountSalesPointBankNavigation", "CrCasAccountSalesPointAccountBankNavigation" });
            var AccountBank = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == Account && x.CrCasAccountBankLessor == LessorCode, new[] { "CrCasAccountBankNoNavigation" });
            var userValidity = await _unitOfWork.CrMasUserBranchValidity.FindAsync(x => x.CrMasUserBranchValidityLessor == LessorCode && x.CrMasUserBranchValidityBranch == BranchCode && x.CrMasUserBranchValidityId == User.CrMasUserInformationCode);
            var userBranchValidityBalance = userValidity.CrMasUserBranchValidityBranchCashBalance + userValidity.CrMasUserBranchValidityBranchSalesPointBalance + userValidity.CrMasUserBranchValidityBranchTransferBalance;
            //Get ContractCode
            DateTime now = DateTime.Now;
            var y = now.ToString("yy");
            var autoinc = GetContractAccountReceipt(LessorCode, BranchCode).CrCasAccountReceiptNo;
            var AccountReceiptNo = y + "-" + sector + "301" + "-" + LessorCode + BranchCode + "-" + autoinc;

            crCasAccountReceipt.CrCasAccountReceiptNo = AccountReceiptNo;
            crCasAccountReceipt.CrCasAccountReceiptYear = y;
            crCasAccountReceipt.CrCasAccountReceiptType = "301"; //Create Contract
            crCasAccountReceipt.CrCasAccountReceiptLessorCode = LessorCode;
            crCasAccountReceipt.CrCasAccountReceiptBranchCode = BranchCode;
            crCasAccountReceipt.CrCasAccountReceiptDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            crCasAccountReceipt.CrCasAccountReceiptPaymentMethod = PaymentMethod;
            crCasAccountReceipt.CrCasAccountReceiptReferenceType = "10";
            crCasAccountReceipt.CrCasAccountReceiptReferenceNo = ContractNo;
            crCasAccountReceipt.CrCasAccountReceiptCar = SerialNo;
            crCasAccountReceipt.CrCasAccountReceiptUser = UserId;

            if (PassingType != "4")
            {
                crCasAccountReceipt.CrCasAccountReceiptBank = SalesPoint?.CrCasAccountSalesPointBankNavigation?.CrMasSupAccountBankCode;
                crCasAccountReceipt.CrCasAccountReceiptAccount = SalesPoint?.CrCasAccountSalesPointAccountBankNavigation?.CrCasAccountBankCode;
                crCasAccountReceipt.CrCasAccountReceiptSalesPoint = SalesPointNo;

                if (SalesPoint.CrCasAccountSalesPointTotalBalance != null) crCasAccountReceipt.CrCasAccountReceiptSalesPointPreviousBalance = SalesPoint.CrCasAccountSalesPointTotalBalance;
                else crCasAccountReceipt.CrCasAccountReceiptSalesPointPreviousBalance = 0;

                if (User.CrMasUserInformationTotalBalance != null) crCasAccountReceipt.CrCasAccountReceiptUserPreviousBalance = User.CrMasUserInformationTotalBalance;
                else crCasAccountReceipt.CrCasAccountReceiptUserPreviousBalance = 0;
            }
            else
            {
                crCasAccountReceipt.CrCasAccountReceiptBank = AccountBank?.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode;
                crCasAccountReceipt.CrCasAccountReceiptAccount = AccountBank?.CrCasAccountBankCode;
                if (User.CrMasUserInformationTotalBalance != null) crCasAccountReceipt.CrCasAccountReceiptUserPreviousBalance = User.CrMasUserInformationTotalBalance;
                else crCasAccountReceipt.CrCasAccountReceiptUserPreviousBalance = 0;
            }
            crCasAccountReceipt.CrCasAccountReceiptBranchUserPreviousBalance = userBranchValidityBalance;
            crCasAccountReceipt.CrCasAccountReceiptRenterId = RenterId;
            if (Renter != null) crCasAccountReceipt.CrCasAccountReceiptRenterPreviousBalance = Renter.CrCasRenterLessorBalance ?? 0;
            else crCasAccountReceipt.CrCasAccountReceiptRenterPreviousBalance = 0;

            crCasAccountReceipt.CrCasAccountReceiptPayment = TotalPayed;
            crCasAccountReceipt.CrCasAccountReceiptReceipt = 0;
            crCasAccountReceipt.CrCasAccountReceiptIsPassing = PassingType;
            crCasAccountReceipt.CrCasAccountReceiptReasons = Reasons;
            crCasAccountReceipt.CrCasAccountReceiptPdfFile = pdfPath;

            if (await _unitOfWork.CrCasAccountReceipt.AddAsync(crCasAccountReceipt) != null) return crCasAccountReceipt;
            return null;
        }
        private string GetSector(string firstChar, string sectorType)
        {
            var sectorCode = "";
            var sector = _unitOfWork.CrMasSupRenterSector.Find(x => x.CrMasSupRenterSectorArName.Trim() == sectorType.Trim());
            if (sector != null) sectorCode = sector.CrMasSupRenterSectorCode;
            else
            {
                if (firstChar == "7") sectorCode = "0";
                else sectorCode = "1";
            }
            return sectorCode;
        }
        private string GetNationality(string firstChar, string nationalityAr, string nationalityEn)
        {
            var nationalityCode = "";
            if (firstChar == "7") nationalityCode = "1000000001"; // None becasue its not human
            else if (string.IsNullOrEmpty(nationalityAr) && string.IsNullOrEmpty(nationalityEn)) nationalityCode = "1000000002";
            else
            {
                var nationlity = _unitOfWork.CrMasSupRenterNationality.Find(x => x.CrMasSupRenterNationalitiesArName == nationalityAr.Trim() || x.CrMasSupRenterNationalitiesEnName == nationalityEn.Trim());
                if (nationlity != null) nationalityCode = nationlity.CrMasSupRenterNationalitiesCode;
                else // If == Null should be enter the new value in RenterNationlity table ;
                {
                    var lastRecordInNationality = _unitOfWork.CrMasSupRenterNationality.GetAll().LastOrDefault();
                    if (lastRecordInNationality == null) nationalityCode = "1000000003";
                    else
                    {
                        int intValue = int.Parse(lastRecordInNationality.CrMasSupRenterNationalitiesCode);
                        int newIntValue = intValue + 1;
                        nationalityCode = newIntValue.ToString();
                    }
                    CrMasSupRenterNationality crMasSupRenterNationality = new CrMasSupRenterNationality
                    {
                        CrMasSupRenterNationalitiesCode = nationalityCode,
                        CrMasSupRenterNationalitiesArName = nationalityAr.Trim(),
                        CrMasSupRenterNationalitiesEnName = nationalityEn.Trim(),
                        CrMasSupRenterNationalitiesFlag = "",
                        CrMasSupRenterNationalitiesCounter = 0,
                        CrMasSupRenterNationalitiesGroupCode = "10",
                        CrMasSupRenterNationalitiesStatus = Status.Active,
                        CrMasSupRenterNationalitiesReasons = ""
                    };
                    _unitOfWork.CrMasSupRenterNationality.Add(crMasSupRenterNationality);
                    //_unitOfWork.Complete();
                }
            }
            return nationalityCode;
        }
        private string GetGender(string firstChar, string GenderAr, string GenderEn)
        {
            var GenderCode = "";
            if (firstChar == "7") GenderCode = "1100000001"; // None becasue its not human
            else if (string.IsNullOrEmpty(GenderAr) && string.IsNullOrEmpty(GenderEn)) GenderCode = "1100000002"; // Not Avaliable Value
            else
            {
                var gender = _unitOfWork.CrMasSupRenterGender.Find(x => x.CrMasSupRenterGenderArName == GenderAr.Trim() || x.CrMasSupRenterGenderEnName == GenderEn.Trim());
                if (gender != null) GenderCode = gender.CrMasSupRenterGenderCode;
                else
                {
                    var lastRecordInGender = _unitOfWork.CrMasSupRenterGender.GetAll().LastOrDefault();
                    if (lastRecordInGender == null) GenderCode = "1100000003";
                    else
                    {
                        int intValue = int.Parse(lastRecordInGender.CrMasSupRenterGenderCode);
                        int newIntValue = intValue + 1;
                        GenderCode = newIntValue.ToString();
                    }

                    CrMasSupRenterGender crMasSupRenterGender = new CrMasSupRenterGender
                    {
                        CrMasSupRenterGenderCode = GenderCode,
                        CrMasSupRenterGenderArName = GenderAr.Trim(),
                        CrMasSupRenterGenderEnName = GenderEn.Trim(),
                        CrMasSupRenterGenderGroupCode = "11",
                        CrMasSupRenterGenderStatus = Status.Active,
                        CrMasSupRenterGenderReasons = ""
                    };
                    _unitOfWork.CrMasSupRenterGender.Add(crMasSupRenterGender);
                    //_unitOfWork.Complete();
                }
            }
            return GenderCode;
        }
        private string GetProfessions(string firstChar, string ProfessionsAr, string ProfessionsEn)
        {
            var ProfessionsCode = "";
            if (firstChar == "7") ProfessionsCode = "1400000001"; // None becasue its not human
            else if (string.IsNullOrEmpty(ProfessionsAr) && string.IsNullOrEmpty(ProfessionsEn)) ProfessionsCode = "1400000002";
            else
            {
                var Professions = _unitOfWork.CrMasSupRenterProfession.Find(x => x.CrMasSupRenterProfessionsArName == ProfessionsAr.Trim() || x.CrMasSupRenterProfessionsEnName == ProfessionsEn.Trim());
                if (Professions != null) ProfessionsCode = Professions.CrMasSupRenterProfessionsCode;
                else
                {
                    var lastRecordInProfessions = _unitOfWork.CrMasSupRenterProfession.GetAll().LastOrDefault();
                    if (lastRecordInProfessions == null) ProfessionsCode = "1400000003";
                    else
                    {
                        int intValue = int.Parse(lastRecordInProfessions.CrMasSupRenterProfessionsCode);
                        int newIntValue = intValue + 1;
                        ProfessionsCode = newIntValue.ToString();
                    }
                    CrMasSupRenterProfession crMasSupRenterProfession = new CrMasSupRenterProfession
                    {
                        CrMasSupRenterProfessionsCode = ProfessionsCode,
                        CrMasSupRenterProfessionsArName = ProfessionsAr.Trim(),
                        CrMasSupRenterProfessionsEnName = ProfessionsEn.Trim(),
                        CrMasSupRenterProfessionsGroupCode = "14",
                        CrMasSupRenterProfessionsStatus = Status.Active,
                        CrMasSupRenterProfessionsReasons = ""
                    };
                    _unitOfWork.CrMasSupRenterProfession.Add(crMasSupRenterProfession);
                    //_unitOfWork.Complete();
                }
            }
            return ProfessionsCode;
        }
        private string GetLicence(string firstChar, string LicenceAr, string LicenceEn)
        {
            var LicenceCode = "";
            if (string.IsNullOrEmpty(LicenceAr) && string.IsNullOrEmpty(LicenceEn)) LicenceCode = "2";
            else
            {
                var Licence = _unitOfWork.CrMasSupRenterDrivingLicense.Find(x => x.CrMasSupRenterDrivingLicenseArName == LicenceAr.Trim() || x.CrMasSupRenterDrivingLicenseEnName == LicenceEn.Trim());
                if (Licence != null) LicenceCode = Licence.CrMasSupRenterDrivingLicenseCode;
                else
                {

                    var lastRecordInLicence = _unitOfWork.CrMasSupRenterDrivingLicense.GetAll().LastOrDefault();
                    if (lastRecordInLicence == null) LicenceCode = "3";
                    else
                    {
                        int intValue = int.Parse(lastRecordInLicence.CrMasSupRenterDrivingLicenseCode);
                        int newIntValue = intValue + 1;
                        LicenceCode = newIntValue.ToString();
                    }
                    CrMasSupRenterDrivingLicense crMasSupRenterDrivingLicense = new CrMasSupRenterDrivingLicense
                    {
                        CrMasSupRenterDrivingLicenseCode = LicenceCode,
                        CrMasSupRenterDrivingLicenseArName = LicenceAr.Trim(),
                        CrMasSupRenterDrivingLicenseEnName = LicenceEn.Trim(),
                        CrMasSupRenterDrivingLicenseStatus = Status.Active,
                        CrMasSupRenterDrivingLicenseReasons = ""
                    };
                    _unitOfWork.CrMasSupRenterDrivingLicense.Add(crMasSupRenterDrivingLicense);
                }
            }
            return LicenceCode;
        }
        private string GetEmployer(string firstChar, string EmployerName)
        {
            var EmployerCode = "";
            if (firstChar == "7") EmployerCode = "1800000001"; // None becasue its not human
            else if (string.IsNullOrEmpty(EmployerName) && string.IsNullOrEmpty(EmployerName)) EmployerCode = "1800000002";
            else
            {
                var Employer = _unitOfWork.CrMasSupRenterEmployer.Find(x => x.CrMasSupRenterEmployerArName == EmployerName.Trim() || x.CrMasSupRenterEmployerEnName == EmployerName.Trim());
                if (Employer != null) EmployerCode = Employer.CrMasSupRenterEmployerCode;
                else
                {

                    var lastRecordInEmployer = _unitOfWork.CrMasSupRenterEmployer.GetAll().LastOrDefault();
                    if (lastRecordInEmployer == null) EmployerCode = "1800000003";
                    else
                    {
                        int intValue = int.Parse(lastRecordInEmployer.CrMasSupRenterEmployerCode);
                        int newIntValue = intValue + 1;
                        EmployerCode = newIntValue.ToString();
                    }
                    CrMasSupRenterEmployer masSupRenterEmployer = new CrMasSupRenterEmployer
                    {
                        CrMasSupRenterEmployerCode = EmployerCode,
                        CrMasSupRenterEmployerArName = EmployerName.Trim(),
                        CrMasSupRenterEmployerEnName = EmployerName.Trim(),
                        CrMasSupRenterEmployerGroupCode = "18",
                        CrMasSupRenterEmployerCounter = 0,
                        CrMasSupRenterEmployerStatus = Status.Active,
                        CrMasSupRenterEmployerReasons = "",

                    };
                    _unitOfWork.CrMasSupRenterEmployer.Add(masSupRenterEmployer);
                    //_unitOfWork.Complete();
                }
            }
            return EmployerCode;
        }
        private string GetRegion(string RegionAr, string RegionEn)
        {
            var RegionCode = "";
            if (string.IsNullOrEmpty(RegionAr) && string.IsNullOrEmpty(RegionEn)) RegionCode = "11"; // Not Avaliable
            else
            {
                var region = _unitOfWork.CrMasSupPostRegion.Find(x => x.CrMasSupPostRegionsArName == RegionAr.Trim() || x.CrMasSupPostRegionsEnName == RegionEn.Trim());
                if (region != null) RegionCode = region.CrMasSupPostRegionsCode;
                else // If == Null should be enter the new value in PostCity table ;
                {
                    var lastRecordInRegion = _unitOfWork.CrMasSupPostRegion.GetAll().LastOrDefault();
                    if (lastRecordInRegion == null) RegionCode = "12";
                    else
                    {
                        int intValue = int.Parse(lastRecordInRegion.CrMasSupPostRegionsCode);
                        int newIntValue = intValue + 1;
                        RegionCode = newIntValue.ToString();
                    }
                    CrMasSupPostRegion crMasSupPostRegion = new CrMasSupPostRegion
                    {
                        CrMasSupPostRegionsCode = RegionCode,
                        CrMasSupPostRegionsArName = RegionAr.Trim(),
                        CrMasSupPostRegionsEnName = RegionEn.Trim(),
                        CrMasSupPostRegionsLatitude = 0,
                        CrMasSupPostRegionsLongitude = 0,
                        CrMasSupPostRegionsStatus = Status.Active,
                        CrMasSupPostRegionsLocation = "",
                        CrMasSupPostRegionsReasons = ""
                    };
                    _unitOfWork.CrMasSupPostRegion.Add(crMasSupPostRegion);
                    //_unitOfWork.Complete();
                }
            }
            return RegionCode;
        }
        private string GetCity(string CityAr, string CityEn, string regionCode)
        {
            var CityCode = "";
            if (string.IsNullOrEmpty(CityAr) && string.IsNullOrEmpty(CityEn)) CityCode = "1700000002"; // Not Avaliable
            else
            {
                var city = _unitOfWork.CrMasSupPostCity.Find(x => x.CrMasSupPostCityArName == CityAr.Trim() || x.CrMasSupPostCityArName == CityEn.Trim());
                var region = _unitOfWork.CrMasSupPostRegion.Find(x => x.CrMasSupPostRegionsCode == regionCode);
                if (city != null) CityCode = city.CrMasSupPostCityCode;
                else // If == Null should be enter the new value in PostCity table ;
                {
                    var lastRecordInCity = _unitOfWork.CrMasSupPostCity.GetAll().LastOrDefault();
                    if (lastRecordInCity == null) CityCode = "1700000003";
                    else
                    {
                        int intValue = int.Parse(lastRecordInCity.CrMasSupPostCityCode);
                        int newIntValue = intValue + 1;
                        CityCode = newIntValue.ToString();
                    }
                    CrMasSupPostCity crMasSupPostCity = new CrMasSupPostCity
                    {
                        CrMasSupPostCityCode = CityCode,
                        CrMasSupPostCityArName = CityAr.Trim(),
                        CrMasSupPostCityEnName = CityEn.Trim(),
                        CrMasSupPostCityRegionsCode = regionCode,
                        CrMasSupPostCityGroupCode = "17",
                        CrMasSupPostCityLongitude = null,
                        CrMasSupPostCityLatitude = null,
                        CrMasSupPostCityRegionsStatus = null,
                        CrMasSupPostCityLocation = "",
                        CrMasSupPostCityCounter = 0,
                        CrMasSupPostCityConcatenateArName = region.CrMasSupPostRegionsArName + "-" + CityAr.Trim(),
                        CrMasSupPostCityConcatenateEnName = region.CrMasSupPostRegionsEnName + "-" + CityEn.Trim(),
                        CrMasSupPostCityStatus = Status.Active,
                        CrMasSupPostCityReasons = ""
                    };
                    _unitOfWork.CrMasSupPostCity.Add(crMasSupPostCity);
                    //_unitOfWork.Complete();
                }
            }
            return CityCode;
        }
        private string GetAge(string date)
        {
            DateTime currentDate = DateTime.Now;
            DateTime birthDate = DateTime.Parse(date);
            int age = currentDate.Year - birthDate.Year;
            if (birthDate > currentDate.AddYears(-age)) age--;

            return age switch
            {
                < 20 => "1",
                >= 20 and <= 25 => "2",
                >= 26 and <= 30 => "3",
                >= 31 and <= 35 => "4",
                >= 36 and <= 41 => "5",
                >= 42 and <= 50 => "6",
                >= 51 and <= 60 => "7",
                > 60 => "8"
            };
        }
        private string GetTypeID(string firstChar, string sector)
        {
            string typeID = "";
            if (firstChar == "1") typeID = "1";
            else if (firstChar == "2") typeID = "2";
            else
            {
                if (sector == "2") typeID = "5";
                else typeID = "6";
            }
            return typeID;
        }
        private CrCasAccountReceipt GetContractAccountReceipt(string LessorCode, string BranchCode)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode)
                                                             .Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));

            CrCasAccountReceipt c = new CrCasAccountReceipt();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasAccountReceiptNo = val.ToString("000000");
            }
            else
            {
                c.CrCasAccountReceiptNo = "000001";
            }

            return c;
        }

        public async Task<bool> UpdateBranchValidity(string BranchCode, string LessorCode, string UserId, string PaymentMethod, decimal AmountPaid)
        {
            var UserValidity = await _unitOfWork.CrMasUserBranchValidity.FindAsync(x => x.CrMasUserBranchValidityId == UserId && x.CrMasUserBranchValidityBranch == BranchCode && x.CrMasUserBranchValidityLessor == LessorCode);
            if (PaymentMethod == "10")
            {
                if (UserValidity.CrMasUserBranchValidityBranchCashAvailable != null) UserValidity.CrMasUserBranchValidityBranchCashAvailable += AmountPaid;
                else UserValidity.CrMasUserBranchValidityBranchCashAvailable = AmountPaid;
                if (UserValidity.CrMasUserBranchValidityBranchCashBalance != null) UserValidity.CrMasUserBranchValidityBranchCashBalance += AmountPaid;
                else UserValidity.CrMasUserBranchValidityBranchCashBalance = AmountPaid;
            }
            else if (PaymentMethod != "40" && PaymentMethod != "10")
            {
                if (UserValidity.CrMasUserBranchValidityBranchSalesPointBalance != null) UserValidity.CrMasUserBranchValidityBranchSalesPointAvailable += AmountPaid;
                else UserValidity.CrMasUserBranchValidityBranchSalesPointAvailable = AmountPaid;
                if (UserValidity.CrMasUserBranchValidityBranchSalesPointBalance != null) UserValidity.CrMasUserBranchValidityBranchSalesPointBalance += AmountPaid;
                else UserValidity.CrMasUserBranchValidityBranchSalesPointBalance = AmountPaid;
            }
            if (_unitOfWork.CrMasUserBranchValidity.Update(UserValidity) != null) return true;
            return false;
        }

        public async Task<bool> UpdateUserBalance(string BranchCode, string LessorCode, string UserId, string PaymentMethod, decimal AmountPaid)
        {
            var UserInformation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == UserId && x.CrMasUserInformationLessor == LessorCode);
            if (UserInformation != null)
            {
                if (UserInformation.CrMasUserInformationAvailableBalance != null) UserInformation.CrMasUserInformationAvailableBalance += AmountPaid;
                else UserInformation.CrMasUserInformationAvailableBalance = AmountPaid;
                if (UserInformation.CrMasUserInformationTotalBalance != null) UserInformation.CrMasUserInformationTotalBalance += AmountPaid;
                else UserInformation.CrMasUserInformationTotalBalance = AmountPaid;
                if (_unitOfWork.CrMasUserInformation.Update(UserInformation) != null) return true;
            }
            return false;
        }

        public async Task<bool> AddRenterAlert(string ContractNo, string LessorCode, string BranchCode, int RentalDays, DateTime ContractEndDate, string SerialNo, string PriceNo)
        {
            CrCasRenterContractAlert crCasRenterContractAlert = new CrCasRenterContractAlert();
            var carPrice = await _unitOfWork.CrCasPriceCarBasic.FindAsync(x => x.CrCasPriceCarBasicNo == PriceNo);

            crCasRenterContractAlert.CrCasRenterContractAlertNo = ContractNo;
            crCasRenterContractAlert.CrCasRenterContractAlertLessor = LessorCode;
            crCasRenterContractAlert.CrCasRenterContractAlertBranch = BranchCode;
            crCasRenterContractAlert.CrCasRenterContractAlertDays = RentalDays;
            crCasRenterContractAlert.CrCasRenterContractAlertHour = 4;
            crCasRenterContractAlert.CrCasRenterContractAlertDayDate = ContractEndDate.AddDays(-1);
            crCasRenterContractAlert.CrCasRenterContractAlertHourDate = ContractEndDate.AddHours(-4);
            crCasRenterContractAlert.CrCasRenterContractAlertEndDate = ContractEndDate;
            crCasRenterContractAlert.CrCasRenterContractAlertStatus = "0";
            if (RentalDays == 1) crCasRenterContractAlert.CrCasRenterContractAlertContractActiviteStatus = "1";
            else if (RentalDays >= 2) crCasRenterContractAlert.CrCasRenterContractAlertContractActiviteStatus = "0";
            crCasRenterContractAlert.CrCasRenterContractAlertContractStatus = Status.Active;
            if (await _unitOfWork.CrCasRenterContractAlert.AddAsync(crCasRenterContractAlert) != null) return true;
            return false;
        }

        public async Task<bool> AddRenterStatistics(CrCasRenterContractBasic Contract)
        {
            CrCasRenterContractStatistic Statistic = new CrCasRenterContractStatistic();
            if (Contract != null)
            {
                Statistic.CrCasRenterContractStatisticsNo = Contract.CrCasRenterContractBasicNo;
                Statistic.CrCasRenterContractStatisticsLessor = Contract.CrCasRenterContractBasicLessor;
                Statistic.CrCasRenterContractStatisticsBranch = Contract.CrCasRenterContractBasicBranch;
                Statistic.CrCasRenterContractStatisticsDate = Contract.CrCasRenterContractBasicIssuedDate;
                //Renter
                Statistic.CrCasRenterContractStatisticsRenter = Contract.CrCasRenterContractBasicRenterId;
                Statistic.CrCasRenterContractStatisticsCarSerialNo = Contract.CrCasRenterContractBasicCarSerailNo;
                Statistic.CrCasRenterContractStatisticsUserOpen = Contract.CrCasRenterContractBasicUserInsert;
                // Post Renter And Branch
                var branch = await _unitOfWork.CrCasBranchPost.FindAsync(x => x.CrCasBranchPostLessor == Contract.CrCasRenterContractBasicLessor && x.CrCasBranchPostBranch == Contract.CrCasRenterContractBasicBranch);
                if (branch != null)
                {
                    Statistic.CrCasRenterContractStatisticsBranchRegions = branch.CrCasBranchPostRegions;
                    Statistic.CrCasRenterContractStatisticsBranchCity = branch.CrCasBranchPostCity;
                }
                var renter = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == Contract.CrCasRenterContractBasicRenterId && x.CrCasRenterLessorCode == Contract.CrCasRenterContractBasicLessor);
                if (renter != null)
                {
                    Statistic.CrCasRenterContractStatisticsRenterRegions = renter.CrCasRenterLessorStatisticsRegions;
                    Statistic.CrCasRenterContractStatisticsRenterCity = renter.CrCasRenterLessorStatisticsCity;
                    Statistic.CrCasRenterContractStatisticsNationalities = renter.CrCasRenterLessorStatisticsNationalities;
                    Statistic.CrCasRenterContractStatisticsGender = renter.CrCasRenterLessorStatisticsGender;
                    Statistic.CrCasRenterContractStatisticsJobs = renter.CrCasRenterLessorStatisticsJobs;
                    Statistic.CrCasRenterContractStatisticsMembership = renter.CrCasRenterLessorMembership;
                }
                //Car 
                var car = await _unitOfWork.CrCasCarInformation.FindAsync(x => x.CrCasCarInformationSerailNo == Contract.CrCasRenterContractBasicCarSerailNo && x.CrCasCarInformationLessor == Contract.CrCasRenterContractBasicLessor);
                if (car != null)
                {
                    Statistic.CrCasRenterContractStatisticsBrand = car.CrCasCarInformationBrand;
                    Statistic.CrCasRenterContractStatisticsModel = car.CrCasCarInformationModel;
                    Statistic.CrCasRenterContractStatisticsCategory = car.CrCasCarInformationCategory;
                    Statistic.CrCasRenterContractStatisticsCarYear = car.CrCasCarInformationYear;
                }
                //Category
                // H Month 
                Statistic.CrCasRenterContractStatisticsGmonthCreate = Contract.CrCasRenterContractBasicIssuedDate?.Month.ToString();
                Statistic.CrCasRenterContractStatisticsHmonthCreate = GetGMonth((DateTime)(Contract.CrCasRenterContractBasicIssuedDate));
                Statistic.CrCasRenterContractStatisticsDayCreate = GetDay((DateTime)(Contract.CrCasRenterContractBasicIssuedDate));
                Statistic.CrCasRenterContractStatisticsTimeCreate = GetTimeCategory((DateTime)(Contract.CrCasRenterContractBasicIssuedDate));
                Statistic.CrCasRenterContractStatisticsDayCount = GetCountDaysCategory((int)Contract.CrCasRenterContractBasicExpectedRentalDays);
                // Renter Mas
                var masRenter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == Contract.CrCasRenterContractBasicRenterId);
                if (masRenter != null)
                {
                    Statistic.CrCasRenterContractStatisticsAgeNo = GetAgeCategory((DateTime)masRenter.CrMasRenterInformationBirthDate);
                }
                //Contract Value Category
                Statistic.CrCasRenterContractStatisticsValueNo = GetContractValueCategory((decimal)Contract.CrCasRenterContractBasicExpectedTotal);
                // Data From Contract 
                Statistic.CrCasRenterContractStatisicsDays = Contract.CrCasRenterContractBasicExpectedRentalDays;
                Statistic.CrCasRenterContractStatisticsRentValue = Contract.CrCasRenterContractBasicExpectedRentValue;
                Statistic.CrCasRenterContractStatisticsAdditionsValue = Contract.CrCasRenterContractBasicAdditionalValue;
                Statistic.CrCasRenterContractStatisticsOptionsValue = Contract.CrCasRenterContractBasicExpectedOptionsValue;
                //Statistic.CrCasRenterContractStatisticsAuthorizationValue = Contract.CrCasRenterContractBasicAuthorizationValue;
                //Statistic.CrCasRenterContractStatisticsAdditionsKmValue = Contract.CrCasRenterContractBasicKmValue;
                //Statistic.CrCasRenterContractStatisticsAdditionsHourValue = Contract.CrCasRenterContractBasicHourValue;
                //Statistic.CrCasRenterContractStatisticsContractValue = Contract.CrCasRenterContractBasicExpectedValueBeforDiscount;
                //Statistic.CrCasRenterContractStatisticsDiscountValue = Contract.CrCasRenterContractBasicExpectedDiscountValue;
                //Statistic.CrCasRenterContractStatisticsContractAfterValue = Contract.CrCasRenterContractBasicExpectedValueAfterDiscount;
                //Statistic.CrCasRenterContractStatisticsTaxValue = Contract.CrCasRenterContractBasicExpectedTaxValue;
                Statistic.CrCasRenterContractStatisticsValueNo = GetContractValueCategory((decimal)Contract.CrCasRenterContractBasicExpectedRentValue);
                if (await _unitOfWork.CrCasRenterContractStatistic.AddAsync(Statistic) != null) return true;
                return false;
            }
            return false;
        }

        public string GetGMonth(DateTime date)
        {
            // Create UmAlQuraCalendar instance
            UmAlQuraCalendar umAlQuraCalendar = new UmAlQuraCalendar();

            // Convert Gregorian date to Hijri date
            int hijriMonth = umAlQuraCalendar.GetMonth(date);

            return hijriMonth.ToString();
        }
        public string GetDay(DateTime date)
        {
            // Get the day of the week
            DayOfWeek dayOfWeek = date.DayOfWeek;
            // Adjust the day of the week to start from 1 for Saturday and end at 7 for Friday
            int adjustedDayOfWeek = ((int)dayOfWeek + 2) % 7;
            // If the day is Sunday (0), it should be represented as 7
            if (adjustedDayOfWeek == 0)
            {
                adjustedDayOfWeek = 7;
            }
            return adjustedDayOfWeek.ToString();
        }
        public string GetTimeCategory(DateTime date)
        {
            int hour = date.Hour;

            if (hour >= 0 && hour <= 2)
            {
                return "1"; // من 00:00 إلى 02:59
            }
            else if (hour >= 3 && hour <= 5)
            {
                return "2"; // من 03:00 إلى 05:59
            }
            else if (hour >= 6 && hour <= 8)
            {
                return "3"; // من 06:00 إلى 08:59
            }
            else if (hour >= 9 && hour <= 11)
            {
                return "4"; // من 09:00 إلى 11:59
            }
            else if (hour >= 12 && hour <= 14)
            {
                return "5"; // من 12:00 إلى 14:59
            }
            else if (hour >= 15 && hour <= 17)
            {
                return "6"; // من 15:00 إلى 17:59
            }
            else if (hour >= 18 && hour <= 20)
            {
                return "7"; // من 18:00 إلى 20:59
            }
            else if (hour >= 21 && hour <= 23)
            {
                return "8"; // من 21:00 إلى 23:59
            }
            else
            {
                return ""; // Handle the case where hour is out of range
            }
        }
        public string GetCountDaysCategory(int daysNo)
        {
            if (daysNo >= 1 && daysNo <= 3)
            {
                return "1"; // من 1 إلى 3
            }
            else if (daysNo >= 4 && daysNo <= 7)
            {
                return "2"; // من 4 إلى 7
            }
            else if (daysNo >= 8 && daysNo <= 10)
            {
                return "3"; // من 8 إلى 10
            }
            else if (daysNo >= 11 && daysNo <= 15)
            {
                return "4"; // من 11 إلى 15
            }
            else if (daysNo >= 16 && daysNo <= 20)
            {
                return "5"; // من 16 إلى 20
            }
            else if (daysNo >= 21 && daysNo <= 25)
            {
                return "6"; // من 21 إلى 25
            }
            else if (daysNo >= 26 && daysNo <= 30)
            {
                return "7"; // من 26 إلى 30
            }
            else if (daysNo > 30)
            {
                return "8"; // أكثر من 30
            }
            else
            {
                return "0";
            }
        }
        public string GetAgeCategory(DateTime birthDate)
        {
            // Calculate the age
            int age = DateTime.Today.Year - birthDate.Year;

            // Adjust the age if the birthday hasn't occurred yet this year
            if (birthDate > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            // Categorize the age
            if (age < 20)
            {
                return "1"; // أقل من 20
            }
            else if (age >= 20 && age <= 25)
            {
                return "2"; // العمر من 21 - 25
            }
            else if (age >= 26 && age <= 30)
            {
                return "3"; // العمر من 26 - 30
            }
            else if (age >= 31 && age <= 35)
            {
                return "4"; // العمر من 31 - 35
            }
            else if (age >= 36 && age <= 41)
            {
                return "5"; // العمر من 36 - 41
            }
            else if (age >= 42 && age <= 50)
            {
                return "6"; // العمر من 41 - 50
            }
            else if (age >= 51 && age <= 60)
            {
                return "7"; // العمر من 51 - 60
            }
            else
            {
                return "8"; // العمر أكثر من 60
            }
        }
        public string GetContractValueCategory(decimal value)
        {
            if (value < 300)
            {
                return "1";
            }
            else if (value > 300 && value <= 500)
            {
                return "2";
            }
            else if (value > 500 && value <= 1000)
            {
                return "3";
            }
            else if (value > 1000 && value <= 1500)
            {
                return "4";
            }
            else if (value > 1500 && value <= 2000)
            {
                return "5";
            }
            else if (value > 2000 && value <= 2500)
            {
                return "6";
            }
            else if (value > 2500 && value <= 3000)
            {
                return "7";
            }
            else if (value > 3000 && value <= 3500)
            {
                return "8";
            }
            else
            {
                return "9"; // العمر أكثر من 60
            }
        }


        public async Task<bool> AddAccountInvoice(string ContractNo, string RenterId, string sector, string LessorCode, string BranchCode, string UserId, string AccountReceiptNo, string pdfPath)
        {
            CrCasAccountInvoice crCasAccountInvoice = new CrCasAccountInvoice();
            var Renter = await _unitOfWork.CrCasRenterLessor.FindAsync(x => x.CrCasRenterLessorId == RenterId && x.CrCasRenterLessorCode == LessorCode);

            //Get ContractCode
            DateTime now = DateTime.Now;
            var y = now.ToString("yy");
            var autoinc = GetAccountInvoice(LessorCode, BranchCode).CrCasAccountInvoiceNo;
            var AccountInvoiceNo = y + "-" + sector + "308" + "-" + LessorCode + BranchCode + "-" + autoinc;

            crCasAccountInvoice.CrCasAccountInvoiceNo = AccountInvoiceNo;
            crCasAccountInvoice.CrCasAccountInvoiceYear = y;
            crCasAccountInvoice.CrCasAccountInvoiceLessorCode = LessorCode;
            crCasAccountInvoice.CrCasAccountInvoiceBranchCode = BranchCode;
            crCasAccountInvoice.CrCasAccountInvoiceDate = DateTime.Now;
            crCasAccountInvoice.CrCasAccountInvoiceType = "308"; // Create Contract
            crCasAccountInvoice.CrCasAccountInvoiceReferenceContract = ContractNo;
            crCasAccountInvoice.CrCasAccountInvoiceReferenceReceipt = AccountReceiptNo;
            crCasAccountInvoice.CrCasAccountInvoiceUserCode = UserId;
            crCasAccountInvoice.CrCasAccountInvoicePdfFile = pdfPath;

            if (await _unitOfWork.CrCasAccountInvoice.AddAsync(crCasAccountInvoice) != null) return true;
            return false;
        }

        private CrCasAccountInvoice GetAccountInvoice(string LessorCode, string BranchCode)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountInvoice.FindAll(x => x.CrCasAccountInvoiceLessorCode == LessorCode &&
                                                                       x.CrCasAccountInvoiceYear == y && x.CrCasAccountInvoiceBranchCode == BranchCode)
                                                             .Max(x => x.CrCasAccountInvoiceNo.Substring(x.CrCasAccountInvoiceNo.Length - 6, 6));

            CrCasAccountInvoice c = new CrCasAccountInvoice();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasAccountInvoiceNo = val.ToString("000000");
            }
            else
            {
                c.CrCasAccountInvoiceNo = "000001";
            }

            return c;
        }

        public async Task<bool> UpdateRenterSignture(string RenterId, string ImagePath)
        {
            var renter = await _unitOfWork.CrMasRenterInformation.FindAsync(x => x.CrMasRenterInformationId == RenterId);
            if (renter != null)
            {
                renter.CrMasRenterInformationSignature = ImagePath;
                if (_unitOfWork.CrMasRenterInformation.Update(renter) != null) return true;
            }
            return false;
        }

        public async Task<bool> AddRenterEvaluation(string ContractNo, string LessorCode, string RenterId, string TypeEvaluation, string UserId)
        {
            CrCasRenterContractEvaluation evaluation = new CrCasRenterContractEvaluation();
            evaluation.CrCasRenterContractEvaluationContract = ContractNo;
            evaluation.CrCasRenterContractEvaluationType = TypeEvaluation;
            evaluation.CrCasRenterContractEvaluationLessor = LessorCode;
            evaluation.CrCasRenterContractEvaluationRenter = RenterId;
            evaluation.CrCasRenterContractEvaluationUser = UserId;
            evaluation.CrCasRenterContractEvaluationDate = DateTime.Now;
            if (await _unitOfWork.CrCasRenterContractEvaluation.AddAsync(evaluation) != null) return true;
            return false;
        }
        private DateTime GetBirthDateMiladiOrHijri(string day, string month, string year, string renterId)
        {
            DateTime birthDate;

            // Check if renterId starts with '1' (indicating Hijri)
            if (renterId.StartsWith("1"))
            {
                // Convert Hijri date to Gregorian date
                HijriCalendar hijriCalendar = new HijriCalendar();
                int dayInt = int.Parse(day);
                int monthInt = int.Parse(month);
                int yearInt = int.Parse(year);

                // Get the birth date from the Hijri date
                birthDate = hijriCalendar.ToDateTime(yearInt, monthInt, dayInt, 0, 0, 0, 0);
            }
            else
            {
                // Treat as Gregorian date
                birthDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
            }

            return birthDate; // Return the DateTime object
        }
    }
}
