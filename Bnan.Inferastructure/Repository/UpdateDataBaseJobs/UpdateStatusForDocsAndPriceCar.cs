using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Bnan.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bnan.Inferastructure.Repository.UpdateDataBaseJobs
{
    public class UpdateStatusForDocsAndPriceCar : IUpdateStatusForDocsAndPriceCar
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UpdateStatusForContracts> _logger;
        public UpdateStatusForDocsAndPriceCar(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, ILogger<UpdateStatusForContracts> logger)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task UpdateBranchDocuments()
        {
            var branchDocuments = await _unitOfWork.CrCasBranchDocument.FindAllAsync(x => x.CrCasBranchDocumentsStatus != Status.Expire);
            var updatedBranchDocuments = new List<CrCasBranchDocument>();
            foreach (var branchDocument in branchDocuments)
            {
                var originalStatus = branchDocument.CrCasBranchDocumentsStatus;
                if (branchDocument.CrCasBranchDocumentsDateAboutToFinish <= DateTime.Now.Date) branchDocument.CrCasBranchDocumentsStatus = Status.AboutToExpire;
                if (branchDocument.CrCasBranchDocumentsEndDate <= DateTime.Now.Date) branchDocument.CrCasBranchDocumentsStatus = Status.Expire;
                if (branchDocument.CrCasBranchDocumentsStatus != originalStatus) updatedBranchDocuments.Add(branchDocument);
            }

            if (updatedBranchDocuments.Any())
            {
                _unitOfWork.CrCasBranchDocument.UpdateRange(updatedBranchDocuments);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdateCarDocumentsAndMaintaince()
        {
            // Fetch all car documents that are not expired
            var carDocumentsMaintenances = await _unitOfWork.CrCasCarDocumentsMaintenance.FindAllAsync(x => x.CrCasCarDocumentsMaintenanceStatus != Status.Expire);
            var updatedCarDocuments = new List<CrCasCarDocumentsMaintenance>();
            var updatedCarInformations = new List<CrCasCarInformation>();
            foreach (var carDocument in carDocumentsMaintenances)
            {
                var originalStatus = carDocument.CrCasCarDocumentsMaintenanceStatus;
                if (carDocument.CrCasCarDocumentsMaintenanceDateAboutToFinish <= DateTime.Now.Date) carDocument.CrCasCarDocumentsMaintenanceStatus = Status.AboutToExpire;
                if (carDocument.CrCasCarDocumentsMaintenanceEndDate <= DateTime.Now.Date)
                {
                    var car = _unitOfWork.CrCasCarInformation.Find(x => x.CrCasCarInformationSerailNo == carDocument.CrCasCarDocumentsMaintenanceSerailNo);
                    carDocument.CrCasCarDocumentsMaintenanceStatus = Status.Expire;
                    car.CrCasCarInformationDocumentationStatus = false;
                    if (!updatedCarInformations.Contains(car)) updatedCarInformations.Add(car);
                }
                if (carDocument.CrCasCarDocumentsMaintenanceStatus != originalStatus) updatedCarDocuments.Add(carDocument);
            }
            if (updatedCarDocuments.Any()) _unitOfWork.CrCasCarDocumentsMaintenance.UpdateRange(updatedCarDocuments);
            if (updatedCarInformations.Any()) _unitOfWork.CrCasCarInformation.UpdateRange(updatedCarInformations);
            // Save changes to the database
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateCompanyContracts()
        {
            var contractCompanies = await _unitOfWork.CrMasContractCompany.FindAllAsync(x => x.CrMasContractCompanyStatus != Status.Expire);
            var updatedContractCompanies = new List<CrMasContractCompany>();
            foreach (var contractCompany in contractCompanies)
            {
                var originalStatus = contractCompany.CrMasContractCompanyStatus;
                if (contractCompany.CrMasContractCompanyAboutToExpire <= DateTime.Now.Date) contractCompany.CrMasContractCompanyStatus = Status.AboutToExpire;
                if (contractCompany.CrMasContractCompanyEndDate <= DateTime.Now.Date) contractCompany.CrMasContractCompanyStatus = Status.Expire;
                if (contractCompany.CrMasContractCompanyStatus != originalStatus) updatedContractCompanies.Add(contractCompany);
            }
            if (updatedContractCompanies.Any())
            {
                _unitOfWork.CrMasContractCompany.UpdateRange(updatedContractCompanies);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task UpdatePricesCar()
        {
            var priceCarBasics = await _unitOfWork.CrCasPriceCarBasic.FindAllAsync(x => x.CrCasPriceCarBasicStatus != Status.Expire);
            var updatedPriceCarBasics = new List<CrCasPriceCarBasic>();
            var updatedCarInformations = new List<CrCasCarInformation>();
            foreach (var priceCarBasic in priceCarBasics)
            {
                var originalStatus = priceCarBasic.CrCasPriceCarBasicStatus;
                if (priceCarBasic.CrCasPriceCarBasicDateAboutToFinish <= DateTime.Now.Date) priceCarBasic.CrCasPriceCarBasicStatus = Status.AboutToExpire;

                if (priceCarBasic.CrCasPriceCarBasicEndDate <= DateTime.Now.Date)
                {
                    priceCarBasic.CrCasPriceCarBasicStatus = Status.Expire;
                    var cars = await _unitOfWork.CrCasCarInformation
                        .FindAllAsync(x => x.CrCasCarInformationDistribution == priceCarBasic.CrCasPriceCarBasicDistributionCode &&
                                   x.CrCasCarInformationModel == priceCarBasic.CrCasPriceCarBasicModelCode &&
                                   x.CrCasCarInformationCategory == priceCarBasic.CrCasPriceCarBasicCategoryCode &&
                                   x.CrCasCarInformationBrand == priceCarBasic.CrCasPriceCarBasicBrandCode);

                    foreach (var car in cars)
                    {
                        car.CrCasCarInformationPriceStatus = false;
                        if (!updatedCarInformations.Contains(car))
                        {
                            updatedCarInformations.Add(car);
                        }
                    }
                }
                if (priceCarBasic.CrCasPriceCarBasicStatus != originalStatus) updatedPriceCarBasics.Add(priceCarBasic);

            }
            if (updatedPriceCarBasics.Any()) _unitOfWork.CrCasPriceCarBasic.UpdateRange(updatedPriceCarBasics);
            if (updatedCarInformations.Any()) _unitOfWork.CrCasCarInformation.UpdateRange(updatedCarInformations);
            // Save changes to the database
            await _unitOfWork.CompleteAsync();
        }
    }
}
