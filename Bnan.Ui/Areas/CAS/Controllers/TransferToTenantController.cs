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
    public class TransferToTenantController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<TransferToTenantController> _localizer;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly ITransferToFromRenter _tranferToRenter;
        private readonly IConvertedText _convertedText;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TransferToTenantController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
             IMapper mapper, IUserService userService, IAccountBank accountBank,
             IUserLoginsService userLoginsService, IToastNotification toastNotification,
             IStringLocalizer<TransferToTenantController> localizer, IAdminstritiveProcedures adminstritiveProcedures, ITransferToFromRenter tranferToRenter, IWebHostEnvironment hostingEnvironment, IConvertedText convertedText) :
             base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _userLoginsService = userLoginsService;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _adminstritiveProcedures = adminstritiveProcedures;
            _tranferToRenter = tranferToRenter;
            _hostingEnvironment = hostingEnvironment;
            _convertedText = convertedText;
        }
        public async Task<IActionResult> Index()
        {
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204003", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "عرض بيانات", "View Informations", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "2";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            var RenterAvaiable = _unitOfWork.CrCasRenterLessor.FindAll(x => x.CrCasRenterLessorCode == lessorCode &&
                                                                          x.CrCasRenterLessorAvailableBalance > 0 &&
                                                                          x.CrCasRenterLessorStatus == Status.Active, new[] { "CrCasRenterLessorNavigation" }).ToList();
            if (RenterAvaiable?.Count() < 1)
            {
                return RedirectToAction("FailedMessageReport_NoData");
            }
            var Evaluation = _unitOfWork.CrMasSysEvaluation.FindAll(x => x.CrMasSysEvaluationsClassification == "1").ToList();

            TransferFromTenantVM transferFromTenantVM = new TransferFromTenantVM();
            transferFromTenantVM.renterLessor = RenterAvaiable;
            transferFromTenantVM.crMasSysEvaluation = Evaluation;

            return View(transferFromTenantVM);
        }
        public async Task<IActionResult> TransferTo(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "2";
            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            var titles = await setTitle("204", "2204003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تحويل", "Transfer", titles[3]);
            var Renter = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == id && x.CrCasRenterLessorCode == lessorCode, new[] {"CrCasRenterContractBasicCrCasRenterContractBasic5s",
                                                                                                                           "CrCasRenterLessorNavigation" });
            var RenterVM = _mapper.Map<RenterLessorVM>(Renter);
            var procedureCodeForReceipt = "302";
            RenterVM.AccountReceiptNo = await GetNextAccountReceiptNo(lessorCode, "100", procedureCodeForReceipt);
            var procedureCodeForAdministritive = "305";
            RenterVM.AdminstritiveNo = await GetNextAdministrativeNo(lessorCode, "100", procedureCodeForAdministritive);
            RenterVM.CrMasSysEvaluation = _unitOfWork.CrMasSysEvaluation.Find(x => x.CrMasSysEvaluationsCode == RenterVM.CrCasRenterLessorDealingMechanism);
            RenterVM.Banks = _unitOfWork.CrMasSupAccountBanks.FindAll(x => x.CrMasSupAccountBankStatus == Status.Active && x.CrMasSupAccountBankCode != "00").ToList();
            RenterVM.AccountBanks = _unitOfWork.CrCasAccountBank.FindAll(x => x.CrCasAccountBankStatus == Status.Active && x.CrCasAccountBankNo != "00" && x.CrCasAccountBankLessor == lessorCode).ToList();
            RenterVM.CrCasBranchInformation = _unitOfWork.CrCasBranchInformation.Find(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationCode == "100");
            RenterVM.RenterInformationIban = Renter.CrCasRenterLessorNavigation.CrMasRenterInformationIban;
            RenterVM.BankSelected = Renter.CrCasRenterLessorNavigation.CrMasRenterInformationBank;
            //RenterVM.Amount = Math.Abs((decimal)Renter.CrCasRenterLessorBalance).ToString();
            return View(RenterVM);
        }
        [HttpPost]
        public async Task<IActionResult> TransferTo(RenterLessorVM renterLessorVM, string AccountReceiptNo, string SavePdfReceipt)
        {

            var userLogin = await _userManager.GetUserAsync(User);
            var lessorCode = userLogin.CrMasUserInformationLessor;
            //save Tracing
            var (mainTask, subTask, system, currentUser) = await SetTrace("204", "2204003", "2");

            await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
            subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
            subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);


            var Renter = _unitOfWork.CrCasRenterLessor.Find(x => x.CrCasRenterLessorId == renterLessorVM.CrCasRenterLessorId && x.CrCasRenterLessorCode == lessorCode, new[] {"CrCasRenterContractBasicCrCasRenterContractBasic5s",
                                                                                                                           "CrCasRenterLessorNavigation" });
            var AddAdminstritive = await _tranferToRenter.SaveAdminstritiveTransferRenter(renterLessorVM.AdminstritiveNo, userLogin.CrMasUserInformationCode, "305", "30", lessorCode, Renter.CrCasRenterLessorId,
                                                                                            decimal.Parse(renterLessorVM.Amount, CultureInfo.InvariantCulture), 0, renterLessorVM.Reasons);

            SavePdfReceipt = FileExtensions.CleanAndCheckBase64StringPdf(SavePdfReceipt);
            if (!string.IsNullOrEmpty(SavePdfReceipt)) SavePdfReceipt = await FileExtensions.SavePdf(_hostingEnvironment, SavePdfReceipt, lessorCode, "100", AccountReceiptNo, "Receipt");
            var CheckAddReceipt = await _tranferToRenter.AddAccountReceiptTransferToRenter(AccountReceiptNo, AddAdminstritive.CrCasSysAdministrativeProceduresNo, Renter.CrCasRenterLessorId, userLogin.CrMasUserInformationCode, "302", "17", lessorCode,
                                                                                        renterLessorVM.FromBank, renterLessorVM.FromAccountBankSelected, renterLessorVM.Amount, "0", SavePdfReceipt,
                                                                                        renterLessorVM.Reasons);
            var CheckUpdateMasRenter = await _tranferToRenter.UpdateRenterInformation(Renter.CrCasRenterLessorId, renterLessorVM.RenterInformationIban, renterLessorVM.BankSelected);
            var CheckUpdateRenterLessor = await _tranferToRenter.UpdateCasRenterLessorTransferTo(Renter.CrCasRenterLessorId, lessorCode, renterLessorVM.Amount);
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

        public async Task<IActionResult> FailedMessageReport_NoData()
        {
            //sidebar Active
            ViewBag.id = "#sidebarAcount";
            ViewBag.no = "2";
            var titles = await setTitle("204", "2204003", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "", "", titles[3]);

            ViewBag.Data = "0";
            return View();

        }
    }
}
