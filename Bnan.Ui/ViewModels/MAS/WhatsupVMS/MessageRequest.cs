namespace Bnan.Ui.ViewModels.MAS.WhatsupVMS
{
    public class MessageRequest
    {
        public string Phone { get; set; }
        public string Message { get; set; }
        //public string CompanyId { get; set; }
    }
    public class MediaRequest
    {
        public string Phone { get; set; }
        public string Message { get; set; }
        public string CompanyId { get; set; }
        public string fileBase64 { get; set; }
        public string filename { get; set; }
    }
    public class ResultResponseWithNo
    {
        public string message { get; set; }
        public string companyId { get; set; }
    }
}
