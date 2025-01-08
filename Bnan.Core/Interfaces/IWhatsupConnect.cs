namespace Bnan.Core.Interfaces
{
    public interface IWhatsupConnect
    {
        Task<bool> AddDefaultWhatsupConnect(string LessorCode);
        Task<bool> AddNewWhatsupConnect(string LessorCode);
        Task<bool> UpdateWhatsupConnectInfo(string LessorCode, string Name, string Mobile, string DeviceType, bool IsBusiness, string UserLogin);
        Task<bool> ChangeStatusOldWhatsupConnect(string LessorCode, string UserLogout);
        Task<bool> ChangeStatusOldWhenDisconnectFromWhatsup(string LessorCode, string LogoutDateTime);
    }
}
