using Bnan.Core.Models;
using Bnan.Ui.ViewModels.MAS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Bnan.Ui.Areas.MAS.Controllers.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly IStringLocalizer<HomeController> _localizer;

        public SidebarMenuViewComponent(IStringLocalizer<HomeController> localizer, UserManager<CrMasUserInformation> userManager)
        {
            _localizer = localizer;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userInfo = await GetUserInfoAsync();
            var sidebarMenu = BuildSidebarMenu(userInfo);
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

        private List<SidebarMenuItem> BuildSidebarMenu(CrMasUserInformation userInfo)
        {
            var sidebarMenu = new List<SidebarMenuItem>();
            // Company Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "101" && l.CrMasUserMainValidationAuthorization == true))
            {
                var companyMenu = new SidebarMenuItem
                {
                    Title = _localizer["Companies"],
                    ItemName = "company-items",
                    IconPath = "/MasSystem/images/Comanies.svg",
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101001" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["CompaniesInformations"], Url = Url.Action("Index", "LessorsKSA", new { area = "MAS" }) });

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101002" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Supportphotos"], Url = Url.Action("Index", "LessorImages", new { area = "MAS" }) });

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101003" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Contracts"], Url = Url.Action("Index", "CompanyContracts", new { area = "MAS" }) });

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101004" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Dues"], Url = Url.Action("Index", "CompanyDues", new { area = "MAS" }) });

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101005" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Clearingdues"], Url = Url.Action("Index", "CompanyOwed", new { area = "MAS" }) });
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101006" && l.CrMasUserSubValidationAuthorization == true))
                    companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["MessagesCompanies"], Url = "#" });

                // Add more submenu items as needed based on permissions

                sidebarMenu.Add(companyMenu);
            }

            // Bnan Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "102" && l.CrMasUserMainValidationAuthorization == true))
            {
                var bnanMenu = new SidebarMenuItem
                {
                    Title = _localizer["Bnan"],
                    ItemName = "sidebarcars",
                    IconPath = "/MasSystem/images/Bnan.svg", // استخدم المسار الصحيح للأيقونة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1102001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    bnanMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["SupportingCompanies"], Url = "#" });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1102002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    bnanMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["MarketingCompanies"], Url = "#" });
                }

                sidebarMenu.Add(bnanMenu);
            }

            // Renter Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "103" && l.CrMasUserMainValidationAuthorization == true))
            {
                var renterMenu = new SidebarMenuItem
                {
                    Title = _localizer["Renter"],
                    ItemName = "sidebarRenter",
                    IconPath = "/MasSystem/images/tenants2.svg", // استخدم المسار الصحيح للأيقونة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1103001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    renterMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Renter Data"], Url = Url.Action("Index", "RenterInformation", new { area = "MAS" }) });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1103002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    renterMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Renter Contract"], Url = Url.Action("Index", "RenterContract", new { area = "MAS" }) });
                }
                sidebarMenu.Add(renterMenu);
            }

            // Reports Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "104" && l.CrMasUserMainValidationAuthorization == true))
            {
                var reportsMenu = new SidebarMenuItem
                {
                    Title = _localizer["Reports"],
                    ItemName = "sidebarReport",
                    IconPath = "/MasSystem/images/reports.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["EmployeeProcedure"],
                        Url = Url.Action("Index", "Report1", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Existing Contracts"],
                        Url = Url.Action("Index", "ReportContract", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Closed Contracts"],
                        Url = Url.Action("FailedMessageReport_NoData", "ReportFClosedContract", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Canceled Contracts"],
                        Url = Url.Action("FailedMessageReport_NoData", "ReportCanceledContract", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104005" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Leased Car Contracts"],
                        Url = Url.Action("FailedMessageReport_NoData", "CarContract", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104006" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Shomos System Contracts"],
                        Url = "#"
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1104007" && l.CrMasUserSubValidationAuthorization == true))
                {
                    reportsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Rental System Contracts"],
                        Url = "#"
                    });
                }

                sidebarMenu.Add(reportsMenu);
            }

            // Statistics 
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "105" && l.CrMasUserMainValidationAuthorization == true))
            {
                var statisticsMenu = new SidebarMenuItem
                {
                    Title = _localizer["Statistics"],
                    ItemName = "sidebarStatistics",
                    IconPath = "/MasSystem/images/stat.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1105001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    statisticsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Tenant Statistics"],
                        Url = Url.Action("Index", "RenterStatistics", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1105002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    statisticsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Car Statistics"],
                        Url = Url.Action("Index", "CarStatistics", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1105003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    statisticsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Contract Statisticss"],
                        Url = Url.Action("Index", "ContractStatistics", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1105004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    statisticsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Car Contract Statistics"],
                        Url = Url.Action("Index", "CarContractStatistics", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1105005" && l.CrMasUserSubValidationAuthorization == true))
                {
                    statisticsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Tenant Contracts Statistics"],
                        Url = Url.Action("Index", "RenterContractStatistics", new { area = "MAS" })
                    });
                }

                sidebarMenu.Add(statisticsMenu);
            }

            // Users Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "106" && l.CrMasUserMainValidationAuthorization == true))
            {
                var usersMenu = new SidebarMenuItem
                {
                    Title = _localizer["Users"],
                    ItemName = "sidebarUsers",
                    IconPath = "/MasSystem/images/employees.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1106001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    usersMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["UsersInfo"],
                        Url = Url.Action("Users", "Users", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1106002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    usersMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Users' Responsibilities System"],
                        Url = Url.Action("SystemValiditions", "Users", new { area = "MAS" })
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1106003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    usersMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["CompanyUsers"],
                        Url = "#"
                    });
                }

                sidebarMenu.Add(usersMenu);
            }

            // Renters Services Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "107" && l.CrMasUserMainValidationAuthorization == true))
            {
                var rentersServicesMenu = new SidebarMenuItem
                {
                    Title = _localizer["RentersServices"],
                    ItemName = "sidebarUsersServices",
                    IconPath = "/MasSystem/images/tenants2.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["UserIdType"],
                        Url = Url.Action("Index", "RenterIdType", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["DrivingLicenseType"],
                        Url = Url.Action("Index", "RenterDrivingLicense", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Nationalities"],
                        Url = Url.Action("Index", "RenterNationality", new { area = "MAS" })
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Gender"],
                        Url = Url.Action("Index", "RenterGender", new { area = "MAS" })
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107005" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Professions"],
                        Url = Url.Action("Index", "RenterProfession", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107006" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Employers"],
                        Url = Url.Action("Index", "RenterEmployer", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107007" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Membership"],
                        Url = Url.Action("Index", "RenterMembership", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1107008" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentersServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Sectors"],
                        Url = Url.Action("Index", "RenterSector", new { area = "MAS" })
                    });
                }

                sidebarMenu.Add(rentersServicesMenu);
            }

            // Cars Services Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "108" && l.CrMasUserMainValidationAuthorization == true))
            {
                var carsServicesMenu = new SidebarMenuItem
                {
                    Title = _localizer["CarsServices"],
                    ItemName = "sidebarCarsServices",
                    IconPath = "/MasSystem/images/cars.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["RegistrationType"],
                        Url = Url.Action("Index", "CarRegistration", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["FuelType"],
                        Url = Url.Action("Index", "CarFuel", new { area = "MAS" })
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Oils"],
                        Url = Url.Action("Index", "CarOil", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["CVTType"],
                        Url = Url.Action("Index", "CarCvt", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108005" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Brands"],
                        Url = Url.Action("Index", "CarBrand", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108006" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Models"],
                        Url = Url.Action("Index", "CarModel", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108007" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Categories"],
                        Url = Url.Action("Index", "CarCategory", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108008" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["CategoriesDistribution"],
                        Url = Url.Action("CarDistribution", "CarDistribution", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108009" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Advantages_Additional"],
                        Url = Url.Action("Index", "CarAdvantage", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1108010" && l.CrMasUserSubValidationAuthorization == true))
                {
                    carsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Colors"],
                        Url = Url.Action("Index", "CarColor", new { area = "MAS" })
                    });
                }

                sidebarMenu.Add(carsServicesMenu);
            }

            // Mails Services Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "109" && l.CrMasUserMainValidationAuthorization == true))
            {
                var mailsServicesMenu = new SidebarMenuItem
                {
                    Title = _localizer["MailsServices"],
                    ItemName = "sidebarMailsServices",
                    IconPath = "/MasSystem/images/Mail (2).svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };
                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1109001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    mailsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Countries"],
                        Url = "#"
                    });
                }
                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1109002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    mailsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Regions"],
                        Url = Url.Action("Index", "PostRegion", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1109003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    mailsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Cities"],
                        Url = Url.Action("Index", "PostCity", new { area = "MAS" })
                    });
                }

                sidebarMenu.Add(mailsServicesMenu);
            }

            // Accounts Services Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "110" && l.CrMasUserMainValidationAuthorization == true))
            {
                var accountsServicesMenu = new SidebarMenuItem
                {
                    Title = _localizer["AccountsServices"],
                    ItemName = "sidebarAccountsServices",
                    IconPath = "/MasSystem/images/calc.svg", // استبدل هذا المسار بأيقونة مناسبة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1110001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    accountsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Banks"],
                        Url = Url.Action("Index", "Bank", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1110002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    accountsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["PaymentMethods"],
                        Url = Url.Action("Index", "AccountPaymentMethod", new { area = "MAS" })
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1110003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    accountsServicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Reference"],
                        Url = Url.Action("Index", "AccountReference", new { area = "MAS" })
                    });
                }

                sidebarMenu.Add(accountsServicesMenu);
            }

            // RentalContracts Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "111" && l.CrMasUserMainValidationAuthorization == true))
            {
                var rentalContractsMenu = new SidebarMenuItem
                {
                    Title = _localizer["RentalContracts"],
                    ItemName = "sidebarRentalContracts",
                    IconPath = "/MasSystem/images/ContractIcon.svg", // استخدم المسار الصحيح للأيقونة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1111001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentalContractsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Additionals"],
                        Url = Url.Action("Index", "ContractAdditional", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1111002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentalContractsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Choices"],
                        Url = Url.Action("Index", "ContractOptions", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1111003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentalContractsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["VirtualInspection"],
                        Url = Url.Action("Index", "CarCheckup", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1111004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    rentalContractsMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["VirtualInspectionDetails"],
                        Url = "#" // استخدم Url الفعلي للـ Action
                    });
                }

                sidebarMenu.Add(rentalContractsMenu);
            }

            // Services Section
            if (userInfo?.CrMasUserMainValidations != null &&
                userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "112" && l.CrMasUserMainValidationAuthorization == true))
            {

                var servicesMenu = new SidebarMenuItem
                {
                    Title = _localizer["Services"],
                    ItemName = "services-items",
                    IconPath = "/MasSystem/images/services.svg", // استخدم المسار الصحيح للأيقونة
                    SubItems = new List<SidebarMenuItem>(),
                    Authorization = true,
                };

                // Add submenu items based on permissions
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112001" && l.CrMasUserSubValidationAuthorization == true))
                {
                    servicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["TechnicalConnectivity"],
                        Url = "#" // استخدم Url الفعلي للـ Action
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112002" && l.CrMasUserSubValidationAuthorization == true))
                {
                    servicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["ClassificationOfCountries"],
                        Url = "#" // استخدم Url الفعلي للـ Action
                    });
                }


                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112003" && l.CrMasUserSubValidationAuthorization == true))
                {
                    servicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["ClassificationOfCompanies"],
                        Url = Url.Action("Index", "LessorClassification", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                    });
                }
                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112004" && l.CrMasUserSubValidationAuthorization == true))
                {
                    servicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Evaluation"],
                        Url = Url.Action("Index", "Evaluation", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                    });
                }

                if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112005" && l.CrMasUserSubValidationAuthorization == true))
                {
                    servicesMenu.SubItems.Add(new SidebarMenuItem
                    {
                        Authorization = true,
                        Title = _localizer["Currency"],
                        Url = "#" // استخدم Url الفعلي للـ Action
                    });
                }

                //if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112003" && l.CrMasUserSubValidationAuthorization == true))
                //{
                //    servicesMenu.SubItems.Add(new SidebarMenuItem
                //    {
                //        Authorization = true,
                //        Title = _localizer["InternationalKeys"],
                //        Url = Url.Action("Index", "InternationalKeys", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                //    });
                //}

                //if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112002" && l.CrMasUserSubValidationAuthorization == true))
                //{
                //    servicesMenu.SubItems.Add(new SidebarMenuItem
                //    {
                //        Authorization = true,
                //        Title = _localizer["FrequentlyAskedQuestions"],
                //        Url = Url.Action("Index", "FrequentlyAskedQuestions", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                //    });
                //}

                //if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1112004" && l.CrMasUserSubValidationAuthorization == true))
                //{
                //    servicesMenu.SubItems.Add(new SidebarMenuItem
                //    {
                //        Authorization = true,
                //        Title = _localizer["TranslatingNumbers"],
                //        Url = Url.Action("Index", "TranslatingNumbers", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                //    });
                //}



                // Add Messages To Users as a static item
                servicesMenu.SubItems.Add(new SidebarMenuItem
                {
                    Authorization = true,
                    Title = _localizer["Messages To Users"],
                    Url = Url.Action("Index", "MessagesToUsers", new { area = "MAS" }) // استخدم Url الفعلي للـ Action
                });

                sidebarMenu.Add(servicesMenu);
            }

            return sidebarMenu;
        }
    }
}

