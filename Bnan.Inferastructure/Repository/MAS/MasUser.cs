using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasUser : IMasUser
    {
        public IUnitOfWork _unitOfWork;

        public MasUser(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasUserInformation>> GetAllAsync()
        {
            var result = await _unitOfWork.CrMasUserInformation.GetAllAsyncAsNoTrackingAsync();
            return result.ToList();
        }



        public async Task<bool> ExistsByDetailsAsync(CrMasUserInformation entity)
        {
            var allUsers = await GetAllAsync();

            return allUsers.Any(x =>
                x.CrMasUserInformationCode != entity.CrMasUserInformationCode && x.CrMasUserInformationLessor == entity.CrMasUserInformationLessor && // Exclude the current entity being updated
                (
                    x.CrMasUserInformationArName == entity.CrMasUserInformationArName ||
                    x.CrMasUserInformationEnName.ToLower().Equals(entity.CrMasUserInformationEnName.ToLower())
                )
            );
        }


        public async Task<bool> ExistsByArabicNameAsync(string arabicName, string code, string lessorCode)
        {
            if (string.IsNullOrEmpty(arabicName)) return false;
            return await _unitOfWork.CrMasUserInformation
                .FindAsync(x => x.CrMasUserInformationArName == arabicName && x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode != code) != null;
        }

        public async Task<bool> ExistsByEnglishNameAsync(string englishName, string code, string lessorCode)
        {
            if (string.IsNullOrEmpty(englishName)) return false;
            return await _unitOfWork.CrMasUserInformation
                .FindAsync(x => x.CrMasUserInformationEnName.ToLower().Equals(englishName.ToLower()) && x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode != code) != null;
        }

        public async Task<bool> ExistsByUserCodeAsync(string userCode)
        {
            if (string.IsNullOrEmpty(userCode)) return false;
            return await _unitOfWork.CrMasUserInformation
                .FindAsync(x => x.CrMasUserInformationCode == userCode) != null;
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, string userCode)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await _unitOfWork.CrMasUserInformation
                .FindAsync(x => x.CrMasUserInformationId == userId && x.CrMasUserInformationCode != userCode) != null;
        }

        public async Task<bool> UpdateUser(CrMasUserInformation model)
        {
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == model.CrMasUserInformationCode);
            if (user == null) return false;
            user.CrMasUserInformationMobileNo = model.CrMasUserInformationMobileNo;
            user.CrMasUserInformationCallingKey = model.CrMasUserInformationCallingKey;
            user.CrMasUserInformationTasksArName = model.CrMasUserInformationTasksArName;
            user.CrMasUserInformationTasksEnName = model.CrMasUserInformationTasksEnName;
            user.CrMasUserInformationEmail = model.CrMasUserInformationEmail;
            user.CrMasUserInformationReasons = model.CrMasUserInformationReasons;
            if (_unitOfWork.CrMasUserInformation.Update(user) != null) return true;
            return false;
        }

        //public async Task<bool> CheckIfCanDeleteIt(string code)
        //{
        //    var rentersLicenceCount = await _unitOfWork.CrMasRenterInformation.CountAsync(x => x.CrMasRenterInformationDrivingLicenseType == code);
        //    return rentersLicenceCount == 0;
        //}
    }
}
