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

namespace Bnan.Ui.Areas.Base.ViewComponents.Components
{
    public class SidebarMenuViewComponent : ViewComponent
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly IStringLocalizer<SidebarMenuViewComponent> _localizer;
        private readonly IUnitOfWork _unitOfWork;

        public SidebarMenuViewComponent(IStringLocalizer<SidebarMenuViewComponent> localizer, UserManager<CrMasUserInformation> userManager, IUnitOfWork unitOfWork)
        {
            _localizer = localizer;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemType)
        {
            var userInfo = await GetUserInfoAsync();
            var sections = GetSectionsForSystem(systemType); // Adjusted to fetch sections based on the system
            var sidebarMenu = await BuildSidebarMenu(userInfo, systemType, sections);
            if (systemType == "CAS") return View("SidebarMenuForCAS", sidebarMenu);
            else if (systemType == "MAS") return View("SidebarMenuForMAS", sidebarMenu);
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
        private List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)>)> GetSectionsForSystem(string systemType)
        {
            if (systemType == "MAS")
            {
                return GetSectionForMas(); // Replace with your existing logic for MAS sections
            }
            else if (systemType == "CAS")
            {
                return GetSectionForCas(); // Add a similar method for CAS sections
            }
            return new List<(string, string, string, List<(string, string, string)>)>();
        }
        private List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)> SubTaskMappings)> GetSectionForMas()
        {
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
            ("1101006", "CompanyMessages", "Index"),
            ("1101007", "#", "#"),
            ("1101008", "#", "#"),
            ("1101009", "#", "#"),
            ("1101010", "#", "#")
        }),
        ("102", "/MasSystem/images/Bnan.svg", "sidebarcars", new List<(string, string, string)>
        {
            ("1102001", "#", "#"),
            ("1102002", "LessorMarketing", "Index"),
            ("1102003", "#", "#"),
            ("1102004", "#", "#"),
            ("1102005", "#", "#"),
            ("1102006", "#", "#"),
            ("1102007", "#", "#"),
            ("1102008", "#", "#"),
            ("1102009", "#", "#"),
            ("1102010", "#", "#")
        }),
        ("103", "/MasSystem/images/tenants2.svg", "sidebarRenter", new List<(string, string, string)>
        {
            ("1103001", "RenterInformation", "Index"),
            ("1103002", "RenterContract", "Index"),
            ("1103003", "#", "#"),
            ("1103004", "#", "#"),
            ("1103005", "#", "#"),
            ("1103006", "#", "#"),
            ("1103007", "#", "#"),
            ("1103008", "#", "#"),
            ("1103009", "#", "#"),
            ("1103010", "#", "#")
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
            ("1104008", "#", "#"),
            ("1104009", "#", "#"),
            ("1104010", "#", "#"),
            ("1104011", "#", "#"),
            ("1104012", "#", "#"),
            ("1104013", "#", "#"),
            ("1104014", "#", "#"),
            ("1104015", "#", "#")
        }),
        ("105", "/MasSystem/images/stat.svg", "sidebarStatistics", new List<(string, string, string)>
        {
            ("1105001", "RenterStatistics", "Index"),
            ("1105002", "CarStatistics", "Index"),
            ("1105003", "ContractStatistics", "Index"),
            ("1105004", "RenterContractStatistics", "Index"),
            ("1105005", "CarContractStatistics", "Index"),
            ("1105006", "#", "#"),
            ("1105007", "#", "#"),
            ("1105008", "#", "#"),
            ("1105009", "#", "#"),
            ("1105010", "#", "#")
        }),
        ("106", "/MasSystem/images/employees.svg", "sidebarUsers", new List<(string, string, string)>
        {
            ("1106001", "Users", "Users"),
            ("1106002", "UsersSystemValiditions", "SystemValiditions"),
            ("1106003", "UsersForCompanies", "Index"),
            ("1106004", "#", "#"),
            ("1106005", "#", "#"),
            ("1106006", "#", "#"),
            ("1106007", "#", "#"),
            ("1106008", "#", "#"),
            ("1106009", "#", "#"),
            ("1106010", "#", "#")
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
            ("1107009", "#", "#"),
            ("1107010", "#", "#")
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
            ("1108011", "#", "#"),
            ("1108012", "#", "#"),
            ("1108013", "#", "#"),
            ("1108014", "#", "#"),
            ("1108015", "#", "#"),
        }),
        ("109", "/MasSystem/images/Mail (2).svg", "sidebarMailsServices", new List<(string, string, string)>
        {
            ("1109001", "Countries", "Index"),
            ("1109002", "PostRegions", "Index"),
            ("1109003", "PostCity", "Index"),
            ("1109004", "#", "#"),
            ("1109005", "#", "#"),
            ("1109006", "#", "#"),
            ("1109007", "#", "#"),
            ("1109008", "#", "#"),
            ("1109009", "#", "#"),
            ("1109010", "#", "#"),
        }),
        ("110", "/MasSystem/images/calc.svg", "sidebarAccountsServices", new List<(string, string, string)>
        {
            ("1110001", "Bank", "Index"),
            ("1110002", "AccountPaymentMethod", "Index"),
            ("1110003", "AccountReference", "Index"),
            ("1110004", "#", "#"),
            ("1110005", "#", "#"),
            ("1110006", "#", "#"),
            ("1110007", "#", "#"),
            ("1110008", "#", "#"),
            ("1110009", "#", "#"),
            ("1110010", "#", "#"),
        }),
        ("111", "/MasSystem/images/ContractIcon.svg", "sidebarRentalContracts", new List<(string, string, string)>
        {
            ("1111001", "ContractAdditional", "Index"),
            ("1111002", "ContractOptions", "Index"),
            ("1111003", "ContractCarCheckup", "Index"),
            ("1111004", "ContractCarCheckupDetails", "Index"),
            ("1111005", "#", "#"),
            ("1111006", "#", "#"),
            ("1111007", "#", "#"),
            ("1111008", "#", "#"),
            ("1111009", "#", "#"),
            ("1111010", "#", "#"),
        }),
        ("112", "/MasSystem/images/services.svg", "services-items", new List<(string, string, string)>
        {
            ("1112001", "TechnicalConnectivity", "Connect"),
            ("1112002", "CountryClassification", "Index"),
            ("1112003", "LessorClassification", "Index"),
            ("1112004", "Rate", "Edit"),
            ("1112005", "Currency", "Edit"),
            ("1112006", "QuestionsAnswer", "Index"),
            ("1112007", "#", "#"),
            ("1112008", "#", "#"),
            ("1112009", "#", "#"),
            ("1112010", "#", "#"),
        })
    };

            return sections;
        }
        private List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)> SubTaskMappings)> GetSectionForCas()
        {
            // إعداد قائمة لتخزين البيانات الخاصة بالأقسام
            var sections = new List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)> SubTaskMappings)>
                                    {
                                        ("201", "/CasSys/images/comany .svg", "company-items", new List<(string, string, string)>
                                        {
                                            ("2201001", "Branches", "Branches"),
                                            ("2201002", "#", "#"),
                                            ("2201003", "LessorOwners_CAS", "Index"),
                                            ("2201004", "#", "#"),
                                            ("2201005", "#", "#"),
                                            ("2201006", "#", "#"),
                                            ("2201007", "#", "#"),
                                            ("2201008", "#", "#"),
                                            ("2201009", "#", "#"),
                                            ("2201010", "#", "#"),

                                        }),
                                        ("202", "/CasSys/images/cars.svg", "cars-items", new List<(string, string, string)>
                                        {
                                            ("2202001", "#", "#"),
                                            ("2202002", "#", "#"),
                                            ("2202003", "#", "#"),
                                            ("2202004", "CarRepair", "Index"),
                                            ("2202005", "CarTransefer", "Index"),
                                            ("2202006", "OwnerChanger", "Index"),
                                            ("2202007", "#", "#"),
                                            ("2202008", "CarPrice", "CarPrice"),
                                            ("2202009", "#", "#"),
                                            ("2202010", "#", "#"),
                                        }),
                                        ("203", "/CasSys/images/tenants.svg", "tenants-items", new List<(string, string, string)>
                                        {
                                            ("2203001", "RenterLessorInformation", "Index"),
                                            ("2203002", "RenterContract", "Index"),
                                            ("2203003", "RenterBalances", "Index"),
                                            ("2203004", "RenterMessages", "Index"),
                                            ("2203005", "#", "#"),
                                            ("2203006", "#", "#"),
                                            ("2203007", "#", "#"),
                                            ("2203008", "#", "#"),
                                            ("2203009", "#", "#"),
                                            ("2203010", "#", "#"),
                                        }),

                                        ("204", "/CasSys/images/accounts.svg", "accounts-items", new List<(string, string, string)>
                                        {
                                            ("2204001", "FeedBox", "FeedBox"),
                                            ("2204002", "#", "#"),
                                            ("2204003", "#", "#"),
                                            ("2204004", "#", "#"),
                                            ("2204005", "#", "#"),
                                            ("2204006", "#", "#"),
                                            ("2204007", "#", "#"),
                                            ("2204008", "#", "#"),
                                            ("2204009", "#", "#"),
                                            ("2204010", "#", "#"),
                                        }),
                                        ("205", "/CasSys/images/reports.svg", "reports-items", new List<(string, string, string)>
                                        {
                                            ("2205001", "Report_AdminstrativeProcedures_Cas", "Index"),
                                            ("2205002", "DailyReport", "Index"),
                                            ("2205003", "ReportActiveContract_Cas", "Index"),
                                            ("2205004", "ReportClosedContract_Cas", "Index"),
                                            ("2205005", "ReportSaved_CanceledContract_Cas", "Index"),
                                            ("2205006", "#", "#"),
                                            ("2205007", "ReportCarContract_cas", "Index"),
                                            ("2205008", "ReportEmployeeContract_cas", "Index"),
                                            ("2205009", "#", "#"),
                                            ("2205010", "ReportFTPemployee", "Index"),
                                            ("2205011", "ReportFTPRenter", "Index"),
                                            ("2205012", "ReportFTPsalesPoint", "Index"),
                                            ("2205013", "#", "#"),
                                            ("2205014", "#", "#"),
                                            ("2205015", "#", "#"),
                                        }),
                                        ("206", "/CasSys/images/statics.svg", "statics-items", new List<(string, string, string)>
                                        {
                                            ("2206001", "CasStatistics_Cars", "Index"),
                                            ("2206002", "CasStatistics_Renters", "Index"),
                                            ("2206003", "CasStatistics_Contracts", "Index"),
                                            ("2206004", "CasStatistics_CarContracts", "Index"),
                                            ("2206005", "CasStatistics_RenterContracts", "Index"),
                                            ("2206006", "#", "#"),
                                            ("2206007", "#", "#"),
                                            ("2206008", "#", "#"),
                                            ("2206009", "#", "#"),
                                            ("2206010", "#", "#"),
                                        }),
                                        ("207", "/CasSys/images/employees.svg", "employees-items", new List<(string, string, string)>
                                        {
                                            ("2207001", "Employees", "Index"),
                                            ("2207002", "EmployeesSystemValiditions", "Index"),
                                            ("2207003", "EmployeesContractValiditions", "Index"),
                                            ("2207004", "#", "#"),
                                            ("2207005", "#", "#"),
                                            ("2207006", "#", "#"),
                                            ("2207007", "#", "#"),
                                            ("2207008", "#", "#"),
                                            ("2207009", "#", "#"),
                                            ("2207010", "#", "#"),
                                        }),
                                        ("208", "/CasSys/images/services.svg", "services-items", new List<(string, string, string)>
                                        {
                                            ("2208001", "AccountBank_CAS", "Index"),
                                            ("2208002", "AccountSalesPoint_CAS", "Index"),
                                            ("2208003", "RenterDriver", "Index"),
                                            ("2208004", "TechnicalConnectivity", "Connect"),
                                            ("2208005", "LessorMechanism", "Mechanism"),
                                            ("2208006", "LessorMembership", "LessorMembership"),
                                            ("2208007", "#", "#"),
                                            ("2208008", "#", "#"),
                                            ("2208009", "#", "#"),
                                            ("2208010", "#", "#"),
                                        })
                                     };

            return sections;
        }
        private async Task<List<SidebarMenuItem>> BuildSidebarMenu(CrMasUserInformation userInfo, string system, List<(string MainTaskCode, string IconPath, string ItemName, List<(string Code, string Controller, string Action)> SubTaskMappings)> sections)
        {
            string currentCulture = CultureInfo.CurrentCulture.Name;

            var sidebarMenu = new List<SidebarMenuItem>();



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
                                    Url = mapping.Action != null ? Url.Action(mapping.Action, mapping.Controller, new { area = system }) : "#"
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

