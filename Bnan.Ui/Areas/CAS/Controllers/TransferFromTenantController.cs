using AutoMapper;
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
using System.Globalization;
namespace Bnan.Ui.Areas.CAS.Controllers
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    public class TransferFromTenantController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<TransferFromTenantController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly ITransferToFromRenter _tranferToRenter;
        private readonly IConvertedText _convertedText;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public TransferFromTenantController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
             IMapper mapper, IUserService userService, IAccountBank accountBank,
             IUserLoginsService userLoginsService, IToastNotification toastNotification,
             IStringLocalizer<TransferFromTenantController> localizer, IAdminstritiveProcedures adminstritiveProcedures, ITransferToFromRenter tranferToRenter, IConvertedText convertedText, IWebHostEnvironment hostingEnvironment) :
             base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
            _tranferToRenter = tranferToRenter;
            _convertedText = convertedText;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204004", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "3";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204004", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterAvaiable = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == lessorCode, new[] { "CrCasRenterLessorNavigation" });

            var Evaluation = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

            TransferFromTenantVM transferFromTenantVM = new TransferFromTenantVM();
            transferFromTenantVM.renterLessor = RenterAvaiable;
            transferFromTenantVM.crMasSysEvaluation = Evaluation;

            return View(transferFromTenantVM);
        }

        [HttpGet]
        public async Task<IActionResult> TransferFrom(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "3";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204004", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تحويل", "Transfer", titles[3]);
            var Renter = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == id && x.CrCasRenterLessorCode == lessorCode, new[] {"CrCasRenterContractBasicCrCasRenterContractBasic5s",
                                                                                                                           "CrCasRenterLessorNavigation" });
            var RenterVM = _mapper.Map<RenterLessorVM>(Renter);
            var procedureCodeForReceipt = "301";
            RenterVM.AccountReceiptNo = await GetNextAccountReceiptNo(lessorCode, "100", procedureCodeForReceipt);
            var procedureCodeForAdministritive = "306";
            RenterVM.AdminstritiveNo = await GetNextAdministrativeNo(lessorCode, "100", procedureCodeForAdministritive);
            RenterVM.CrCasBranchInformation = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationCode == "100");
            RenterVM.CrMasSysEvaluation = _unitOfWork.CrMasSysEvaluation.Find(x => x.CrMasSysEvaluationsCode == RenterVM.CrCasRenterLessorDealingMechanism);
            RenterVM.Banks = _unitOfWork.CrMasSupAccountBanks.FindAll(x => x.CrMasSupAccountBankStatus == Status.Active && x.CrMasSupAccountBankCode != "00").ToList();
            RenterVM.AccountBanks = _unitOfWork.CrCasAccountBank.FindAll(x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankNo != "00" && x.CrCasAccountBankLessor == lessorCode).ToList();
            RenterVM.RenterInformationIban = Renter.CrCasRenterLessorNavigation.CrMasRenterInformationIban;
            RenterVM.BankSelected = Renter.CrCasRenterLessorNavigation.CrMasRenterInformationBank;
            return View(RenterVM);
        }
        [HttpPost]
        public async Task<IActionResult> TransferFrom(RenterLessorVM renterLessorVM, string AccountReceiptNo, string SavePdfReceipt)
        {

            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204004", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var Renter = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == renterLessorVM.CrCasRenterLessorId && x.CrCasRenterLessorCode == lessorCode, new[] {"CrCasRenterContractBasicCrCasRenterContractBasic5s",
                                                                                                                           "CrCasRenterLessorNavigation" });
            var AddAdminstritive = await _tranferToRenter.SaveAdminstritiveTransferRenter(renterLessorVM.AdminstritiveNo, userLogin.CrMasUserInformationCode, "306", "30", lessorCode, Renter.CrCasRenterLessorId,
                                                                                   0, decimal.Parse(renterLessorVM.Amount, CultureInfo.InvariantCulture), renterLessorVM.Reasons);

            SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
            if (!string.IsNullOrEmpty(SavePdfReceipt)) SavePdfReceipt = await FileExtensions.SavePdf(_hostingEnvironment, SavePdfReceipt, lessorCode, "100", AccountReceiptNo, "Receipt");
            var CheckAddReceipt = await _tranferToRenter.AddAccountReceiptTransferToRenter(AccountReceiptNo, AddAdminstritive.CrCasSysAdministrativeProceduresNo, Renter.CrCasRenterLessorId, userLogin.CrMasUserInformationCode, "301", "16", lessorCode,
                                                                                        renterLessorVM.FromBank, renterLessorVM.FromAccountBankSelected, "0", renterLessorVM.Amount, SavePdfReceipt, renterLessorVM.Reasons);

            var CheckUpdateMasRenter = await _tranferToRenter.UpdateRenterInformation(Renter.CrCasRenterLessorId, renterLessorVM.RenterInformationIban, renterLessorVM.BankSelected);
            var CheckUpdateRenterLessor = await _tranferToRenter.UpdateCasRenterLessorTransferFrom(Renter.CrCasRenterLessorId, lessorCode, renterLessorVM.Amount);


            if (AddAdminstritive != null && CheckAddReceipt && CheckUpdateMasRenter && CheckUpdateRenterLessor)
            {
                if (await _unitOfWork.CompleteAsync() > 1) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                else _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> GetAccountBankNo(string AccountNo)
        {
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var account = await _unitOfWork.CrCasAccountBank.FindAsync(x => x.CrCasAccountBankCode == AccountNo && x.CrCasAccountBankLessor == lessorCode, new[] { "CrCasAccountBankNoNavigation" });
            var result = new
            {
                accountNo = account.CrCasAccountBankCode,
                accountIban = account.CrCasAccountBankIban,
                bankNo = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankCode,
                arBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankArName,
                enBank = account.CrCasAccountBankNoNavigation?.CrMasSupAccountBankEnName,
            };
            return Json(result);
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
