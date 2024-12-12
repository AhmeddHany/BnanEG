namespace Bnan.Core.Interfaces.UpdateDataBaseJobs
{
    public interface IUpdateStatusForDocsAndPriceCar
    {
        Task UpdateBranchDocuments();
        Task UpdateCompanyContracts();
        Task UpdateCarDocumentsAndMaintaince();
        Task UpdatePricesCar();
    }
}
