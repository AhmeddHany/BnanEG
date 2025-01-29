using Bnan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Bnan.Ui.ViewModels.MAS
{
    public class Date_ReportClosedContract_MAS_VM
    {
        public DateTime? dates { get; set; }
    }
    public class list_String_4
    {
        public string? id_key { get; set; }
        public string? nameAr { get; set; }
        public string? nameEn { get; set; }
        public string? str4 { get; set; }

    }
    public class list_String_2
    {
        public string? id_key { get; set; }
        public string? str2 { get; set; }

    }

    public class MASChartBranchDataVM
    {
        public string? Code { get; set; }
        public string? ArName { get; set; }
        public string? EnName { get; set; }
        public decimal Value { get; set; }
        public bool IsTrue { get; set; }

        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }

        public string? ArName_for_table_Model { get; set; } = "";
        public string? EnName_for_table_Model { get; set; } = "";
    }
}
