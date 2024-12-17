namespace Bnan.Ui.ViewModels.MAS.WhatsupVMS
{
    public class ClientInfoWhatsup
    {
        public string Name { get; set; }            // اسم العميل
        public string Mobile { get; set; }          // رقم الهاتف
        public string DeviceType { get; set; }      // نوع الجهاز
        public string IsBussenis { get; set; }      // حالة العمل

        private int? serial;
        public int? Serial                          // معرف الاتصالات (مرن لاستقبال نص أو عدد)
        {
            get => serial;
            set
            {
                if (value.HasValue)
                    serial = value;
            }
        }

        // التعامل مع الحقول الزمنية بشكل مرن
        private DateTime? lastLogin;
        public string LastLogin
        {
            get => lastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
            set
            {
                if (DateTime.TryParse(value, out var date))
                    lastLogin = date;
                else
                    lastLogin = null;
            }
        }

        private DateTime? lastLogOut;
        public string LastLogOut
        {
            get => lastLogOut?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
            set
            {
                if (DateTime.TryParse(value, out var date))
                    lastLogOut = date;
                else
                    lastLogOut = null;
            }
        }
    }
}
