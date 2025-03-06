using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
//using Bnan.Inferastructure.Repository.CAS;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.BS;
using Bnan.Ui.ViewModels.CAS;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.Drawing;
using System.Globalization;
using System.Numerics;
namespace Bnan.Ui.Areas.CAS.Controllers.CasReports
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]
    public class DailyReportController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<DailyReportController> _localizer;
        private readonly string pageNumber = SubTasks.Daily_Report_Cas;


        public DailyReportController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<DailyReportController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _baseRepo = BaseRepo;
            _masBase = masBase;
            _toastNotification = toastNotification;
            _env = webHostEnvironment;
            _localizer = localizer;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            await SetPageTitleAsync(string.Empty, pageNumber);

            listDailyReportVM VM = new listDailyReportVM();

            await SetPageTitleAsync(Status.Update, pageNumber);

            var listmaxDate = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new Date_DailyReportVM
                    {
                        dates = x.CrCasAccountReceiptDate,
                    }));

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
                predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                && (x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302")
                && x.CrCasAccountReceiptDate > start && x.CrCasAccountReceiptDate <= end
                ,
                selectProjection: query => query.Select(x => new DailyReport_ReciptVM
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

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation", "CrCasAccountReceiptNavigation" }
                );

            var all_UsersData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrMasUserInformationLessor == user.CrMasUserInformationLessor,
                selectProjection: query => query.Select(x => new cas_userData
                {
                    id_key = x.CrMasUserInformationCode,
                    nameAr = x.CrMasUserInformationArName,
                    nameEn = x.CrMasUserInformationEnName,
                })
                );

            var all_BranchesData = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
            predicate: x => x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationStatus != Status.Deleted,
            selectProjection: query => query.Select(x => new cas_BranchData
            {
                id_key = x.CrCasBranchInformationCode,
                nameAr = x.CrCasBranchInformationArShortName,
                nameEn = x.CrCasBranchInformationEnShortName,
                balance = x.CrCasBranchInformationTotalBalance,
                availableBalance = x.CrCasBranchInformationAvailableBalance,
                reservedBalance = x.CrCasBranchInformationReservedBalance,
            })
            );

            var all_userIds_recipt = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                predicate: x => x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302",
                selectProjection: query => query.Select(x => new cas_list_String_2
                {
                    id_key = x.CrCasAccountReceiptUser,
                })
                );
            all_userIds_recipt.DistinctBy(x => x.id_key).ToList();
            all_UsersData = all_UsersData.Where(x => all_userIds_recipt.Any(y => y.id_key?.Trim() == x.id_key?.Trim())).ToList();

            sumitionofClass_DailyReport_VM summition = new sumitionofClass_DailyReport_VM();
            foreach (var single in all_Recipts)
            {
                summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                summition.Creditor_Total += single.CrCasAccountReceiptPayment;
            }

            foreach (var single2 in all_BranchesData)
            {

                summition.balance += single2.balance;
                summition.reservedBalance += single2.reservedBalance;
                summition.avilableBalance += single2.availableBalance;
            }


            if (all_UsersData.Count == 0)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "DailyReport");
            }
            if (all_Recipts.Count > 0)
            {
                all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
            }
            VM.UserId = user.CrMasUserInformationLessor;
            VM.all_UsersData = all_UsersData;
            VM.all_BranchesData = all_BranchesData;
            VM.summition = summition;
            VM.all_Recipts = all_Recipts;
            VM.start_Date = start.ToString("yyyy-MM-dd");
            VM.end_Date = end.AddDays(-1).ToString("yyyy-MM-dd");
            return View(VM);
        }

        [HttpGet]
        //[Route("/CAS/DailyReport/GetContractsByStatus")]
        public async Task<PartialViewResult> GetContractsByStatus(string status, string start, string end)
        {
            var user = await _userManager.GetUserAsync(User);

            listDailyReportVM VM = new listDailyReportVM();

            if (start == "undefined-undefined-") start = "";
            if (end == "undefined-undefined-") end = "";
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                start = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                end = DateTime.Now.ToString("dd-MM-yyyy");
            }
            if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                var start_Date = DateTime.Parse(start);
                var end_Date = DateTime.Parse(end).AddDays(1);
                VM.start_Date = start_Date.ToString("yyyy-MM-dd");
                VM.end_Date = end_Date.AddDays(-1).ToString("yyyy-MM-dd");

                await SetPageTitleAsync(Status.Update, pageNumber);
                var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                && x.CrCasAccountReceiptDate > start_Date && x.CrCasAccountReceiptDate <= end_Date
                ,
                selectProjection: query => query.Select(x => new DailyReport_ReciptVM
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

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation", "CrCasAccountReceiptNavigation" }
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

                var all_UsersData = await _unitOfWork.CrMasUserInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrMasUserInformationLessor == user.CrMasUserInformationLessor,
                    selectProjection: query => query.Select(x => new cas_userData
                    {
                        id_key = x.CrMasUserInformationCode,
                        nameAr = x.CrMasUserInformationArName,
                        nameEn = x.CrMasUserInformationEnName,
                    })
                    );

                var all_BranchesData = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationStatus != Status.Deleted,
                    selectProjection: query => query.Select(x => new cas_BranchData
                    {
                        id_key = x.CrCasBranchInformationCode,
                        nameAr = x.CrCasBranchInformationArShortName,
                        nameEn = x.CrCasBranchInformationEnShortName,
                        balance = x.CrCasBranchInformationTotalBalance,
                        availableBalance = x.CrCasBranchInformationAvailableBalance,
                        reservedBalance = x.CrCasBranchInformationReservedBalance,
                    })
                    );

                var all_userIds_recipt = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasAccountReceiptType == "301" || x.CrCasAccountReceiptType == "302",
                    selectProjection: query => query.Select(x => new cas_list_String_2
                    {
                        id_key = x.CrCasAccountReceiptUser,
                    })
                    );
                all_userIds_recipt.DistinctBy(x => x.id_key).ToList();
                all_UsersData = all_UsersData.Where(x => all_userIds_recipt.Any(y => y.id_key?.Trim() == x.id_key?.Trim())).ToList();

                sumitionofClass_DailyReport_VM summition = new sumitionofClass_DailyReport_VM();
                foreach (var single in all_Recipts)
                {
                    summition.Debitor_Total += single.CrCasAccountReceiptReceipt;
                    summition.Creditor_Total += single.CrCasAccountReceiptPayment;
                }

                foreach (var single2 in all_BranchesData)
                {

                    summition.balance += single2.balance;
                    summition.reservedBalance += single2.reservedBalance;
                    summition.avilableBalance += single2.availableBalance;
                }

                if (all_Recipts.Count > 0)
                {
                    all_Recipts = all_Recipts.OrderBy(x => x.CrCasAccountReceiptDate).ToList();
                }
                VM.UserId = user.CrMasUserInformationLessor;
                VM.all_UsersData = all_UsersData;
                VM.summition = summition;
                VM.all_Recipts = all_Recipts;


                return PartialView("_DataTableDailyReport", VM);
            }
            listDailyReportVM VM2 = new listDailyReportVM();

            return PartialView("_DataTableDailyReport", VM2);
        }




        [HttpPost]
        public async Task<IActionResult> createExcel_saveAs_Receipt_Action(string list_ids_string, string list_serials_string, string debits, string credits, string start, string end)
        {
            if (string.IsNullOrEmpty(list_ids_string) || string.IsNullOrEmpty(list_serials_string))
            {
                var result5 = new
                {
                    code = 4,
                };
                return Json(result5);
            }
            // تحويل السلسلة إلى قائمة باستخدام Split
            List<string> list_recipt_ids = new List<string>(list_ids_string.Split(','));
            List<string> list_serials = new List<string>(list_serials_string.Split(','));

            var id = "55";
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // تحويل list_recipt_ids إلى قائمة من ReceiptModel
                List<ReceiptModel> receipt_ids = list_recipt_ids.Select(id => new ReceiptModel { Id = id }).ToList();

                listDailyReportVM VM = new listDailyReportVM();

                await SetPageTitleAsync(Status.Update, pageNumber);

                var lessorImage = await _unitOfWork.CrMasLessorImage.FindAllAsNoTrackingAsync(x => x.CrMasLessorImageCode == user.CrMasUserInformationLessor);

                var Ar_pathFormoula = lessorImage?.FirstOrDefault()?.CrMasLessorImageArDailyReport?.Replace("~/", "./wwwroot/");
                var En_pathFormoula = lessorImage?.FirstOrDefault()?.CrMasLessorImageEnDailyReport?.Replace("~/", "./wwwroot/");

                var all_Recipts = await _unitOfWork.CrCasAccountReceipt.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrCasAccountReceiptLessorCode == user.CrMasUserInformationLessor
                ,
                selectProjection: query => query.Select(x => new DailyReport_ReciptVM
                {
                    CrCasAccountReceiptNo = x.CrCasAccountReceiptNo,
                    CrCasAccountReceiptType = x.CrCasAccountReceiptType,
                    CrCasAccountReceiptDate = x.CrCasAccountReceiptDate,
                    CrCasAccountReceiptLessorCode = x.CrCasAccountReceiptLessorCode,
                    CrCasAccountReceiptReferenceNo = x.CrCasAccountReceiptReferenceNo,
                    CrCasAccountReceiptPayment = x.CrCasAccountReceiptPayment,
                    CrCasAccountReceiptReceipt = x.CrCasAccountReceiptReceipt,
                    PaymentMethod_Ar = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodArName,
                    PaymentMethod_En = x.CrCasAccountReceiptPaymentMethodNavigation.CrMasSupAccountPaymentMethodEnName,
                    ReferanceType_Ar = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceArName,
                    ReferanceType_En = x.CrCasAccountReceiptReferenceTypeNavigation.CrMasSupAccountReceiptReferenceEnName,
                    Salespoint_Ar = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointArName,
                    Salespoint_En = x.CrCasAccountReceiptSalesPointNavigation.CrCasAccountSalesPointEnName,
                    branch_Ar = x.CrCasAccountReceiptNavigation.CrCasBranchInformationArShortName,
                    branch_En = x.CrCasAccountReceiptNavigation.CrCasBranchInformationEnShortName,

                })
                , includes: new string[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation", "CrCasAccountReceiptSalesPointNavigation", "CrCasAccountReceiptNavigation" }
                );

                var all_BranchesData = await _unitOfWork.CrCasBranchInformation.FindAllWithSelectAsNoTrackingAsync(
                    predicate: x => x.CrCasBranchInformationLessor == user.CrMasUserInformationLessor && x.CrCasBranchInformationStatus != Status.Deleted,
                    selectProjection: query => query.Select(x => new cas_BranchData
                    {
                        id_key = x.CrCasBranchInformationCode,
                        nameAr = x.CrCasBranchInformationArShortName,
                        nameEn = x.CrCasBranchInformationEnShortName,
                        balance = x.CrCasBranchInformationTotalBalance,
                        availableBalance = x.CrCasBranchInformationAvailableBalance,
                        reservedBalance = x.CrCasBranchInformationReservedBalance,
                    })
                    );

                all_Recipts = all_Recipts.Where(x => receipt_ids.Any(y => y.Id?.Trim() == x.CrCasAccountReceiptNo)).ToList();

                if (all_Recipts==null || all_Recipts?.Count < 1)
                {
                    var result4 = new
                    {
                        code = 4,
                    };
                    return Json(result4);
                }


                var renamedListAr = all_Recipts.Select(recipt => new
                {
                    //receipt_Serial = list_serials[all_Recipts.IndexOf(recipt)],
                    receipt_Serial = all_Recipts.IndexOf(recipt) + 1,
                    receipt_No = recipt.CrCasAccountReceiptNo,
                    //space = " ",
                    reciptDate = recipt.CrCasAccountReceiptDate?.ToString("dd-MM-yyyy"),
                    branch = recipt.branch_Ar,
                    reference = recipt.ReferanceType_Ar,
                    referenceNo = recipt.CrCasAccountReceiptReferenceNo,
                    salesPoint = recipt.Salespoint_Ar,
                    paymentMethod = recipt.PaymentMethod_Ar,
                    Debit = recipt.CrCasAccountReceiptReceipt,
                    Credit = recipt.CrCasAccountReceiptPayment,
                }).ToList();

                var renamedListEn = all_Recipts.Select(recipt => new
                {
                    //receipt_Serial = list_serials[all_Recipts.IndexOf(recipt)],
                    receipt_Serial = all_Recipts.IndexOf(recipt) + 1,
                    receipt_No = recipt.CrCasAccountReceiptNo,
                    //space = " ",
                    reciptDate = recipt.CrCasAccountReceiptDate?.ToString("dd-MM-yyyy"),
                    branch = recipt.branch_En,
                    reference = recipt.ReferanceType_En,
                    referenceNo = recipt.CrCasAccountReceiptReferenceNo,
                    salesPoint = recipt.Salespoint_En,
                    paymentMethod = recipt.PaymentMethod_En,
                    Debit = recipt.CrCasAccountReceiptReceipt,
                    Credit = recipt.CrCasAccountReceiptPayment,
                }).ToList();

                //////// Ar - En
                //var folderAr = "C:/ArDailyReport";
                //var folderEn = "C:/EnDailyReport";
                // العثور على آخر \ في المسار
                int lastBackslashIndexEn = En_pathFormoula.LastIndexOf('/');
                int lastBackslashIndexAr = Ar_pathFormoula.LastIndexOf('/');

                var folderAr = " ";
                var folderEn = " ";
                // إذا كان هناك \ في السلسلة، قم بقطع السلسلة حتى هذا الموضع
                if (lastBackslashIndexEn >= 0)
                {
                    folderEn = En_pathFormoula.Substring(0, lastBackslashIndexEn);
                }
                // إذا كان هناك \ في السلسلة، قم بقطع السلسلة حتى هذا الموضع
                if (lastBackslashIndexAr >= 0)
                {
                    folderAr = Ar_pathFormoula.Substring(0, lastBackslashIndexAr);
                }
                var newfileName = "DailyReportEn" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                string newPath = " ";
                //var folderSource = "C:/Users/HP/Desktop/Excels";



                if (CultureInfo.CurrentUICulture.Name == "en-US")
                {

                    if (En_pathFormoula?.Length < 2)
                    {
                        var result3 = new
                        {
                            code = 3,
                        };
                        return Json(result3);
                    }
                    if (!System.IO.File.Exists(En_pathFormoula))
                    {
                        //Console.WriteLine("الملف غير موجود.");
                        var result2 = new
                        {
                            code = 2,
                        };
                        return Json(result2);
                    }


                    //string originalFilePath_En = folderSource + "/" + "DailyReportFormula" + ".xlsx";
                    string originalFilePath_En = En_pathFormoula??" ";

                    // مسار النسخة الجديدة En
                    string newFilePath_En = folderEn + "/" + newfileName;
                    newPath = newFilePath_En;

                    // فتح الملف الأصلي
                    using (var workbook_En = new XLWorkbook(originalFilePath_En))
                    {
                        // الحصول على الورقة المطلوبة (أو إضافة ورقة جديدة إذا كانت غير موجودة)
                        var worksheet_En = workbook_En.Worksheets.Worksheet("DailyReport");

                        // أو إنشاء ورقة جديدة
                        if (worksheet_En == null)
                        {
                            worksheet_En = workbook_En.AddWorksheet("DailyReport");
                        }

                        // كتابة في خلايا محددة
                        worksheet_En.Cell("D9").Value = start;
                        worksheet_En.Cell("F9").Value = end;
                        worksheet_En.Cell("J9").Value = user.CrMasUserInformationEnName;
                        worksheet_En.Cell("D11").Value = debits;
                        worksheet_En.Cell("D12").Value = credits;

                        // تعيين البيانات إلى الورقة بدءًا من D15
                        worksheet_En.Cell("B15").InsertData(renamedListEn);



                        // حفظ التغييرات كملف جديد
                        workbook_En.SaveAs(newFilePath_En);
                    }
                }
                else
                {
                    if (Ar_pathFormoula?.Length < 2)
                    {
                        var result3 = new
                        {
                            code = 3,
                        };
                        return Json(result3);
                    }
                    if (!System.IO.File.Exists(Ar_pathFormoula))
                    {
                        //Console.WriteLine("الملف غير موجود.");
                        var result2 = new
                        {
                            code = 2,
                        };
                        return Json(result2);
                    }
                    newfileName = newfileName.Replace("En", "Ar");


                    // مسار الملف الأصلي old excel Ar
                    //string originalFilePath_Ar = folderSource + "/" + "DailyReportFormula" + ".xlsx";
                    string originalFilePath_Ar = Ar_pathFormoula??" ";

                    // مسار النسخة الجديدة Ar
                    string newFilePath_Ar = folderAr + "/" + newfileName;
                    newPath = newFilePath_Ar;

                    // فتح الملف الأصلي
                    using (var workbook_Ar = new XLWorkbook(originalFilePath_Ar))
                    {
                        // الحصول على الورقة المطلوبة (أو إضافة ورقة جديدة إذا كانت غير موجودة)
                        var worksheet = workbook_Ar.Worksheets.Worksheet("DailyReport");

                        // أو إنشاء ورقة جديدة
                        if (worksheet == null)
                        {
                            worksheet = workbook_Ar.AddWorksheet("DailyReport");
                        }

                        // كتابة في خلايا محددة
                        worksheet.Cell("D9").Value = start;
                        worksheet.Cell("F9").Value = end;
                        worksheet.Cell("J9").Value = user.CrMasUserInformationArName;
                        worksheet.Cell("D11").Value = debits;
                        worksheet.Cell("D12").Value = credits;

                        // تعيين البيانات إلى الورقة بدءًا من D15
                        worksheet.Cell("B15").InsertData(renamedListAr);



                        // حفظ التغييرات كملف جديد
                        workbook_Ar.SaveAs(newFilePath_Ar);
                    }
                }
                System.Threading.Thread.Sleep(100);  // تأخير 0.1 ثانية
                // استخدام Task.Run لحذف الملف في الخلفية بعد 1 دقائق
                Task.Run(async () =>
                {
                    await Task.Delay(60000); // 1 دقائق
                    DeleteFile(newPath); // حذف الملف بعد التأخير
                });
                try
                {
                    //newPath = "D:\\1 New BnanEG\\Bnan.Ui\\wwwroot\\images\\Company\\4004\\Support Images\\DailyReportAr20250227092618.xlsx";
                    //newPath = "/images/Company/4004/Support Images/DailyReportAr20250227092618.xlsx";
                    //// فتح الملف وتقديمه للتنزيل
                    //var fileBytes = System.IO.File.ReadAllBytes(newPath);
                    //var linkpathToDownload = File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", newfileName);

                    newPath = newPath.Replace("./wwwroot",""); 
                    var result = new
                    {
                        code = 1,
                        linkpath = newPath,
                        fileName = newfileName,


                    };
                    return Json(result);
                }
                catch (Exception ex)
                {
                    // التعامل مع الأخطاء إذا حدثت أثناء تحميل الملف
                    return Json(new { code = 0, message = "حدث خطأ أثناء تحميل الملف: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                var result1 = new
                {
                    code = 0,
                };
                return Json(result1);

            }
        }

        // API endpoint لحذف الملف بناءً على المسار الذي يتم إرساله
        private void DeleteFile(string filePath)
        {
            filePath = "./wwwroot" + filePath;
            // حذف الملف بعد 3 دقائق
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                Console.WriteLine($"File {filePath} has been deleted after 3 minutes.");
            }
            else
            {
                Console.WriteLine($"File {filePath} not found for deletion.");
            }
        }


        ////worksheet.Cell("D11").Style.NumberFormat.Format = "#,##0.00";  // تنسيق مع فواصل الآلاف و2 خانة عشري
        ////worksheet.Cell("D12").Style.NumberFormat.Format = "#,##0.00";  // تنسيق مع فواصل الآلاف و2 خانة عشري

        //// تعيين البيانات إلى الورقة بدءًا من B15
        //worksheet.Cell("B15").InsertData(renamedList);

        ////// ضبط محاذاة النص ليكون في المنتصف (أفقيًا وعموديًا) لأسماء الأعمدة
        //worksheet.Range("B15:L1014").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ////worksheet.Range("B10:G10").Style.Fill.BackgroundColor = XLColor.BabyBlue;
        //worksheet.Range("B15:L1014").Style.Font.FontColor = XLColor.Black;
        //worksheet.Range("B15:L1014").Style.Font.Bold = true;
        //worksheet.Range("B15:L1014").Style.Font.FontSize = 8;

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
            return RedirectToAction("Index", "DailyReport");
        }


    }
}
