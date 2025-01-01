using AutoMapper;
using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.Base;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Inferastructure.Filters;
using Bnan.Ui.Areas.Base.Controllers;
using Bnan.Ui.ViewModels.CAS;
using Bnan.Ui.ViewModels.Identitiy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using NToastNotify;
using System.Globalization;

namespace Bnan.Ui.Areas.CAS.Controllers.Employees
{
    [Area("CAS")]
    [Authorize(Roles = "CAS")]
    [ServiceFilter(typeof(SetCurrentPathCASFilter))]

    public class EmployeesController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserLoginsService _userLoginsService;
        private readonly IAdminstritiveProcedures _adminstritiveProcedures;
        private readonly IUserMainValidtion _userMainValidtion;
        private readonly IUserSubValidition _userSubValidition;
        private readonly IUserProcedureValidition _userProcedureValidition;
        private readonly IUserBranchValidity _userBranchValidity;
        private readonly IUserContractValididation _userContractValididation;
        private readonly IToastNotification _toastNotification;
        private readonly IStringLocalizer<EmployeesController> _localizer;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBaseRepo _baseRepo;

        private readonly string pageNumber = SubTasks.CrMasUserInformationForCAS;


        public EmployeesController(UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment, IUserLoginsService userLoginsService, IUserMainValidtion userMainValidtion, IUserSubValidition userSubValidition, IUserProcedureValidition userProcedureValidition, IUserBranchValidity userBranchValidity, IToastNotification toastNotification, IStringLocalizer<EmployeesController> localizer, IUserContractValididation userContractValididation, IAdminstritiveProcedures adminstritiveProcedures, IWebHostEnvironment hostingEnvironment, IBaseRepo baseRepo) : base(userManager, unitOfWork, mapper)
        {
            _authService = authService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _userLoginsService = userLoginsService;
            _userMainValidtion = userMainValidtion;
            _userSubValidition = userSubValidition;
            _userProcedureValidition = userProcedureValidition;
            _userBranchValidity = userBranchValidity;
            _toastNotification = toastNotification;
            _localizer = localizer;
            _userContractValididation = userContractValididation;
            _adminstritiveProcedures = adminstritiveProcedures;
            _hostingEnvironment = hostingEnvironment;
            _baseRepo = baseRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lessorCode = user.CrMasUserInformationLessor;

            await SetPageTitleAsync(string.Empty, pageNumber);
            // Check Validition
            if (!await _baseRepo.CheckValidation(user.CrMasUserInformationCode, pageNumber, Status.ViewInformation))
            {
                _toastNotification.AddErrorToastMessage(_localizer["AuthEmplpoyee_No_auth"], new ToastrOptions { PositionClass = _localizer["toastPostion"], Title = "", }); //  إلغاء العنوان الجزء العلوي
                return RedirectToAction("Index", "Home");
            }
            // Retrieve active driving licenses
            var usersInfo = await _unitOfWork.CrMasUserInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode != "CAS" + user.CrMasUserInformationLessor &&
                                                  x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                  x.CrMasUserInformationStatus == Status.Active);

