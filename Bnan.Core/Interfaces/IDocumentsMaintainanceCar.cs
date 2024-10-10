using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface IDocumentsMaintainanceCar
    {
        Task<bool> AddDocumentCar(string serialNumber, string lessorCode, string branchCode, int currentMeter);
        Task<bool> AddMaintainaceCar(string serialNumber, string lessorCode, string branchCode, int currentMeter);
        Task<bool> UpdateDocumentCar(CrCasCarDocumentsMaintenance crCasCarDocumentsMaintenance);
        Task<bool> UpdateMaintainceCar(CrCasCarDocumentsMaintenance crCasCarDocumentsMaintenance);
        Task<bool> CheckMaintainceAndDocsCar(string serialNumber, string lessorCode, string classificationCode, string procudureCode);

    }
}
