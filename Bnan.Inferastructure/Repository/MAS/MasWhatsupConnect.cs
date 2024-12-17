using Bnan.Core.Extensions;
using Bnan.Core.Interfaces;
using Bnan.Core.Interfaces.MAS;
using Bnan.Core.Models;

namespace Bnan.Inferastructure.Repository.MAS
{
    public class MasWhatsupConnect : IMasWhatsupConnect
    {
        public IUnitOfWork _unitOfWork;

        public MasWhatsupConnect(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddDefaultWhatsupConnect(string LessorCode)
        {
            CrCasLessorWhatsupConnect crCasLessorConnect = new CrCasLessorWhatsupConnect();
            crCasLessorConnect.CrCasLessorWhatsupConnectId = await GetNextWhatsupConnectId(LessorCode);
            crCasLessorConnect.CrCasLessorWhatsupConnectLessor = LessorCode;
            crCasLessorConnect.CrCasLessorWhatsupConnectSerial = 0;
            crCasLessorConnect.CrCasLessorWhatsupConnectStatus = Status.Renewed;
            if (await _unitOfWork.CrCasLessorWhatsupConnect.AddAsync(crCasLessorConnect) != null) return true;
            return false;
        }

        public async Task<bool> AddNewWhatsupConnect(string LessorCode)
        {
            var lessorConnections = await _unitOfWork.CrCasLessorWhatsupConnect.FindAllAsNoTrackingAsync(x => x.CrCasLessorWhatsupConnectLessor == LessorCode);
            int nextSerial = 0; // القيمة الافتراضية للسيريال إذا لم تكن هناك سجلات
            if (!lessorConnections.Any()) return false;

            var maxSerial = lessorConnections.Max(x => x.CrCasLessorWhatsupConnectSerial);
            if (maxSerial != null) maxSerial += 1;
            else maxSerial = nextSerial;

            CrCasLessorWhatsupConnect crCasLessorConnect = new CrCasLessorWhatsupConnect();
            crCasLessorConnect.CrCasLessorWhatsupConnectId = await GetNextWhatsupConnectId(LessorCode);
            crCasLessorConnect.CrCasLessorWhatsupConnectLessor = LessorCode;
            crCasLessorConnect.CrCasLessorWhatsupConnectSerial = maxSerial;
            crCasLessorConnect.CrCasLessorWhatsupConnectStatus = Status.Renewed;
            if (await _unitOfWork.CrCasLessorWhatsupConnect.AddAsync(crCasLessorConnect) != null) return true;
            return false;
        }

        public async Task<bool> ChangeStatusOldWhatsupConnect(string LessorCode, string UserLogout)
        {
            var lastConnection = await GetLastWhatsupConnect(LessorCode);
            if (lastConnection == null) return false;
            lastConnection.CrCasLessorWhatsupConnectStatus = Status.Deleted;
            lastConnection.CrCasLessorWhatsupConnectUserLogout = UserLogout;
            lastConnection.CrCasLessorWhatsupConnectLogoutDatetime = DateTime.Now;
            if (_unitOfWork.CrCasLessorWhatsupConnect.Update(lastConnection) != null) return true;
            return false;
        }

        public async Task<bool> ChangeStatusOldWhenDisconnectFromWhatsup(string LessorCode, string LogoutDateTime)
        {
            var lastConnection = await GetLastWhatsupConnect(LessorCode);
            if (lastConnection == null) return false;
            lastConnection.CrCasLessorWhatsupConnectStatus = Status.Deleted;
            // تعيين وقت تسجيل الخروج
            if (DateTime.TryParse(LogoutDateTime, out DateTime parsedLogoutDateTime)) lastConnection.CrCasLessorWhatsupConnectLogoutDatetime = parsedLogoutDateTime;
            else lastConnection.CrCasLessorWhatsupConnectLogoutDatetime = DateTime.Now;
            if (_unitOfWork.CrCasLessorWhatsupConnect.Update(lastConnection) != null) return true;
            return false;
        }

        public async Task<bool> UpdateWhatsupConnectInfo(string LessorCode, string Name, string Mobile, string DeviceType, bool IsBusiness, string UserLogin)
        {
            var lastConnection = await GetLastWhatsupConnect(LessorCode);
            if (lastConnection == null) return false;
            lastConnection.CrCasLessorWhatsupConnectName = Name;
            lastConnection.CrCasLessorWhatsupConnectMobile = Mobile;
            lastConnection.CrCasLessorWhatsupConnectDeviceType = DeviceType;
            lastConnection.CrCasLessorWhatsupConnectIsBusiness = IsBusiness;
            lastConnection.CrCasLessorWhatsupConnectUserLogin = UserLogin;
            lastConnection.CrCasLessorWhatsupConnectLoginDatetime = DateTime.Now;
            lastConnection.CrCasLessorWhatsupConnectStatus = Status.Active;
            if (_unitOfWork.CrCasLessorWhatsupConnect.Update(lastConnection) != null) return true;
            return false;
        }

        private async Task<CrCasLessorWhatsupConnect> GetLastWhatsupConnect(string LessorCode)
        {
            var lastConnect = await _unitOfWork.CrCasLessorWhatsupConnect
                .FindAllAsync(x => x.CrCasLessorWhatsupConnectLessor == LessorCode);
            return lastConnect?.OrderByDescending(x => x.CrCasLessorWhatsupConnectSerial).FirstOrDefault();
        }

        private async Task<string> GetNextWhatsupConnectId(string lessorCode)
        {
            // اجلب السجلات من قاعدة البيانات بناءً على lessorCode
            var lastConnect = await _unitOfWork.CrCasLessorWhatsupConnect
                .FindAllAsync(x => x.CrCasLessorWhatsupConnectLessor == lessorCode);

            // احصل على آخر رقم ID بناءً على الترتيب التنازلي
            var lastRecord = lastConnect
                .OrderByDescending(x => x.CrCasLessorWhatsupConnectId)
                .FirstOrDefault()?.CrCasLessorWhatsupConnectId;

            if (!string.IsNullOrEmpty(lastRecord))
            {
                // قم بتقسيم الرقم إلى البادئة (Prefix) والرقم التسلسلي
                var prefix = lastRecord.Substring(0, lastRecord.Length - 6); // الجزء بدون الرقم التسلسلي
                var serialNumber = lastRecord.Substring(lastRecord.Length - 6, 6); // الرقم التسلسلي الأخير

                // زيادة الرقم التسلسلي بمقدار 1
                Int64 nextValue = Int64.Parse(serialNumber) + 1;

                // تجميع الرقم الجديد مع البادئة
                return $"{prefix}{nextValue.ToString("000000")}";
            }
            else
            {
                // إذا لم يكن هناك أي سجل، ارجع إلى الرقم الأولي
                return $"24-1000-{lessorCode}100-000001";
            }
        }

    }
}
