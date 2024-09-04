using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Bnan.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;


namespace Bnan.Inferastructure.Repository.UpdateDataBaseJobs
{
    public class UpdateStatusForUser : IUpdateStatusForUser
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _client;
        private readonly ILogger<UpdateStatusForUser> _logger;
        private readonly SignInManager<CrMasUserInformation> _signInManager;
        public UpdateStatusForUser(IUnitOfWork unitOfWork, HttpClient client, ILogger<UpdateStatusForUser> logger, SignInManager<CrMasUserInformation> signInManager)
        {
            _unitOfWork = unitOfWork;
            _client = client;
            _logger = logger;
            _signInManager = signInManager;
        }
        public async Task RefreshLogin()
        {
            var users = await _unitOfWork.CrMasUserInformation.FindAllAsync(d => d.CrMasUserInformationStatus != "D" && d.CrMasUserInformationOperationStatus == true);
            foreach (var user in users)
            {
                var timeToday = DateTime.Now;
                var exitTimer = (double)(user.CrMasUserInformationExitTimer);
                if (user.CrMasUserInformationLastActionDate?.AddMinutes(exitTimer+3) <= timeToday)
                {
                    user.CrMasUserInformationOperationStatus = false;
                    _unitOfWork.CrMasUserInformation.Update(user);
                }
            }
            // Complete unit of work if changes were made
            if (users.Any(u => u.CrMasUserInformationOperationStatus == false))
            {
               await _unitOfWork.CompleteAsync();
            }
        }
    }
}
