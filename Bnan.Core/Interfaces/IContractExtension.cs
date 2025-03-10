﻿using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IContractExtension
    {
        Task<CrCasRenterContractBasic> AddRenterExtensionContract(string ContractNo, string DaysNo, string UserInsert, string AmountPayed, string Reasons);
        Task<bool> UpdateStatusOldContract(string ContractNo);
        Task<bool> UpdateAlertContract(CrCasRenterContractBasic Contract);
        Task<bool> UpdateRenterLessor(string RenterId, string LessorCode, decimal TotalPayed, decimal RequiredValue);
        Task<bool> UpdateBranchBalance(string BranchCode, string LessorCode, decimal AmountPaid);
        Task<bool> UpdateSalesPointBalance(string BranchCode, string LessorCode, string SalesPointCode, decimal AmountPaid);
        Task<bool> UpdateBranchValidity(string BranchCode, string LessorCode, string UserId, string PaymentMethod, decimal AmountPaid);
        Task<bool> UpdateUserBalance(string BranchCode, string LessorCode, string UserId, string PaymentMethod, decimal AmountPaid);
        Task<CrCasAccountReceipt> AddAccountReceipt(string ContractNo, string LessorCode, string BranchCode, string PaymentMethod, string Account, string SerialNo, string SalesPointNo,
                                     decimal TotalPayed, string RenterId, string UserId, string PassingType, string Reasons,string SavePdf);
        Task<bool> AddAccountInvoice(string ContractNo, string RenterId, string LessorCode, string BranchCode, string UserId, string AccountReceiptNo, string pdfPath);

    }
}
