namespace Bnan.Core.Extensions
{
    public class ApiResponseStatus
    {
        public static string Success { get; set; } = "Success"; // عملية ناجحة
        public static string Failure { get; set; } = "Failure"; // فشل عام
        public static string AlreadyExists { get; set; } = "AlreadyExists"; // البيانات موجودة مسبقًا
        public static string NotFound { get; set; } = "NotFound"; // لم يتم العثور على البيانات
        public static string ValidationError { get; set; } = "ValidationError"; // خطأ في التحقق من البيانات
        public static string Unauthorized { get; set; } = "Unauthorized"; // غير مصرح
        public static string Forbidden { get; set; } = "Forbidden"; // الوصول مرفوض
        public static string ServerError { get; set; } = "ServerError"; // خطأ في الخادم
        public static string BadRequest { get; set; } = "BadRequest"; // طلب غير صالح
        public static string Conflict { get; set; } = "Conflict"; // تعارض في البيانات
    }
}
