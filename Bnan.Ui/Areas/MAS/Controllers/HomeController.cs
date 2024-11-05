using AutoMapper;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Bnan.Inferastructure.Extensions;
using Bnan.Ui.Areas.Base.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Bnan.Ui.Areas.MAS.Controllers
{
    [Area("MAS")]
    [Authorize(Roles = "MAS")]
    public class HomeController : BaseController
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        public HomeController(IStringLocalizer<HomeController> localizer, BnanSCContext context, IUnitOfWork unitOfWork, UserManager<CrMasUserInformation> userManager, IMapper mapper) : base(userManager, unitOfWork, mapper)
        {
            _localizer = localizer;
        }
        public async Task<IActionResult> Index()
        {
            //To Set Title 
            var titles = await setTitle("105", "1105001", "1");
            await ViewData.SetPageTitleAsync(titles[0], "", "", "", "", titles[3]);
            // Retrieve user information
            var user = await _userManager.GetUserAsync(User);

            //var userInfo = await _userManager.Users
            // .Include(l => l.CrMasUserMainValidations)
            // .Include(l => l.CrMasUserSubValidations)
            // .Include(l => l.CrMasUserInformationLessorNavigation)
            // .FirstOrDefaultAsync(l => l.UserName == user.UserName);
            //// Generate dynamic sidebar menu
            //var sidebarMenu = BuildSidebarMenu(userInfo);
            //ViewData["SidebarMenu"] = sidebarMenu;
            return View();
        }
        //public List<SidebarMenuItem> BuildSidebarMenu(CrMasUserInformation userInfo)
        //{
        //    var sidebarMenu = new List<SidebarMenuItem>();

        //    if (userInfo?.CrMasUserMainValidations != null &&
        //        userInfo.CrMasUserMainValidations.Any(l => l.CrMasUserMainValidationMainTasks == "101" && l.CrMasUserMainValidationAuthorization == true))
        //    {
        //        var companyMenu = new SidebarMenuItem
        //        {
        //            Title = _localizer["Companies"],
        //            ItemName = "company-items",
        //            IconPath = "/MasSystem/images/Comanies.svg",
        //            SubItems = new List<SidebarMenuItem>(),
        //            Authorization = true,
        //        };

        //        // Add submenu items based on permissions
        //        if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101001" && l.CrMasUserSubValidationAuthorization == true))
        //            companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["CompaniesInformations"], Url = Url.Action("Index", "LessorsKSA", new { area = "MAS" }) });

        //        if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101002" && l.CrMasUserSubValidationAuthorization == true))
        //            companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Supportphotos"], Url = Url.Action("Index", "LessorImages", new { area = "MAS" }) });

        //        if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101003" && l.CrMasUserSubValidationAuthorization == true))
        //            companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Contracts"], Url = Url.Action("Index", "CompanyContracts", new { area = "MAS" }) });

        //        if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101004" && l.CrMasUserSubValidationAuthorization == true))
        //            companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Dues"], Url = Url.Action("Index", "CompanyDues", new { area = "MAS" }) });

        //        if (userInfo.CrMasUserSubValidations.Any(l => l.CrMasUserSubValidationSubTasks == "1101005" && l.CrMasUserSubValidationAuthorization == true))
        //            companyMenu.SubItems.Add(new SidebarMenuItem { Authorization = true, Title = _localizer["Clearingdues"], Url = Url.Action("Index", "CompanyOwed", new { area = "MAS" }) });

        //        // Add more submenu items as needed based on permissions

        //        sidebarMenu.Add(companyMenu);
        //    }

        //    return sidebarMenu;
        //}
        [HttpGet]
        public IActionResult SetLanguage(string returnUrl, string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );

            return LocalRedirect(returnUrl);
        }
    }
}
