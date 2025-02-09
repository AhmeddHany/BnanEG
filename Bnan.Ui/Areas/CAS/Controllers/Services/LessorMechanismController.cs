using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Repository;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.CAS.MecanismInputs;
using Bnan.Ui.ViewModels.MAS.UserValiditySystem;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;

namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class LessorMechanismController : BaseController
    {
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<LessorMechanismController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IUserLoginsService _userLoginsService ;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.LessorMechanizmCAS;


        public LessorMechanismController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAdminstritiveProcedures AdminstritiveProcedures, IToastNotification toastNotification, IUserLoginsService userLoginsService, IStringLocalizer<LessorMechanismController> localizer, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = AdminstritiveProcedures;
            _userLoginsService = userLoginsService;
            _baseRepo = baseRepo;
        }

        public async Task<IActionResult> Mechanism()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }

            var lessorMechanisms = _unitOfWork.CrCasLessorMechanism.FindAll(x => x.CrCasLessorMechanismCode == user.CrMasUserInformationLessor,
                new[] { "CrCasLessorMechanismCodeNavigation", "CrCasLessorMechanismProceduresNavigation" });
            var model = _mapper.Map<List<MechanismVM>>(lessorMechanisms);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditMechanism([FromBody] CheckAndInputsModels model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null && model == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            var lessorMechanisms = _unitOfWork.CrCasLessorMechanism.FindAll(x => x.CrCasLessorMechanismCode == currentUser.CrMasUserInformationLessor,
                new[] { "CrCasLessorMechanismCodeNavigation", "CrCasLessorMechanismProceduresNavigation" });
            if (lessorMechanisms == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }
            foreach (var item in model.ContractValiditions)
            {
                var id = item.id;
                var checkBox = item.valueCheckBox;
                var days = item.Days;
                var mechanism = lessorMechanisms.FirstOrDefault(x => x.CrCasLessorMechanismProcedures == id);
                if (mechanism != null)
                {
                    mechanism.CrCasLessorMechanismActivate = checkBox;
                    mechanism.CrCasLessorMechanismDaysAlertAboutExpire = int.Parse(days);
                    _unitOfWork.CrCasLessorMechanism.Update(mechanism);
                }
            }
            foreach (var item in model.CarsDocuments)
            {
                var id = item.id;
                var checkBox = item.valueCheckBox;
                var days = item.Days;
                var mechanism = lessorMechanisms.FirstOrDefault(x => x.CrCasLessorMechanismProcedures == id);
                if (mechanism != null)
                {
                    mechanism.CrCasLessorMechanismActivate = checkBox;
                    mechanism.CrCasLessorMechanismDaysAlertAboutExpire = int.Parse(days);
                    _unitOfWork.CrCasLessorMechanism.Update(mechanism);
                }
            }
            foreach (var item in model.CarsFixed)
            {
                var id = item.id;
                var checkBox = item.valueCheckBox;
                var days = item.Days;
                var km = item.Km;
                var mechanism = lessorMechanisms.FirstOrDefault(x => x.CrCasLessorMechanismProcedures == id);
                if (mechanism != null)
                {
                    mechanism.CrCasLessorMechanismActivate = checkBox;
                    mechanism.CrCasLessorMechanismDaysAlertAboutExpire = int.Parse(days);
                    mechanism.CrCasLessorMechanismKmAlertAboutExpire = int.Parse(km);
                    _unitOfWork.CrCasLessorMechanism.Update(mechanism);
                }
            }
            foreach (var item in model.Additional)
            {
                var id = item.id;
                var checkBox = item.valueCheckBox;
                var days = item.Days;
                var mechanism = lessorMechanisms.FirstOrDefault(x => x.CrCasLessorMechanismProcedures == id);
                if (mechanism != null)
                {
                    mechanism.CrCasLessorMechanismActivate = checkBox;
                    mechanism.CrCasLessorMechanismDaysAlertAboutExpire = int.Parse(days);
                    _unitOfWork.CrCasLessorMechanism.Update(mechanism);
                }
            }
            //await _unitOfWork.CompleteAsync();
            if (await _unitOfWork.CompleteAsync() > 0)
            {
                await SaveTracingForChange(Status.Update);
                await UpdateMechanism();
            }
            return Json(new { success = true });
        }
        public async Task<bool> UpdateMechanism()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var lessorCode = currentUser.CrMasUserInformationLessor;
            // Docments 
            var Documents = _unitOfWork.CrCasBranchDocument.FindAll(x => x.CrCasBranchDocumentsLessor == lessorCode && x.CrCasBranchDocumentsStatus == Status.Active);
            if (Documents != null)
            {
                foreach (var item in Documents)
                {
                    var AboutToExpire =  _unitOfWork.CrCasLessorMechanism.FindAsync(l => l.CrCasLessorMechanismCode == item.CrCasBranchDocumentsLessor
                                                                                     && l.CrCasLessorMechanismProcedures == item.CrCasBranchDocumentsProcedures
                                                                                     && l.CrCasLessorMechanismProceduresClassification == item.CrCasBranchDocumentsProceduresClassification).Result.CrCasLessorMechanismDaysAlertAboutExpire;
                    item.CrCasBranchDocumentsDateAboutToFinish = item.CrCasBranchDocumentsEndDate?.AddDays(-(double)AboutToExpire);
                    _unitOfWork.CrCasBranchDocument.Update(item);
                }
            }
            // CompanyContract 
            var CompanyContracts = _unitOfWork.CrMasContractCompany.FindAll(x => x.CrMasContractCompanyLessor == lessorCode && x.CrMasContractCompanyStatus == Status.Active);
            if (CompanyContracts != null)
            {
                foreach (var item in CompanyContracts)
                {
                    var AboutToExpire = _unitOfWork.CrCasLessorMechanism.FindAsync(l => l.CrCasLessorMechanismCode == item.CrMasContractCompanyLessor
                                                                                     && l.CrCasLessorMechanismProcedures == item.CrMasContractCompanyProcedures
                                                                                     && l.CrCasLessorMechanismProceduresClassification == item.CrMasContractCompanyProceduresClassification).Result.CrCasLessorMechanismDaysAlertAboutExpire;
                    item.CrMasContractCompanyAboutToExpire = item.CrMasContractCompanyEndDate?.AddDays(-(double)AboutToExpire);
                    _unitOfWork.CrMasContractCompany.Update(item);
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }
        private async Task SaveTracingForChange(string status)
        {


            var recordAr = "التنبيهات";
            var recordEn = "Alerts";
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
        public  IActionResult SuccessToast()
        {
           _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }
    }
}
