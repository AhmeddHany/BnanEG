using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Inferastructure.Repository.MAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.Areas.CAS.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.MasReports
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class ReportFTPemployeeController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportFTPemployeeController> _localizer;
        private readonly string pageNumber = SubTasks.MasReport6;


        public ReportFTPemployeeController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportFTPemployeeController> localizer) : base(userManager, unitOfWork, mapper)
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

            var all_userInfo = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationStatus != Status.Deleted,
                //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                selectProjection: query => query.Select(x => new userinfo_FTP_VM
                {
                    CrMasUserInformationCode = x.CrMasUserInformationCode,
                    CrMasUserInformationLessor = x.CrMasUserInformationLessor,
                    CrMasUserInformationArName = x.CrMasUserInformationArName,
                    CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                    CrMasUserInformationReservedBalance = x.CrMasUserInformationReservedBalance,
                    CrMasUserInformationTotalBalance = x.CrMasUserInformationTotalBalance,
                    CrMasUserInformationAvailableBalance = x.CrMasUserInformationAvailableBalance,
                    CrMasUserInformationCreditLimit = x.CrMasUserInformationCreditLimit,
                    CrMasUserInformationLastActionDate = x.CrMasUserInformationLastActionDate,
                    CrMasUserInformationPicture = x.CrMasUserInformationPicture,
                    CrMasUserInformationOperationStatus = x.CrMasUserInformationOperationStatus,
                    CrMasUserInformationStatus = x.CrMasUserInformationStatus,
                })
                //, includes: new string[] { "CrCasRenterContractBasicCarSerailNoNavigation" }
                );
            var all_userIds_recipt = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302",
                selectProjection: query => query.Select(x => new list_String_2
                {
                    id_key = x.CrCasAccountReceiptUser,
                })
                );
            //all_userIds_recipt.DistinctBy(x => x.id_key).ToList();
            var all_bonds = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
                   predicate: x => x.CrCasAccountReceiptType == "301",
                   columnSelector: x => x.CrCasAccountReceiptUser  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                                   //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                   );
            var all_exchanges = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
               predicate: x => x.CrCasAccountReceiptType == "302",
               columnSelector: x => x.CrCasAccountReceiptUser  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                               //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
               );


            for (int i = 0; i < all_userInfo.Count; i++)
            {

                var thisId = all_userIds_recipt.Find(x => x.id_key == all_userInfo[i]?.CrMasUserInformationCode)?.id_key;
                if (string.IsNullOrEmpty(thisId))
                {
                    all_userInfo.RemoveAt(i);
                    i--;  // تقليل الفهرس لأن القائمة أصبحت أقصر
                }
            }

            var all_lessors = await _unitOfWork.CrMasLessorInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasLessorInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new list_String_4
                {
                    id_key = x.CrMasLessorInformationCode,
                    nameAr = x.CrMasLessorInformationArShortName,
                    nameEn = x.CrMasLessorInformationEnShortName,
                })
                );
            if (all_exchanges?.Count == 0)
            {
                var objSanad = new TResult2()
                {
                    Column = "0",
                    RowCount = 0
                };
                all_exchanges.Add(objSanad);
                all_exchanges.Add(objSanad);
            }

            if (all_userInfo?.Count == 0 || all_bonds?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            ViewBag.radio = "A";


            listReportFTPemployeeVM VM = new listReportFTPemployeeVM();
            VM.all_usersinfo = all_userInfo;
            VM.all_bonds = all_bonds;
            VM.all_exchanges = all_exchanges;
            VM.all_userIds_recipt = all_userIds_recipt;
            VM.all_lessors = all_lessors;

            return View(VM);
        }

        [HttpGet]
        //[Route("/MAS/ReportFTPemployee/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string status, string id, string start, string end)
        {
            listReportFTPemployeeVM VM = new listReportFTPemployeeVM();

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
                predicate: x => x.CrCasAccountReceiptUser == id
                && (x.CrCasAccountReceiptDate > start_Date && x.CrCasAccountReceiptDate <= end_Date)
                ,
                selectProjection: query => query.Select(x => new ReciptVM
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

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation" }
                );

                if (status == "1")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "1").ToList();
                }
                else if (status == "2")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "2").ToList();
                }
                else if (status == "3")
                {
                    all_Recipts = all_Recipts.Where(x => x.CrCasAccountReceiptIsPassing == "3" || x.CrCasAccountReceiptIsPassing == "4").ToList();
                }

                var ThisUserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasUserInformationCode == id,
                    selectProjection: query => query.Select(x => new CrMasUserInformation
                    {
                        CrMasUserInformationCode = x.CrMasUserInformationCode,
                        CrMasUserInformationArName = x.CrMasUserInformationArName,
                        CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                    })
                    );

                sumitionofClass_FTPemployee_VM summition = new sumitionofClass_FTPemployee_VM();
                foreach (var single in all_Recipts)
                {

                    summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                    summition.Creditor_Total += single.CrCasAccountReceiptPayment;

                }

                var all_lessors = await _unitOfWork.CrMasLessorInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasLessorInformationStatus != Status.Deleted,
                    selectProjection: query => query.Select(x => new list_String_4
                    {
                        id_key = x.CrMasLessorInformationCode,
                        nameAr = x.CrMasLessorInformationArShortName,
                        nameEn = x.CrMasLessorInformationEnShortName,
                    })
                    );
                if (all_Recipts.Count > 0)
                {
                    all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
                }
                VM.UserId = id;
                VM.ThisUserData = ThisUserData?.FirstOrDefault();
                VM.summition = summition;
                VM.all_Recipts = all_Recipts;
                VM.all_lessors = all_lessors;


                return PartialView("_EditpartDataTableReportFTPemployee", VM);
            }
            listReportFTPemployeeVM VM2 = new listReportFTPemployeeVM();

            return PartialView("_EditpartDataTableReportFTPemployee", VM2);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            listReportFTPemployeeVM VM = new listReportFTPemployeeVM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasAccountReceiptUser == id,
                    selectProjection: query => query.Select(x => new Date_ReportActiveContractVM
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
                predicate: x => x.CrCasAccountReceiptUser == id
                && (x.CrCasAccountReceiptDate > start && x.CrCasAccountReceiptDate <= end)
                ,
                selectProjection: query => query.Select(x => new ReciptVM
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

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation" }
                );

            var ThisUserData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationCode == id,
                selectProjection: query => query.Select(x => new CrMasUserInformation
                {
                    CrMasUserInformationCode = x.CrMasUserInformationCode,
                    CrMasUserInformationArName = x.CrMasUserInformationArName,
                    CrMasUserInformationEnName = x.CrMasUserInformationEnName,
                })
                );
            var all_lessors = await _unitOfWork.CrMasLessorInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasLessorInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new list_String_4
                {
                    id_key = x.CrMasLessorInformationCode,
                    nameAr = x.CrMasLessorInformationArShortName,
                    nameEn = x.CrMasLessorInformationEnShortName,
                })
                );

            sumitionofClass_FTPemployee_VM summition = new sumitionofClass_FTPemployee_VM();
            foreach (var single in all_Recipts)
            {

                summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                summition.Creditor_Total += single.CrCasAccountReceiptPayment;

            }


            if (ThisUserData.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "ReportFTPemployee");
            }
            if (all_Recipts.Count > 0)
            {
                all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
            }
            VM.UserId = id;
            VM.ThisUserData = ThisUserData?.FirstOrDefault();
            VM.summition = summition;
            VM.all_Recipts = all_Recipts;
            VM.all_lessors = all_lessors;
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
            return RedirectToAction("Index", "ReportFTPemployee");
        }


    }
}
