using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bnan.Inferastructure.Quartz
{
    public class UpdateStatusForDocsAndPriceCarJob : IJob
    {
        private readonly IUpdateStatusForDocsAndPriceCar _forDocsAndPriceCar;
        private readonly ILogger<UpdateStatusForDocsAndPriceCarJob> _logger;

        public UpdateStatusForDocsAndPriceCarJob(IUpdateStatusForDocsAndPriceCar forDocsAndPriceCar, ILogger<UpdateStatusForDocsAndPriceCarJob> logger)
        {
            _forDocsAndPriceCar = forDocsAndPriceCar;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting UpdateBranchDocuments at: {Time}", DateTime.Now);
            await _forDocsAndPriceCar.UpdateBranchDocuments();
            _logger.LogInformation("Completed UpdateBranchDocuments at: {Time}", DateTime.Now);

            _logger.LogInformation("Starting UpdateCarDocumentsAndMaintaince at: {Time}", DateTime.Now);
            await _forDocsAndPriceCar.UpdateCarDocumentsAndMaintaince();
            _logger.LogInformation("Completed UpdateCarDocumentsAndMaintaince at: {Time}", DateTime.Now);

            _logger.LogInformation("Starting UpdatePricesCar at: {Time}", DateTime.Now);
            await _forDocsAndPriceCar.UpdatePricesCar();
            _logger.LogInformation("Completed UpdatePricesCar at: {Time}", DateTime.Now);

            _logger.LogInformation("Starting UpdateCompanyContracts at: {Time}", DateTime.Now);
            await _forDocsAndPriceCar.UpdateCompanyContracts();
            _logger.LogInformation("Completed UpdateCompanyContracts at: {Time}", DateTime.Now);
        }
    }
}
