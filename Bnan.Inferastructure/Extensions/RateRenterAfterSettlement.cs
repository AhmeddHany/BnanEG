using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Inferastructure.Extensions
{
    public static class RateRenterAfterSettlement
    {
        public static void SetRenterTempData(this Controller controller, string renterId,string contractNo)
        {
            controller.TempData["RenterID"] = renterId;
            controller.TempData["ContractNo"] = contractNo;
            controller.TempData["ShowModal"] = true; // Set the flag to show the modal
        }

        public static bool ShouldShowModal(this Controller controller, out string renterId, out string contractNo)
        {
            renterId = controller.TempData["RenterID"]?.ToString();
            contractNo = controller.TempData["ContractNo"]?.ToString();
            bool showModal = controller.TempData["ShowModal"] != null && (bool)controller.TempData["ShowModal"];

            // Optionally clear TempData to ensure it's not available in subsequent requests
            controller.TempData.Remove("ShowModal");

            return showModal;
        }
    }
}
