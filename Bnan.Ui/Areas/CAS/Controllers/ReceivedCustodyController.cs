﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class ReceivedCustodyController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<ReceivedCustodyController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly ICustody _Custody;
        private readonly IConvertedText _convertedText;
        private readonly IWebHostEnvironment _hostingEnvironment;



        public ReceivedCustodyController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
             IMapper mapper, IUserService userService, IAccountBank accountBank,
             IUserLoginsService userLoginsService, IToastNotification toastNotification,
             IStringLocalizer<ReceivedCustodyController> localizer, IAdminstritiveProcedures adminstritiveProcedures, ICustody custody, IConvertedText convertedText, IWebHostEnvironment hostingEnvironment) :
             base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
            _Custody = custody;
            _convertedText = convertedText;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204002", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "1";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var AllAdminstritives = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                       x.CrCasSysAdministrativeProceduresCode == "304" &&
                                                                                       x.CrCasSysAdministrativeProceduresTargeted != userLogin.CrMasUserInformationCode, new[] { "CrCasSysAdministrativeProceduresUserInsertNavigation" }).ToList();
            List<AdmintritiveCustodyVM> admintritives = new List<AdmintritiveCustodyVM>();
            foreach (var item in AllAdminstritives)
            {
                AdmintritiveCustodyVM newAdmins = new AdmintritiveCustodyVM();
                newAdmins.CrCasSysAdministrativeProceduresNo = item.CrCasSysAdministrativeProceduresNo;
                newAdmins.CrCasSysAdministrativeProceduresDate = item.CrCasSysAdministrativeProceduresDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDocStartDate = item.CrCasSysAdministrativeProceduresDocStartDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDocEndDate = item.CrCasSysAdministrativeProceduresDocEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                var AccountReceipt = await _unitOfWork.CrCasAccountReceipt.FindAsync(x => x.CrCasAccountReceiptPassingReference == item.CrCasSysAdministrativeProceduresNo, new[] { "CrCasAccountReceiptSalesPointNavigation" });
                newAdmins.SalesPointNavigation = _unitOfWork.CrCasAccountSalesPoint.Find(x => x.CrCasAccountSalesPointCode == item.CrCasSysAdministrativeProceduresDocNo);
                newAdmins.CrCasSysAdministrativeProceduresCreditor = item.CrCasSysAdministrativeProceduresCreditor?.ToString("N2", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDebit = item.CrCasSysAdministrativeProceduresDebit?.ToString("N2", CultureInfo.InvariantCulture);
                newAdmins.UserInsertNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == item.CrCasSysAdministrativeProceduresTargeted);
                newAdmins.UserReceviedNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == item.CrCasSysAdministrativeProceduresUserInsert);
                newAdmins.CrCasSysAdministrativeProceduresStatus = item.CrCasSysAdministrativeProceduresStatus;
                newAdmins.DatePassing = AccountReceipt?.CrCasAccountReceiptPassingDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresReasons = item.CrCasSysAdministrativeProceduresReasons;
                admintritives.Add(newAdmins);
            }
            var result = admintritives.Where(l => l.CrCasSysAdministrativeProceduresStatus == "I"); //UnderProcedure
            if (result.Count() > 0)
            {
                return View(result);
            }
            var result2 = admintritives.Where(l => l.CrCasSysAdministrativeProceduresStatus == "Q"); //Accepted

            if (result2.Count() > 0)
            {
                return View(result2);
            }
            var result3 = admintritives.Where(l => l.CrCasSysAdministrativeProceduresStatus == "Z"); //Rejected

            if (result3.Count() > 0)
            {
                return View(result3);
            }

            return View(admintritives);
        }


        [HttpGet]
        public async Task<PartialViewResult> GetCustodyStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "1";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;

            if (!string.IsNullOrEmpty(status))
            {
                var AdminstritiveAll = _unitOfWork.CrCasSysAdministrativeProcedure.FindAll(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                                x.CrCasSysAdministrativeProceduresCode == "304" &&
                                                                                                x.CrCasSysAdministrativeProceduresTargeted != userLogin.CrMasUserInformationCode, new[] { "CrCasSysAdministrativeProceduresUserInsertNavigation" }).ToList();
                List<AdmintritiveCustodyVM> admintritives = new List<AdmintritiveCustodyVM>();
                foreach (var item in AdminstritiveAll)
                {
                    AdmintritiveCustodyVM newAdmins = new AdmintritiveCustodyVM();
                    newAdmins.CrCasSysAdministrativeProceduresNo = item.CrCasSysAdministrativeProceduresNo;
                    newAdmins.CrCasSysAdministrativeProceduresDate = item.CrCasSysAdministrativeProceduresDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    newAdmins.CrCasSysAdministrativeProceduresDocStartDate = item.CrCasSysAdministrativeProceduresDocStartDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    newAdmins.CrCasSysAdministrativeProceduresDocEndDate = item.CrCasSysAdministrativeProceduresDocEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    var AccountReceipt = await _unitOfWork.CrCasAccountReceipt.FindAsync(x => x.CrCasAccountReceiptPassingReference == item.CrCasSysAdministrativeProceduresNo, new[] { "CrCasAccountReceiptSalesPointNavigation" });

                    newAdmins.SalesPointNavigation = _unitOfWork.CrCasAccountSalesPoint.Find(x => x.CrCasAccountSalesPointCode == item.CrCasSysAdministrativeProceduresDocNo);
                    newAdmins.CrCasSysAdministrativeProceduresCreditor = item.CrCasSysAdministrativeProceduresCreditor?.ToString("N2", CultureInfo.InvariantCulture);
                    newAdmins.CrCasSysAdministrativeProceduresDebit = item.CrCasSysAdministrativeProceduresDebit?.ToString("N2", CultureInfo.InvariantCulture);
                    newAdmins.UserInsertNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == item.CrCasSysAdministrativeProceduresTargeted);
                    newAdmins.UserReceviedNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == item.CrCasSysAdministrativeProceduresUserInsert);
                    newAdmins.CrCasSysAdministrativeProceduresStatus = item.CrCasSysAdministrativeProceduresStatus;
                    newAdmins.DatePassing = AccountReceipt?.CrCasAccountReceiptPassingDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                    newAdmins.CrCasSysAdministrativeProceduresReasons = item.CrCasSysAdministrativeProceduresReasons;
                    admintritives.Add(newAdmins);
                }



                if (status == Status.All) return PartialView("_CustodyData", admintritives);
                return PartialView("_CustodyData", admintritives.Where(l => l.CrCasSysAdministrativeProceduresStatus == status));
            }
            return PartialView();
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);




            AdmintritiveCustodyVM newAdmins = new AdmintritiveCustodyVM();
            var adminstritive = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAsync(x => x.CrCasSysAdministrativeProceduresNo == id);
            if (adminstritive != null)
            {
                var Receipts = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptPassingReference == adminstritive.CrCasSysAdministrativeProceduresNo,
                                                                          new[] { "CrCasAccountReceiptPaymentMethodNavigation", "CrCasAccountReceiptReferenceTypeNavigation" }).ToList();

                newAdmins.CrCasSysAdministrativeProceduresNo = adminstritive.CrCasSysAdministrativeProceduresNo;
                newAdmins.CrCasSysAdministrativeProceduresDate = adminstritive.CrCasSysAdministrativeProceduresDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDocStartDate = adminstritive.CrCasSysAdministrativeProceduresDocStartDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDocEndDate = adminstritive.CrCasSysAdministrativeProceduresDocEndDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                var AccountReceipt = await _unitOfWork.CrCasAccountReceipt.FindAsync(x => x.CrCasAccountReceiptPassingReference == adminstritive.CrCasSysAdministrativeProceduresNo, new[] { "CrCasAccountReceiptSalesPointNavigation" });

                newAdmins.SalesPointNavigation = _unitOfWork.CrCasAccountSalesPoint.Find(x => x.CrCasAccountSalesPointCode == adminstritive.CrCasSysAdministrativeProceduresDocNo);
                newAdmins.CrCasSysAdministrativeProceduresCreditor = adminstritive.CrCasSysAdministrativeProceduresCreditor?.ToString("N2", CultureInfo.InvariantCulture);
                newAdmins.CrCasSysAdministrativeProceduresDebit = adminstritive.CrCasSysAdministrativeProceduresDebit?.ToString("N2", CultureInfo.InvariantCulture);
                newAdmins.TotalAmount = (adminstritive.CrCasSysAdministrativeProceduresCreditor - adminstritive.CrCasSysAdministrativeProceduresDebit)?.ToString("N2", CultureInfo.InvariantCulture);
                newAdmins.UserInsertNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == adminstritive.CrCasSysAdministrativeProceduresTargeted);
                newAdmins.UserReceviedNavigation = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == adminstritive.CrCasSysAdministrativeProceduresUserInsert);
                newAdmins.BranchInformationNavigation = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationCode == adminstritive.CrCasSysAdministrativeProceduresBranch);
                newAdmins.CrCasSysAdministrativeProceduresStatus = adminstritive.CrCasSysAdministrativeProceduresStatus;
                newAdmins.CrCasSysAdministrativeProceduresReasons = adminstritive.CrCasSysAdministrativeProceduresReasons;
                newAdmins.AccountReceipt = Receipts;
                newAdmins.NewReceiptNo = _Custody.GetAccountReceiptNo(adminstritive.CrCasSysAdministrativeProceduresBranch, userLogin.CrMasUserInformationLessor);
                var paymentMethod = _unitOfWork.CrCasAccountSalesPoint.Find(x => x.CrCasAccountSalesPointCode == adminstritive.CrCasSysAdministrativeProceduresDocNo);
                if (paymentMethod.CrCasAccountSalesPointBank == "00")
                {
                    newAdmins.PaymentMethodAr = "نقدا";
                    newAdmins.PaymentMethodEn = "CASH";

                }
                else
                {
                    newAdmins.PaymentMethodAr = "نقاط بيع";
                    newAdmins.PaymentMethodEn = "Sales Point";
                }
                return View(newAdmins);
            }

            _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ActionCustody(AdmintritiveCustodyVM custodyVM, string status, string SavePdfReceipt, string AccountReceiptNo, string Reasons)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;

            var Adminstritive = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAsync(x => x.CrCasSysAdministrativeProceduresNo == custodyVM.CrCasSysAdministrativeProceduresNo);
            if (Adminstritive != null)
            {
                var CheckAdminstritive = true;
                CheckAdminstritive = await _Custody.UpdateAdminstritive(Adminstritive.CrCasSysAdministrativeProceduresNo, userLogin.CrMasUserInformationCode, status, Reasons);

                //save Tracing
                var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204002", "2");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);




                var CheckAddReceipt = true;
                var CheckUpdateBranch = true;
                var CheckUpdateSalesPoint = true;
                var CheckUpdateUserInfo = true;
                var CheckUpdateUserValidtyBrn = true;
                var CheckUpdateAccountReceipts = true;


                if (status == Status.Accept)
                {
                    SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
                    if (!string.IsNullOrEmpty(SavePdfReceipt)) SavePdfReceipt = await SavePdfAsync(SavePdfReceipt, lessorCode, Adminstritive.CrCasSysAdministrativeProceduresBranch, AccountReceiptNo, "Receipt");
                    CheckAddReceipt = await _Custody.AddAccountReceiptReceivedCustody(Adminstritive.CrCasSysAdministrativeProceduresNo,
                                                                                  lessorCode, Adminstritive.CrCasSysAdministrativeProceduresBranch,
                                                                                  custodyVM.TotalAmount, Adminstritive.CrCasSysAdministrativeProceduresTargeted, SavePdfReceipt, Reasons);
                }

                CheckUpdateAccountReceipts = _Custody.UpdateAccountReceiptReceivedCustody(Adminstritive.CrCasSysAdministrativeProceduresNo, userLogin.CrMasUserInformationCode, status, custodyVM.CrCasSysAdministrativeProceduresReasons);
                CheckUpdateBranch = await _Custody.UpdateBranchReceivedCustody(Adminstritive.CrCasSysAdministrativeProceduresBranch, lessorCode, custodyVM.TotalAmount, status);
                CheckUpdateSalesPoint = await _Custody.UpdateSalesPointReceivedCustody(lessorCode, Adminstritive.CrCasSysAdministrativeProceduresBranch, Adminstritive.CrCasSysAdministrativeProceduresNo, custodyVM.TotalAmount, status);
                CheckUpdateUserInfo = await _Custody.UpdateUserInfoReceivedCustody(Adminstritive.CrCasSysAdministrativeProceduresTargeted, lessorCode, custodyVM.TotalAmount, status);
                CheckUpdateUserValidtyBrn = await _Custody.UpdateBranchValidityReceivedCustody(Adminstritive.CrCasSysAdministrativeProceduresTargeted.Trim(), lessorCode, Adminstritive.CrCasSysAdministrativeProceduresBranch,
                                                                                               Adminstritive.CrCasSysAdministrativeProceduresNo, custodyVM.TotalAmount, status);



                if (CheckAdminstritive && CheckAddReceipt && CheckUpdateBranch &&
                    CheckUpdateSalesPoint && CheckUpdateUserInfo &&
                    CheckUpdateUserValidtyBrn) if (await _unitOfWork.CompleteAsync() > 1) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            }

            return RedirectToAction("Index");
        }
        private async Task<string> SavePdfAsync(string? savePdf, string lessorCode, string branchCode, string contractNo, string type)
        {
            if (!string.IsNullOrEmpty(savePdf))
            {
                return await FileExtensions.SavePdf(_hostingEnvironment, savePdf, lessorCode, branchCode, contractNo, type);
            }
            return string.Empty;
        }
        [HttpGet]
        public async Task<IActionResult> Get_ConvertedNumber_Action(string our_No)
        {

            var (ArConcatenate, EnConcatenate) = _convertedText.ConvertNumber(our_No, "Ar");

            var result = new
            {
                ar_concatenate = ArConcatenate,
                en_concatenate = EnConcatenate,
            };
            return Json(result);
        }
    }
}
