using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.Owners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Bnan.Ui.Areas.Owners.Controllers
{
    [Area("Owners")]
    [Authorize(Roles = "OWN")]
    public class RentersController : BaseController
    {
        public RentersController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
        }
        public async Task<IActionResult> Indicators()
        {
            ViewBag.Branches = false;
            ViewBag.Dashboard = false;
            ViewBag.Employees = false;
            ViewBag.Indicators = true;
            ViewBag.Cars = false;
            ViewBag.Contracts = false;
            ViewBag.Tenants = true;

            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Indicators", "Renters", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "المؤشرات", "المستأجرين", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);
            var Renters = await _unitOfWork.CrCasRenterLessor.FindAllAsNoTrackingAsync(x => x.CrCasRenterLessorCode == x.CrCasRenterLessorCode, new[] { "CrCasRenterLessorStatisticsNationalitiesNavigation", "CrCasRenterLessorStatisticsJobsNavigation", "CrCasRenterLessorMembershipNavigation", "CrCasRenterLessorStatisticsCityNavigation" });
            var Contracts = await _unitOfWork.CrCasRenterContractStatistic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractStatisticsLessor == lessorCode, new[] { "CrCasRenterContractStatisticsNavigation.CrCasRenterLessorStatisticsNationalitiesNavigation",
                                                                                                                                               "CrCasRenterContractStatisticsMembershipNavigation",
                                                                                                                                               "CrCasRenterContractStatisticsNavigation.CrCasRenterLessorStatisticsJobsNavigation",
                                                                                                                                               "CrCasRenterContractStatisticsNavigation.CrCasRenterLessorStatisticsCityNavigation",
                                                                                                                                               "CrCasRenterContractStatisticsBranchCityNavigation"});
            ownersLayoutVM.NationalityStaticitis = GetSNationalityList(Contracts, lessorCode);
            ownersLayoutVM.MembershipStaticitis = GetMemberShipList(Contracts, lessorCode);
            ownersLayoutVM.ProfessionStaticitis = GetProffesionsList(Contracts, lessorCode);
            ownersLayoutVM.BranchCityStaticitis = GetCityBranchList(Contracts, lessorCode);
            ownersLayoutVM.CityStaticitis = GetCityRentersList(Contracts, lessorCode);
            ownersLayoutVM.AgeStaticitis = GetAgeList(Contracts, lessorCode);
            return View(ownersLayoutVM);
        }
        private List<OwnStatictsVM> GetSNationalityList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.Where(x => x.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsNationalities).ToList();
            List<OwnStatictsVM> NationlitystatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsNationalities == renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesCode);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesArName;
                ownStatictsVM.EnName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesEnName;
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsNationalitiesNavigation?.CrMasSupRenterNationalitiesCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                NationlitystatictsVMs.Add(ownStatictsVM);
            }
            return NationlitystatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetMemberShipList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.Where(x => x.CrCasRenterContractStatisticsMembershipNavigation?.CrMasSupRenterMembershipStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsMembership).ToList();
            List<OwnStatictsVM> MemperShipstatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsMembership == renterContract.CrCasRenterContractStatisticsMembershipNavigation?.CrMasSupRenterMembershipCode);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = renterContract.CrCasRenterContractStatisticsMembershipNavigation?.CrMasSupRenterMembershipArName;
                ownStatictsVM.EnName = renterContract.CrCasRenterContractStatisticsMembershipNavigation?.CrMasSupRenterMembershipEnName;
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsMembershipNavigation?.CrMasSupRenterMembershipCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                MemperShipstatictsVMs.Add(ownStatictsVM);
            }
            return MemperShipstatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetProffesionsList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.Where(x => x.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsJobsNavigation?.CrMasSupRenterProfessionsStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsJobs).ToList();
            List<OwnStatictsVM> ProfessionstatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsJobs == renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsJobsNavigation?.CrMasSupRenterProfessionsCode);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsJobsNavigation?.CrMasSupRenterProfessionsArName;
                ownStatictsVM.EnName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsJobsNavigation?.CrMasSupRenterProfessionsEnName;
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsJobsNavigation?.CrMasSupRenterProfessionsCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);

                ProfessionstatictsVMs.Add(ownStatictsVM);
            }
            return ProfessionstatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetCityBranchList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.Where(x => x.CrCasRenterContractStatisticsBranchCityNavigation?.CrMasSupPostCityStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsBranchCity).ToList();
            List<OwnStatictsVM> CitystatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsBranchCity == renterContract.CrCasRenterContractStatisticsBranchCityNavigation?.CrMasSupPostCityCode);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = renterContract.CrCasRenterContractStatisticsBranchCityNavigation?.CrMasSupPostCityArName;
                ownStatictsVM.EnName = renterContract.CrCasRenterContractStatisticsBranchCityNavigation?.CrMasSupPostCityEnName;
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsBranchCityNavigation?.CrMasSupPostCityCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                CitystatictsVMs.Add(ownStatictsVM);
            }
            return CitystatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetCityRentersList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.Where(x => x.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsCityNavigation?.CrMasSupPostCityStatus != Status.Deleted).DistinctBy(x => x.CrCasRenterContractStatisticsRenterCity).ToList();
            List<OwnStatictsVM> CitystatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsRenterCity == renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsCityNavigation?.CrMasSupPostCityCode);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsCityNavigation?.CrMasSupPostCityArName;
                ownStatictsVM.EnName = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsCityNavigation?.CrMasSupPostCityEnName;
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsNavigation?.CrCasRenterLessorStatisticsCityNavigation?.CrMasSupPostCityCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                CitystatictsVMs.Add(ownStatictsVM);
            }
            return CitystatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetAgeList(List<CrCasRenterContractStatistic> Contracts, string lessorCode)
        {
            var RenterContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsAgeNo).ToList();
            List<OwnStatictsVM> AgeStatictsVMs = new List<OwnStatictsVM>();
            foreach (var renterContract in RenterContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsAgeNo == renterContract.CrCasRenterContractStatisticsAgeNo);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (renterContract.CrCasRenterContractStatisticsAgeNo)
                {
                    case "1":
                        ownStatictsVM.ArName = "أقل من 20";
                        ownStatictsVM.EnName = "Less Than 20";
                        break;
                    case "2":
                        ownStatictsVM.ArName = " 20 - 25";
                        ownStatictsVM.EnName = " 20 - 25";
                        break;
                    case "3":
                        ownStatictsVM.ArName = " 26 - 30";
                        ownStatictsVM.EnName = " 26 - 30";
                        break;
                    case "4":
                        ownStatictsVM.ArName = " 31 - 35";
                        ownStatictsVM.EnName = " 31 - 35";
                        break;
                    case "5":
                        ownStatictsVM.ArName = " 36 - 41";
                        ownStatictsVM.EnName = " 36 - 41";
                        break;
                    case "6":
                        ownStatictsVM.ArName = " 42 - 50";
                        ownStatictsVM.EnName = " 42 - 50";
                        break;
                    case "7":
                        ownStatictsVM.ArName = " 51 - 60";
                        ownStatictsVM.EnName = " 51 - 60";
                        break;
                    case "8":
                        ownStatictsVM.ArName = "أكثر من 60";
                        ownStatictsVM.EnName = "More Than 60";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخرى";
                        ownStatictsVM.EnName = "Others";
                        break;
                }
                ownStatictsVM.Code = renterContract.CrCasRenterContractStatisticsAgeNo;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                AgeStatictsVMs.Add(ownStatictsVM);
            }
            return AgeStatictsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
    }
}
