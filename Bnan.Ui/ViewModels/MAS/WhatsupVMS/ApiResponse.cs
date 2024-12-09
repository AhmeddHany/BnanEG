namespace Bnan.Ui.ViewModels.MAS.WhatsupVMS
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public ClientInfoWhatsup? Data { get; set; }
    }
}
