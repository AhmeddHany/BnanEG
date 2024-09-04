using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.MiddleWare
{
    public class LogoutMiddleware
    {
        private readonly RequestDelegate _next;

        public LogoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<CrMasUserInformation> userManager, SignInManager<CrMasUserInformation> signInManager, IUnitOfWork unitOfWork)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var userInfo = await unitOfWork.CrMasUserInformation.GetByIdAsync(user.Id);
                    var timeToday = DateTime.Now;
                    var exitTimer = (double)(userInfo.CrMasUserInformationExitTimer);
                    if (userInfo != null && userInfo.CrMasUserInformationOperationStatus == false && user.CrMasUserInformationLastActionDate?.AddMinutes(exitTimer + 3) <= timeToday)
                    {
                        await signInManager.SignOutAsync();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers["Location"] = "/Identity/Account/Login";
                        return;
                    }
                }
            }
            await _next(context);
        }

       
    }
}
