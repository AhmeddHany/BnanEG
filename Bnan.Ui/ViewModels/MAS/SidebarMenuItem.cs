namespace Bnan.Ui.ViewModels.MAS
{
    public class SidebarMenuItem
    {
        public string Title { get; set; }
        public string ItemName { get; set; }
        public string IconPath { get; set; }
        public string Url { get; set; }
        public bool Authorization { get; set; }
        public List<SidebarMenuItem> SubItems { get; set; }
    }
}
