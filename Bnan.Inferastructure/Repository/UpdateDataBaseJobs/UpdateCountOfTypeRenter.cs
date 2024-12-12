using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.UpdateDataBaseJobs
{
    public class UpdateCountOfTypeRenter : IUpdateCountOfTypeRenter
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCountOfTypeRenter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CrMasRenterInformation>> GetActiveRenters()
        {
            var renters = await _unitOfWork.CrMasRenterInformation.FindAllAsync(x => x.CrMasRenterInformationStatus == Status.Active);
            return renters.ToList();
        }
        public async Task<List<CrCasCarInformation>> GetActiveCars()
        {
            var Cars = await _unitOfWork.CrCasCarInformation.FindAllAsync(x => x.CrCasCarInformationStatus != Status.Deleted);
            return Cars.ToList();
        }
        public async Task<List<CrMasRenterPost>> GetActivePostRenter()
        {
            var rentersPost = await _unitOfWork.CrMasRenterPost.FindAllAsync(x => x.CrMasRenterPostStatus != Status.Deleted);
            return rentersPost.ToList();
        }



        public async Task UpdateNationalitiesCount(List<CrMasRenterInformation> renters)
        {
            // حساب عدد المستأجرين لكل جنسية
            var nationalityCounts = renters
                .GroupBy(r => r.CrMasRenterInformationNationality) // يفترض وجود NationalityId في جدول المستأجرين
                .Select(group => new
                {
                    NationalityCode = group.Key,
                    Count = group.Count()
                }).ToList();

            // تحديث جدول الجنسيات
            foreach (var nationalityCount in nationalityCounts)
            {
                var nationality = await _unitOfWork.CrMasSupRenterNationality.FindAsync(x => x.CrMasSupRenterNationalitiesCode == nationalityCount.NationalityCode);
                if (nationality != null)
                {
                    nationality.CrMasSupRenterNationalitiesCounter = nationalityCount.Count;
                    _unitOfWork.CrMasSupRenterNationality.Update(nationality);
                }
            }

            // حفظ التغييرات
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateKeyCallingsCount(List<CrMasRenterInformation> renters)
        {
            // حساب عدد المستأجرين لكل جنسية
            var CallingKeysCount = renters
                .GroupBy(r => r.CrMasRenterInformationCountreyKey) // يفترض وجود NationalityId في جدول المستأجرين
                .Select(group => new
                {
                    CallingKeyCode = group.Key,
                    Count = group.Count()
                }).ToList();

            // تحديث جدول الجنسيات
            foreach (var callingKeyCount in CallingKeysCount)
            {
                var callingkey = await _unitOfWork.CrMasSysCallingKeys.FindAsync(x => x.CrMasSysCallingKeysNo == callingKeyCount.CallingKeyCode);
                if (callingkey != null)
                {
                    callingkey.CrMasSysCallingKeysCount = callingKeyCount.Count;
                    _unitOfWork.CrMasSysCallingKeys.Update(callingkey);
                }
            }

            // حفظ التغييرات
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateColorCarsCount(List<CrCasCarInformation> cars)
        {
            // حساب اللون الرئيسي للسيارة
            var colorCarsCount = cars
                .GroupBy(r => r.CrCasCarInformationMainColor) // يفترض وجود NationalityId في جدول المستأجرين
                .Select(group => new
                {
                    ColorCode = group.Key,
                    Count = group.Count()
                }).ToList();

            // تحديث جدول الالوان
            foreach (var colorCar in colorCarsCount)
            {
                var color = await _unitOfWork.CrMasSupCarColor.FindAsync(x => x.CrMasSupCarColorCode == colorCar.ColorCode);
                if (color != null)
                {
                    color.CrMasSupCarColorCounter = colorCar.Count;
                    _unitOfWork.CrMasSupCarColor.Update(color);
                }
            }
            // حفظ التغييرات
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateCountriesPostRenterCount(List<CrMasRenterPost> rentersPost)
        {
            // حساب عدد المستأجرين لكل مدينه
            var renterPostCount = rentersPost
                .GroupBy(r => r.CrMasRenterPostCity) // يفترض وجود NationalityId في جدول المستأجرين
                .Select(group => new
                {
                    CityCode = group.Key,
                    Count = group.Count()
                }).ToList();

            // تحديث جدول المدن
            foreach (var renterPost in renterPostCount)
            {
                var post = await _unitOfWork.CrMasSupPostCity.FindAsync(x => x.CrMasSupPostCityCode == renterPost.CityCode);
                if (post != null)
                {
                    post.CrMasSupPostCityCounter = renterPost.Count;
                    _unitOfWork.CrMasSupPostCity.Update(post);
                }
            }
            // حفظ التغييرات
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateDistributionCarCount()
        {
            var distributions = await _unitOfWork.CrMasSupCarDistribution.FindAllAsync(x => x.CrMasSupCarDistributionStatus == Status.Active);
            var cars = await _unitOfWork.CrCasCarInformation
                .FindAllAsNoTrackingAsync(x => x.CrCasCarInformationStatus != Status.Sold && x.CrCasCarInformationStatus != Status.Deleted);

            var updatedDistributions = new List<CrMasSupCarDistribution>();

            foreach (var distribution in distributions)
            {
                var count = cars.Count(x => x.CrCasCarInformationDistribution == distribution.CrMasSupCarDistributionCode);
                if (count > 0 && count != distribution.CrMasSupCarDistributionCount)
                {
                    distribution.CrMasSupCarDistributionCount = count;
                    updatedDistributions.Add(distribution);
                }
            }
            if (updatedDistributions.Any())
            {
                _unitOfWork.CrMasSupCarDistribution.UpdateRange(updatedDistributions);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
