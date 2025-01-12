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
    public class CarsController : BaseController
    {
        public CarsController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
        }
        public async Task<IActionResult> Indicators()
        {
            ViewBag.Branches = false;
            ViewBag.Dashboard = false;
            ViewBag.Employees = false;
            ViewBag.Indicators = true;
            ViewBag.Cars = true;
            ViewBag.Contracts = false;
            ViewBag.Tenants = false;

            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            if (CultureInfo.CurrentUICulture.Name == "en-US") await ViewData.SetPageTitleAsync("Owners", "Indicators", "Cars", "", "", userLogin.CrMasUserInformationEnName);
            else await ViewData.SetPageTitleAsync("الملاك", "مؤشرات", "السيارات", "", "", userLogin.CrMasUserInformationArName);
            var ownersLayoutVM = await OwnersDashboadInfo(lessorCode);
            var Contracts = await _unitOfWork.CrCasRenterContractStatistic.FindAllAsNoTrackingAsync(x => x.CrCasRenterContractStatisticsLessor == lessorCode, new[] { "CrCasRenterContractStatisticsModelNavigation", "CrCasRenterContractStatisticsCategoryNavigation", "CrCasRenterContractStatisticsBrandNavigation" });
            ownersLayoutVM.ModelCarStaticitis = GetModelCarList(Contracts);
            ownersLayoutVM.CategoryCarStaticitis = GetCategoryCarList(Contracts);
            ownersLayoutVM.BrandCarStaticitis = GetBrandCarList(Contracts);
            ownersLayoutVM.YearCarStaticitis = GetYearCarList(Contracts);
            return View(ownersLayoutVM);
        }

        private List<OwnStatictsVM> GetModelCarList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsModel);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsModel == contract.CrCasRenterContractStatisticsModel);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = contract.CrCasRenterContractStatisticsModelNavigation.CrMasSupCarModelArConcatenateName;
                ownStatictsVM.EnName = contract.CrCasRenterContractStatisticsModelNavigation.CrMasSupCarModelConcatenateEnName;
                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsModelNavigation.CrMasSupCarModelCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count > 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetCategoryCarList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsCategory);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsCategory == contract.CrCasRenterContractStatisticsCategory);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = contract.CrCasRenterContractStatisticsCategoryNavigation.CrMasSupCarCategoryArName;
                ownStatictsVM.EnName = contract.CrCasRenterContractStatisticsCategoryNavigation.CrMasSupCarCategoryEnName;
                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsCategoryNavigation.CrMasSupCarCategoryCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count > 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetBrandCarList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsBrand);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsBrand == contract.CrCasRenterContractStatisticsBrand);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.ArName = contract.CrCasRenterContractStatisticsBrandNavigation.CrMasSupCarBrandArName;
                ownStatictsVM.EnName = contract.CrCasRenterContractStatisticsBrandNavigation.CrMasSupCarBrandEnName;
                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsBrandNavigation.CrMasSupCarBrandCode;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count > 0).Take(3).ToList();
        }
        private List<OwnStatictsVM> GetYearCarList(List<CrCasRenterContractStatistic> Contracts)
        {
            var ContractsStatics = Contracts.DistinctBy(x => x.CrCasRenterContractStatisticsCarYear);
            List<OwnStatictsVM> StaticsVMs = new List<OwnStatictsVM>();
            foreach (var contract in ContractsStatics)
            {
                var Count = Contracts.Count(x => x.CrCasRenterContractStatisticsCarYear == contract.CrCasRenterContractStatisticsCarYear);
                OwnStatictsVM ownStatictsVM = new OwnStatictsVM();
                ownStatictsVM.Code = contract.CrCasRenterContractStatisticsCarYear;
                ownStatictsVM.Count = Count;
                var Percent = (decimal)Count / Contracts.Count() * 100;
                ownStatictsVM.Percent = Math.Round(Percent, 2);
                StaticsVMs.Add(ownStatictsVM);
            }
            return StaticsVMs.GroupBy(x => x.Code).Select(g => g.First()).OrderByDescending(x => x.Count).Where(x => x.Count > 0).Take(3).ToList();
        }

    }
}
