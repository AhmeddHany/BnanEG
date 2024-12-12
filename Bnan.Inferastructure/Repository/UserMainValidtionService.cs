using Bnan.Core.Interfaces;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository
{
    public class UserMainValidtionService : IUserMainValidtion
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _UserService;

        public UserMainValidtionService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _UserService = userService;
        }

        //public async Task<bool> AddMainValidaitionToUserWhenAddLessor(string userCode)
        //{
        //    var MainTasks = _unitOfWork.CrMasSysMainTasks.FindAll(l => l.CrMasSysMainTasksStatus == "A" && l.CrMasSysMainTasksSystem == "2");
        //    var user = await _UserService.GetUserByUserNameAsync(userCode);


        //    foreach (var item in MainTasks)
        //    {
        //        CrMasUserMainValidation CrMasUserMainValidation = new CrMasUserMainValidation();
        //        if (item.CrMasSysMainTasksCode == "206")
        //        {
        //            CrMasUserMainValidation = new CrMasUserMainValidation
        //            {
        //                CrMasUserMainValidationUser = user.CrMasUserInformationCode,
        //                CrMasUserMainValidationMainTasks = item.CrMasSysMainTasksCode,
        //                CrMasUserMainValidationMainSystem = "2",
        //                CrMasUserMainValidationAuthorization = true
        //            };
        //            await _unitOfWork.CrMasUserMainValidations.AddAsync(CrMasUserMainValidation);
        //        }
        //        /*  else
        //          {
        //              CrMasUserMainValidation = new CrMasUserMainValidation
        //              {
        //                  CrMasUserMainValidationUser = user.CrMasUserInformationCode,
        //                  CrMasUserMainValidationMainTasks = item.CrMasSysMainTasksCode,
        //                  CrMasUserMainValidationMainSystem = "2",
        //                  CrMasUserMainValidationAuthorization = false
        //              };
        //          }*/

        //    }



        //    return true;
        //}

        public async Task<bool> AddMainValidaitionToUserCASFromMAS(string userCode)
        {
            var MainTasks = await _unitOfWork.CrMasSysMainTasks.FindAllAsNoTrackingAsync(l => l.CrMasSysMainTasksStatus == "A" && l.CrMasSysMainTasksSystem == "2" &&
                                                                                             (l.CrMasSysMainTasksCode == "207" || l.CrMasSysMainTasksCode == "208"));
            var user = await _UserService.GetUserByUserNameAsync(userCode);
            foreach (var item in MainTasks)
            {
                CrMasUserMainValidation CrMasUserMainValidation = new CrMasUserMainValidation();
                CrMasUserMainValidation = new CrMasUserMainValidation
                {
                    CrMasUserMainValidationUser = user.CrMasUserInformationCode,
                    CrMasUserMainValidationMainTasks = item.CrMasSysMainTasksCode,
                    CrMasUserMainValidationMainSystem = "2",
                    CrMasUserMainValidationAuthorization = true
                };
                if (await _unitOfWork.CrMasUserMainValidations.AddAsync(CrMasUserMainValidation) == null) return false;
            }
            return true;
        }

        public async Task<bool> AddMainValiditionsForEachUser(string userCode, string systemCode)
        {
            var mainTasks = await _unitOfWork.CrMasSysMainTasks.FindAllAsNoTrackingAsync(x => x.CrMasSysMainTasksSystem == systemCode);
            foreach (var item in mainTasks)
            {
                if (item.CrMasSysMainTasksCode != "207")
                {
                    CrMasUserMainValidation crMasUserMainValidation = new CrMasUserMainValidation();
                    crMasUserMainValidation.CrMasUserMainValidationUser = userCode;
                    crMasUserMainValidation.CrMasUserMainValidationMainSystem = systemCode;
                    crMasUserMainValidation.CrMasUserMainValidationMainTasks = item.CrMasSysMainTasksCode;
                    crMasUserMainValidation.CrMasUserMainValidationAuthorization = false;
                    if (await _unitOfWork.CrMasUserMainValidations.AddAsync(crMasUserMainValidation) == null) return false;
                }
            }
            return true;
        }
    }
}
