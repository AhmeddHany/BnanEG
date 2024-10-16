﻿namespace Bnan.Ui.ViewModels.CAS
{
    public class ChartBranchDataVM
    {
        public string? Code { get; set; }
        public string? ArName { get; set; }
        public string? EnName { get; set; }
        public decimal Value { get; set; }
        public bool IsTrue { get; set; }

        public string? ArName_for_table_Model { get; set; } = "";
        public string? EnName_for_table_Model { get; set; } = "";
    }
}
