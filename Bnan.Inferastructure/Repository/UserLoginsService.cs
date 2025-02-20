using Bnan.Core.Interfaces;
using Bnan.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Bnan.Inferastructure.Repository
{
    public class UserLoginsService : BaseRepository<CrMasUserLogin>, IUserLoginsService
    {
        private readonly UserManager<CrMasUserInformation> _userManager;
        private readonly SignInManager<CrMasUserInformation> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;


        public UserLoginsService(UserManager<CrMasUserInformation> userManager, SignInManager<CrMasUserInformation> signInManager, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, BnanSCContext db) : base(db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }
        public async Task SaveTracing(string userCode, string operationAr, string operationEn, string mainTaskCode, string subTaskCode,
            string mainTaskAr, string subTaskAr, string mainTaskEn, string subTaskEn, string systemCode, string systemAr, string systemEn)
        {
            int newLoginNo;
            CrMasUserLogin userLogin = new CrMasUserLogin();

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            // to get last record to auto increament
            var userLogins = _unitOfWork.CrMasUserLogins.Count();
            if (userLogins == 0)
            {
                newLoginNo = 1;
            }
            else
            {
                var lastUserLogin = _unitOfWork.CrMasUserLogins.GetAll().OrderByDescending(x => x.CrMasUserLoginNo).FirstOrDefault(x => x.CrMasUserLoginNo != null);
                newLoginNo = lastUserLogin != null ? lastUserLogin.CrMasUserLoginNo + 1 : 1;
            }

            var currentDate = DateTime.Now;
            TimeSpan loginEntryTime = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);
            if (user != null)
            {
                userLogin.CrMasUserLoginNo = newLoginNo;
                userLogin.CrMasUserLoginEntryDate = currentDate.Date;
                userLogin.CrMasUserLoginEntryTime = loginEntryTime;
                userLogin.CrMasUserLoginUser = user.CrMasUserInformationCode;
                userLogin.CrMasUserLoginLessor = user.CrMasUserInformationLessor;
                userLogin.CrMasUserLoginBranch = user.CrMasUserInformationDefaultBranch;
                userLogin.CrMasUserLoginSystem = systemCode;
                userLogin.CrMasUserLoginMainTask = mainTaskCode;
                userLogin.CrMasUserLoginSubTask = subTaskCode;
                userLogin.CrMasUserLoginArOperation = operationAr;
                userLogin.CrMasUserLoginEnOperation = operationEn;
                //userLogin.CrMasUserLoginConcatenateOperationArDescription = $"{user.CrMasUserInformationArName.Trim()} - {systemAr} - {mainTaskAr} - {subTaskAr} - {operationAr}";
                //userLogin.CrMasUserLoginConcatenateOperationEnDescription = $"{user.CrMasUserInformationEnName.Trim()} - {systemEn} - {mainTaskEn} - {mainTaskEn} - {operationEn}";
                userLogin.CrMasUserLoginConcatenateOperationArDescription = $"{systemAr} - {mainTaskAr} - {subTaskAr} - {operationAr}";
                userLogin.CrMasUserLoginConcatenateOperationEnDescription = $"{systemEn} - {mainTaskEn} - {subTaskEn} - {operationEn}";
                await _unitOfWork.CrMasUserLogins.AddAsync(userLogin);
                await _unitOfWork.CompleteAsync();

            }
        }

        public async Task SaveTracing(string userCode, string recordAr, string recordEn, string operationAr, string operationEn, string mainTaskCode, string subTaskCode, string mainTaskAr,
                                        string subTaskAr, string mainTaskEn, string subTaskEn, string systemCode, string systemAr, string systemEn)
        {
            var userLogin = new CrMasUserLogin();
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (currentUser == null) return;

            // تعيين رقم الدخول (Login No) الجديد
            userLogin.CrMasUserLoginNo = await GetNewLoginNumberAsync();

            // إعداد تاريخ ووقت الدخول الحالي
            var currentDate = DateTime.Now;
            userLogin.CrMasUserLoginEntryDate = currentDate.Date;
            userLogin.CrMasUserLoginEntryTime = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);

            // تعبئة الحقول بالمعلومات الضرورية
            userLogin.CrMasUserLoginUser = currentUser.CrMasUserInformationCode;
            userLogin.CrMasUserLoginLessor = currentUser.CrMasUserInformationLessor;
            userLogin.CrMasUserLoginBranch = currentUser.CrMasUserInformationDefaultBranch;
            userLogin.CrMasUserLoginSystem = systemCode;
            userLogin.CrMasUserLoginMainTask = mainTaskCode;
            userLogin.CrMasUserLoginSubTask = subTaskCode;
            userLogin.CrMasUserLoginArOperation = operationAr;
            userLogin.CrMasUserLoginEnOperation = operationEn;

            // وصف العملية باللغة العربية والإنجليزية
            userLogin.CrMasUserLoginConcatenateOperationArDescription = $"{systemAr} - {mainTaskAr} - {subTaskAr} - {operationAr} - {recordAr}";
            userLogin.CrMasUserLoginConcatenateOperationEnDescription = $"{systemEn} - {mainTaskEn} - {subTaskEn} - {operationEn} - {recordEn}";

            // حفظ في قاعدة البيانات
            await _unitOfWork.CrMasUserLogins.AddAsync(userLogin);
            await _unitOfWork.CompleteAsync();
        }

        private async Task<int> GetNewLoginNumberAsync()
        {
            var userLoginsCount = await _unitOfWork.CrMasUserLogins.CountAsync();

            if (userLoginsCount == 0) return 1;

            // الحصول على آخر رقم مستخدم إذا كان موجودًا
            var lastUserLogin = _unitOfWork.CrMasUserLogins
                .GetAll()
                .OrderByDescending(x => x.CrMasUserLoginNo)
                .FirstOrDefault(x => x.CrMasUserLoginNo != null);

            return lastUserLogin?.CrMasUserLoginNo + 1 ?? 1;
        }
        public async Task SaveAdminstrative_with_Set_Date(DateTime SetedDate ,string sector, string ProcedureCode, string lessorCode, string branchCode, string Classification, string? Target, string? operationAr, string? operationEn
            , string UserInsert, string? Status, string? Reasons, string? CarFrom, string? CarTo, decimal? Debit, decimal? Credit, string? Doc_No, DateTime? Doc_Date, DateTime? Doc_Start, DateTime? Doc_End)
        {
            var thisProcedure = new CrCasSysAdministrativeProcedure();
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (currentUser == null) return;

            // إعداد تاريخ ووقت الدخول الحالي
            var currentDate = DateTime.Now;

            var year = currentDate.ToString("yy", CultureInfo.InvariantCulture);

            var CodeWithout_serial = (year + "-" + sector + ProcedureCode + "-" + lessorCode + branchCode + "-") ?? "";

            // تعيين رقم الدخول (Login No) الجديد
            thisProcedure.CrCasSysAdministrativeProceduresNo = await GetNewAdminstrativeNumberAsync(CodeWithout_serial);


            thisProcedure.CrCasSysAdministrativeProceduresDate = SetedDate.Date;
            thisProcedure.CrCasSysAdministrativeProceduresTime = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);

            // تعبئة الحقول بالمعلومات الضرورية
            thisProcedure.CrCasSysAdministrativeProceduresYear = year;
            thisProcedure.CrCasSysAdministrativeProceduresSector = sector;
            thisProcedure.CrCasSysAdministrativeProceduresCode = ProcedureCode;
            thisProcedure.CrCasSysAdministrativeProceduresClassification = Classification;

            thisProcedure.CrCasSysAdministrativeProceduresLessor = lessorCode;
            thisProcedure.CrCasSysAdministrativeProceduresBranch = branchCode;

            thisProcedure.CrCasSysAdministrativeProceduresTargeted = Target;
            thisProcedure.CrCasSysAdministrativeProceduresDebit = Debit;
            thisProcedure.CrCasSysAdministrativeProceduresCreditor = Credit;
            thisProcedure.CrCasSysAdministrativeProceduresDocNo = Doc_No;
            thisProcedure.CrCasSysAdministrativeProceduresDocDate = Doc_Date;
            thisProcedure.CrCasSysAdministrativeProceduresDocStartDate = Doc_Start;
            thisProcedure.CrCasSysAdministrativeProceduresDocEndDate = Doc_End;

            thisProcedure.CrCasSysAdministrativeProceduresCarFrom = CarFrom;
            thisProcedure.CrCasSysAdministrativeProceduresCarTo = CarTo;

            thisProcedure.CrCasSysAdministrativeProceduresUserInsert = UserInsert;
            thisProcedure.CrCasSysAdministrativeProceduresArDescription = operationAr;
            thisProcedure.CrCasSysAdministrativeProceduresEnDescription = operationEn;

            thisProcedure.CrCasSysAdministrativeProceduresStatus = Status;
            thisProcedure.CrCasSysAdministrativeProceduresReasons = Reasons;

            //// وصف العملية باللغة العربية والإنجليزية
            //thisProcedure.CrMasUserLoginConcatenateOperationArDescription = $"{systemAr} - {mainTaskAr} - {subTaskAr} - {operationAr} - {recordAr}";
            //thisProcedure.CrMasUserLoginConcatenateOperationEnDescription = $"{systemEn} - {mainTaskEn} - {subTaskEn} - {operationEn} - {recordEn}";

            // حفظ في قاعدة البيانات
            await _unitOfWork.CrCasSysAdministrativeProcedure.AddAsync(thisProcedure);
            await _unitOfWork.CompleteAsync();
        }

        public async Task SaveAdminstrative(string sector, string ProcedureCode, string lessorCode, string branchCode, string Classification, string? Target, string? operationAr, string? operationEn
            , string UserInsert, string? Status, string? Reasons, string? CarFrom, string? CarTo, decimal? Debit, decimal? Credit, string? Doc_No, DateTime? Doc_Date, DateTime? Doc_Start, DateTime? Doc_End)      
        {
            var thisProcedure = new CrCasSysAdministrativeProcedure();
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (currentUser == null) return;

            // إعداد تاريخ ووقت الدخول الحالي
            var currentDate = DateTime.Now;

            var year = currentDate.ToString("yy",CultureInfo.InvariantCulture);

            var CodeWithout_serial = (year+"-"+ sector+ ProcedureCode+"-"+ lessorCode+ branchCode+"-") ??"";

            // تعيين رقم الدخول (Login No) الجديد
            thisProcedure.CrCasSysAdministrativeProceduresNo = await GetNewAdminstrativeNumberAsync(CodeWithout_serial);


            thisProcedure.CrCasSysAdministrativeProceduresDate = currentDate.Date;
            thisProcedure.CrCasSysAdministrativeProceduresTime = new TimeSpan(currentDate.Hour, currentDate.Minute, currentDate.Second);

            // تعبئة الحقول بالمعلومات الضرورية
            thisProcedure.CrCasSysAdministrativeProceduresYear = year;
            thisProcedure.CrCasSysAdministrativeProceduresSector = sector;
            thisProcedure.CrCasSysAdministrativeProceduresCode = ProcedureCode;
            thisProcedure.CrCasSysAdministrativeProceduresClassification = Classification;

            thisProcedure.CrCasSysAdministrativeProceduresLessor = lessorCode;
            thisProcedure.CrCasSysAdministrativeProceduresBranch = branchCode;

            thisProcedure.CrCasSysAdministrativeProceduresTargeted = Target;
            thisProcedure.CrCasSysAdministrativeProceduresDebit = Debit;
            thisProcedure.CrCasSysAdministrativeProceduresCreditor = Credit;
            thisProcedure.CrCasSysAdministrativeProceduresDocNo = Doc_No;
            thisProcedure.CrCasSysAdministrativeProceduresDocDate = Doc_Date;
            thisProcedure.CrCasSysAdministrativeProceduresDocStartDate = Doc_Start;
            thisProcedure.CrCasSysAdministrativeProceduresDocEndDate = Doc_End;

            thisProcedure.CrCasSysAdministrativeProceduresCarFrom = CarFrom;
            thisProcedure.CrCasSysAdministrativeProceduresCarTo = CarTo;

            thisProcedure.CrCasSysAdministrativeProceduresUserInsert = UserInsert;
            thisProcedure.CrCasSysAdministrativeProceduresArDescription = operationAr;
            thisProcedure.CrCasSysAdministrativeProceduresEnDescription = operationEn;

            thisProcedure.CrCasSysAdministrativeProceduresStatus = Status;
            thisProcedure.CrCasSysAdministrativeProceduresReasons = Reasons;

            //// وصف العملية باللغة العربية والإنجليزية
            //thisProcedure.CrMasUserLoginConcatenateOperationArDescription = $"{systemAr} - {mainTaskAr} - {subTaskAr} - {operationAr} - {recordAr}";
            //thisProcedure.CrMasUserLoginConcatenateOperationEnDescription = $"{systemEn} - {mainTaskEn} - {subTaskEn} - {operationEn} - {recordEn}";

            // حفظ في قاعدة البيانات
            await _unitOfWork.CrCasSysAdministrativeProcedure.AddAsync(thisProcedure);
            await _unitOfWork.CompleteAsync();
        }


        private async Task<string> GetNewAdminstrativeNumberAsync(string AdminstrativeId_withoutSerial)
        {
            var AdminstrativeCount = await _unitOfWork.CrCasSysAdministrativeProcedure.FindAllAsync(x=>x.CrCasSysAdministrativeProceduresNo.StartsWith(AdminstrativeId_withoutSerial));

            var serial = (AdminstrativeCount.Count()+1).ToString("000000", CultureInfo.InvariantCulture)??"000001";

            return (AdminstrativeId_withoutSerial + serial);
        }
    }
}
