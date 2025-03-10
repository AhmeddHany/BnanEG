﻿using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Repository
{
    public class Custody : ICustody
    {
        private IUnitOfWork _unitOfWork;

        public Custody(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> UpdateAccountReceipt(string ReceiptNo, string ReferenceNo)
        {
            var receipt = await _unitOfWork.CrCasAccountReceipt.FindAsync(x => x.CrCasAccountReceiptNo == ReceiptNo);
            if (receipt != null)
            {
                receipt.CrCasAccountReceiptIsPassing = "2";
                receipt.CrCasAccountReceiptPassingReference = ReferenceNo;
                if (_unitOfWork.CrCasAccountReceipt.Update(receipt) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateBranch(string BranchCode, string lessorCode, string Creditor)
        {
            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationCode == BranchCode);
            if (branch != null)
            {
                if (branch.CrCasBranchInformationAvailableBalance == null) branch.CrCasBranchInformationAvailableBalance = 0;
                if (branch.CrCasBranchInformationReservedBalance == null) branch.CrCasBranchInformationReservedBalance = 0;

                branch.CrCasBranchInformationAvailableBalance -= decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                branch.CrCasBranchInformationReservedBalance += decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                if (_unitOfWork.CrCasBranchInformation.Update(branch) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserInfo(string UserCode, string lessorCode, string Creditor)
        {
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode == UserCode);
            if (user != null)
            {
                user.CrMasUserInformationAvailableBalance -= decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                user.CrMasUserInformationReservedBalance += decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                if (_unitOfWork.CrMasUserInformation.Update(user) != null) return true;
            }
            return false;
        }
        public async Task<bool> UpdateSalesPoint(string lessorCode, string BranchCode, string SalesPointCode, string Creditor)
        {
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointLessor == lessorCode &&
                                                                               x.CrCasAccountSalesPointCode == SalesPointCode &&
                                                                               x.CrCasAccountSalesPointBrn == BranchCode);
            if (SalesPoint != null)
            {
                SalesPoint.CrCasAccountSalesPointTotalAvailable -= decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                SalesPoint.CrCasAccountSalesPointTotalReserved += decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                if (_unitOfWork.CrCasAccountSalesPoint.Update(SalesPoint) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateBranchValidity(string UserCode, string lessorCode, string BranchCode, string BankNo, string Creditor)
        {
            var UserBranchValididy = await _unitOfWork.CrMasUserBranchValidity.FindAsync(x => x.CrMasUserBranchValidityId == UserCode && x.CrMasUserBranchValidityLessor == lessorCode &&
                                                                                           x.CrMasUserBranchValidityBranch == BranchCode);
            if (UserBranchValididy != null && BankNo != "" && BankNo != null)
            {
                if (BankNo == "00")
                {
                    UserBranchValididy.CrMasUserBranchValidityBranchCashAvailable -= decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                    UserBranchValididy.CrMasUserBranchValidityBranchCashReserved += decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                }
                else
                {
                    UserBranchValididy.CrMasUserBranchValidityBranchSalesPointAvailable -= decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                    UserBranchValididy.CrMasUserBranchValidityBranchSalesPointReserved += decimal.Parse(Creditor, CultureInfo.InvariantCulture);
                }
                if (_unitOfWork.CrMasUserBranchValidity.Update(UserBranchValididy) != null) return true;

            }
            return false;
        }
        public async Task<bool> AddAccountReceiptReceivedCustody(string AdmintritiveNo, string lessorCode, string BranchCode,
                                                                 string TotalAmount,string userCode,string SavePdfReceipt, string reasons)
        {

            CrCasAccountReceipt receipt = new CrCasAccountReceipt();
            var adminstritive = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAsync(x => x.CrCasSysAdministrativeProceduresNo == AdmintritiveNo);
            var accountReceipt = _unitOfWork.CrCasAccountReceipt.Find(x => x.CrCasAccountReceiptPassingReference == AdmintritiveNo);
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == accountReceipt.CrCasAccountReceiptSalesPoint);
            var UserTarget = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationCode == userCode.Trim());
            var userValidity = await _unitOfWork.CrMasUserBranchValidity.FindAsync(x => x.CrMasUserBranchValidityLessor == lessorCode && x.CrMasUserBranchValidityBranch == BranchCode && x.CrMasUserBranchValidityId == userCode);
            // Get Previous Balance From Branch Validity and plus above it admistrive creditor 
            var userBranchValidityBalance = userValidity.CrMasUserBranchValidityBranchCashAvailable + userValidity.CrMasUserBranchValidityBranchSalesPointAvailable + userValidity.CrMasUserBranchValidityBranchTransferAvailable+adminstritive.CrCasSysAdministrativeProceduresCreditor;
            receipt.CrCasAccountReceiptNo = GetAccountReceiptNo(adminstritive.CrCasSysAdministrativeProceduresBranch,lessorCode);
            receipt.CrCasAccountReceiptYear = DateTime.Now.ToString("yy");
            receipt.CrCasAccountReceiptType = "302";
            receipt.CrCasAccountReceiptLessorCode = lessorCode;
            receipt.CrCasAccountReceiptBranchCode = BranchCode;
            receipt.CrCasAccountReceiptDate = DateTime.Now;
            if (SalesPoint.CrCasAccountSalesPointBank == "00") receipt.CrCasAccountReceiptPaymentMethod = "10";
            else receipt.CrCasAccountReceiptPaymentMethod = "40";
            receipt.CrCasAccountReceiptReferenceType = "15";
            receipt.CrCasAccountReceiptReferenceNo = adminstritive.CrCasSysAdministrativeProceduresNo;
            receipt.CrCasAccountReceiptReceipt = decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
            receipt.CrCasAccountReceiptPayment = 0;
            receipt.CrCasAccountReceiptBank = SalesPoint.CrCasAccountSalesPointBank;
            receipt.CrCasAccountReceiptAccount = SalesPoint.CrCasAccountSalesPointAccountBank;
            receipt.CrCasAccountReceiptSalesPoint = SalesPoint.CrCasAccountSalesPointCode;
            receipt.CrCasAccountReceiptSalesPointPreviousBalance = SalesPoint.CrCasAccountSalesPointTotalBalance;
            receipt.CrCasAccountReceiptUser = UserTarget.CrMasUserInformationCode;
            receipt.CrCasAccountReceiptUserPreviousBalance = UserTarget.CrMasUserInformationTotalBalance;
            receipt.CrCasAccountReceiptBranchUserPreviousBalance = userBranchValidityBalance;
            receipt.CrCasAccountReceiptIsPassing = "3";
            receipt.CrCasAccountReceiptPassingUser = adminstritive.CrCasSysAdministrativeProceduresUserInsert;
            receipt.CrCasAccountReceiptPassingDate = DateTime.Now;
            receipt.CrCasAccountReceiptPdfFile = SavePdfReceipt;
            receipt.CrCasAccountReceiptReasons = reasons;
            if (await _unitOfWork.CrCasAccountReceipt.AddAsync(receipt) != null) return true;
            return false;
        }

        public async Task<bool> UpdateAdminstritive(string AdminstritiveNo, string UserCode, string status, string Reasons)
        {
            var Adminstritive = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAsync(x => x.CrCasSysAdministrativeProceduresNo == AdminstritiveNo);
            if (Adminstritive != null)
            {
                Adminstritive.CrCasSysAdministrativeProceduresStatus = status;
                Adminstritive.CrCasSysAdministrativeProceduresUserInsert = UserCode;
                if (!string.IsNullOrEmpty(Reasons)) Adminstritive.CrCasSysAdministrativeProceduresReasons = Reasons;
                if (status==Status.Reject)
                {
                    Adminstritive.CrCasSysAdministrativeProceduresArDescription = "تم رفض استلام العهدة";
                    Adminstritive.CrCasSysAdministrativeProceduresEnDescription = "Receipt custody was refused";

                }
                else
                {
                    Adminstritive.CrCasSysAdministrativeProceduresArDescription = "تم قبول استلام العهدة";
                    Adminstritive.CrCasSysAdministrativeProceduresEnDescription = "Receipt custody was accepted";
                }
                if (_unitOfWork.CrCasSysAdministrativeProcedure.Update(Adminstritive) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateBranchReceivedCustody(string BranchCode, string lessorCode, string TotalAmount, string status)
        {
            var branch = await _unitOfWork.CrCasBranchInformation.FindAsync(x => x.CrCasBranchInformationLessor == lessorCode && x.CrCasBranchInformationCode == BranchCode);
            if (branch != null)
            {
                if (branch.CrCasBranchInformationAvailableBalance == null) branch.CrCasBranchInformationAvailableBalance = 0;
                if (branch.CrCasBranchInformationReservedBalance == null) branch.CrCasBranchInformationReservedBalance = 0;
                if (status == Status.Accept)
                {
                    branch.CrCasBranchInformationReservedBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    branch.CrCasBranchInformationTotalBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }
                else
                {
                    branch.CrCasBranchInformationReservedBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    branch.CrCasBranchInformationAvailableBalance += decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }

                if (_unitOfWork.CrCasBranchInformation.Update(branch) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateSalesPointReceivedCustody(string lessorCode, string BranchCode, string AdminstritiveNo, string TotalAmount, string status)
        {
            var accountReceipt = _unitOfWork.CrCasAccountReceipt.Find(x => x.CrCasAccountReceiptPassingReference == AdminstritiveNo);
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == accountReceipt.CrCasAccountReceiptSalesPoint);

            if (SalesPoint != null)
            {
                if (status == Status.Accept)
                {
                    SalesPoint.CrCasAccountSalesPointTotalBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    SalesPoint.CrCasAccountSalesPointTotalReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }
                else
                {
                    SalesPoint.CrCasAccountSalesPointTotalAvailable += decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    SalesPoint.CrCasAccountSalesPointTotalReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }

                if (_unitOfWork.CrCasAccountSalesPoint.Update(SalesPoint) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserInfoReceivedCustody(string UserCode, string lessorCode, string TotalAmount, string status)
        {
            var user = await _unitOfWork.CrMasUserInformation.FindAsync(x => x.CrMasUserInformationLessor == lessorCode && x.CrMasUserInformationCode == UserCode);
            if (user != null)
            {
                if (status == Status.Accept)
                {
                    user.CrMasUserInformationTotalBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    user.CrMasUserInformationReservedBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }
                else
                {
                    user.CrMasUserInformationAvailableBalance += decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    user.CrMasUserInformationReservedBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                }

                if (_unitOfWork.CrMasUserInformation.Update(user) != null) return true;
            }
            return false;
        }

        public async Task<bool> UpdateBranchValidityReceivedCustody(string UserCode, string lessorCode, string BranchCode, string AdminstritiveNo, string TotalAmount, string status)
        {
            var UserBranchValididy = await _unitOfWork.CrMasUserBranchValidity.FindAsync(x => x.CrMasUserBranchValidityId == UserCode && x.CrMasUserBranchValidityLessor == lessorCode &&
                                                                               x.CrMasUserBranchValidityBranch == BranchCode);

            var accountReceipt = _unitOfWork.CrCasAccountReceipt.Find(x => x.CrCasAccountReceiptPassingReference == AdminstritiveNo);
            var SalesPoint = await _unitOfWork.CrCasAccountSalesPoint.FindAsync(x => x.CrCasAccountSalesPointCode == accountReceipt.CrCasAccountReceiptSalesPoint);


            if (UserBranchValididy != null && SalesPoint.CrCasAccountSalesPointBank != "" && SalesPoint.CrCasAccountSalesPointBank != null)
            {
                if (SalesPoint.CrCasAccountSalesPointBank == "00")
                {
                    if (status == Status.Accept)
                    {
                        UserBranchValididy.CrMasUserBranchValidityBranchCashBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                        UserBranchValididy.CrMasUserBranchValidityBranchCashReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        UserBranchValididy.CrMasUserBranchValidityBranchCashAvailable += decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                        UserBranchValididy.CrMasUserBranchValidityBranchCashReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    }

                }
                else
                {
                    if (status == Status.Accept)
                    {
                        UserBranchValididy.CrMasUserBranchValidityBranchSalesPointBalance -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                        UserBranchValididy.CrMasUserBranchValidityBranchSalesPointReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        UserBranchValididy.CrMasUserBranchValidityBranchSalesPointAvailable += decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                        UserBranchValididy.CrMasUserBranchValidityBranchSalesPointReserved -= decimal.Parse(TotalAmount, CultureInfo.InvariantCulture);
                    }

                }
                if (_unitOfWork.CrMasUserBranchValidity.Update(UserBranchValididy) != null) return true;

            }
            return false;
        }

        public bool UpdateAccountReceiptReceivedCustody(string AdminstritiveNo, string UserCode, string status, string Reasons)
        {
            var AccountReceipts = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptPassingReference == AdminstritiveNo);
            var PasssingNo = "";
            if (status == Status.Accept) PasssingNo = "3";
            else PasssingNo = "1";
            if (AccountReceipts != null && AccountReceipts.Count() != 0)
            {
                foreach (var AccountReceipt in AccountReceipts)
                {
                    if (status == Status.Reject) AccountReceipt.CrCasAccountReceiptPassingReference = null;
                    if(status==Status.Accept) AccountReceipt.CrCasAccountReceiptPassingDate = DateTime.Now.Date;
                    AccountReceipt.CrCasAccountReceiptIsPassing = PasssingNo;
                    AccountReceipt.CrCasAccountReceiptPassingUser = UserCode;
                    AccountReceipt.CrCasAccountReceiptReasons = Reasons;
                    _unitOfWork.CrCasAccountReceipt.Update(AccountReceipt);
                }
                return true;
            }
            return false;
        }

        public string GetAccountReceiptNo(string BranchCode, string LessorCode)
        {
           
            DateTime year = DateTime.Now;
            var y = year.ToString("yy");
            var Lrecord = _unitOfWork.CrCasAccountReceipt.FindAll(x => x.CrCasAccountReceiptLessorCode == LessorCode &&
                x.CrCasAccountReceiptType == "302"
                && x.CrCasAccountReceiptYear == y).Max(x => x.CrCasAccountReceiptNo.Substring(x.CrCasAccountReceiptNo.Length - 6, 6));
            string Serial;
            if (Lrecord != null)
            {
                Int64 val = Int64.Parse(Lrecord) + 1;
                Serial = val.ToString("000000");
            }
            else
            {
                Serial = "000001";
            }
            var receipt = y + "-" + "1" + "302" + "-" + LessorCode + BranchCode + "-" + Serial;
            return receipt;
        }
    }
}
