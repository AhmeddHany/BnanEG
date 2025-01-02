using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;

namespace Bnan.Ui.Areas.MAS.Controllers.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly IStringLocalizer<SidebarMenuViewComponent> _localizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string currentCulture = CultureInfo.CurrentCulture.Name;

        public SidebarMenuViewComponent(IStringLocalizer<SidebarMenuViewComponent> localizer, UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork)
        {
            _localizer = localizer;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userInfo = await GetUserInfoAsync();
            var sidebarMenu = await BuildSidebarMenu(userInfo);
            return View(sidebarMenu);
        }

        private async Task<CrMasUserInformation> GetUserInfoAsync()
        {
            var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
            var userInfo = await _userManager.Users
                .Include(l => l.CrMasUserMainValidations)
                .Include(l => l.CrMasUserSubValidations)
                .Include(l => l.CrMasUserInformationLessorNavigation)
                .FirstOrDefaultAsync(l => l.UserName == user.UserName);
            return userInfo;
        }

        //private async Task<List<SidebarMenuItem>> BuildSidebarMenu(CrMasUserInformation userInfo)
        //{
        //    var sidebarMenu = new List<SidebarMenuItem>();
        //    // Company Section (101)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "101" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "101");
        //        var companyMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "company-items",
        //            IconPath = "/MasSystem/images/Comanies.svg",
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "101" && x.CrMasSysSubTasksStatus != Status.Deleted);

        //        var companySubTaskMappings = new List<(string Code, string Controller, string Action)>
        //            {
        //                ("1101001", "LessorsKSA", "Index"),
        //                ("1101002", "LessorImages", "Index"),
        //                ("1101003", "CompanyContracts", "CompanyContracts"),
        //                ("1101004", "CompanyDues", "Index"),
        //                ("1101005", "CompanyOwed", "Index"),
        //                ("1101006", "#", null)
        //            };

        //        foreach (var mapping in companySubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code && x.CrMasSysSubTasksStatus != Status.Deleted);
        //                companyMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }

        //        sidebarMenu.Add(companyMenu);
        //    }

        //    // Bnan Section (102)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "102" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "102");
        //        var bnanMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarcars",
        //            IconPath = "/MasSystem/images/Bnan.svg",
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "102" && x.CrMasSysSubTasksStatus != Status.Deleted);

        //        var bnanSubTaskMappings = new List<(string Code, string Controller, string Action)>
        //            {
        //                ("1102001", "#", null),
        //                ("1102002", "LessorMarketing", "Index")
        //            };

        //        foreach (var mapping in bnanSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                bnanMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }

        //        sidebarMenu.Add(bnanMenu);
        //    }

        //    // Renter Section (103)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "103" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "103");

        //        var renterMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarRenter",
        //            IconPath = "/MasSystem/images/tenants2.svg", // استخدم المسار الصحيح للأيقونة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "103" && x.CrMasSysSubTasksStatus != Status.Deleted);

        //        var renterSubTaskMappings = new List<(string Code, string Controller, string Action)>
        //            {
        //                ("1103001",  "RenterInformation", "Index"),
        //                ("1103002", "RenterContract", "Index")
        //            };

        //        foreach (var mapping in renterSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                renterMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(renterMenu);
        //    }

        //    // Reports Section (104)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "104" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "104");

        //        var reportsMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarReport",
        //            IconPath = "/MasSystem/images/reports.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "104" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var reportsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1104001", "Report1", "Index"),
        //                ("1104002", "#", null),
        //                ("1104003", "#", null),
        //                ("1104004", "#", null),
        //                ("1104005", "#", null),
        //                ("1104006", "#", null),
        //                ("1104007", "#", null),
        //                ("1104008", "#", null),
        //                ("1104009", "#", null),
        //                ("1104010", "#", null),
        //                ("1104011", "#", null),
        //                ("1104012", "#", null),
        //                ("1104013", "#", null),
        //                ("1104014", "#", null),
        //                ("1104015", "#", null),
        //            };

        //        foreach (var mapping in reportsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                reportsMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(reportsMenu);
        //    }

        //    // Statistics  (105)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "105" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "105");

        //        var statisticsMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarStatistics",
        //            IconPath = "/MasSystem/images/stat.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "105" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var statisticsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1105001", "RenterStatistics", "Index"),
        //                ("1105002", "CarStatistics", "Index"),
        //                ("1105003", "ContractStatistics", "Index"),
        //                ("1105004", "CarContractStatistics", "Index"),
        //                ("1105005", "RenterContractStatistics", "Index"),
        //                ("1105006", "#", null),
        //                ("1105007", "#", null),
        //                ("1105008", "#", null),
        //                ("1105009", "#", null),
        //                ("1105010", "#", null),
        //            };

        //        foreach (var mapping in statisticsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                statisticsMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(statisticsMenu);
        //    }

        //    // Users Section (106)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "106" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "106");

        //        var usersMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarUsers",
        //            IconPath = "/MasSystem/images/employees.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "106" && x.CrMasSysSubTasksStatus != Status.Deleted);

        //        var usersSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1106001", "Users", "Users"),
        //                ("1106002", "UsersSystemValiditions", "SystemValiditions"),
        //                ("1106003", "UsersForCompanies", "Index"),
        //                ("1106004", "#", null),
        //                ("1106005", "#", null),
        //                ("1106006", "#", null),
        //                ("1106007", "#", null),
        //                ("1106008", "#", null),
        //                ("1106009", "#", null),
        //                ("1106010", "#", null),
        //            };
        //        foreach (var mapping in usersSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                usersMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(usersMenu);
        //    }

        //    // Renters Services (107)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "107" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "107");

        //        var rentersServicesMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarUsersServices",
        //            IconPath = "/MasSystem/images/tenants2.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "107" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var rentersSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1107001", "RenterIdType", "Index"),
        //                ("1107002", "RenterDrivingLicense", "Index"),
        //                ("1107003", "RenterNationality", "Index"),
        //                ("1107004", "RenterGender", "Index"),
        //                ("1107005", "RenterProfession", "Index"),
        //                ("1107006", "RenterEmployer", "Index"),
        //                ("1107007", "RenterMembership", "Index"),
        //                ("1107008", "RenterSector", "Index"),
        //                ("1107009", "#", null),
        //                ("1107010", "#", null),
        //            };
        //        foreach (var mapping in rentersSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                rentersServicesMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(rentersServicesMenu);
        //    }

        //    // Cars Services (108)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "108" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "108");

        //        var carsServicesMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarCarsServices",
        //            IconPath = "/MasSystem/images/cars.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "108" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var carsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1108001", "CarRegistration", "Index"),
        //                ("1108002", "CarFuel", "Index"),
        //                ("1108003", "CarOil", "Index"),
        //                ("1108004", "CarCvt", "Index"),
        //                ("1108005", "CarBrand", "Index"),
        //                ("1108006", "CarModel", "Index"),
        //                ("1108007", "CarCategory", "Index"),
        //                ("1108008", "CarDistribution", "CarDistribution"),
        //                ("1108009", "CarAdvantage", "Index"),
        //                ("1108010", "CarColor", "Index"),
        //                ("1108011", "#", null),
        //                ("1108012", "#", null),
        //                ("1108013", "#", null),
        //                ("1108014", "#", null),
        //                ("1108015", "#", null),
        //            };
        //        foreach (var mapping in carsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                carsServicesMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }

        //        sidebarMenu.Add(carsServicesMenu);
        //    }

        //    // Mails Services (109)
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "109" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "109");

        //        var mailsServicesMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarMailsServices",
        //            IconPath = "/MasSystem/images/Mail (2).svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "109" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var mailsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1109001", "Countries", "Index"),
        //                ("1109002", "PostRegions", "Index"),
        //                ("1109003", "PostCity", "Index"),
        //                ("1109004", "#", null),
        //                ("1109005", "#", null),
        //                ("1109006", "#", null),
        //                ("1109007", "#", null),
        //                ("1109008", "#", null),
        //                ("1109009", "#", null),
        //                ("1109010", "#", null),
        //             };
        //        foreach (var mapping in mailsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                mailsServicesMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(mailsServicesMenu);
        //    }

        //    // Accounts Services Section
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "110" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "110");

        //        var accountsServicesMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarAccountsServices",
        //            IconPath = "/MasSystem/images/calc.svg", // استبدل هذا المسار بأيقونة مناسبة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "110" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var accountsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1110001", "Bank", "Index"),
        //                ("1110002", "AccountPaymentMethod", "Index"),
        //                ("1110003", "AccountReference", "Index"),
        //                ("1110004", "#", null),
        //                ("1110005", "#", null),
        //                ("1110006", "#", null),
        //                ("1110007", "#", null),
        //                ("1110008", "#", null),
        //                ("1110009", "#", null),
        //                ("1110010", "#", null),
        //             };
        //        foreach (var mapping in accountsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                accountsServicesMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(accountsServicesMenu);
        //    }

        //    // RentalContracts Section
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "111" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "111");

        //        var rentalContractsMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "sidebarRentalContracts",
        //            IconPath = "/MasSystem/images/ContractIcon.svg", // استخدم المسار الصحيح للأيقونة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };

        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "111" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var rentalContractsSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1111001", "ContractAdditional", "Index"),
        //                ("1111002", "ContractOptions", "Index"),
        //                ("1111003", "ContractCarCheckup", "Index"),
        //                ("1111004", "ContractCarCheckupDetails", "Index"),
        //                ("1111005", "#", null),
        //                ("1111006", "#", null),
        //                ("1111007", "#", null),
        //                ("1111008", "#", null),
        //                ("1111009", "#", null),
        //                ("1111010", "#", null),
        //             };
        //        foreach (var mapping in rentalContractsSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                rentalContractsMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(rentalContractsMenu);
        //    }

        //    // Services Section
        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "112" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == "112");

        //        var servicesMenu = new SidebarMenuItem
        //        {
        //            Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
        //            ItemName = "services-items",
        //            IconPath = "/MasSystem/images/services.svg", // استخدم المسار الصحيح للأيقونة
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };
        //        var subTasks = await _unitOfWork.CrMasSysSubTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == "112" && x.CrMasSysSubTasksStatus != Status.Deleted);
        //        var servicesSubTaskMappings = new List<(string Code, string Controller, string? Action)>
        //            {
        //                ("1112001", "TechnicalConnectivity", "Connect"),
        //                ("1112002", "CountryClassification", "Index"),
        //                ("1112003", "LessorClassification", "Index"),
        //                ("1112004", "Rate", "Edit"),
        //                ("1112005", "Currency", "Edit"),
        //                ("1112006", "QuestionsAnswer", "Index"),
        //                ("1112007", "#", null),
        //                ("1112008", "#", null),
        //                ("1112009", "#", null),
        //                ("1112010", "#", null),
        //             };
        //        foreach (var mapping in servicesSubTaskMappings)
        //        {
        //            if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == mapping.Code && l.CrMasUserSubValidationAuthorization == true))
        //            {
        //                var subTask = subTasks.FirstOrDefault(x => x.CrMasSysSubTasksCode == mapping.Code);
        //                servicesMenu.SubItems.Add(new SidebarMenuItem
        //                {
        //                    Authorization = true,
        //                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
        //                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
        //                });
        //            }
        //        }
        //        sidebarMenu.Add(servicesMenu);
        //    }

        //    return sidebarMenu;
        //}


        private async Task<List<SidebarMenuItem>> BuildSidebarMenu(CrMasUserInformation userInfo)
        {
            var sidebarMenu = new List<SidebarMenuItem>();

            // إعداد قائمة لتخزين البيانات الخاصة بالأقسام
            var sections = new List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)> SubTaskMappings)>
                                    {
                                        ("101", "/MasSystem/images/Comanies.svg", "company-items", new List<(string, string, string)>
                                        {
                                            ("1101001", "LessorsKSA", "Index"),
                                            ("1101002", "LessorImages", "Index"),
                                            ("1101003", "CompanyContracts", "CompanyContracts"),
                                            ("1101004", "CompanyDues", "Index"),
                                            ("1101005", "CompanyOwed", "Index"),
                                            ("1101006", "#", null)
                                        }),
                                        ("102", "/MasSystem/images/Bnan.svg", "sidebarcars", new List<(string, string, string)>
                                        {
                                            ("1102001", "#", null),
                                            ("1102002", "LessorMarketing", "Index")
                                        }),
                                        ("103", "/MasSystem/images/tenants2.svg", "sidebarRenter", new List<(string, string, string)>
                                        {
                                            ("1103001", "RenterInformation", "Index"),
                                            ("1103002", "RenterContract", "Index")
                                        }),
                                        ("104", "/MasSystem/images/reports.svg", "sidebarReport", new List<(string, string, string)>
                                        {
                                            ("1104001", "Report1", "Index"),
                                            ("1104002", "ReportActiveContract", "Index"),
                                            ("1104003", "ReportClosedContract", "Index"),
                                            ("1104004", "ReportSaved_CanceledContract", "Index"),
                                            ("1104005", "ReportCarContract", "Index"),
                                            ("1104006", "ReportFTPemployee", "Index"),
                                            ("1104007", "ReportFTPrenter", "Index"),
                                            ("1104008", "#", null),
                                            ("1104009", "#", null),
                                            ("1104010", "#", null),
                                            ("1104011", "#", null),
                                            ("1104012", "#", null),
                                            ("1104013", "#", null),
                                            ("1104014", "#", null),
                                            ("1104015", "#", null)
                                        }),
                                        ("105", "/MasSystem/images/stat.svg", "sidebarStatistics", new List<(string, string, string)>
                                        {
                                            ("1105001", "RenterStatistics", "Index"),
                                            ("1105002", "CarStatistics", "Index"),
                                            ("1105003", "ContractStatistics", "Index"),
                                            ("1105004", "RenterContractStatistics", "Index"),
                                            ("1105005", "CarContractStatistics", "Index"),
                                            ("1105006", "#", null),
                                            ("1105007", "#", null),
                                            ("1105008", "#", null),
                                            ("1105009", "#", null),
                                            ("1105010", "#", null)
                                        }),
                                        ("106", "/MasSystem/images/employees.svg", "sidebarUsers", new List<(string, string, string)>
                                        {
                                            ("1106001", "Users", "Users"),
                                            ("1106002", "UsersSystemValiditions", "SystemValiditions"),
                                            ("1106003", "UsersForCompanies", "Index"),
                                            ("1106004", "#", null),
                                            ("1106005", "#", null),
                                            ("1106006", "#", null),
                                            ("1106007", "#", null),
                                            ("1106008", "#", null),
                                            ("1106009", "#", null),
                                            ("1106010", "#", null)
                                        }),
                                        ("107", "/MasSystem/images/tenants2.svg", "sidebarUsersServices", new List<(string, string, string)>
                                        {
                                            ("1107001", "RenterIdType", "Index"),
                                            ("1107002", "RenterDrivingLicense", "Index"),
                                            ("1107003", "RenterNationality", "Index"),
                                            ("1107004", "RenterGender", "Index"),
                                            ("1107005", "RenterProfession", "Index"),
                                            ("1107006", "RenterEmployer", "Index"),
                                            ("1107007", "RenterMembership", "Index"),
                                            ("1107008", "RenterSector", "Index"),
                                            ("1107009", "#", null),
                                            ("1107010", "#", null)
                                        }),
                                        ("108", "/MasSystem/images/cars.svg", "sidebarCarsServices", new List<(string, string, string)>
                                        {
                                                        ("1108001", "CarRegistration", "Index"),
                                                        ("1108002", "CarFuel", "Index"),
                                                        ("1108003", "CarOil", "Index"),
                                                        ("1108004", "CarCvt", "Index"),
                                                        ("1108005", "CarBrand", "Index"),
                                                        ("1108006", "CarModel", "Index"),
                                                        ("1108007", "CarCategory", "Index"),
                                                        ("1108008", "CarDistribution", "CarDistribution"),
                                                        ("1108009", "CarAdvantage", "Index"),
                                                        ("1108010", "CarColor", "Index"),
                                                        ("1108011", "#", null),
                                                        ("1108012", "#", null),
                                                        ("1108013", "#", null),
                                                        ("1108014", "#", null),
                                                        ("1108015", "#", null),
                                        }),
                                        ("109", "/MasSystem/images/Mail (2).svg", "sidebarMailsServices", new List<(string, string, string)>
                                        {
                                                        ("1109001", "Countries", "Index"),
                                                        ("1109002", "PostRegions", "Index"),
                                                        ("1109003", "PostCity", "Index"),
                                                        ("1109004", "#", null),
                                                        ("1109005", "#", null),
                                                        ("1109006", "#", null),
                                                        ("1109007", "#", null),
                                                        ("1109008", "#", null),
                                                        ("1109009", "#", null),
                                                        ("1109010", "#", null),
                                        }),
                                        ("110", "/MasSystem/images/calc.svg", "sidebarAccountsServices", new List<(string, string, string)>
                                        {
                                                        ("1110001", "Bank", "Index"),
                                                        ("1110002", "AccountPaymentMethod", "Index"),
                                                        ("1110003", "AccountReference", "Index"),
                                                        ("1110004", "#", null),
                                                        ("1110005", "#", null),
                                                        ("1110006", "#", null),
                                                        ("1110007", "#", null),
                                                        ("1110008", "#", null),
                                                        ("1110009", "#", null),
                                                        ("1110010", "#", null),
                                        }),
                                        ("111", "/MasSystem/images/ContractIcon.svg", "sidebarRentalContracts", new List<(string, string, string)>
                                        {
                                            ("1111001", "ContractAdditional", "Index"),
                                            ("1111002", "ContractOptions", "Index"),
                                            ("1111003", "ContractCarCheckup", "Index"),
                                            ("1111004", "ContractCarCheckupDetails", "Index"),
                                            ("1111005", "#", null),
                                            ("1111006", "#", null),
                                            ("1111007", "#", null),
                                            ("1111008", "#", null),
                                            ("1111009", "#", null),
                                            ("1111010", "#", null),
                                        }),
                                        ("112", "/MasSystem/images/services.svg", "services-items", new List<(string, string, string)>
                                        {
                                                        ("1112001", "TechnicalConnectivity", "Connect"),
                                                        ("1112002", "CountryClassification", "Index"),
                                                        ("1112003", "LessorClassification", "Index"),
                                                        ("1112004", "Rate", "Edit"),
                                                        ("1112005", "Currency", "Edit"),
                                                        ("1112006", "QuestionsAnswer", "Index"),
                                                        ("1112007", "#", null),
                                                        ("1112008", "#", null),
                                                        ("1112009", "#", null),
                                                        ("1112010", "#", null),
                                        }),

                                    };

            // تخزين صلاحيات المستخدم في متغيرات لتقليل الوصول المتكرر
            var validMainTaskCodes = userInfo?.CrMasUserMainValidations
                .Where(l => (bool)l.CrMasUserMainValidationAuthorization)
                .Select(l => l.CrMasUserMainValidationMainTasks)
                .ToHashSet();

            var validSubTaskCodes = userInfo?.CrMasUserSubValidations
                .Where(l => (bool)l.CrMasUserSubValidationAuthorization)
                .Select(l => l.CrMasUserSubValidationSubTasks)
                .ToHashSet();

            // إنشاء القوائم الجانبية بناءً على صلاحيات المستخدم
            foreach (var section in sections)
            {
                if (validMainTaskCodes?.Contains(section.MainTaskCode) ?? false)
                {
                    var mainTask = await _unitOfWork.CrMasSysMainTasks.FindAsync(x => x.CrMasSysMainTasksCode == section.MainTaskCode);
                    var sectionMenu = new SidebarMenuItem
                    {
                        Title = currentCulture == "en-US" ? mainTask?.CrMasSysMainTasksEnName : mainTask?.CrMasSysMainTasksArName,
                        ItemName = section.ItemName,
                        IconPath = section.IconPath,
                        SubItems = new List<SidebarMenuItem>(),
                        Authorization = true,
                    };

                    // استخدام قاموس لتحسين البحث في الـ subTasks
                    var subTasks = await _unitOfWork.CrMasSysSubTasks
                        .FindAllAsNoTrackingAsync(x => x.CrMasSysSubTasksMainCode == section.MainTaskCode && x.CrMasSysSubTasksStatus != Status.Deleted);

                    var subTaskDictionary = subTasks.ToDictionary(x => x.CrMasSysSubTasksCode);

                    foreach (var mapping in section.SubTaskMappings)
                    {
                        if (validSubTaskCodes?.Contains(mapping.Code) ?? false)
                        {
                            if (subTaskDictionary.TryGetValue(mapping.Code, out var subTask))
                            {
                                sectionMenu.SubItems.Add(new SidebarMenuItem
                                {
                                    Authorization = true,
                                    Title = currentCulture == "en-US" ? subTask?.CrMasSysSubTasksEnName : subTask?.CrMasSysSubTasksArName,
                                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = "MAS" }) : mapping.Controller
                                });
                            }
                        }
                    }
                    sidebarMenu.Add(sectionMenu);
                }
            }

            return sidebarMenu;
        }
    }
}

