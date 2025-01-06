namespace Bnan.Ui.ViewModels.CAS.Employees
{
    public class AuthrizationBranchesVM
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool Authrization { get; set; }
        public bool IfCanChangeAuthrization { get; set; }
        public bool BranchActiveOrHold { get; set; }
    }
}
