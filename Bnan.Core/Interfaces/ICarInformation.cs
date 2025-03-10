﻿using Bnan.Core.Models;

namespace Bnan.Core.Interfaces
{
    public interface ICarInformation
    {
        Task<bool> AddCarInformation(CrCasCarInformation crCasCarInformation);
        Task<bool> AddAdvantagesToCar(string serialNumber, string advantageCode, string lessor, string distributionCode, string status);
        Task<bool> UpdateCarInformation(CrCasCarInformation crCasCarInformation);
        Task<bool> UpdateAdvantagesToCar(string serialNumber, string advantageCode, string lessor, string status);
        Task<string> UpdateCarToSale(CrCasCarInformation crCasCarInformation);
        Task<bool> SoldCar(CrCasCarInformation crCasCarInformation);
        Task<bool> CancelOffer(string serialNo, string lessorCode);
    }
}
