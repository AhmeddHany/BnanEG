using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Linq;
using System.Web.WebPages;

namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class CompanyDuesController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly UserManager<CrMasUserInformation> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService _userService;
        private readonly IFinancialTransactionOfRenter _FinancialTransactionOfRenter;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<CompanyDuesController> _localizer;


        public CompanyDuesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IFinancialTransactionOfRenter FinancialTransactionOfRenter,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<CompanyDuesController> localizer) : base(userManager, unitOfWork, mapper)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _userService = userService;
            _FinancialTransactionOfRenter = FinancialTransactionOfRenter;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "3";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101004", "1");
            ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("101", "1101004", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();
            var LastCompanyOwed = _unitOfWork.CrCasAccountContractCompanyOwed.GetAll().Max(x => x.CrCasAccountContractCompanyOwedDate);


            if (LastCompanyOwed == null)
            {
                LastCompanyOwed = DateTime.Today.Date;
            }

            var startDate = LastCompanyOwed?.AddDays(-30);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.EndDate = LastCompanyOwed?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var AllTaxOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x => x.CrCasAccountContractCompanyOwedCompanyCode == "4001" && x.CrCasAccountContractCompanyOwedDate <= LastCompanyOwed && x.CrCasAccountContractCompanyOwedDate >= startDate).DistinctBy(x=>x.CrCasAccountContractCompanyOwedContractCom?.FirstOrDefault()).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();

            //if (LastCompanyOwed == null || AllCompanies?.Count() < 1) 
            //{ 
            //    return RedirectToAction("FailedMessageReport_NoData");
            //}


            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

            CompanyDues_VM companyDues_VM = new CompanyDues_VM();
            companyDues_VM.CrMasLessorInformation = AllCompanies;
            companyDues_VM.CrCasAccountContractCompanyOwed = AllTaxOwed;
            //companyDues_VM.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
            //if(companyDues_VM.Company_ContractNo == null)
            //{
            //    ViewBag.Company_ContractNo = "-00";
            //}
            //else
            //{
            //    ViewBag.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
            //}

            //ViewBag.Company_ContractNo = "222222";


            return View(companyDues_VM);
        }





        [HttpGet]
        public async Task<PartialViewResult> GetAllContractsByDate_statusAsync(string _max, string _mini, string status, string Company, string Contract)
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "3";
            var (mainTask, subTask, system, currentUser) = await SetTrace("101", "1101004", "1");
            //ViewBag.CurrentLessor = currentUser.CrMasUserInformationLessor;

            var titles = await setTitle("101", "1101004", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            if (!string.IsNullOrEmpty(_max) && !string.IsNullOrEmpty(_mini) && _max.Length > 0 && Contract != null && Contract != "")
            {
                ViewBag.StartDate = DateTime.Parse(_mini).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.EndDate = DateTime.Parse(_max).Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                _max = DateTime.Parse(_max).Date.AddDays(1).ToString("yyyy-MM-dd");
                var AllTaxOwed = _unitOfWork.CrCasAccountContractCompanyOwed.FindAll(x =>
                x.CrCasAccountContractCompanyOwedCompanyCode == Company
                && x.CrCasAccountContractCompanyOwedContractCom == Contract
                && x.CrCasAccountContractCompanyOwedDate < DateTime.Parse(_max).Date && x.CrCasAccountContractCompanyOwedDate >= DateTime.Parse(_mini).Date).OrderByDescending(x => x.CrCasAccountContractCompanyOwedDate).ToList();
                
                var AllCompanies = _unitOfWork.CrMasLessorInformation.FindAll(x => x.CrMasLessorInformationCode != "0000").OrderBy(x => x.CrMasLessorInformationCode).ToList();

                CompanyDues_VM companyDues_VM = new CompanyDues_VM();
                companyDues_VM.CrMasLessorInformation = AllCompanies;
                //ViewBag.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                if (status == "All")
                {
                    companyDues_VM.CrCasAccountContractCompanyOwed = AllTaxOwed;
                    //companyDues_VM.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                    //if (companyDues_VM.Company_ContractNo == null)
                    //{
                    //    

                    //    ViewBag.Company_ContractNo = "-00";
                    //}
                    //else
                    //{
                    //    
                    //    ViewBag.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                    //}


                    return PartialView("_DataTableBasic", companyDues_VM);
                }
                else
                {

                    if (status == "1")
                    {
                        //ViewBag.Company_ContractNo = "5555";
                        AllTaxOwed = AllTaxOwed.Where(x => x.CrCasAccountContractCompanyOwedAccrualStatus == true).ToList();
                    }
                    else if (status == "0")
                    {
                        //ViewBag.Company_ContractNo = "333333";
                        AllTaxOwed = AllTaxOwed.Where(x => x.CrCasAccountContractCompanyOwedAccrualStatus == false).ToList();

                    }
                    else
                    {
                    }
                    companyDues_VM.CrCasAccountContractCompanyOwed = AllTaxOwed;
                    //companyDues_VM.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                    //if (companyDues_VM.Company_ContractNo == null)
                    //{
                    //    

                    //    ViewBag.Company_ContractNo = "-00";
                    //}
                    //else
                    //{
                    //    ViewBag.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                    //}

                    //ViewBag.Company_ContractNo = AllTaxOwed?.FirstOrDefault()?.CrCasAccountContractCompanyOwedContractCom;
                    return PartialView("_DataTableBasic", companyDues_VM);
                }

            }
            return PartialView();
        }


        [HttpGet]
        public async Task<IActionResult> GetAllContractsByCom(string Company)
        {
            List<string> contracts = new List<string>();
            var Contract_Type = "";

            if (Company != null)
            {
                var list_All_contracts_DropDown = _unitOfWork.CrMasContractCompany.FindAll(x => x.CrMasContractCompanyLessor == Company && x.CrMasContractCompanyProcedures == "112" && x.CrMasContractCompanyStatus !="N" ).OrderByDescending(x => x.CrMasContractCompanyDate).ToList();
                foreach(var item in list_All_contracts_DropDown)
                {
                    contracts.Add(item.CrMasContractCompanyNo);
                }
                Contract_Type = list_All_contracts_DropDown?.FirstOrDefault()?.CrMasContractCompanyActivation;

            }

            var result = new
            {
                    contracts = contracts,
                    contract_1_Type = Contract_Type,
                    accountIban = "22",
                    bankNo = "22",
                    arBank = "22",
                    enBank = "22",
                };
                return Json(result);
                //RedirectToAction("SuccessToast", "TaxOwed");
                //return Json(null);
                return Json(new { code = 1 });
            //return View();
        }



        [HttpGet]
        public async Task<IActionResult> GetNameTypeOfContract(string Contract)
        {
            var contract_type = "";

            if (Contract != null)
            {
                var NameTypeOfContract = _unitOfWork.CrMasContractCompany.Find(x => x.CrMasContractCompanyNo == Contract);
                contract_type = NameTypeOfContract?.CrMasContractCompanyActivation;
            }

            var result = new
            {
                contract_type = contract_type,

            };
            return Json(result);
            //RedirectToAction("SuccessToast", "TaxOwed");
            //return Json(null);
            return Json(new { code = 1 });
            //return View();
        }



    public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarCompany";
            ViewBag.no = "3";
            var titles = await setTitle("205", "1101004", "1");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            ViewBag.Data = "0";
            return View();

        }


    }
}