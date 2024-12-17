namespace Bnan.Ui.ViewModels.MAS.WhatsupVMS
{
    public class ApiResponse
    {
        public bool Status { get; set; }            // حالة النجاح أو الفشل
        public string Message { get; set; }         // الرسالة العامة
        public string Key { get; set; }             // مفتاح الحالة لتحديد نوع الرد
        public ClientInfoWhatsup Data { get; set; } // بيانات العميل
    }
}
