﻿namespace Bnan.Core.Interfaces.MAS
{
    public interface IMasWhatsupConnect
    {
        Task<bool> AddDefaultWhatsupConnect(string LessorCode);
        Task<bool> AddNewWhatsupConnect(string LessorCode);
        Task<bool> UpdateWhatsupConnectInfo(string LessorCode, string Name, string Mobile, string DeviceType, bool IsBusiness, string UserLogin);
        Task<bool> ChangeStatusOldWhatsupConnect(string LessorCode, string UserLogout);

    }
}