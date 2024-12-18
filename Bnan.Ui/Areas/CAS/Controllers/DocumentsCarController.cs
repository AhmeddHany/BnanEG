﻿using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class DocumentsCarController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserService _UserService;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IDocumentsMaintainanceCar _documentsMaintainance;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<DocumentsCarController> _localizer;



        public DocumentsCarController(UserManager<CrMasUserInformation> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IWebHostEnvironment webHostEnvironment,
            IUserLoginsService userLoginsService,
            IAdminstritiveProcedures adminstritiveProcedures,
            IDocumentsMaintainanceCar documentsMaintainance,
            IToastNotification toastNotification,
            IStringLocalizer<DocumentsCarController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _UserService = userService;
            _webHostEnvironment = webHostEnvironment;
            _userLoginsService = userLoginsService;
            _adminstritiveProcedures = adminstritiveProcedures;
            _documentsMaintainance = documentsMaintainance;
            _toastNotification = toastNotification;
            _localizer = localizer;
        }
        [HttpGet]
        public async Task<ActionResult> DocumentsCar()
        {
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("202", "2202002", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            //sidebar Active
            ViewBag.id = "#sidebarcars";
            ViewBag.no = "1";

            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("202", "2202002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);
            var Docs = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(l => l.CrCasCarDocumentsMaintenanceLessor == lessorCode && (l.CrCasCarDocumentsMaintenanceStatus == Status.Renewed || l.CrCasCarDocumentsMaintenanceStatus == Status.AboutToExpire || l.CrCasCarDocumentsMaintenanceStatus == Status.Expire) && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Deleted && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Sold && l.CrCasCarDocumentsMaintenanceProceduresClassification == "12", new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarDocumentsMaintenanceSerailNoNavigation" });
            if (Docs.Count() == 0)
            {
                var Docs_active = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(l => l.CrCasCarDocumentsMaintenanceLessor == lessorCode && l.CrCasCarDocumentsMaintenanceStatus == Status.Active && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Deleted && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Sold && l.CrCasCarDocumentsMaintenanceProceduresClassification == "12", new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarDocumentsMaintenanceSerailNoNavigation" });
                return View(Docs_active);
            }
            return View(Docs);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetDocumentsCarByStatus(string status)
        {
            //sidebar Active
            ViewBag.id = "#sidebarcars";
            ViewBag.no = "1";

            var lessor = await _UserService.GetUserLessor(User);
            if (!string.IsNullOrEmpty(status))
            {
                if (status == Status.All)
                {
                    var DocumentsCarbyStatusAll = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(l => l.CrCasCarDocumentsMaintenanceLessor == lessor.CrMasUserInformationLessor && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Deleted && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Sold && l.CrCasCarDocumentsMaintenanceProceduresClassification == "12", new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarDocumentsMaintenanceSerailNoNavigation" });
                    return PartialView("_DataTableDocsCar", DocumentsCarbyStatusAll);
                }
                var DocumentbyStatus = _unitOfWork.CrCasCarDocumentsMaintenance.FindAll(l => l.CrCasCarDocumentsMaintenanceStatus == status && l.CrCasCarDocumentsMaintenanceLessor == lessor.CrMasUserInformationLessor && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Deleted && l.CrCasCarDocumentsMaintenanceCarStatus != Status.Sold && l.CrCasCarDocumentsMaintenanceProceduresClassification == "12", new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarDocumentsMaintenanceSerailNoNavigation" });
                return PartialView("_DataTableDocsCar", DocumentbyStatus);
            }
            return PartialView();
        }
        [HttpGet]
        public async Task<IActionResult> Edit([FromQuery] string Procedureid, [FromQuery] string SerialNumber)
        {
            //sidebar Active
            ViewBag.id = "#sidebarcars";
            ViewBag.no = "1";

            // Set Title
            var titles = await setTitle("202", "2202002", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            var user = await _UserService.GetUserLessor(User);
            var DocumentCar = _unitOfWork.CrCasCarDocumentsMaintenance.Find(l => l.CrCasCarDocumentsMaintenanceLessor == user.CrMasUserInformationLessor && l.CrCasCarDocumentsMaintenanceSerailNo == SerialNumber && l.CrCasCarDocumentsMaintenanceProcedures == Procedureid, new[] { "CrCasCarDocumentsMaintenanceProceduresNavigation", "CrCasCarDocumentsMaintenanceSerailNoNavigation" });
            ViewBag.date = DocumentCar.CrCasCarDocumentsMaintenanceDate?.ToString("dd/MM/yyyy");
            ViewBag.startDate = DocumentCar.CrCasCarDocumentsMaintenanceStartDate?.ToString("dd/MM/yyyy");
            ViewBag.endDate = DocumentCar.CrCasCarDocumentsMaintenanceEndDate?.ToString("dd/MM/yyyy");

            var DocumentsCarVM = _mapper.Map<DocumentsMaintainceCarVM>(DocumentCar);
            if (DocumentsCarVM.CrCasCarDocumentsMaintenanceStatus == Status.Expire || DocumentsCarVM.CrCasCarDocumentsMaintenanceStatus == Status.Renewed)
            {
                ViewBag.date = null;
                ViewBag.startDate = null;
                ViewBag.endDate = null;
                DocumentsCarVM.CrCasCarDocumentsMaintenanceNo = null;
                DocumentsCarVM.CrCasCarDocumentsMaintenanceDate = null;
                DocumentsCarVM.CrCasCarDocumentsMaintenanceStartDate = null;
                DocumentsCarVM.CrCasCarDocumentsMaintenanceEndDate = null;
                DocumentsCarVM.CrCasCarDocumentsMaintenanceImage = null;
            }
            return View(DocumentsCarVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DocumentsMaintainceCarVM documentsMaintainceCarVM, IFormFile? DoucmentImg)
        {
            if (ModelState.IsValid)
            {
                var documentsMaintainceCar = _mapper.Map<CrCasCarDocumentsMaintenance>(documentsMaintainceCarVM);
                string foldername = $"{"images\\Company"}\\{documentsMaintainceCarVM.CrCasCarDocumentsMaintenanceLessor}\\{"Cars"}\\{documentsMaintainceCar.CrCasCarDocumentsMaintenanceSerailNo}";
                string filePathImage;
                if (DoucmentImg != null)
                {
                    var sysProcedure = _unitOfWork.CrMasSysProcedure.Find(x => x.CrMasSysProceduresClassification == "12" && x.CrMasSysProceduresCode == documentsMaintainceCarVM.CrCasCarDocumentsMaintenanceProcedures);
                    string fileNameImg = sysProcedure.CrMasSysProceduresCode;
                    filePathImage = await DoucmentImg.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                    documentsMaintainceCar.CrCasCarDocumentsMaintenanceImage = filePathImage;
                }
                else if (documentsMaintainceCarVM.CrCasCarDocumentsMaintenanceImage != null)
                {
                    documentsMaintainceCar.CrCasCarDocumentsMaintenanceImage = documentsMaintainceCar.CrCasCarDocumentsMaintenanceImage;
                }
                else
                {
                    documentsMaintainceCar.CrCasCarDocumentsMaintenanceImage = null;
                }
                if (await _documentsMaintainance.UpdateDocumentCar(documentsMaintainceCar) &&
                    await _documentsMaintainance.CheckMaintainceAndDocsCar(documentsMaintainceCar.CrCasCarDocumentsMaintenanceSerailNo, documentsMaintainceCar.CrCasCarDocumentsMaintenanceLessor, "12", documentsMaintainceCar.CrCasCarDocumentsMaintenanceProcedures))
                {
                    await _unitOfWork.CompleteAsync();
                    //SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("202", "2202002", "2");
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "212", "20", currentUser.CrMasUserInformationLessor, "100",
                    documentsMaintainceCar.CrCasCarDocumentsMaintenanceProcedures, null, null, documentsMaintainceCar.CrCasCarDocumentsMaintenanceLessor, documentsMaintainceCar.CrCasCarDocumentsMaintenanceDate, documentsMaintainceCar.CrCasCarDocumentsMaintenanceStartDate, documentsMaintainceCar.CrCasCarDocumentsMaintenanceEndDate,
                    null, null, "اضافة", "Insert", "I", null);
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("DocumentsCar", "DocumentsCar");

                }
            }
            return View(documentsMaintainceCarVM);
        }


        [HttpDelete]
        public async Task<bool> EditDocumentStatus(string DocumentCarLessor, string DocumentCarBranch, string DocumentCarProcedures, string SerialNumber, string status)
        {
            string sAr = "";
            string sEn = "";
            var lessor = await _unitOfWork.CrMasLessorInformation.GetByIdAsync(DocumentCarLessor);
            var CarDocument = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAsync(x => x.CrCasCarDocumentsMaintenanceLessor == DocumentCarLessor &&
                                                                                        x.CrCasCarDocumentsMaintenanceBranch == DocumentCarBranch &&
                                                                                        x.CrCasCarDocumentsMaintenanceProcedures == DocumentCarProcedures &&
                                                                                        x.CrCasCarDocumentsMaintenanceSerailNo == SerialNumber);
            var car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationLessor == lessor.CrMasLessorInformationCode &&
                                                              x.CrCasCarInformationBranch == CarDocument.CrCasCarDocumentsMaintenanceBranch &&
                                                              x.CrCasCarInformationSerailNo == CarDocument.CrCasCarDocumentsMaintenanceSerailNo);
            if (lessor != null)
            {
                if (await CheckUserSubValidationProcdures("2202002", status))
                {
                    if (status == Status.Deleted)
                    {
                        string foldername = $"{"images\\Company"}\\{CarDocument.CrCasCarDocumentsMaintenanceLessor}\\{"Cars"}\\{CarDocument.CrCasCarDocumentsMaintenanceSerailNo}";
                        var sysProcedure = _unitOfWork.CrMasSysProcedure.Find(x => x.CrMasSysProceduresClassification == "12" && x.CrMasSysProceduresCode == CarDocument.CrCasCarDocumentsMaintenanceProcedures);
                        string fileNameImg = sysProcedure.CrMasSysProceduresEnName;
                        sAr = "حذف";
                        sEn = "Remove";
                        CarDocument.CrCasCarDocumentsMaintenanceStatus = Status.Renewed;
                        CarDocument.CrCasCarDocumentsMaintenanceDate = null;
                        CarDocument.CrCasCarDocumentsMaintenanceStartDate = null;
                        CarDocument.CrCasCarDocumentsMaintenanceEndDate = null;
                        CarDocument.CrCasCarDocumentsMaintenanceDateAboutToFinish = null;
                        CarDocument.CrCasCarDocumentsMaintenanceImage = null;
                        CarDocument.CrCasCarDocumentsMaintenanceNo = null;
                        _unitOfWork.CrCasCarDocumentsMaintenance.Update(CarDocument);
                        car.CrCasCarInformationDocumentationStatus = false;
                        _unitOfWork.CrCasCarInformation.Update(car);
                        await _unitOfWork.CompleteAsync();
                        await FileExtensions.RemoveFile(_webHostEnvironment, foldername, fileNameImg, ".png");

                    }

                    // SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("202", "2202002", "2");
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "212", "20", currentUser.CrMasUserInformationLessor, "100",
                        CarDocument.CrCasCarDocumentsMaintenanceProcedures, null, null, CarDocument.CrCasCarDocumentsMaintenanceSerailNo, CarDocument.CrCasCarDocumentsMaintenanceDate, CarDocument.CrCasCarDocumentsMaintenanceStartDate, CarDocument.CrCasCarDocumentsMaintenanceEndDate,
                        null, null, sAr, sEn, "U", null);

                    return true;
                }
            }

            return false;

        }
        public IActionResult SuccessMessage()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("DocumentsCar", "DocumentsCar");
        }
    }
}
