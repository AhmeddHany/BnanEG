namespace Bnan.Ui.ViewModels.CAS.Notifications
{
    public class EmployeesInfoVM
    {
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? AvaliableBalance { get; set; }
        public string? ResevedBalance { get; set; }
        public string? Picture { get; set; }
        public bool? OnlineOrOffline { get; set; }
        public string? LastActionDate { get; set; }
        public string? LastActionTime { get; set; }
        public bool? HaveCustodyNotAccepted { get; set; }
        public bool? CreditLimitExceeded { get; set; }
    }
}
