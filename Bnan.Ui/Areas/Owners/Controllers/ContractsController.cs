using AutoMapper;
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
    public class ContractsController : BaseController
    {
        public ContractsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
        }
        public async Task<IActionResult> Indicators()
        {
            ViewBag.Branches = false;
            ViewBag.Dashboard = false;
            ViewBag.Employees = false;
            ViewBag.Indicators = true;
            ViewBag.Cars = false;
            ViewBag.Contracts = true;
            ViewBag.Tenants = false;

            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Indicators", "Contracts", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "مؤشرات", "العقود", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);
            var Contracts = await _unitOfWork.CrCasRenterContractStatistic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractStatisticsLessor == lessorCode, new[] { "CrCasRenterContractStatistics" });

            ownersLayoutVM.ContractsCount = Contracts.Count;
            ownersLayoutVM.BranchStaticitis = GetBranchContractsList(Contracts);
            ownersLayoutVM.AmountValueStaticitis = GetAmountValueContractsList(Contracts);
            ownersLayoutVM.DaysCountStaticitis = GetDaysCountContractsList(Contracts);
            ownersLayoutVM.DaysStaticitis = GetDaysContractsList(Contracts);
            ownersLayoutVM.DistanceKMStaticitis = GetDistanceKMContractsList(Contracts);
            ownersLayoutVM.TimeStaticitis = GetTimeContractsList(Contracts);

            return View(ownersLayoutVM);
        }

        private List<OwnStatictsVM> GetBranchContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsBranch);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatistics?.CrCasBranchInformationCode == contract.CrCasRenterContractStatisticsBranch);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = contract.CrCasRenterContractStatistics?.CrCasBranchInformationArShortName;
                ownStatictsVM.EnName = contract.CrCasRenterContractStatistics?.CrCasBranchInformationEnShortName;
                ownStatictsVM.Code = contract.CrCasRenterContractStatistics?.CrCasBranchInformationCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetAmountValueContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsValueNo);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsValueNo == contract.CrCasRenterContractStatisticsValueNo);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (contract.CrCasRenterContractStatisticsValueNo)
                {
                    case "1":
                        ownStatictsVM.ArName = "أقل من 300";
                        ownStatictsVM.EnName = "Less Than 300";
                        break;
                    case "2":
                        ownStatictsVM.ArName = "301 - 500";
                        ownStatictsVM.EnName = "301 - 500";
                        break;
                    case "3":
                        ownStatictsVM.ArName = "501 - 1000";
                        ownStatictsVM.EnName = "501 - 1000";
                        break;
                    case "4":
                        ownStatictsVM.ArName = "1001 - 1500";
                        ownStatictsVM.EnName = "1001 - 1500";
                        break;
                    case "5":
                        ownStatictsVM.ArName = "1501 - 2000";
                        ownStatictsVM.EnName = "1501 - 2000";
                        break;
                    case "6":
                        ownStatictsVM.ArName = "2001 - 3000";
                        ownStatictsVM.EnName = "2001 - 3000";
                        break;
                    case "7":
                        ownStatictsVM.ArName = "3001 - 4000";
                        ownStatictsVM.EnName = "3001 - 4000";
                        break;
                    case "8":
                        ownStatictsVM.ArName = "4001 - 5000";
                        ownStatictsVM.EnName = "4001 - 5000";
                        break;
                    case "9":
                        ownStatictsVM.ArName = "أكثر من 5000";
                        ownStatictsVM.EnName = "More Than 5000";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخري";
                        ownStatictsVM.EnName = "Others";
                        break;
                }

                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsValueNo;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetDaysContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsDayCreate);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsDayCreate == contract.CrCasRenterContractStatisticsDayCreate);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (contract.CrCasRenterContractStatisticsDayCreate)
                {
                    case "1":
                        ownStatictsVM.ArName = "السبت";
                        ownStatictsVM.EnName = "Saturday";
                        break;
                    case "2":
                        ownStatictsVM.ArName = "الأحد";
                        ownStatictsVM.EnName = "Sunday";
                        break;
                    case "3":
                        ownStatictsVM.ArName = "الإثنين";
                        ownStatictsVM.EnName = "Monday";
                        break;
                    case "4":
                        ownStatictsVM.ArName = "الثلاثاء";
                        ownStatictsVM.EnName = "Tuesday";
                        break;
                    case "5":
                        ownStatictsVM.ArName = "الأربعاء";
                        ownStatictsVM.EnName = "Wednesday";
                        break;
                    case "6":
                        ownStatictsVM.ArName = "الخميس";
                        ownStatictsVM.EnName = "Thursday";
                        break;
                    case "7":
                        ownStatictsVM.ArName = "الجمعة";
                        ownStatictsVM.EnName = "Friday";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخري";
                        ownStatictsVM.EnName = "Others";
                        break;
                }

                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsDayCreate;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetTimeContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsTimeCreate);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsTimeCreate == contract.CrCasRenterContractStatisticsTimeCreate);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (contract.CrCasRenterContractStatisticsTimeCreate)
                {
                    case "1":
                        ownStatictsVM.ArName = "00:00 - 02:59";
                        ownStatictsVM.EnName = "00:00 - 02:59";
                        break;
                    case "2":
                        ownStatictsVM.ArName = "03:00 - 05:59";
                        ownStatictsVM.EnName = "03:00 - 05:59";
                        break;
                    case "3":
                        ownStatictsVM.ArName = "06:00 - 08:59";
                        ownStatictsVM.EnName = "06:00 - 08:59";
                        break;
                    case "4":
                        ownStatictsVM.ArName = "09:00 - 11:59";
                        ownStatictsVM.EnName = "09:00 - 11:59";
                        break;
                    case "5":
                        ownStatictsVM.ArName = "12:00 - 14:59";
                        ownStatictsVM.EnName = "12:00 - 14:59";
                        break;
                    case "6":
                        ownStatictsVM.ArName = "15:00 - 17:59";
                        ownStatictsVM.EnName = "15:00 - 17:59";
                        break;
                    case "7":
                        ownStatictsVM.ArName = "18:00 - 20:59";
                        ownStatictsVM.EnName = "18:00 - 20:59";
                        break;
                    case "8":
                        ownStatictsVM.ArName = "21:00 - 23:59";
                        ownStatictsVM.EnName = "21:00 - 23:59";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخري";
                        ownStatictsVM.EnName = "Others";
                        break;
                }

                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsTimeCreate;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetDaysCountContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsDayCount);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsDayCount == contract.CrCasRenterContractStatisticsDayCount);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (contract.CrCasRenterContractStatisticsDayCount)
                {
                    case "1":
                        ownStatictsVM.ArName = "1 - 3";
                        ownStatictsVM.EnName = "1 - 3";
                        break;
                    case "2":
                        ownStatictsVM.ArName = "4 - 7";
                        ownStatictsVM.EnName = "4 - 7";
                        break;
                    case "3":
                        ownStatictsVM.ArName = "8 - 10";
                        ownStatictsVM.EnName = "8 - 10";
                        break;
                    case "4":
                        ownStatictsVM.ArName = "11 - 15";
                        ownStatictsVM.EnName = "11 - 15";
                        break;
                    case "5":
                        ownStatictsVM.ArName = "16 - 20";
                        ownStatictsVM.EnName = "16 - 20";
                        break;
                    case "6":
                        ownStatictsVM.ArName = "21 - 25";
                        ownStatictsVM.EnName = "21 - 25";
                        break;
                    case "7":
                        ownStatictsVM.ArName = "26 - 30";
                        ownStatictsVM.EnName = "26 - 30";
                        break;
                    case "8":
                        ownStatictsVM.ArName = "30 أكثر من";
                        ownStatictsVM.EnName = "More than 30";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخرى";
                        ownStatictsVM.EnName = "Others";
                        break;
                }

                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsDayCount;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetDistanceKMContractsList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsKm);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsKm == contract.CrCasRenterContractStatisticsKm);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                switch (contract.CrCasRenterContractStatisticsKm)
                {
                    case "1":
                        ownStatictsVM.ArName = "أقل من 100";
                        ownStatictsVM.EnName = "Less Than 100";
                        break;
                    case "2":
                        ownStatictsVM.ArName = "101 - 200";
                        ownStatictsVM.EnName = "101 - 200";
                        break;
                    case "3":
                        ownStatictsVM.ArName = "201 - 300";
                        ownStatictsVM.EnName = "201 - 300";
                        break;
                    case "4":
                        ownStatictsVM.ArName = "301 - 400";
                        ownStatictsVM.EnName = "301 - 400";
                        break;
                    case "5":
                        ownStatictsVM.ArName = "400 أكثر من";
                        ownStatictsVM.EnName = "More Than 400";
                        break;
                    default:
                        ownStatictsVM.ArName = "اخرى";
                        ownStatictsVM.EnName = "Other";
                        break;
                }

                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsKm;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count != 0).Take(3).ToList();
        }

    }
}
