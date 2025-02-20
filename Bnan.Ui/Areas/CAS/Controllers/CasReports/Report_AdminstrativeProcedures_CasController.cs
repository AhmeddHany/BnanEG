using AutoMapper;
using Azure;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
//using Bnan.Inferastructure.Repository.CAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class Report_AdminstrativeProcedures_CasController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<Report_AdminstrativeProcedures_CasController> _localizer;
        private readonly string pageNumber = SubTasks.Administrative_procedures_Report_Cas;


        public Report_AdminstrativeProcedures_CasController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<Report_AdminstrativeProcedures_CasController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masRenterInformation = masRenterInformation;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            // Set page titles
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);

            var lessorCode = user?.CrMasUserInformationLessor??" ";
            var listmaxDate = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasSysAdministrativeProceduresLessor == lessorCode,
                    selectProjection: query => query.Select(x => new Date_Report_AdminstrativeProcedures_CasVM
                    {
                        dates = x.CrCasSysAdministrativeProceduresDate,
                    }));

            if (listmaxDate?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var maxDate = listmaxDate.Max(x => x.dates)?.ToString("yyyy-MM-dd");

            var end = DateTime.Now.AddDays(1);
            var start = DateTime.Now.AddMonths(-1);
            if (maxDate != null)
            {
                end = DateTime.Parse(maxDate).AddDays(1);
                start = DateTime.Parse(maxDate).AddMonths(-1);
            }

            var all_Adminstrative_procedures = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasSysAdministrativeProceduresLessor == lessorCode
                && Convert.ToInt16(x.CrCasSysAdministrativeProceduresClassification??"0") <30
                && x.CrCasSysAdministrativeProceduresDate > start && x.CrCasSysAdministrativeProceduresDate <= end,
                selectProjection: query => query.Select(x => new Report_AdminstrativeProcedures_CasVM
                {
                    CrCasSysAdministrativeProceduresNo = x.CrCasSysAdministrativeProceduresNo,
                    CrCasSysAdministrativeProceduresCode = x.CrCasSysAdministrativeProceduresCode,
                    CrCasSysAdministrativeProceduresLessor = x.CrCasSysAdministrativeProceduresLessor,
                    CrCasSysAdministrativeProceduresDocNo = x.CrCasSysAdministrativeProceduresDocNo,
                    CrCasSysAdministrativeProceduresDate = x.CrCasSysAdministrativeProceduresDate,
                    CrCasSysAdministrativeProceduresTime = x.CrCasSysAdministrativeProceduresTime,
                    CrCasSysAdministrativeProceduresCarFrom = x.CrCasSysAdministrativeProceduresCarFrom,
                    CrCasSysAdministrativeProceduresCarTo = x.CrCasSysAdministrativeProceduresCarTo,
                    CrCasSysAdministrativeProceduresArDescription = x.CrCasSysAdministrativeProceduresArDescription,
                    CrCasSysAdministrativeProceduresEnDescription = x.CrCasSysAdministrativeProceduresEnDescription,
                    procedure_ArName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresArName,
                    procedure_EnName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresEnName,
                    UserArName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationArName,
                    UserEnName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationEnName,
                    CrCasSysAdministrativeProceduresTargeted = x.CrCasSysAdministrativeProceduresTargeted,
                    CrCasSysAdministrativeProceduresUserInsert = x.CrCasSysAdministrativeProceduresUserInsert,
                    CrCasSysAdministrativeProceduresStatus = x.CrCasSysAdministrativeProceduresStatus,
                    CrCasSysAdministrativeProceduresReasons = x.CrCasSysAdministrativeProceduresReasons,
                })
                , includes: new string[] { "CrCasSysAdministrativeProceduresCodeNavigation", "CrCasSysAdministrativeProceduresUserInsertNavigation" }
                );


            var all_Branches = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasBranchInformationLessor == lessorCode,
                selectProjection: query => query.Select(x => new cas_list_String_4
                {
                    id_key = x.CrCasBranchInformationCode,
                    nameAr = x.CrCasBranchInformationArShortName,
                    nameEn = x.CrCasBranchInformationEnShortName,
                })
                //, includes: new string[] { "CrCasSysAdministrativeProceduresCodeNavigation", "CrCasSysAdministrativeProceduresUserInsertNavigation" }
                );
            var all_Contractes = await _unitOfWork.CrMasContractCompany.FindAllWithSelectAsNoTrackingAsync(
           predicate: x => x.CrMasContractCompanyLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrMasContractCompanyNo,
                   nameAr = x.CrMasContractCompanyProceduresNavigation.CrMasSysProceduresArName,
                   nameEn = x.CrMasContractCompanyProceduresNavigation.CrMasSysProceduresEnName,
               })
               , includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_owners = await _unitOfWork.CrCasOwners.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasOwnersLessorCode == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasOwnersCode,
                   nameAr = x.CrCasOwnersArName,
                   nameEn = x.CrCasOwnersEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );

            var all_users = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrMasUserInformationLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrMasUserInformationCode,
                   nameAr = x.CrMasUserInformationArName,
                   nameEn = x.CrMasUserInformationEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_Drivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasRenterPrivateDriverInformationLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasRenterPrivateDriverInformationId,
                   nameAr = x.CrCasRenterPrivateDriverInformationArName,
                   nameEn = x.CrCasRenterPrivateDriverInformationEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_Renters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: null,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrMasRenterInformationId,
                   nameAr = x.CrMasRenterInformationArName,
                   nameEn = x.CrMasRenterInformationEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_Banks = await _unitOfWork.CrCasAccountBank.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasAccountBankLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasAccountBankCode,
                   nameAr = x.CrCasAccountBankArName,
                   nameEn = x.CrCasAccountBankEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_SalesPoints = await _unitOfWork.CrCasAccountSalesPoint.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasAccountSalesPointLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasAccountSalesPointCode,
                   nameAr = x.CrCasAccountSalesPointArName,
                   nameEn = x.CrCasAccountSalesPointEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_cars = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasCarInformationLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasCarInformationSerailNo,
                   nameAr = x.CrCasCarInformationConcatenateArName,
                   nameEn = x.CrCasCarInformationConcatenateEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_docCars = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasCarDocumentsMaintenanceProcedures,
                   str4 = x.CrCasCarDocumentsMaintenanceSerailNo,
                   //nameAr = x.CrCasCarInformationConcatenateArName,
                   //nameEn = x.CrCasCarInformationConcatenateEnName,
               })
               //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
               );
            var all_carDistributions = await _unitOfWork.CrCasPriceCarBasic.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasPriceCarBasicLessorCode == lessorCode,
               selectProjection: query => query.Select(x => new cas_list_String_4
               {
                   id_key = x.CrCasPriceCarBasicDistributionCode,
                   nameAr = x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateArName,
                   nameEn = x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateEnName,
               })
               , includes: new string[] { "CrCasPriceCarBasicDistributionCodeNavigation" }
               );

            foreach (var item in all_Adminstrative_procedures)
            {
                if (item.CrCasSysAdministrativeProceduresTargeted?.Length > 1)
                {

                    if (item.CrCasSysAdministrativeProceduresClassification == "10"
                        || item.CrCasSysAdministrativeProceduresCode == "201" || item.CrCasSysAdministrativeProceduresCode == "202"
                        )
                    {
                        var Branch = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (Branch != null)
                        {
                            // if 201 then it is branch
                            item.Target_ArName = $"{Branch.nameAr}";
                            item.Target_EnName = $"{Branch.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresClassification == "12" || item.CrCasSysAdministrativeProceduresClassification == "13"
                        || item.CrCasSysAdministrativeProceduresCode == "212" || item.CrCasSysAdministrativeProceduresCode == "213"
                        )
                    {
                        //// if 212 then it is docCar
                        //var docCar = all_docCars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim() && x.str4 == item.CrCasSysAdministrativeProceduresDocNo?.Trim());
                        //if (docCar != null)
                        //{
                        //    var car2 = all_cars?.Find(x => x.id_key == docCar.str4?.Trim());
                        //    if (car2 != null)
                        //    {
                        //        item.Target_ArName = $"{car2.nameAr}";
                        //        item.Target_EnName = $"{car2.nameEn}";
                        //    }
                        //    //item.Target_ArName = $"{docCar.nameAr}";
                        //    //item.Target_EnName = $"{docCar.nameEn}";
                        //}
                        var car2 = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresDocNo?.Trim());
                        if (car2 != null)
                        {
                            item.Target_ArName = $"{car2.nameAr}";
                            item.Target_EnName = $"{car2.nameEn}";
                        }
                        else
                        {
                            var car3 = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                            if (car3 != null)
                            {
                                item.Target_ArName = $"{car3.nameAr}";
                                item.Target_EnName = $"{car3.nameEn}";
                            }
                        }
                    }

                    else if (item.CrCasSysAdministrativeProceduresCode == "203")
                    {
                        // if 203 then it is Contract Company
                        var contract = all_Contractes?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (contract != null)
                        {
                            item.Target_ArName = $"{contract.nameAr}";
                            item.Target_EnName = $"{contract.nameEn}";
                        }

                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "204")
                    {
                        // if 204 then it is Owners
                        var owner = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (owner != null)
                        {
                            item.Target_ArName = $"{owner.nameAr}";
                            item.Target_EnName = $"{owner.nameEn}";
                        }

                    }
                    else if (Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") > 230 && Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") < 235)
                    {
                        // if 231 then it is Employee
                        var singleUser = all_users?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (singleUser != null)
                        {
                            item.Target_ArName = $"{singleUser.nameAr}";
                            item.Target_EnName = $"{singleUser.nameEn}";
                        }
                    }
                    /////
                    //////
                    //////////
                    else if (item.CrCasSysAdministrativeProceduresCode == "215")
                    {
                        // if 215 then it is Car - branch
                        var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                        var branch1 = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarFrom?.Trim());
                        var branch2 = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarTo?.Trim());
                        if (car != null)
                        {
                            //item.Target_ArName = $"{car.nameAr}";
                            //item.Target_EnName = $"{car.nameEn}";
                            item.Target_ArName = $"( {car.id_key} ) من ( {branch1?.nameAr ?? " "} ) إلى ( {branch2?.nameAr ?? " "} )";
                            item.Target_EnName = $"( {car.id_key} ) From ( {branch1?.nameEn ?? " "} ) To ( {branch2?.nameEn ?? " "} )";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "216")
                    {
                        // if 216 then it is Car - owner
                        var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                        var owner1 = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarFrom?.Trim());
                        var owner2 = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarTo?.Trim());
                        if (car != null)
                        {
                            //item.Target_ArName = $"{car.nameAr}";
                            //item.Target_EnName = $"{car.nameEn}";
                            item.Target_ArName = $"( {car.id_key} ) من ( {owner1?.nameAr ?? " "} ) إلى ( {owner2?.nameAr ?? " "} )";
                            item.Target_EnName = $"( {car.id_key} ) From ( {owner1?.nameEn ?? " "} ) To ( {owner2?.nameEn ?? " "} )";
                        }
                    }
                    //////
                    ///////
                    //////////
                    else if (Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") > 210 && Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") < 219)
                    {
                        // if 211 then it is car
                        var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (car != null)
                        {
                            item.Target_ArName = $"{car.nameAr}";
                            item.Target_EnName = $"{car.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "246")
                    {
                        // if 246 then it is Driver
                        var Driver = all_Drivers?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (Driver != null)
                        {
                            item.Target_ArName = $"{Driver.nameAr}";
                            item.Target_EnName = $"{Driver.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "247")
                    {
                        // if 247 then it is Renter
                        var Renter = all_Renters?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (Renter != null)
                        {
                            item.Target_ArName = $"{Renter.nameAr}";
                            item.Target_EnName = $"{Renter.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "243")
                    {
                        // if 243 then it is Bank
                        var Bank = all_Banks?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (Bank != null)
                        {
                            item.Target_ArName = $"{Bank.nameAr}";
                            item.Target_EnName = $"{Bank.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "244")
                    {
                        // if 244 then it is SalesPoint
                        var SalesPoint = all_SalesPoints?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (SalesPoint != null)
                        {
                            item.Target_ArName = $"{SalesPoint.nameAr}";
                            item.Target_EnName = $"{SalesPoint.nameEn}";
                        }
                    }
                    else if (item.CrCasSysAdministrativeProceduresCode == "219")
                    {
                        // if 219 then it is CarDistribution
                        var CarDistribution = all_carDistributions?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                        if (CarDistribution != null)
                        {
                            item.Target_ArName = $"{CarDistribution.nameAr}";
                            item.Target_EnName = $"{CarDistribution.nameEn}";
                        }
                    }

                }
            }

            listReport_AdminstrativeProcedures_CasVM VM = new listReport_AdminstrativeProcedures_CasVM();
            
            VM.all_Adminstrative_procedures = all_Adminstrative_procedures;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/Report_AdminstrativeProcedures_Cas/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string start, string end)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessorCode = user?.CrMasUserInformationLessor ?? " ";

            //sidebar Closed
            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                var all_Adminstrative_procedures = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasSysAdministrativeProceduresLessor == user.CrMasUserInformationLessor
                && Convert.ToInt16(x.CrCasSysAdministrativeProceduresClassification ?? "0") < 30
                && x.CrCasSysAdministrativeProceduresDate > start_Date && x.CrCasSysAdministrativeProceduresDate <= end_Date,
                    selectProjection: query => query.Select(x => new Report_AdminstrativeProcedures_CasVM
                    {
                        CrCasSysAdministrativeProceduresNo = x.CrCasSysAdministrativeProceduresNo,
                        CrCasSysAdministrativeProceduresCode = x.CrCasSysAdministrativeProceduresCode,
                        CrCasSysAdministrativeProceduresLessor = x.CrCasSysAdministrativeProceduresLessor,
                        CrCasSysAdministrativeProceduresDocNo = x.CrCasSysAdministrativeProceduresDocNo,
                        CrCasSysAdministrativeProceduresDate = x.CrCasSysAdministrativeProceduresDate,
                        CrCasSysAdministrativeProceduresTime = x.CrCasSysAdministrativeProceduresTime,
                        CrCasSysAdministrativeProceduresCarFrom = x.CrCasSysAdministrativeProceduresCarFrom,
                        CrCasSysAdministrativeProceduresCarTo = x.CrCasSysAdministrativeProceduresCarTo,
                        CrCasSysAdministrativeProceduresArDescription = x.CrCasSysAdministrativeProceduresArDescription,
                        CrCasSysAdministrativeProceduresEnDescription = x.CrCasSysAdministrativeProceduresEnDescription,
                        procedure_ArName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresArName,
                        procedure_EnName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresEnName,
                        UserArName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationArName,
                        UserEnName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationEnName,
                        CrCasSysAdministrativeProceduresTargeted = x.CrCasSysAdministrativeProceduresTargeted,
                        CrCasSysAdministrativeProceduresUserInsert = x.CrCasSysAdministrativeProceduresUserInsert,
                        CrCasSysAdministrativeProceduresStatus = x.CrCasSysAdministrativeProceduresStatus,
                        CrCasSysAdministrativeProceduresReasons = x.CrCasSysAdministrativeProceduresReasons,
                    })
                , includes: new string[] { "CrCasSysAdministrativeProceduresCodeNavigation", "CrCasSysAdministrativeProceduresUserInsertNavigation" }
                    );

                var all_Branches = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasBranchInformationLessor == lessorCode,
                    selectProjection: query => query.Select(x => new cas_list_String_4
                    {
                        id_key = x.CrCasBranchInformationCode,
                        nameAr = x.CrCasBranchInformationArShortName,
                        nameEn = x.CrCasBranchInformationEnShortName,
                    })
                //, includes: new string[] { "CrCasSysAdministrativeProceduresCodeNavigation", "CrCasSysAdministrativeProceduresUserInsertNavigation" }
                    );
                var all_Contractes = await _unitOfWork.CrMasContractCompany.FindAllWithSelectAsNoTrackingAsync(
               predicate: x => x.CrMasContractCompanyLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrMasContractCompanyNo,
                       nameAr = x.CrMasContractCompanyProceduresNavigation.CrMasSysProceduresArName,
                       nameEn = x.CrMasContractCompanyProceduresNavigation.CrMasSysProceduresEnName,
                   })
                   , includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_owners = await _unitOfWork.CrCasOwners.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasOwnersLessorCode == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasOwnersCode,
                       nameAr = x.CrCasOwnersArName,
                       nameEn = x.CrCasOwnersEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );

                var all_users = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrMasUserInformationCode,
                       nameAr = x.CrMasUserInformationArName,
                       nameEn = x.CrMasUserInformationEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_Drivers = await _unitOfWork.CrCasRenterPrivateDriverInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterPrivateDriverInformationLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasRenterPrivateDriverInformationId,
                       nameAr = x.CrCasRenterPrivateDriverInformationArName,
                       nameEn = x.CrCasRenterPrivateDriverInformationEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_Renters = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: null,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrMasRenterInformationId,
                       nameAr = x.CrMasRenterInformationArName,
                       nameEn = x.CrMasRenterInformationEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_Banks = await _unitOfWork.CrCasAccountBank.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasAccountBankLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasAccountBankCode,
                       nameAr = x.CrCasAccountBankArName,
                       nameEn = x.CrCasAccountBankEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_SalesPoints = await _unitOfWork.CrCasAccountSalesPoint.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasAccountSalesPointLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasAccountSalesPointCode,
                       nameAr = x.CrCasAccountSalesPointArName,
                       nameEn = x.CrCasAccountSalesPointEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_cars = await _unitOfWork.CrCasCarInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasCarInformationLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasCarInformationSerailNo,
                       nameAr = x.CrCasCarInformationConcatenateArName,
                       nameEn = x.CrCasCarInformationConcatenateEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_docCars = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasCarDocumentsMaintenanceLessor == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasCarDocumentsMaintenanceProcedures,
                       str4 = x.CrCasCarDocumentsMaintenanceSerailNo,
                       //nameAr = x.CrCasCarInformationConcatenateArName,
                       //nameEn = x.CrCasCarInformationConcatenateEnName,
                   })
                   //, includes: new string[] { "CrMasContractCompanyProceduresNavigation" }
                   );
                var all_carDistributions = await _unitOfWork.CrCasPriceCarBasic.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasPriceCarBasicLessorCode == lessorCode,
                   selectProjection: query => query.Select(x => new cas_list_String_4
                   {
                       id_key = x.CrCasPriceCarBasicDistributionCode,
                       nameAr = x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateArName,
                       nameEn = x.CrCasPriceCarBasicDistributionCodeNavigation.CrMasSupCarDistributionConcatenateEnName,
                   })
                   , includes: new string[] { "CrCasPriceCarBasicDistributionCodeNavigation" }
                   );

            foreach (var item in all_Adminstrative_procedures)
            {
                    if (item.CrCasSysAdministrativeProceduresTargeted?.Length > 1)
                    {

                        if (item.CrCasSysAdministrativeProceduresClassification == "10"
                            || item.CrCasSysAdministrativeProceduresCode =="201"|| item.CrCasSysAdministrativeProceduresCode == "202"
                            )
                        {
                            var Branch = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (Branch != null)
                            {
                                // if 201 then it is branch
                                item.Target_ArName = $"{Branch.nameAr}";
                                item.Target_EnName = $"{Branch.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresClassification == "12" || item.CrCasSysAdministrativeProceduresClassification == "13"
                            || item.CrCasSysAdministrativeProceduresCode == "212" || item.CrCasSysAdministrativeProceduresCode == "213"
                            )
                        {
                            //// if 212 then it is docCar
                            //var docCar = all_docCars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim() && x.str4 == item.CrCasSysAdministrativeProceduresDocNo?.Trim());
                            //if (docCar != null)
                            //{
                            //    var car2 = all_cars?.Find(x => x.id_key == docCar.str4?.Trim());
                            //    if (car2 != null)
                            //    {
                            //        item.Target_ArName = $"{car2.nameAr}";
                            //        item.Target_EnName = $"{car2.nameEn}";
                            //    }
                            //    //item.Target_ArName = $"{docCar.nameAr}";
                            //    //item.Target_EnName = $"{docCar.nameEn}";
                            //}
                            var car2 = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresDocNo?.Trim());
                            if (car2 != null)
                            {
                                item.Target_ArName = $"{car2.nameAr}";
                                item.Target_EnName = $"{car2.nameEn}";
                            }
                            else
                            {
                                var car3 = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                                if (car3 != null)
                                {
                                    item.Target_ArName = $"{car3.nameAr}";
                                    item.Target_EnName = $"{car3.nameEn}";
                                }
                            }
                        }

                        else if (item.CrCasSysAdministrativeProceduresCode == "203")
                        {
                            // if 203 then it is Contract Company
                            var contract = all_Contractes?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (contract != null)
                            {
                                item.Target_ArName = $"{contract.nameAr}";
                                item.Target_EnName = $"{contract.nameEn}";
                            }

                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "204")
                        {
                            // if 204 then it is Owners
                            var owner = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (owner != null)
                            {
                                item.Target_ArName = $"{owner.nameAr}";
                                item.Target_EnName = $"{owner.nameEn}";
                            }

                        }
                        else if (Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") > 230 && Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") < 235)
                        {
                            // if 231 then it is Employee
                            var singleUser = all_users?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (singleUser != null)
                            {
                                item.Target_ArName = $"{singleUser.nameAr}";
                                item.Target_EnName = $"{singleUser.nameEn}";
                            }
                        }
                        /////
                        //////
                        //////////
                        else if (item.CrCasSysAdministrativeProceduresCode == "215")
                        {
                            // if 215 then it is Car - branch
                            var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                            var branch1 = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarFrom?.Trim());
                            var branch2 = all_Branches?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarTo?.Trim());
                            if (car != null)
                            {
                                //item.Target_ArName = $"{car.nameAr}";
                                //item.Target_EnName = $"{car.nameEn}";
                                item.Target_ArName = $"( {car.id_key} ) من ( {branch1?.nameAr ?? " "} ) إلى ( {branch2?.nameAr ?? " "} )";
                                item.Target_EnName = $"( {car.id_key} ) From ( {branch1?.nameEn ?? " "} ) To ( {branch2?.nameEn ?? " "} )";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "216")
                        {
                            // if 216 then it is Car - owner
                            var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted?.Trim());
                            var owner1 = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarFrom?.Trim());
                            var owner2 = all_owners?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresCarTo?.Trim());
                            if (car != null)
                            {
                                //item.Target_ArName = $"{car.nameAr}";
                                //item.Target_EnName = $"{car.nameEn}";
                                item.Target_ArName = $"( {car.id_key} ) من ( {owner1?.nameAr ?? " "} ) إلى ( {owner2?.nameAr ?? " "} )";
                                item.Target_EnName = $"( {car.id_key} ) From ( {owner1?.nameEn ?? " "} ) To ( {owner2?.nameEn ?? " "} )";
                            }
                        }
                        //////
                        ///////
                        //////////
                        else if (Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") > 210 && Convert.ToInt16(item.CrCasSysAdministrativeProceduresCode ?? "0") < 219)
                        {
                            // if 211 then it is car
                            var car = all_cars?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (car != null)
                            {
                                item.Target_ArName = $"{car.nameAr}";
                                item.Target_EnName = $"{car.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "246")
                        {
                            // if 246 then it is Driver
                            var Driver = all_Drivers?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (Driver != null)
                            {
                                item.Target_ArName = $"{Driver.nameAr}";
                                item.Target_EnName = $"{Driver.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "247")
                        {
                            // if 247 then it is Renter
                            var Renter = all_Renters?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (Renter != null)
                            {
                                item.Target_ArName = $"{Renter.nameAr}";
                                item.Target_EnName = $"{Renter.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "243")
                        {
                            // if 243 then it is Bank
                            var Bank = all_Banks?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (Bank != null)
                            {
                                item.Target_ArName = $"{Bank.nameAr}";
                                item.Target_EnName = $"{Bank.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "244")
                        {
                            // if 244 then it is SalesPoint
                            var SalesPoint = all_SalesPoints?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (SalesPoint != null)
                            {
                                item.Target_ArName = $"{SalesPoint.nameAr}";
                                item.Target_EnName = $"{SalesPoint.nameEn}";
                            }
                        }
                        else if (item.CrCasSysAdministrativeProceduresCode == "219")
                        {
                            // if 219 then it is CarDistribution
                            var CarDistribution = all_carDistributions?.Find(x => x.id_key == item.CrCasSysAdministrativeProceduresTargeted.Trim());
                            if (CarDistribution != null)
                            {
                                item.Target_ArName = $"{CarDistribution.nameAr}";
                                item.Target_EnName = $"{CarDistribution.nameEn}";
                            }
                        }

                    }
                }

                listReport_AdminstrativeProcedures_CasVM VM = new listReport_AdminstrativeProcedures_CasVM();

                VM.all_Adminstrative_procedures = all_Adminstrative_procedures;
                return PartialView("_DataTableReport_AdminstrativeProcedures_Cas", VM);
            }
            listReport_AdminstrativeProcedures_CasVM VM2 = new listReport_AdminstrativeProcedures_CasVM();

            return PartialView("_DataTableReport_AdminstrativeProcedures_Cas", VM2);
            //return PartialView();
        }

        [HttpGet]
        //[Route("/CAS/Report_AdminstrativeProcedures_Cas/GetContractsByStatus")]
        public async Task<IActionResult> GetContractsData(string contractId)
        {
            var user = await _userManager.GetUserAsync(User);

          
                var this_Contracts = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasSysAdministrativeProceduresLessor == user.CrMasUserInformationLessor
                && x.CrCasSysAdministrativeProceduresNo == contractId.Trim()
                 && Convert.ToInt16(x.CrCasSysAdministrativeProceduresClassification ?? "0") < 30,
                    selectProjection: query => query.Select(x => new Report_AdminstrativeProcedures_CasVM
                    {
                        CrCasSysAdministrativeProceduresNo = x.CrCasSysAdministrativeProceduresNo,
                        CrCasSysAdministrativeProceduresCode = x.CrCasSysAdministrativeProceduresCode,
                        CrCasSysAdministrativeProceduresLessor = x.CrCasSysAdministrativeProceduresLessor,
                        CrCasSysAdministrativeProceduresDate = x.CrCasSysAdministrativeProceduresDate,
                        CrCasSysAdministrativeProceduresTime = x.CrCasSysAdministrativeProceduresTime,
                        CrCasSysAdministrativeProceduresCarFrom = x.CrCasSysAdministrativeProceduresCarFrom,
                        CrCasSysAdministrativeProceduresCarTo = x.CrCasSysAdministrativeProceduresCarTo,
                        CrCasSysAdministrativeProceduresArDescription = x.CrCasSysAdministrativeProceduresArDescription,
                        CrCasSysAdministrativeProceduresEnDescription = x.CrCasSysAdministrativeProceduresEnDescription,
                        procedure_ArName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresArName,
                        procedure_EnName = x.CrCasSysAdministrativeProceduresCodeNavigation.CrMasSysProceduresEnName,
                        UserArName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationArName,
                        UserEnName = x.CrCasSysAdministrativeProceduresUserInsertNavigation.CrMasUserInformationEnName,
                        CrCasSysAdministrativeProceduresTargeted = x.CrCasSysAdministrativeProceduresTargeted,
                        CrCasSysAdministrativeProceduresUserInsert = x.CrCasSysAdministrativeProceduresUserInsert,
                        CrCasSysAdministrativeProceduresStatus = x.CrCasSysAdministrativeProceduresStatus,
                        CrCasSysAdministrativeProceduresReasons = x.CrCasSysAdministrativeProceduresReasons,
                    })
                , includes: new string[] { "CrCasSysAdministrativeProceduresCodeNavigation", "CrCasSysAdministrativeProceduresUserInsertNavigation" }
                    );

             

            var single = this_Contracts?.FirstOrDefault();


            // #############

            var Contract_pdf = "#";
            var Contract_pdf_blank = "";
            var Invoice_pdf = "#";
            var Invoice_pdf_blank = "";
            var TGA_pdf = "#";
            var TGA_pdf_blank = "";


            var response = new
                {
                    reciptPdf_href = Invoice_pdf,
                    reciptPdf_target = Invoice_pdf_blank,
                    ContractPdf_href = Contract_pdf,
                    ContractPdf_target = Contract_pdf_blank,
                    TGAPdf_href = TGA_pdf,
                    TGAPdf_target = TGA_pdf_blank,
                };

                return Json(response);
            
       
            return Json(null);
            //return PartialView();
        }

        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasRenterInformation licence, string status)
        {


            var recordAr = licence.CrMasRenterInformationArName;
            var recordEn = licence.CrMasRenterInformationEnName;
            var (operationAr, operationEn) = GetStatusTranslation(status);

            var (mainTask, subTask, system, currentUser) = await SetTrace(pageNumber);

            await _userLoginsService.SaveTracing(
                currentUser.CrMasUserInformationCode,
                recordAr,
                recordEn,
                operationAr,
                operationEn,
                mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode,
                mainTask.CrMasSysMainTasksArName,
                subTask.CrMasSysSubTasksArName,
                mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName,
                system.CrMasSysSystemCode,
                system.CrMasSysSystemArName,
                system.CrMasSysSystemEnName);
        }

        [HttpPost]
        public IActionResult DisplayToastError_NoUpdate(string messageText)
        {
            //نص الرسالة _localizer["AuthEmplpoyee_NoUpdate"] === messageText ; 
            if (messageText == null || messageText == "") messageText = "..";
            _toastNotification.AddErrorToastMessage(messageText, new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return Json(new { success = true });
        }


        public IActionResult DisplayToastSuccess_withIndex()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
            return RedirectToAction("Index", "Report_AdminstrativeProcedures_Cas");
        }


    }
}
