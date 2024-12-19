using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NToastNotify;
using System.Numerics;
namespace Bnan.Ui.Areas.MAS.Controllers.Services
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    [ServiceFilter(typeof(SetCurrentPathMASFilter))]
    public class QuestionsAnswerController : BaseController
    {
        private readonly IUserLoginsService _userLoginsService;
        private readonly IUserService _userService;
        private readonly IMasQuestionsAnswer _masQuestionsAnswer;
        private readonly IBaseRepo _baseRepo;
        private readonly IMasBase _masBase;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<QuestionsAnswerController> _localizer;
        private readonly string pageNumber = SubTasks.CrMasSysQuestionsAnswer;


        public QuestionsAnswerController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork,
            IMapper mapper, IUserService userService, IMasQuestionsAnswer masQuestionsAnswer, IBaseRepo BaseRepo, IMasBase masBase,
            IUserLoginsService userLoginsService, IToastNotification toastNotification, IWebHostEnvironment webHostEnvironment, IStringLocalizer<QuestionsAnswerController> localizer) : base(userManager, unitOfWork, mapper)
        {
            _userService = userService;
            _masQuestionsAnswer = masQuestionsAnswer;
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

            var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);


            // Retrieve active driving licenses
            var questionsAnswers = await _unitOfWork.CrMasSysQuestionsAnswer
                .FindAllAsNoTrackingAsync(x => x.CrMasSysQuestionsAnswerStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!questionsAnswers.Any())
            {
                questionsAnswers = await _unitOfWork.CrMasSysQuestionsAnswer
                    .FindAllAsNoTrackingAsync(x => x.CrMasSysQuestionsAnswerStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            QuestionsAnswerVM VM = new QuestionsAnswerVM();
            VM.allMainTaskSystem = allMainTaskSystem;
            VM.crMasSysQuestionsAnswer = questionsAnswers.OrderBy(x => x.CrMasSysQuestionsAnswerNo).ToList();
            VM.allsystemNames = allsystemName;
            return View(VM);
        }
        [HttpGet]
        public async Task<PartialViewResult> GetQuestionsAnswerByStatus(string status, string search)
        {
            //sidebar Active

            if (!string.IsNullOrEmpty(status))
            {
                var QuestionsAnswersAll = await _unitOfWork.CrMasSysQuestionsAnswer.FindAllAsNoTrackingAsync(x => x.CrMasSysQuestionsAnswerStatus == Status.Active ||
                                                                                                                            x.CrMasSysQuestionsAnswerStatus == Status.Deleted ||
                                                                                                                            x.CrMasSysQuestionsAnswerStatus == Status.Hold);
                var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                    //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                    );
                var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);

                QuestionsAnswerVM VM = new QuestionsAnswerVM();
                VM.allMainTaskSystem = allMainTaskSystem;
                VM.allsystemNames = allsystemName;
                if (status == Status.All)
                {
                    var FilterAll = QuestionsAnswersAll.FindAll(x => x.CrMasSysQuestionsAnswerStatus != Status.Deleted &&
                                                                         (x.CrMasSysQuestionsAnswerArQuestions.Contains(search) ||
                                                                          x.CrMasSysQuestionsAnswerEnQuestions.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasSysQuestionsAnswerNo.Contains(search)));
                    VM.crMasSysQuestionsAnswer = FilterAll.OrderBy(x => x.CrMasSysQuestionsAnswerNo).ToList();
                    return PartialView("_DataTableQuestionsAnswer", VM);
                }
                var FilterByStatus = QuestionsAnswersAll.FindAll(x => x.CrMasSysQuestionsAnswerStatus == status &&
                                                                            (
                                                                           x.CrMasSysQuestionsAnswerArQuestions.Contains(search) ||
                                                                           x.CrMasSysQuestionsAnswerEnQuestions.ToLower().Contains(search.ToLower()) ||
                                                                           x.CrMasSysQuestionsAnswerNo.Contains(search)));
                VM.crMasSysQuestionsAnswer = FilterByStatus.OrderBy(x => x.CrMasSysQuestionsAnswerNo).ToList();
                return PartialView("_DataTableQuestionsAnswer", VM);
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> AddQuestionsAnswer()
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "QuestionsAnswer");
            }
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Insert))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "QuestionsAnswer");
            }

            // Set Title
            var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);


            QuestionsAnswerVM questionsAnswerVM = new QuestionsAnswerVM();
            questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
            questionsAnswerVM.allsystemNames = allsystemName;
            //questionsAnswerVM.CrMasSysQuestionsAnswerNo = await GenerateLicenseCodeAsync();
            return View(questionsAnswerVM);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestionsAnswer(QuestionsAnswerVM questionsAnswerVM)
        {


            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);

            if (!ModelState.IsValid || questionsAnswerVM == null)
            {
                questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
                questionsAnswerVM.allsystemNames = allsystemName;
                return View("AddQuestionsAnswer", questionsAnswerVM);
            }
            try
            {
                //// Check If code > 9 get error , because code is char(1)
                if (questionsAnswerVM.CrMasSysQuestionsAnswerNo.Length != 6 || questionsAnswerVM.CrMasSysQuestionsAnswerNo.Length == 6 && questionsAnswerVM.CrMasSysQuestionsAnswerNo.Substring(4, 2) == "99")
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_AddMore"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
                    questionsAnswerVM.allsystemNames = allsystemName;
                    return View("AddQuestionsAnswer", questionsAnswerVM);
                }
                // Map ViewModel to Entity
                var questionsAnswerEntity = _mapper.Map<CrMasSysQuestionsAnswer>(questionsAnswerVM);

                // Generate and set the Driving License Code
                //questionsAnswerVM.CrMasSysQuestionsAnswerNo = await GenerateLicenseCodeAsync();
                // Set status and add the record
                questionsAnswerEntity.CrMasSysQuestionsAnswerStatus = "A";
                await _unitOfWork.CrMasSysQuestionsAnswer.AddAsync(questionsAnswerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي


                await SaveTracingForLicenseChange(user, questionsAnswerEntity, Status.Insert);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
                questionsAnswerVM.allsystemNames = allsystemName;
                return View("AddQuestionsAnswer", questionsAnswerVM);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            await SetPageTitleAsync(Status.Update, pageNumber);
            // if value with code less than 2 Deleted
            if (long.Parse(id) < 2 + 1)
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_NoUpdate"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "QuestionsAnswer");
            }
            var contract = await _unitOfWork.CrMasSysQuestionsAnswer.FindAsync(x => x.CrMasSysQuestionsAnswerNo == id);
            if (contract == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["SomethingWrongPleaseCallAdmin"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "QuestionsAnswer");
            }
            var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);

            var model = _mapper.Map<QuestionsAnswerVM>(contract);
            model.allMainTaskSystem = allMainTaskSystem;
            model.allsystemNames = allsystemName;

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(QuestionsAnswerVM questionsAnswerVM)
        {

            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Insert, pageNumber);

            var allMainTaskSystem = await _unitOfWork.CrMasSysMainTasks.FindAllWithSelectAsNoTrackingAsync(
                //predicate: x => x.CrCasCarInformationStatus != Status.Deleted,
                predicate: x => x.CrMasSysMainTasksStatus == Status.Active,
                selectProjection: query => query.Select(x => new CrMasSysMainTask
                {
                    CrMasSysMainTasksCode = x.CrMasSysMainTasksCode,
                    CrMasSysMainTasksSystem = x.CrMasSysMainTasksSystem,
                    CrMasSysMainTasksArName = x.CrMasSysMainTasksArName,
                    CrMasSysMainTasksEnName = x.CrMasSysMainTasksEnName,
                    CrMasSysMainTasksStatus = x.CrMasSysMainTasksStatus
                })
                //,includes: new string[] { "RelatedEntity1", "RelatedEntity2" } 
                );
            var allsystemName = await _unitOfWork.CrMasSysSystems.FindAllAsNoTrackingAsync(x => x.CrMasSysSystemStatus == Status.Active);

            if (user == null && questionsAnswerVM == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "QuestionsAnswer");
            }
            try
            {
                //Check Validition
                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.Update) || ModelState.IsValid == false)
                {
                    _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                    questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
                    questionsAnswerVM.allsystemNames = allsystemName;
                    return View("Edit", questionsAnswerVM);
                }
                var questionsAnswerEntity = _mapper.Map<CrMasSysQuestionsAnswer>(questionsAnswerVM);

                _unitOfWork.CrMasSysQuestionsAnswer.Update(questionsAnswerEntity);
                if (await _unitOfWork.CompleteAsync() > 0) _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي

                await SaveTracingForLicenseChange(user, questionsAnswerEntity, Status.Update);
                return RedirectToAction("Index", "QuestionsAnswer");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                questionsAnswerVM.allMainTaskSystem = allMainTaskSystem;
                questionsAnswerVM.allsystemNames = allsystemName;
                return View("Edit", questionsAnswerVM);
            }
        }
        [HttpPost]
        public async Task<string> EditStatus(string code, string status)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "false";

            var licence = await _unitOfWork.CrMasSysQuestionsAnswer.FindAsync(x => x.CrMasSysQuestionsAnswerNo == code);
            if (licence == null) return "false";

            try
            {

                if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, status)) return "false_auth";
                if (status == Status.UnDeleted || status == Status.UnHold) status = Status.Active;
                licence.CrMasSysQuestionsAnswerStatus = status;
                _unitOfWork.CrMasSysQuestionsAnswer.Update(licence);
                _unitOfWork.Complete();
                await SaveTracingForLicenseChange(user, licence, status);
                return "true";
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        //Error exist message when run post action to get what is the exist field << Help Up in Back End
        private async Task AddModelErrorsAsync(CrMasSysQuestionsAnswer entity)
        {

            //if (await _masQuestionsAnswer.ExistsByArabicNameAsync(entity.CrMasSysQuestionsAnswerArQuestions, entity.CrMasSysQuestionsAnswerNo))
            //{
            //    ModelState.AddModelError("CrMasSysQuestionsAnswerArQuestions", _localizer["Existing"]);
            //}

            //if (await _masQuestionsAnswer.ExistsByEnglishNameAsync(entity.CrMasSysQuestionsAnswerEnQuestions, entity.CrMasSysQuestionsAnswerNo))
            //{
            //    ModelState.AddModelError("CrMasSysQuestionsAnswerEnQuestions", _localizer["Existing"]);
            //}

            //if (await _masQuestionsAnswer.ExistsByNaqlCodeAsync((int)entity.CrMasSysQuestionsAnswerNaqlCode, entity.CrMasSysQuestionsAnswerNo))
            //{
            //    ModelState.AddModelError("CrMasSysQuestionsAnswerNaqlCode", _localizer["Existing"]);
            //}

            //if (await _masQuestionsAnswer.ExistsByNaqlIdAsync((int)entity.CrMasSysQuestionsAnswerNaqlId, entity.CrMasSysQuestionsAnswerNo))
            //{
            //    ModelState.AddModelError("CrMasSysQuestionsAnswerNaqlId", _localizer["Existing"]);
            //}
        }

        //Error exist message when change input without run post action >> help us in front end
        [HttpGet]
        public async Task<JsonResult> CheckChangedField(string existName, string dataField)
        {
            var All_QuestionsAnswers = await _unitOfWork.CrMasSysQuestionsAnswer.GetAllAsync();
            var errors = new List<ErrorResponse>();

            if (!string.IsNullOrEmpty(dataField) && All_QuestionsAnswers != null)
            {
                //// Check for existing Arabic driving license
                //if (existName == "CrMasSysQuestionsAnswerArQuestions" && All_QuestionsAnswers.Any(x => x.CrMasSysQuestionsAnswerArQuestions == dataField))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSysQuestionsAnswerArQuestions", Message = _localizer["Existing"] });
                //}
                //// Check for existing English driving license
                //else if (existName == "CrMasSysQuestionsAnswerEnQuestions" && All_QuestionsAnswers.Any(x => x.CrMasSysQuestionsAnswerEnQuestions?.ToLower() == dataField.ToLower()))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSysQuestionsAnswerEnQuestions", Message = _localizer["Existing"] });
                //}
                //// Check for existing rental system number
                //else if (existName == "CrMasSysQuestionsAnswerNaqlCode" && Int64.TryParse(dataField, out var code) && code != 0 && All_QuestionsAnswers.Any(x => x.CrMasSysQuestionsAnswerNaqlCode == code))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSysQuestionsAnswerNaqlCode", Message = _localizer["Existing"] });
                //}
                //// Check for existing rental system ID
                //else if (existName == "CrMasSysQuestionsAnswerNaqlId" && Int64.TryParse(dataField, out var id) && id != 0 && All_QuestionsAnswers.Any(x => x.CrMasSysQuestionsAnswerNaqlId == id))
                //{
                //    errors.Add(new ErrorResponse { Field = "CrMasSysQuestionsAnswerNaqlId", Message = _localizer["Existing"] });
                //}
            }

            return Json(new { errors });
        }

        //Helper Methods 
        private async Task<string> GenerateLicenseCodeAsync()
        {
            var allLicenses = await _unitOfWork.CrMasSysQuestionsAnswer.GetAllAsync();
            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.Last().CrMasSysQuestionsAnswerNo) + 1).ToString() : "200000";
        }

        private async Task<string> GenerateLicenseCodeAsync(string task, string system)
        {

            var allLicenses = await _unitOfWork.CrMasSysQuestionsAnswer.FindAllAsNoTrackingAsync(x => x.CrMasSysQuestionsAnswerMainTask == task && x.CrMasSysQuestionsAnswerSystem == system);
            if (!allLicenses.Any()) return system + task + "01";

            return allLicenses.Any() ? (BigInteger.Parse(allLicenses.MaxBy(x => x.CrMasSysQuestionsAnswerNo).CrMasSysQuestionsAnswerNo) + 1).ToString() : system + task + "01";
        }
        private async Task SaveTracingForLicenseChange(CrMasUserInformation user, CrMasSysQuestionsAnswer licence, string status)
        {


            var recordAr = licence.CrMasSysQuestionsAnswerNo;
            var recordEn = licence.CrMasSysQuestionsAnswerNo;
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

        [HttpGet]
        [Route("/MAS/QuestionsAnswer/GenerateajaxCodeAsync/")]
        public async Task<JsonResult> GenerateajaxCodeAsync(string task, string system)
        {
            var allLicenses = await _unitOfWork.CrMasSysQuestionsAnswer.FindAllAsNoTrackingAsync(x => x.CrMasSysQuestionsAnswerMainTask == task && x.CrMasSysQuestionsAnswerSystem == system);

            var thisCode = allLicenses.Any() ? (BigInteger.Parse(allLicenses.MaxBy(x => x.CrMasSysQuestionsAnswerNo).CrMasSysQuestionsAnswerNo) + 1).ToString() : system + task + "01";
            return new JsonResult(new { code = thisCode });

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
            return RedirectToAction("Index", "QuestionsAnswer");
        }


    }
}
