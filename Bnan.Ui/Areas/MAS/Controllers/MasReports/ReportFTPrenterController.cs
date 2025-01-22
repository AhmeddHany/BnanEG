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
    public class ReportFTPrenterController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasRenterInformation _masRenterInformation;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<ReportFTPrenterController> _localizer;
        private readonly string pageNumber = SubTasks.MasReport7;


        public ReportFTPrenterController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasRenterInformation masRenterInformation, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<ReportFTPrenterController> localizer) : base(userManager, unitOfWork, mapper)
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

            var all_RenterInfo = await _unitOfWork.CrCasRenterLessor.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasRenterLessorStatus != Status.Deleted,
                //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
                selectProjection: query => query.Select(x => new Renterinfo_FTP_VM
                {
                    CrCasRenterLessorId = x.CrCasRenterLessorId,
                    CrCasRenterLessorCode = x.CrCasRenterLessorCode,
                    Renter_Ar = x.CrCasRenterLessorNavigation.CrMasRenterInformationArName,
                    Renter_En = x.CrCasRenterLessorNavigation.CrMasRenterInformationEnName,
                    CrCasRenterLessorAvailableBalance = x.CrCasRenterLessorAvailableBalance,
                    CrCasRenterLessorBalance = x.CrCasRenterLessorBalance,
                    CrCasRenterLessorReservedBalance = x.CrCasRenterLessorReservedBalance,
                    CrCasRenterLessorContractTradedAmount = x.CrCasRenterLessorContractTradedAmount,
                    CrCasRenterLessorDateFirstInteraction = x.CrCasRenterLessorDateFirstInteraction,
                    CrCasRenterLessorDateLastContractual = x.CrCasRenterLessorDateLastContractual,
                    CrCasRenterLessorStatus = x.CrCasRenterLessorStatus,
                })
                , includes: new string[] { "CrCasRenterLessorNavigation" }
                );
            var all_RenterInfo2 = all_RenterInfo.DistinctBy(x => x.CrCasRenterLessorId);
            foreach (var r in all_RenterInfo2)
            {
                var theseSingles = all_RenterInfo.Where(x => x.CrCasRenterLessorId == r.CrCasRenterLessorId);
                r.CrCasRenterLessorReservedBalance = theseSingles.Sum(x => x.CrCasRenterLessorReservedBalance);
                r.CrCasRenterLessorBalance = theseSingles.Sum(x => x.CrCasRenterLessorBalance);
                r.CrCasRenterLessorAvailableBalance = theseSingles.Sum(x => x.CrCasRenterLessorAvailableBalance);
                r.CrCasRenterLessorContractTradedAmount = theseSingles.Sum(x => x.CrCasRenterLessorContractTradedAmount);
            }
            var all_RenterInfo3 = all_RenterInfo2?.ToList();



            ////var all_RenterIds = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
            ////    predicate: x => x.CrMasRenterInformationStatus != Status.Deleted,
            ////    //x.CrCasCarInformationLastContractDate > start && x.CrCasCarInformationLastContractDate <= end,
            ////    selectProjection: query => query.Select(x => new list_String_4
            ////    {
            ////        id_key = x.CrMasRenterInformationId,
            ////        nameAr = x.CrMasRenterInformationArName,
            ////        nameEn = x.CrMasRenterInformationEnName,
            ////        str4 = x.CrMasRenterInformationStatus,
            ////    })
            ////    //, includes: new string[] { "CrCasRenterLessorNavigation" }
            ////    );


            //var all_RentersIds_recipt = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
            //    predicate: x=>x.CrCasAccountReceiptType =="301" || x.CrCasAccountReceiptType == "302",
            //    selectProjection: query => query.Select(x => new list_String_2
            //    {
            //        id_key = x.CrCasAccountReceiptRenterId,
            //    })
            //    );
            // //all_RentersIds_recipt.DistinctBy(x => x.id_key).ToList();

            var all_bonds = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
                   predicate: x => x.CrCasAccountReceiptType == "301",
                   columnSelector: x => x.CrCasAccountReceiptRenterId  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                                       //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                   );
            var all_exchanges = await _unitOfWork.CrCasAccountReceipt.FindCountByColumnAsync<CrCasAccountReceipt>(
               predicate: x => x.CrCasAccountReceiptType == "302",
               columnSelector: x => x.CrCasAccountReceiptRenterId  // تحديد العمود الذي نريد التجميع بناءً عليه
                                                                   //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
               );

            //// filter الموظفين اللي مش له recipte
            //for (int i = 0; i < all_RenterInfo.Count; i++)
            //{

            //    var thisId = all_RentersIds_recipt.Find(x => x.id_key == all_RenterInfo[i]?.CrCasRenterLessorId)?.id_key;
            //    if (string.IsNullOrEmpty(thisId))
            //    {
            //        all_RenterInfo.RemoveAt(i);
            //        i--;  // تقليل الفهرس لأن القائمة أصبحت أقصر
            //    }
            //}

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
            if (all_RenterInfo3?.Count == 0 || all_bonds?.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["NoDataToShow"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            ViewBag.radio = "A";


            listReportFTPrenterVM VM = new listReportFTPrenterVM();
            VM.all_Rentersinfo = all_RenterInfo3;
            VM.all_bonds = all_bonds;
            VM.all_exchanges = all_exchanges;
            //VM.all_RentersIds_recipt = all_RentersIds_recipt;

            return View(VM);
        }

        [HttpGet]
        //[Route("/MAS/ReportFTPrenter/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string id, string start, string end)
        {
            listReportFTPrenterVM VM = new listReportFTPrenterVM();

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                VM.start_Date = start_Date.ToString("yyyy-MM-dd");
                VM.end_Date = end_Date.AddDays(-1).ToString("yyyy-MM-dd");

                await SetPageTitleAsync(Status.Update, pageNumber);
                var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptRenterId == id
                && (x.CrCasAccountReceiptDate > start_Date && x.CrCasAccountReceiptDate <= end_Date)
                ,
                selectProjection: query => query.Select(x => new Recipt_ForRenter_VM
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

                var all_UsersData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasUserInformationStatus != Status.Deleted,
                    selectProjection: query => query.Select(x => new list_String_4
                    {
                        id_key = x.CrMasUserInformationCode,
                        nameAr = x.CrMasUserInformationArName,
                        nameEn = x.CrMasUserInformationEnName,
                    })
                    );

                var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasRenterInformationId == id,
                    selectProjection: query => query.Select(x => new list_String_4
                    {
                        id_key = x.CrMasRenterInformationId,
                        nameAr = x.CrMasRenterInformationArName,
                        nameEn = x.CrMasRenterInformationEnName,
                    })
                    );

                sumitionofClass_FTPrenter_VM summition = new sumitionofClass_FTPrenter_VM();
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
                VM.ThisRenterData = ThisRenterData?.FirstOrDefault();
                VM.all_UsersData = all_UsersData;
                VM.summition = summition;
                VM.all_Recipts = all_Recipts;
                VM.all_lessors = all_lessors;


                return PartialView("_EditpartDataTableReportFTPrenter", VM);
            }
            listReportFTPrenterVM VM2 = new listReportFTPrenterVM();

            return PartialView("_EditpartDataTableReportFTPrenter", VM2);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            listReportFTPrenterVM VM = new listReportFTPrenterVM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasAccountReceiptRenterId == id.Trim(),
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
            var minDate = listmaxDate.Min(x => x.dates)?.ToString("yyyy-MM-dd");

            var end = DateTime.Now.AddDays(1);
            var start = DateTime.Now.AddMonths(-1);
            if (maxDate != null)
            {
                end = DateTime.Parse(maxDate).AddDays(1).Date;
                start = DateTime.Parse(maxDate).AddMonths(-1).Date;
            }

            var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptRenterId == id.Trim()
                && (x.CrCasAccountReceiptDate > start && x.CrCasAccountReceiptDate <= end)
                ,
                selectProjection: query => query.Select(x => new Recipt_ForRenter_VM
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

            var all_UsersData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationStatus != Status.Deleted,
                selectProjection: query => query.Select(x => new list_String_4
                {
                    id_key = x.CrMasUserInformationCode,
                    nameAr = x.CrMasUserInformationArName,
                    nameEn = x.CrMasUserInformationEnName,
                })
                );
            var ThisRenterData = await _unitOfWork.CrMasRenterInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasRenterInformationId == id,
                selectProjection: query => query.Select(x => new list_String_4
                {
                    id_key = x.CrMasRenterInformationId,
                    nameAr = x.CrMasRenterInformationArName,
                    nameEn = x.CrMasRenterInformationEnName,
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

            sumitionofClass_FTPrenter_VM summition = new sumitionofClass_FTPrenter_VM();
            foreach (var single in all_Recipts)
            {

                summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                summition.Creditor_Total += single.CrCasAccountReceiptPayment;

            }


            //if (all_UsersData.Count == 0 || ThisRenterData.Count == 0)
            //{
            //    _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            //    return RedirectToAction("Index", "ReportFTPrenter");
            //}
            if (all_Recipts.Count > 0)
            {
                all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
            }
            VM.UserId = id;
            VM.ThisRenterData = ThisRenterData?.FirstOrDefault();
            VM.all_UsersData = all_UsersData;
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
            return RedirectToAction("Index", "ReportFTPrenter");
        }


    }
}
