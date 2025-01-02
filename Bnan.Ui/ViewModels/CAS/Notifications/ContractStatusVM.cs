namespace Bnan.Ui.ViewModels.CAS.Notifications
{
    public class ContractStatusVM
    {
        public int ExpiredCount { get; set; }
        public int ExpireTodayCount { get; set; }
        public int ExpireLaterCount { get; set; }
        public int ExpireTommorrowCount { get; set; }
        public int SavedCount { get; set; }
        public int SuspendCount { get; set; }
        public bool HaveExpireOrNot { get; set; }
    }
}
