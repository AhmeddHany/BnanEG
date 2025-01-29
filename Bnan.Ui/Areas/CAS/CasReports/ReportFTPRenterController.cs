using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
//using Bnan.Inferastructure.Repository.CAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class ReportFTPRenterController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportFTPRenterController> _localizer;
        private readonly string pageNumber = SubTasks.FT_Renters_Report_Cas;


        public ReportFTPRenterController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportFTPRenterController> localizer) : base(userManager, unitOfWork, mapper)
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
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var all_CasRenterLessor = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterLessorCode == user.CrMasUserInformationLessor
                && x.CrCasRenterLessorContractCount > 0 && x.CrCasRenterLessorStatus != Status.Deleted,
                //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                selectProjection: query => query.Select(x => new info_FTP_CasRenterLessor_Renter_VM
                {
                    CrCasRenterLessorId = x.CrCasRenterLessorId,
                    TradedAmount = x.CrCasRenterLessorContractTradedAmount,
                    CrCasRenterLessorCode = x.CrCasRenterLessorCode,
                    Renter_Ar = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
                    Renter_En = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
                    TotalBalance = x.CrCasRenterLessorBalance,
                    AvailableBalance = x.CrCasRenterLessorAvailableBalance,
                    ReservedBalance = x.CrCasRenterLessorReservedBalance,
                })
                , includes: new string[] { "CrCasRenterLessorNavigation" }
                );

            all_CasRenterLessor = all_CasRenterLessor.DistinctBy(x => x.CrCasRenterLessorId).ToList();

            //var all_bonds = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
            //       predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor && x.CrCasAccountReceiptType == "301",
            //       columnSelector: x => x.CrCasAccountReceiptRenterId  // تحديد العمود الذي نريد التجميع بناءً عليه
            //                                                       //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            //       );
            //var all_exchanges = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
            //   predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor && x.CrCasAccountReceiptType == "302",
            //   columnSelector: x => x.CrCasAccountReceiptRenterId  // تحديد العمود الذي نريد التجميع بناءً عليه
            //                                                       //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
            //   );


            if (all_CasRenterLessor?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            ViewBag.radio = "A";


            listReportFTPRenterVM VM = new listReportFTPRenterVM();
            VM.all_CasRenterLessor = all_CasRenterLessor;
            //VM.all_bonds = all_bonds;
            //VM.all_exchanges = all_exchanges;
            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/ReportFTPRenter/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string status,string id,string start, string end)
        {
            var user = await _userManager.GetUserAsync(User);

            listReportFTPRenterVM VM = new listReportFTPRenterVM();

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                VM.start_Date = start_Date.ToString("yyyy-MM-dd");
                VM.end_Date = end_Date.AddDays(-1).ToString("yyyy-MM-dd");

                await SetPageTitleAsync(Status.Update, pageNumber);
                var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptRenterId == id
                && x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                && (x.CrCasAccountReceiptDate > start_Date && x.CrCasAccountReceiptDate <= end_Date)
                ,
                selectProjection: query => query.Select(x => new ReciptVM_Renter
                {
                    CrCasAccountReceiptNo = x.CrCasAccountReceiptNo,
                    CrCasAccountReceiptType = x.CrCasAccountReceiptType,
                    CrCasAccountReceiptDate = x.CrCasAccountReceiptDate,
                    CrCasAccountReceiptLessorCode = x.CrCasAccountReceiptLessorCode,
                    CrCasAccountReceiptReferenceType = x.CrCasAccountReceiptReferenceType,
                    CrCasAccountReceiptSalesPoint = x.CrCasAccountReceiptSalesPoint,
                    CrCasAccountReceiptReferenceNo = x.CrCasAccountReceiptReferenceNo,
                    CrCasAccountReceiptPayment = x.CrCasAccountReceiptPayment,
                    CrCasAccountReceiptReceipt = x.CrCasAccountReceiptReceipt,
                    CrCasAccountReceiptRenterId = x.CrCasAccountReceiptRenterId,
                    CrCasAccountReceiptUser = x.CrCasAccountReceiptUser,
                    CrCasAccountReceiptIsPassing = x.CrCasAccountReceiptIsPassing,
                    CrCasAccountReceiptPassingReference = x.CrCasAccountReceiptPassingReference,
                    CrCasAccountReceiptPassingUser = x.CrCasAccountReceiptPassingUser,
                    CrCasAccountReceiptPdfFile = x.CrCasAccountReceiptPdfFile,
                    PaymentMethod_Ar = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodArName,
                    PaymentMethod_En = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodEnName,
                    ReferanceType_Ar = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceArName,
                    ReferanceType_En = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceEnName,
                    Salespoint_Ar = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointArName,
                    Salespoint_En = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointEnName,
                    branch_Ar = x.CrCasAccountReceiptNavigation.CrCasBranchInformationArShortName,
                    branch_En = x.CrCasAccountReceiptNavigation.CrCasBranchInformationEnShortName,
                    userPreviousBalance = x.CrCasAccountReceiptUserPreviousBalance,

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation", "CrCasAccountReceiptNavigation" }
                );

                if (status=="1")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "1").ToList();
                }
                else if (status == "2")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "2").ToList();
                }
                else if(status == "3")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "3" || x.CrCasAccountReceiptIsPassing == "4").ToList();
                }

                var all_UserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasUserInformationLessor == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new UserInfo_FTR_Renter
                    {
                        CrMasUserInformationCode = x.CrMasUserInformationCode,
                        CrMasUserInformationArName = x.CrMasUserInformationArName,
                        CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                    })
                    );

                var This_RenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasRenterInformationId == id.Trim(),
                    selectProjection: query => query.Select(x => new RenterInfo_FTR_Renter
                    {
                        CrMasRenterInformationId = x.CrMasRenterInformationId,
                        CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                        CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                    })
                    );

                sumitionofClass_FTPRenter_VM summition = new sumitionofClass_FTPRenter_VM();
                foreach (var single in all_Recipts)
                {

                    summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                    summition.Creditor_Total += single.CrCasAccountReceiptPayment;

                }

                if (all_Recipts.Count > 0)
                {
                    all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
                }
                VM.RenterId = id;
                VM.all_UserData = all_UserData;
                VM.This_RenterData = This_RenterData?.FirstOrDefault();
                VM.summition = summition;
                VM.all_Recipts = all_Recipts;

                return PartialView("_EditpartDataTableReportFTPRenter", VM);
            }
            listReportFTPRenterVM VM2 = new listReportFTPRenterVM();

            return PartialView("_EditpartDataTableReportFTPRenter", VM2);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            listReportFTPRenterVM VM = new listReportFTPRenterVM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x=> x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                    && x.CrCasAccountReceiptRenterId == id,
                    selectProjection: query => query.Select(x => new Date_ReportFTPRenterVM
                    {
                        dates = x.CrCasAccountReceiptDate,
                    }));

            //if (listmaxDate?.Count == 0 || string.IsNullOrEmpty(id))
            if (string.IsNullOrEmpty(id))
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var maxDate = listmaxDate.Max(x => x.dates)?.ToString("yyyy-MM-dd");
            //var minDate = listmaxDate.Min(x => x.dates)?.ToString("yyyy-MM-dd");

            var end = DateTime.Now.AddDays(1);
            var start = DateTime.Now.AddMonths(-1);
            if (maxDate != null)
            {
                end = DateTime.Parse(maxDate).AddDays(1).Date;
                start = DateTime.Parse(maxDate).AddMonths(-1).Date;
            }

            var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptRenterId == id
                && x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                && (x.CrCasAccountReceiptDate > start && x.CrCasAccountReceiptDate <= end)
                ,
                selectProjection: query => query.Select(x => new ReciptVM_Renter
                {
                    CrCasAccountReceiptNo = x.CrCasAccountReceiptNo,
                    CrCasAccountReceiptType = x.CrCasAccountReceiptType,
                    CrCasAccountReceiptDate = x.CrCasAccountReceiptDate,
                    CrCasAccountReceiptLessorCode=x.CrCasAccountReceiptLessorCode,
                    CrCasAccountReceiptReferenceType = x.CrCasAccountReceiptReferenceType,
                    CrCasAccountReceiptSalesPoint = x.CrCasAccountReceiptSalesPoint,
                    CrCasAccountReceiptReferenceNo = x.CrCasAccountReceiptReferenceNo,
                    CrCasAccountReceiptPayment = x.CrCasAccountReceiptPayment,
                    CrCasAccountReceiptReceipt = x.CrCasAccountReceiptReceipt,
                    CrCasAccountReceiptRenterId = x.CrCasAccountReceiptRenterId,
                    CrCasAccountReceiptUser = x.CrCasAccountReceiptUser,
                    CrCasAccountReceiptIsPassing = x.CrCasAccountReceiptIsPassing,
                    CrCasAccountReceiptPassingReference = x.CrCasAccountReceiptPassingReference,
                    CrCasAccountReceiptPassingUser = x.CrCasAccountReceiptPassingUser,
                    CrCasAccountReceiptPdfFile = x.CrCasAccountReceiptPdfFile,
                    PaymentMethod_Ar = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodArName,
                    PaymentMethod_En = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodEnName,
                    ReferanceType_Ar = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceArName,
                    ReferanceType_En = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceEnName,
                    Salespoint_Ar = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointArName,
                    Salespoint_En = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointEnName,
                    branch_Ar = x.CrCasAccountReceiptNavigation.CrCasBranchInformationArShortName,
                    branch_En = x.CrCasAccountReceiptNavigation.CrCasBranchInformationEnShortName,
                    userPreviousBalance = x.CrCasAccountReceiptUserPreviousBalance,

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation", "CrCasAccountReceiptNavigation" }
                );

            var all_UserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new UserInfo_FTR_Renter
                {
                    CrMasUserInformationCode = x.CrMasUserInformationCode,
                    CrMasUserInformationArName = x.CrMasUserInformationArName,
                    CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                })
                );
            var This_RenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasRenterInformationId == id.Trim(),
                selectProjection: query => query.Select(x => new RenterInfo_FTR_Renter
                {
                    CrMasRenterInformationId = x.CrMasRenterInformationId,
                    CrMasRenterInformationArName = x.CrMasRenterInformationArName,
                    CrMasRenterInformationEnName = x.CrMasRenterInformationEnName,
                })
                );

            sumitionofClass_FTPRenter_VM summition = new sumitionofClass_FTPRenter_VM();
            foreach (var single in all_Recipts)
            {

                summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                summition.Creditor_Total += single.CrCasAccountReceiptPayment;

            }


            if (all_UserData.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ReportFTPRenter");
            }
            if (all_Recipts.Count > 0 )
            {
                all_Recipts = all_Recipts.OrderBy(x=>x.CrCasAccountReceiptDate).ToList();
            }
            VM.RenterId = id;
            VM.all_UserData = all_UserData;
            VM.This_RenterData = This_RenterData?.FirstOrDefault();
            VM.summition = summition;
            VM.all_Recipts = all_Recipts;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(VM);
        }

        [HttpGet]
        public async Task<IActionResult> GetReceiptDetails(string ReceiptNo)
        {
            var receipt = await _unitOfWork.CrCasAccountReceipt.FindAsync(x => x.CrCasAccountReceiptNo == ReceiptNo, new[] {
                                                                                                                            "CrCasAccountReceiptReferenceTypeNavigation",
                                                                                                                            "CrCasAccountReceiptBankNavigation",
                                                                                                                            "CrCasAccountReceiptSalesPointNavigation",
                                                                                                                            "CrCasAccountReceiptPaymentMethodNavigation",
                                                                                                                             "CrCasAccountReceiptAccountNavigation"});
            var userRecevied = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == receipt.CrCasAccountReceiptPassingUser);
            if (receipt == null) return Json(false);
            ReceiptDetailsVM receiptDetails = new ReceiptDetailsVM();

            receiptDetails.ReceiptNo = ReceiptNo;
            receiptDetails.Date = receipt.CrCasAccountReceiptDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            receiptDetails.Creditor = receipt.CrCasAccountReceiptPayment?.ToString("N2", CultureInfo.InvariantCulture);
            receiptDetails.Debit = receipt.CrCasAccountReceiptReceipt?.ToString("N2", CultureInfo.InvariantCulture);
            receiptDetails.ReferenceNo = receipt.CrCasAccountReceiptReferenceNo;
            receiptDetails.ReferenceTypeAr = receipt.CrCasAccountReceiptReferenceTypeNavigation?.CrMasSupAccountReceiptReferenceArName;
            receiptDetails.ReferenceTypeEn = receipt.CrCasAccountReceiptReferenceTypeNavigation?.CrMasSupAccountReceiptReferenceEnName;
            if (receipt.CrCasAccountReceiptBank == "00")
            {
                receiptDetails.AccountBankCode = "";
                receiptDetails.BankAr = "";
                receiptDetails.BankEn = "";
            }
            else
            {
                receiptDetails.AccountBankCode = receipt.CrCasAccountReceiptAccountNavigation?.CrCasAccountBankIban;
                receiptDetails.BankAr = receipt.CrCasAccountReceiptBankNavigation?.CrMasSupAccountBankArName;
                receiptDetails.BankEn = receipt.CrCasAccountReceiptBankNavigation?.CrMasSupAccountBankEnName;
            }
            receiptDetails.SalesPointAr = receipt.CrCasAccountReceiptSalesPointNavigation?.CrCasAccountSalesPointArName;
            receiptDetails.SalesPointEn = receipt.CrCasAccountReceiptSalesPointNavigation?.CrCasAccountSalesPointEnName;
            receiptDetails.PaymentMethodAr = receipt.CrCasAccountReceiptPaymentMethodNavigation?.CrMasSupAccountPaymentMethodArName;
            receiptDetails.PaymentMethodEn = receipt.CrCasAccountReceiptPaymentMethodNavigation?.CrMasSupAccountPaymentMethodEnName;
            receiptDetails.CustodyNo = receipt.CrCasAccountReceiptPassingReference;
            receiptDetails.StatusReceipt = receipt.CrCasAccountReceiptIsPassing;
            receiptDetails.UserReceivedAr = userRecevied?.CrMasUserInformationArName;
            receiptDetails.UserReceivedEn = userRecevied?.CrMasUserInformationEnName;
            receiptDetails.ReceivedDate = receipt.CrCasAccountReceiptPassingDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            receiptDetails.Reasons = receipt.CrCasAccountReceiptReasons;



            return Json(receiptDetails);
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
            return RedirectToAction("Index", "ReportFTPRenter");
        }


    }
}
