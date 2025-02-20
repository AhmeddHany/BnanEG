using Bnan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IUserLoginsService
    {
        Task SaveTracing(string userCode, string operationAr, string operationEn, string mainTaskCode, string subTaskCode,
            string mainTaskAr, string subTaskAr, string mainTaskEn, string subTaskEn, string systemCode, string systemAr, string systemEn);

        Task SaveTracing(string userCode, string RecordAr, string RecordEn, string operationAr, string operationEn, string mainTaskCode, string subTaskCode,
            string mainTaskAr, string subTaskAr, string mainTaskEn, string subTaskEn, string systemCode, string systemAr, string systemEn);

        Task SaveAdminstrative(string sector, string ProcedureCode, string lessorCode, string branchCode, string Classification, string? Target, string? operationAr, string? operationEn
            , string UserInsert, string? Status, string? Reasons, string? CarFrom, string? CarTo, decimal? Debit, decimal? Credit, string? Doc_No, DateTime? Doc_Date, DateTime? Doc_Start, DateTime? Doc_End);

        Task SaveAdminstrative_with_Set_Date(DateTime SetedDate,string sector, string ProcedureCode, string lessorCode, string branchCode, string Classification, string? Target, string? operationAr, string? operationEn
    , string UserInsert, string? Status, string? Reasons, string? CarFrom, string? CarTo, decimal? Debit, decimal? Credit, string? Doc_No, DateTime? Doc_Date, DateTime? Doc_Start, DateTime? Doc_End);
        
    }
}
