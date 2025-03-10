﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.BS.Controllers
{
    [Area("BS")]
    [Authorize(Roles = "BS")]
    public class FeedBoxController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IFeedBoxBS _feedBox;
        private readonly IStringLocalizer<FeedBoxController> _localizer;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string pageNumber = SubTasks.FeedBoxBranches;

        public FeedBoxController(IStringLocalizer<FeedBoxController> localizer, IUnitOfWork unitOfWork, UserManager<CrMasUserInformation> userManager, IMapper mapper, IToastNotification toastNotification, IFeedBoxBS feedBox, IWebHostEnvironment hostingEnvironment) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
            _toastNotification = toastNotification;
            _feedBox = feedBox;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            //To Set Title 
            var userLogin = await _userManager.GetUserAsync(User);
            if (userLogin == null) return RedirectToAction("Login", "Account");
            await SetPageTitleAsync(string.Empty, pageNumber);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var bSLayoutVM = await GetBranchesAndLayout();


            var adminstrive = _unitOfWork.CrCasSysAdministrativeProcedure.Find(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                 x.CrCasSysAdministrativeProceduresTargeted == userLogin.CrMasUserInformationCode &&
                                                                                 x.CrCasSysAdministrativeProceduresCode == "303" &&
                                                                                 x.CrCasSysAdministrativeProceduresStatus == Status.Insert);
            //Get ACcount Receipt
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var autoinc1 = GetFeedBoxAccountReceipt(lessorCode, bSLayoutVM.SelectedBranch, "301").CrCasAccountReceiptNo;
            var AccountReceiptNo = y + "-" + "1" + "301" + "-" + lessorCode + bSLayoutVM.SelectedBranch + "-" + autoinc1;
            ViewBag.AccountReceiptNo = AccountReceiptNo;
            ViewBag.ReferenceNo = adminstrive.CrCasSysAdministrativeProceduresNo;

            bSLayoutVM.CrCasSysAdministrativeProcedure = adminstrive;
            return View(bSLayoutVM);
        }
        [HttpPost]
        public async Task<IActionResult> AcceptOrNot(string AdministrativeNo, string status, string branch, string reasons, string AccountReceiptNo, string SavePdfReceipt)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var adminstrive = _unitOfWork.CrCasSysAdministrativeProcedure.Find(x => x.CrCasSysAdministrativeProceduresLessor == lessorCode &&
                                                                                 x.CrCasSysAdministrativeProceduresTargeted == userLogin.CrMasUserInformationCode &&
                                                                                 x.CrCasSysAdministrativeProceduresCode == "303" &&
                                                                                 x.CrCasSysAdministrativeProceduresStatus == Status.Insert);

            var CheckUpdateAdminstrive = true;
            var CheckAddReceipt = true;
            var CheckUpdateUser = true;
            var CheckUpdateBranch = true;
            var CheckUpdateSalesPoint = true;
            var CheckUpdateBranchValidity = true;
            CheckUpdateAdminstrive = await _feedBox.UpdateAdminstritive(adminstrive.CrCasSysAdministrativeProceduresNo, userLogin.CrMasUserInformationCode, status, reasons);
            if (status == Status.Accept)
            {
                SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
                if (!string.IsNullOrEmpty(SavePdfReceipt)) SavePdfReceipt = await FileExtensions.SavePdf(_hostingEnvironment, SavePdfReceipt, lessorCode, branch, AccountReceiptNo, "Receipt");
                CheckAddReceipt = await _feedBox.AddAccountReceipt(adminstrive.CrCasSysAdministrativeProceduresNo, lessorCode, userLogin.CrMasUserInformationCode,
                                                                   userLogin.CrMasUserInformationDefaultBranch, (decimal)adminstrive.CrCasSysAdministrativeProceduresDebit, reasons, SavePdfReceipt);

                CheckUpdateUser = await _feedBox.UpdateUserInfo(userLogin.CrMasUserInformationCode, lessorCode, (decimal)adminstrive.CrCasSysAdministrativeProceduresDebit);


                CheckUpdateBranch = await _feedBox.UpdateBranch(userLogin.CrMasUserInformationDefaultBranch, lessorCode, (decimal)adminstrive.CrCasSysAdministrativeProceduresDebit);

                CheckUpdateSalesPoint = await _feedBox.UpdateSalesPoint(lessorCode, userLogin.CrMasUserInformationDefaultBranch, (decimal)adminstrive.CrCasSysAdministrativeProceduresDebit);

                CheckUpdateBranchValidity = await _feedBox.UpdateBranchValidity(userLogin.CrMasUserInformationCode, lessorCode, userLogin.CrMasUserInformationDefaultBranch, (decimal)adminstrive.CrCasSysAdministrativeProceduresDebit);
            }
            if (CheckUpdateAdminstrive && CheckAddReceipt && CheckUpdateBranch &&
                   CheckUpdateSalesPoint && CheckUpdateUser && CheckUpdateBranchValidity)
            {
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            }
            else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });

            return RedirectToAction("Index", "Home");

        }
        private CrCasAccountReceipt GetFeedBoxAccountReceipt(string LessorCode, string BranchCode, string procedure)
        {
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                                                                       x.CrCasAccountReceiptYear == y && x.CrCasAccountReceiptBranchCode == BranchCode && x.CrCasAccountReceiptType == procedure)
                                                             .Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));

            CrCasAccountReceipt c = new CrCasAccountReceipt();
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                c.CrCasAccountReceiptNo = val.ToString("000000");
            }
            else
            {
                c.CrCasAccountReceiptNo = "000001";
            }

            return c;
        }

        public IActionResult SuccesssMessageForFeedBox()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }
    }
}
