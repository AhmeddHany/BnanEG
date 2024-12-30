namespace Bnan.Core.Extensions
{
    public static class Status
    {
        public static string Active { get; set; } = "A";
        public static string Hold { get; set; } = "H";
        public static string Deleted { get; set; } = "D";
        public static string UnderProcssing { get; set; } = "U";
        public static string AboutToExpire { get; set; } = "X";
        public static string Expire { get; set; } = "E";
        public static string Renewed { get; set; } = "N";
        public static string Rented { get; set; } = "R";
        public static string ForSale { get; set; } = "T";
        public static string RendAndForSale { get; set; } = "V";
        public static string Sold { get; set; } = "S";
        public static string Maintaince { get; set; } = "M";
        public static string Wating { get; set; } = "W";
        public static string Message { get; set; } = "M";
        public static string Insert { get; set; } = "I";
        public static string Accept { get; set; } = "Q";
        public static string Reject { get; set; } = "Z";
        public static string All { get; set; } = "All";
        public static string Save { get; set; } = "S";
        public static string ViewInformation { get; set; } = "VI";
        public static string Closed { get; set; } = "F";
        public static string Cancel { get; set; } = "C";
        public static string Saved { get; set; } = "S";
        public static string Suspend { get; set; } = "L";
        public static string Extension { get; set; } = "Y";
        public static string UnDeleted { get; set; } = "UD";
        public static string UnHold { get; set; } = "UH";
        public static string ResetPassword { get; set; } = "RP";
        public static string ChangePassword { get; set; } = "CP";
        public static string UpdateStatus { get; set; } = "US";
        public static string Update { get; set; } = "U";
        public static string Custody { get; set; } = "1";
        public static string Booked { get; set; } = "2";
        public static string Transfer { get; set; } = "3";
        public static string Change { get; set; } = "4";
        public static string CatchReceipt { get; set; } = "301";
        public static string PaymentReceipt { get; set; } = "302";
        public static string MASLessorCode { get; set; } = "0000";
        public static string MASUserCode { get; set; } = "MAS0000";
    }
}
