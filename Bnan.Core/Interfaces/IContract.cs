using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IContract
    {

        // New System
        Task<CrMasRenterInformation> AddRenterToMASRenterInformation(CrMasRenterInformation model, string EmployerName, string day, string month, string year);
        Task<CrMasRenterPost> AddRenterForMASRenterPost(string RenterId, string cityCode, string arAdress, string enAdress);
        Task<CrCasRenterLessor> AddRenterToCasRenterLessor(string LessorCode, CrMasRenterInformation crMasRenterInformation, CrMasRenterPost crMasRenterPost);
        Task<CrMasRenterInformation> UpdateRenterToMASRenterInformation(CrMasRenterInformation model, string EmployerName,string personalType);
        Task<CrMasRenterPost> UpdateRenterForMASRenterPost(string RenterId, string cityCode, string arAdress, string enAdress);
        Task<CrCasRenterLessor> UpdateRenterToCasRenterLessor(string LessorCode, CrMasRenterInformation crMasRenterInformation, CrMasRenterPost crMasRenterPost);
        // New System
        Task<bool> AddRenterContractChoice(string LessorCode,string ContractNo, string SerialNo, string PriceNo, string Choice);
        Task<bool> AddRenterContractAdditional(string LessorCode, string ContractNo, string SerialNo, string PriceNo, string Choice);
        Task<bool> AddRenterContractAdvantages(CrCasPriceCarAdvantage crCasPriceCarAdvantage, string ContractNo);
        Task<bool> AddRenterContractCheckUp(string LessorCode, string ContractNo, string SerialNo, string PriceNo, string CheckUpCode, string ReasonCheckCode, bool Status, string Reasons);
        Task<CrCasRenterContractBasic> AddRenterContractBasic(string LessorCode, string BranchCode, string ContractNo, string RenterId,string sectorCodeForRenter, string DriverId, string PrivateDriver,
                                                       string AdditionalDriver, string SerialNo, string PriceNo, string DaysNo, string UserFreeHour, string UserFreeKm,
                                                       string CurrentMeter, string OptionsTotal, string AdditionalTotal, string ContractValueAfterDiscount,
                                                       string DiscountValue, string ContractValueBeforeDiscount, string TaxValue, string TotalAmount, string UserInsert,
                                                       string Authrization,string UserDiscount, string AmountPayed, string ContractPdf,string PolicyCode,string SourceCode, string Reasons);
        Task<bool> AddRenterContractAuthrization(string ContractNo,string LessorCode,string AuthrizationType,string AuthrizationValue);
        Task<bool> UpdateCarInformation(string SerialNo, string LessorCode, string BranchCode, DateTime LastContract, int DaysNo, int CurrentMeter,string ExpireMaintainceCount);
        Task<string> UpdateCarDocMaintainance(string SerialNo, string LessorCode, string BranchCode, int CurrentMeter);
        Task<bool> UpdateRenterLessor(string RenterId, string LessorCode,DateTime LastContract,decimal TotalPayed, decimal RequiredValue, string RenterReasons);
        Task<bool> UpdateMasRenter(string RenterId);
        Task<bool> UpdateDriverStatus(string DriverId, string LessorCode,string Reasons);
        Task<bool> UpdatePrivateDriverStatus(string PrivateDriverId, string LessorCode);
        Task<CrCasAccountReceipt> AddAccountReceipt(string ContractNo, string LessorCode,string BranchCode,string PaymentMethod,string Account,string SerialNo,string SalesPointNo,
                                     decimal TotalPayed,string RenterId,string sector, string UserId, string PassingType,string Reasons,string pdfPath);

        Task<bool> AddAccountInvoice(string ContractNo,string RenterId, string sector, string LessorCode, string BranchCode, string UserId, string AccountReceiptNo, string pdfPath);
        Task<bool> UpdateBranchBalance(string BranchCode, string LessorCode, decimal AmountPaid);
        Task<bool> UpdateSalesPointBalance(string BranchCode, string LessorCode,string SalesPointCode, decimal AmountPaid);
        Task<bool> UpdateBranchValidity(string BranchCode, string LessorCode,string UserId,string PaymentMethod, decimal AmountPaid);
        Task<bool> UpdateUserBalance(string BranchCode, string LessorCode,string UserId,string PaymentMethod, decimal AmountPaid);
        Task<bool> AddRenterAlert(string ContractNo, string LessorCode,string BranchCode, int RentalDays, DateTime ContractEndDate, string SerialNo, string PriceNo);
        Task<bool> AddRenterEvaluation(string ContractNo, string LessorCode,string RenterId, string TypeEvaluation, string UserId);
        Task<bool> AddRenterStatistics(CrCasRenterContractBasic Contract);
        Task<bool> UpdateRenterSignture(string RenterId, string ImagePath );


    }
}