            // If no active licenses, retrieve all licenses
            if (!usersInfo.Any())
            {
                usersInfo = await _unitOfWork.CrMasUserInformation
                .FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode != "CAS" + user.CrMasUserInformationLessor &&
                                                  x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                  x.CrMasUserInformationStatus == Status.Hold);
                ViewBag.radio = "All";
            }
            else ViewBag.radio = "A";
            return View(usersInfo);
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByStatus(string status, string search)
        {
            var user = await _userManager.GetUserAsync(User);
            var lessorCode = user.CrMasUserInformationLessor;
            if (!string.IsNullOrEmpty(status))
            {
                var employeesInfo = await _unitOfWork.CrMasUserInformation.FindAllAsNoTrackingAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode != "CAS" + user.CrMasUserInformationLessor &&
                                                                                                     x.CrMasUserInformationCode != user.CrMasUserInformationCode &&
                                                                                                    (x.CrMasUserInformationStatus == Status.Active ||
                                                                                                     x.CrMasUserInformationStatus == Status.Deleted ||
                                                                                                     x.CrMasUserInformationStatus == Status.Hold));

                if (status == Status.All)
                {
                    var FilterAll = employeesInfo.FindAll(x => x.CrMasUserInformationStatus != Status.Deleted &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                    return PartialView("_DataTableEmployees", FilterAll);
                }
                var FilterByStatus = employeesInfo.FindAll(x => x.CrMasUserInformationStatus == status &&
                                                                         (x.CrMasUserInformationArName.Contains(search) ||
                                                                          x.CrMasUserInformationEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationTasksArName.Contains(search) ||
                                                                          x.CrMasUserInformationTasksEnName.ToLower().Contains(search.ToLower()) ||
                                                                          x.CrMasUserInformationCode.Contains(search)));
                return PartialView("_DataTableEmployees", FilterByStatus);
            }
            return PartialView();
        }

        public async Task<IActionResult> AddEmployee()
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "0";
            //To Set Title;
            var titles = await setTitle("206", "2206001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "اضافة", "Create", titles[3]);
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            var currentUser = await _userManager.GetUserAsync(User);
            var lastUser = _userManager.Users.ToList().LastOrDefault(x => x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor);
            ViewBag.Branches = _unitOfWork.CrCasBranchInformation.FindAll(x => x.CrCasBranchInformationLessor == currentUser.CrMasUserInformationLessor && x.CrCasBranchInformationStatus == Status.Active);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(RegisterViewModel model, IFormFile? UserSignatureFile, IFormFile? UserImgFile, List<string> CheckboxBranchesWithData,
            bool CrMasUserInformationAuthorizationAdmin, bool CrMasUserInformationAuthorizationOwner, bool CrMasUserInformationAuthorizationBranch)
        {
            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.Branches = _unitOfWork.CrCasBranchInformation.FindAll(x => x.CrCasBranchInformationLessor == currentUser.CrMasUserInformationLessor);

            if (ModelState.IsValid)
            {
                var userLogin = await _userManager.GetUserAsync(User);
                var user = await _userService.GetUserByUserNameAsync(model.CrMasUserInformationCode);
                if (user != null)
                {

                    ModelState.AddModelError("Exist", _localizer["EmployeeExist"]);
                    return View(model);
                }

                string foldername = $"{"images\\Company"}\\{userLogin.CrMasUserInformationLessor}\\{"Users"}\\{model.CrMasUserInformationCode}";
                string filePathImage;
                string filePathSignture;

                if (UserImgFile != null)
                {
                    string fileNameImg = "Image";
                    filePathImage = await UserImgFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png");
                }
                else
                {
                    filePathImage = "~/images/common/user.jpg";
                }
                if (UserSignatureFile != null)
                {
                    string fileNameSignture = "Signture";
                    filePathSignture = await UserSignatureFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameSignture, ".png");
                }
                else
                {
                    filePathSignture = "~/images/common/DefualtUserSignature.png";
                }


                model.CrMasUserInformationSignature = filePathSignture;
                model.CrMasUserInformationPicture = filePathImage;
                model.CrMasUserInformationLessor = userLogin.CrMasUserInformationLessor;
                model.CrMasUserInformationAuthorizationAdmin = CrMasUserInformationAuthorizationAdmin;
                model.CrMasUserInformationAuthorizationOwner = CrMasUserInformationAuthorizationOwner;
                model.CrMasUserInformationAuthorizationBranch = CrMasUserInformationAuthorizationBranch;
                var crMasUserInformation = _mapper.Map<CrMasUserInformation>(model);
                var createUser = await _authService.RegisterForCasAsync(crMasUserInformation);

                if (!createUser)
                {
                    ModelState.AddModelError("Exist", "Something went wrong");
                    return View(model);
                }
                //Add Role 
                var newUser = await _userService.GetUserByUserNameAsync(crMasUserInformation.CrMasUserInformationCode);
                await _authService.AddRoleAsync(newUser, "CAS");

                //Add Main Validitions
                if (!await _userMainValidtion.AddMainValiditionsForEachUser(newUser.CrMasUserInformationCode, "2"))
                {
                    ModelState.AddModelError("Exist", "Something went wrong");
                    return View(model);
                }

                //Add Sub Validitions
                if (!await _userSubValidition.AddSubValiditionsForEachUser(newUser.CrMasUserInformationCode, "2"))
                {
                    ModelState.AddModelError("Exist", "Something went wrong");
                    return View(model);
                }

                //Add Procedures Validitions
                if (!await _userProcedureValidition.AddProceduresValiditionsForEachUser(newUser.CrMasUserInformationCode, "2"))
                {
                    ModelState.AddModelError("Exist", "Something went wrong");
                    return View(model);
                }



                //Add Contract Validitions
                if (!await _userContractValididation.AddContractValiditionsForEachUserInCas(newUser.CrMasUserInformationCode, null))
                {
                    ModelState.AddModelError("Exist", "Something went wrong");
                    return View(model);
                }

                List<CheckboxBranchesAuthData> checkboxDataList = new List<CheckboxBranchesAuthData>();

                // Deserialize and filter the checkbox data
                foreach (var item in CheckboxBranchesWithData)
                {
                    List<CheckboxBranchesAuthData> deserializedData = JsonConvert.DeserializeObject<List<CheckboxBranchesAuthData>>(item);
                    checkboxDataList.AddRange(deserializedData);
                }

                foreach (var item in checkboxDataList)
                {
                    var id = item.Id;
                    var value = item.Value;
                    if (newUser.CrMasUserInformationAuthorizationBranch == true)
                    {
                        if (value.ToLower() == "true")
                        {
                            await _userBranchValidity.AddUserBranchValidity(newUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, id, Status.Active);
                        }
                        else
                        {
                            await _userBranchValidity.AddUserBranchValidity(newUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, id, Status.Deleted);
                        }
                    }
                    else
                    {
                        await _userBranchValidity.AddUserBranchValidity(newUser.CrMasUserInformationCode, userLogin.CrMasUserInformationLessor, id, Status.Deleted);

                    }
                }
                if (newUser != null) await ChangeRoleAsync(newUser);
                // SaveTracing
                var (mainTask, subTask, system, currentUserr) = await SetTrace("206", "2206001", "2");
                await _userLoginsService.SaveTracing(currentUserr.CrMasUserInformationCode, "اضافة", "Add", mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);

                await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "234", "20", currentUser.CrMasUserInformationLessor, "100",
                    newUser.CrMasUserInformationCode, null, null, null, null, null, null, null, null, "اضافة",
                    "Insert", "I", null);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastSave"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Employees", "Employees");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            //sidebar Active
            ViewBag.id = "#sidebarUsers";
            ViewBag.no = "0";

            //To Set Title 
            var titles = await setTitle("206", "2206001", "2");
            await ViewData.SetPageTitleAsync(titles[0], titles[1], titles[2], "تعديل", "Edit", titles[3]);
            var user = await _userService.GetUserByUserNameAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("Exist", "SomeThing Wrong is happened");
                return View("Employees");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            var lastUser = _userManager.Users.ToList().LastOrDefault(x => x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor);
            var crMasUserInformation = _mapper.Map<RegisterViewModel>(user);
            ViewBag.CreditLimit = crMasUserInformation.CrMasUserInformationCreditLimit?.ToString("N2", CultureInfo.InvariantCulture);
            crMasUserInformation.CrMasUserBranchValidities = _unitOfWork.CrMasUserBranchValidity.FindAll(x => x.CrMasUserBranchValidityId == user.CrMasUserInformationCode).ToList();
            crMasUserInformation.CrCasBranchInformations = _unitOfWork.CrCasBranchInformation.FindAll(x => x.CrCasBranchInformationLessor == currentUser.CrMasUserInformationLessor && x.CrCasBranchInformationStatus != Status.Deleted).ToList();

            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view

            return View(crMasUserInformation);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RegisterViewModel model, string CreditLimit, bool CrMasUserInformationAuthorizationAdmin, bool CrMasUserInformationAuthorizationOwner, bool CrMasUserInformationAuthorizationBranch, List<string> CheckboxBranchesWithData)
        {
            ModelState.Remove("CrMasUserInformationId");
            if (ModelState.IsValid)
            {

                var user = await _userService.GetUserByUserNameAsync(model.CrMasUserInformationCode);
                if (user != null)
                {
                    user.CrMasUserInformationTasksArName = model.CrMasUserInformationTasksArName;
                    user.CrMasUserInformationTasksEnName = model.CrMasUserInformationTasksEnName;
                    user.CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey;
                    user.CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo;
                    user.CrMasUserInformationCreditLimit = decimal.Parse(CreditLimit, CultureInfo.InvariantCulture);
                    user.CrMasUserInformationReasons = model.CrMasUserInformationReasons;
                    user.CrMasUserInformationAuthorizationAdmin = CrMasUserInformationAuthorizationAdmin;
                    user.CrMasUserInformationAuthorizationOwner = CrMasUserInformationAuthorizationOwner;

                    List<CheckboxBranchesAuthData> checkboxDataList = new List<CheckboxBranchesAuthData>();

                    // Deserialize and filter the checkbox data
                    foreach (var item in CheckboxBranchesWithData)
                    {
                        List<CheckboxBranchesAuthData> deserializedData = JsonConvert.DeserializeObject<List<CheckboxBranchesAuthData>>(item);
                        checkboxDataList.AddRange(deserializedData);
                    }

                    foreach (var item in checkboxDataList)
                    {
                        var id = item.Id;
                        var value = item.Value;
                        if (CrMasUserInformationAuthorizationBranch == true && (value.ToLower() == "on" || value.ToLower() == "true"))
                        {
                            await _userBranchValidity.UpdateUserBranchValidity(user.CrMasUserInformationCode, user.CrMasUserInformationLessor, id, Status.Active);
                        }
                        else
                        {
                            await _userBranchValidity.UpdateUserBranchValidity(user.CrMasUserInformationCode, user.CrMasUserInformationLessor, id, Status.Deleted);
                        }

                    }
                    var BranchValidity = _unitOfWork.CrMasUserBranchValidity.FindAll(x => x.CrMasUserBranchValidityId == user.CrMasUserInformationCode &&
                                                                                       x.CrMasUserBranchValidityLessor == user.CrMasUserInformationLessor &&
                                                                                       x.CrMasUserBranchValidityBranchStatus == Status.Active);
                    if (BranchValidity.Count() < 1) user.CrMasUserInformationAuthorizationBranch = false;
                    else user.CrMasUserInformationAuthorizationBranch = true;
                    await _userService.UpdateAsync(user);
                    await ChangeRoleAsync(user);
                    //SaveTracing
                    var (mainTask, subTask, system, currentUser) = await SetTrace("206", "2206001", "2");
                    await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, "تعديل", "Edit", mainTask.CrMasSysMainTasksCode,
                    subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                    subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                    // Save Adminstrive Procedures
                    await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "234", "20", currentUser.CrMasUserInformationLessor, "100",
                        user.CrMasUserInformationCode, null, null, null, null, null, null, null, null, "تعديل",
                       "Edit", "U", null);
                    _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                    return RedirectToAction("Employees", "Employees");

                }
            }
            return View(model);

        }
        private async Task<bool> ChangeRoleAsync(CrMasUserInformation user)
        {
            // Define role mappings using a list of tuples, converting nullable booleans to non-nullable
            var roleMappings = new List<(bool PropertyValue, string Role)>
                    {
                        (user.CrMasUserInformationAuthorizationBranch ?? false, RolesStrings.BS),
                        (user.CrMasUserInformationAuthorizationOwner ?? false, RolesStrings.OWN),
                        (user.CrMasUserInformationAuthorizationAdmin ?? false, RolesStrings.CAS)
                    };

            // Iterate through role mappings and perform role operations
            foreach (var (propertyValue, role) in roleMappings)
            {
                var userHasRole = await _userManager.IsInRoleAsync(user, role);

                if (propertyValue && !userHasRole)
                {
                    await _authService.AddRoleAsync(user, role);
                }
                else if (!propertyValue && userHasRole)
                {
                    await _authService.RemoveRoleAsync(user, role);
                }
            }
            return true;
        }
        [HttpPost]
        public async Task<IActionResult> EditStatus(string code, string status)
        {
            string sAr = "";
            string sEn = "";
            var user = await _userService.GetUserByUserNameAsync(code);
            if (user != null)
            {
                if (status == Status.Hold)
                {
                    sAr = "ايقاف";
                    sEn = "Hold Employee";
                    user.CrMasUserInformationStatus = Status.Hold;
                }
                else if (status == Status.Deleted)
                {
                    sAr = "حذف";
                    sEn = "Remove Employee";
                    user.CrMasUserInformationStatus = Status.Deleted;
                }
                else if (status == Status.Active)
                {
                    sAr = "استرجاع";
                    sEn = "Retrive Employee";
                    user.CrMasUserInformationStatus = Status.Active;
                }
                await _unitOfWork.CompleteAsync();
                // SaveTracing
                var (mainTask, subTask, system, currentUser) = await SetTrace("206", "2206001", "2");
                await _userLoginsService.SaveTracing(currentUser.CrMasUserInformationCode, sAr, sEn, mainTask.CrMasSysMainTasksCode,
                subTask.CrMasSysSubTasksCode, mainTask.CrMasSysMainTasksArName, subTask.CrMasSysSubTasksArName, mainTask.CrMasSysMainTasksEnName,
                subTask.CrMasSysSubTasksEnName, system.CrMasSysSystemCode, system.CrMasSysSystemArName, system.CrMasSysSystemEnName);
                // Save Adminstrive Procedures
                await _adminstritiveProcedures.SaveAdminstritive(currentUser.CrMasUserInformationCode, "1", "234", "20", currentUser.CrMasUserInformationLessor, "100",
                    user.CrMasUserInformationCode, null, null, null, null, null, null, null, null, sAr, sEn, "U", null);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Employees", "Employees");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string code)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var employee = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == code && x.CrMasUserInformationLessor == currentUser.CrMasUserInformationLessor);
            if (employee != null)
            {
                // Remove the current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(employee);
                if (removePasswordResult.Succeeded)
                {
                    // Add the new password
                    var addPasswordResult = await _userManager.AddPasswordAsync(employee, code);
                    if (addPasswordResult.Succeeded)
                    {
                        //employee.CrMasUserInformationPassWord = code;
                        await _unitOfWork.CompleteAsync();
                        return Json(true);
                    }
                }
            }

            return Json(false);
        }

        [HttpGet]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, SubTasks.MyAccountCAS);

            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }

            var callingKeys = _unitOfWork.CrMasSysCallingKeys.FindAll(x => x.CrMasSysCallingKeysStatus == Status.Active);
            var callingKeyList = callingKeys.Select(c => new SelectListItem { Value = c.CrMasSysCallingKeysCode.ToString(), Text = c.CrMasSysCallingKeysNo }).ToList();
            ViewData["CallingKeys"] = callingKeyList; // Pass the callingKeys to the view
            var crMasUserInformation = _mapper.Map<RegisterViewModel>(user);
            return View(crMasUserInformation);
        }

        [HttpPost]
        public async Task<IActionResult> MyAccount(RegisterViewModel model, string UserSignatureFile, IFormFile UserImgFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }

            string foldername = $"{"images\\Bnan\\Users"}\\{model.CrMasUserInformationCode}";
            string filePathImage;
            string filePathSignture;
            var oldPathImage = model.CrMasUserInformationPicture;
            var oldPathSignture = model.CrMasUserInformationSignature;
            if (oldPathImage == "~/images/common/user.jpg") oldPathImage = "";
            if (oldPathSignture == "~/images/common/DefualtUserSignature.png") oldPathSignture = "";
            if (UserImgFile != null)
            {
                string fileNameImg = "Image_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
                filePathImage = await UserImgFile.SaveImageAsync(_webHostEnvironment, foldername, fileNameImg, ".png", user.CrMasUserInformationPicture);
            }
            else if (string.IsNullOrEmpty(oldPathImage))
            {
                filePathImage = "~/images/common/user.jpg";
            }
            else
            {
                filePathImage = user.CrMasUserInformationPicture;

            }

            if (!string.IsNullOrEmpty(UserSignatureFile))
            {
                string fileNameSignture = "Signture_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقتs
                filePathSignture = await _hostingEnvironment.SaveSigntureImage(UserSignatureFile, user.CrMasUserInformationCode, user.CrMasUserInformationSignature, "Users");
            }
            else if (string.IsNullOrEmpty(oldPathSignture))
            {
                filePathSignture = "~/images/common/DefualtUserSignature.png";
            }
            else
            {
                filePathSignture = user.CrMasUserInformationSignature;
            }
            user.CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey;
            user.CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo;
            user.CrMasUserInformationEmail = model.CrMasUserInformationEmail;
            user.CrMasUserInformationExitTimer = model.CrMasUserInformationExitTimer;
            user.CrMasUserInformationSignature = filePathSignture;
            user.CrMasUserInformationPicture = filePathImage;
            user.CrMasUserInformationRemindMe = model.CrMasUserInformationRemindMe;
            user.CrMasUserInformationDefaultLanguage = model.CrMasUserInformationDefaultLanguage;
            await _userManager.UpdateAsync(user);
            await SaveTracingForUserChange(user, Status.Update, SubTasks.MyAccountCAS);
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            await SetPageTitleAsync(Status.Update, SubTasks.ChangePasswordCAS);
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage(_localizer["ToastFailed"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            // تحقق من صحة النموذج
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // الحصول على المستخدم الحالي
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            await SetPageTitleAsync(Status.Update, SubTasks.ChangePasswordCAS);
            // تحقق من كلمة المرور الحالية
            if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassInCorrect", _localizer["CurrentPassInCorrect"]);
                return View(model);
            }
            // منع استخدام نفس كلمة المرور الحالية
            if (model.NewPassword == model.CurrentPassword)
            {
                ModelState.AddModelError("CannotUseCurrentPassword", _localizer["CannotUserCurrentPassword"]);
                return View(model);
            }
            // تغيير كلمة المرور
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword?.Trim(), model.NewPassword?.Trim());
            if (result.Succeeded)
            {
                user.CrMasUserInformationChangePassWordLastDate = DateTime.Now.Date;
                _unitOfWork.Complete();
                await SaveTracingForUserChange(user, Status.ChangePassword, SubTasks.ChangePasswordCAS);
                _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
                return RedirectToAction("Index", "Home");
            }

            // التعامل مع الأخطاء أثناء تغيير كلمة المرور
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        private async Task SaveTracingForUserChange(CrMasUserInformation userCreated, string status, string pageNumber)
        {


            var recordAr = $"{userCreated.CrMasUserInformationArName} - {userCreated.CrMasUserInformationTasksArName}";
            var recordEn = $"{userCreated.CrMasUserInformationEnName} - {userCreated.CrMasUserInformationTasksEnName}";
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
        public IActionResult SuccessToast()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("EmployeeSystemValiditions", "Employees");
        }
        public IActionResult SuccessResetPassword()
        {
            _toastNotification.AddSuccessToastMessage(_localizer["ToastEdit"], new ToastrOptions { PositionClass = _localizer["toastPostion"] });
            return RedirectToAction("Employees", "Employees");
        }
    }
}
