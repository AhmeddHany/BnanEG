using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bnan.Core.Interfaces.UpdateDataBaseJobs;
using Bnan.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Bnan.Core.Models;
using System.Drawing;


namespace Bnan.Inferastructure.Repository.UpdateDataBaseJobs
{
    public class UpdateStatusForContracts : IUpdateStatusForContracts
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UpdateStatusForContracts> _logger;
        public UpdateStatusForContracts(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, ILogger<UpdateStatusForContracts> logger)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task UpdateDatabase()
        {
            var contracts = _unitOfWork.CrCasRenterContractAlert.FindAll(d => d.CrCasRenterContractAlertContractStatus == "A").ToList();
            foreach (var item in contracts)
            {
                var contract = _unitOfWork.CrCasRenterContractBasic.Find(x => x.CrCasRenterContractBasicNo == item.CrCasRenterContractAlertNo, new[] { "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });
                var user = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == contract.CrCasRenterContractBasicUserInsert, new[] { "CrMasUserInformationLessorNavigation.CrMasLessorImage" });
                var renter = _unitOfWork.CrMasRenterInformation.Find(x => x.CrMasRenterInformationId == contract.CrCasRenterContractBasicRenterId);
                await ProcessContractAlert(item, contract, user, renter);
            }
            _logger.LogInformation("Finished database update at {Time}", DateTime.Now);
        }
        private async Task ProcessContractAlert(CrCasRenterContractAlert item, CrCasRenterContractBasic contract, CrMasUserInformation user, CrMasRenterInformation renter)
        {
            var path = "";
            bool isStatusUpdated = false;

            if (ShouldUpdateStatusForTommorrowAlert(item))
            {
                UpdateStatusForTommorrowAlert(item, user, ref path);
            }

            if (ShouldUpdateStatusFor24HoursRenterAlert(item))
            {
                UpdateStatusFor24HoursRenterAlert(item, user, ref path);
                isStatusUpdated = true;
            }

            if (ShouldUpdateStatusForTodayAlert(item))
            {
                UpdateStatusForTodayAlert(item, user, ref path);
            }

            if (ShouldUpdateStatusForFourHourAlert(item))
            {
                UpdateStatusForFourHourAlert(item, user, ref path);
                isStatusUpdated = true;
            }

            if (ShouldUpdateStatusForExpiredAlert(item))
            {
                UpdateStatusForExpiredAlert(item, contract);
                isStatusUpdated = true;
            }

            if (isStatusUpdated)
            {
                string message = CreateAlertMessage(item) + " - BNANSC LocalHost";
                string toNumber;
                string userPhoneNumber = user.CrMasUserInformationCallingKey + user.CrMasUserInformationMobileNo;
                string renterPhoneNumber = renter.CrMasRenterInformationCountreyKey + renter.CrMasRenterInformationMobile;
                if (int.Parse(user.CrMasUserInformationLessor) > 4009) toNumber = renterPhoneNumber;
                else toNumber = userPhoneNumber;
                if (!string.IsNullOrEmpty(toNumber)) await SendMessageOnly(toNumber, message);
                // Uncomment if you want to send a photo as well
                // await SendPhotoBeforeOneDay(contract, path, toNumber);
            }
        }
        private bool ShouldUpdateStatusForTommorrowAlert(CrCasRenterContractAlert item)
        {
            return item.CrCasRenterContractAlertDayDate?.Date <= DateTime.Now.Date && item.CrCasRenterContractAlertContractActiviteStatus == "0" && item.CrCasRenterContractAlertDays > 1;
        }
        private void UpdateStatusForTommorrowAlert(CrCasRenterContractAlert item, CrMasUserInformation user, ref string path)
        {
            item.CrCasRenterContractAlertContractActiviteStatus = "1";
            _unitOfWork.CrCasRenterContractAlert.Update(item);
            _unitOfWork.Complete();
        }
        private bool ShouldUpdateStatusFor24HoursRenterAlert(CrCasRenterContractAlert item)
        {
            return (item.CrCasRenterContractAlertEndDate.Value - DateTime.Now).TotalHours <= 24 && item.CrCasRenterContractAlertStatus == "0" && item.CrCasRenterContractAlertDays > 1;
        }
        private void UpdateStatusFor24HoursRenterAlert(CrCasRenterContractAlert item, CrMasUserInformation user, ref string path)
        {
            item.CrCasRenterContractAlertStatus = "1";
            item.CrCasRenterContractAlertArStatusMsg = "العقد ينتهي بعد 24 ساعة";
            item.CrCasRenterContractAlertEnStatusMsg = "The contract expires after 24 hour";
            _unitOfWork.CrCasRenterContractAlert.Update(item);
            _unitOfWork.Complete();
        }
        private bool ShouldUpdateStatusForTodayAlert(CrCasRenterContractAlert item)
        {
            return item.CrCasRenterContractAlertEndDate?.Date == DateTime.Now.Date && item.CrCasRenterContractAlertContractActiviteStatus != "2";
        }
        private void UpdateStatusForTodayAlert(CrCasRenterContractAlert item, CrMasUserInformation user, ref string path)
        {
            item.CrCasRenterContractAlertContractActiviteStatus = "2";
            item.CrCasRenterContractAlertArStatusMsg = "العقد ينتهي اليوم";
            item.CrCasRenterContractAlertEnStatusMsg = "The contract expires today";
            _unitOfWork.CrCasRenterContractAlert.Update(item);
            _unitOfWork.Complete();
        }
        private bool ShouldUpdateStatusForFourHourAlert(CrCasRenterContractAlert item)
        {
            return item.CrCasRenterContractAlertHourDate <= DateTime.Now && item.CrCasRenterContractAlertStatus != "2";
        }
        private void UpdateStatusForFourHourAlert(CrCasRenterContractAlert item, CrMasUserInformation user, ref string path)
        {
            item.CrCasRenterContractAlertStatus = "2";
            item.CrCasRenterContractAlertArStatusMsg = "العقد ينتهي بعد 4 ساعات";
            item.CrCasRenterContractAlertEnStatusMsg = "Contract expires in 4 hours";
            _unitOfWork.CrCasRenterContractAlert.Update(item);
            _unitOfWork.Complete();
        }
        private bool ShouldUpdateStatusForExpiredAlert(CrCasRenterContractAlert item)
        {
            return item.CrCasRenterContractAlertEndDate <= DateTime.Now && item.CrCasRenterContractAlertContractActiviteStatus != "3";
        }
        private void UpdateStatusForExpiredAlert(CrCasRenterContractAlert item, CrCasRenterContractBasic contract)
        {
            item.CrCasRenterContractAlertStatus = "3";
            item.CrCasRenterContractAlertContractActiviteStatus = "3";
            item.CrCasRenterContractAlertArStatusMsg = "العقد انتهى";
            item.CrCasRenterContractAlertEnStatusMsg = "The Contract Is Expired";
            item.CrCasRenterContractAlertContractStatus = "E";
            _unitOfWork.CrCasRenterContractAlert.Update(item);
            _unitOfWork.Complete();
        }
        private string CreateAlertMessage(CrCasRenterContractAlert item)
        {
            string reversedContractNumber = ReverseParts(item.CrCasRenterContractAlertNo);
            string EndDateContract = item.CrCasRenterContractAlertEndDate?.ToString("yyyy/MM/dd HH:mm");
            return $"{item.CrCasRenterContractAlertArStatusMsg} - {reversedContractNumber} - {EndDateContract} - LocalHost ";
        }
        private async Task SendMessageOnly(string toNumber, string messageText)
        {
            var client = _httpClientFactory.CreateClient();
            string url = $"https://business.enjazatik.com/api/v1/send-message?number={Uri.EscapeDataString(toNumber)}&message={Uri.EscapeDataString(messageText)}&token={Uri.EscapeDataString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJudW1iZXIiOiJKYXNlcjExIiwic2VyaWFsIjoiMTk5ZmUzYjFlYjc2MjNlIiwiaWF0IjoxNzA3NzMxNjI4LCJleHAiOjE3OTQxMzE2Mjh9.O_4RW4vYAays1ZL7D-OlOQh6C5P5xVYrT3pZ2Oi9Yak")}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw if not a success code

                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Message sent successfully: {Response}", responseBody);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Error sending message: {Error}", e.Message);
            }
        }
        private string ReverseParts(string input)
        {
            string[] parts = input.Split('-');
            Array.Reverse(parts);
            return string.Join("-", parts);
        }

        //public async Task UpdateDatabase()
        //{

        //    var contracts = _unitOfWork.CrCasRenterContractAlert.FindAll(d => d.CrCasRenterContractAlertContractStatus == "A").ToList();
        //    foreach (var item in contracts)
        //    {
        //        var IsTrue = true;
        //        string? path = "";
        //        //check if contract will end after 24 hours
        //        var contract = _unitOfWork.CrCasRenterContractBasic.Find(x => x.CrCasRenterContractBasicNo == item.CrCasRenterContractAlertNo, new[] { "CrCasRenterContractBasic5.CrCasRenterLessorNavigation" });
        //        var user = _unitOfWork.CrMasUserInformation.Find(x => x.CrMasUserInformationCode == contract.CrCasRenterContractBasicUserInsert, new[] { "CrMasUserInformationLessorNavigation.CrMasLessorImage" });


        //        if (item.CrCasRenterContractAlertDayDate <= DateTime.Now && item.CrCasRenterContractAlertStatus == "0" && item.CrCasRenterContractAlertDays > 2)
        //        {
        //            item.CrCasRenterContractAlertStatus = "1";
        //            item.CrCasRenterContractAlertContractActiviteStatus = "1";
        //            item.CrCasRenterContractAlertArStatusMsg = "العقد ينتهي غدا";
        //            item.CrCasRenterContractAlertEnStatusMsg = "The contract expires tomorrow";
        //            path = user.CrMasUserInformationLessorNavigation.CrMasLessorImage.CrMasLessorImageContract24Hour;
        //            _unitOfWork.CrCasRenterContractAlert.Update(item);
        //            IsTrue = false;
        //        }
        //        if (item.CrCasRenterContractAlertDayDate?.Date == DateTime.Now.Date && item.CrCasRenterContractAlertContractActiviteStatus != "2")
        //        {
        //            item.CrCasRenterContractAlertContractActiviteStatus = "2";
        //            _unitOfWork.CrCasRenterContractAlert.Update(item);
        //        }
        //        // check if contract will end after 4 hours
        //        if ((item.CrCasRenterContractAlertHourDate <= DateTime.Now && (item.CrCasRenterContractAlertStatus == "1" || item.CrCasRenterContractAlertStatus == "0")))
        //        {
        //            item.CrCasRenterContractAlertStatus = "2";
        //            //item.CrCasRenterContractAlertContractActiviteStatus = "0";
        //            item.CrCasRenterContractAlertArStatusMsg = "العقد ينتهي بعد 4 ساعات";
        //            item.CrCasRenterContractAlertEnStatusMsg = "Contract expires in 4 hours";
        //            path = user.CrMasUserInformationLessorNavigation.CrMasLessorImage.CrMasLessorImageContract4Hour;
        //            _unitOfWork.CrCasRenterContractAlert.Update(item);
        //            IsTrue = false;
        //        }
        //        // check if contract will end Now
        //        if (item.CrCasRenterContractAlertEndDate <= DateTime.Now && item.CrCasRenterContractAlertStatus == "2")
        //        {
        //            item.CrCasRenterContractAlertStatus = "3";
        //            item.CrCasRenterContractAlertContractActiviteStatus = "3";
        //            item.CrCasRenterContractAlertArStatusMsg = "العقد انتهى";
        //            item.CrCasRenterContractAlertEnStatusMsg = "The Contract Is Expired";
        //            item.CrCasRenterContractAlertContractStatus = "E";
        //            path = user.CrMasUserInformationLessorNavigation.CrMasLessorImage.CrMasLessorImageContractFinished;
        //            var contractBasic = _unitOfWork.CrCasRenterContractBasic.FindAll(d => d.CrCasRenterContractBasicNo == item.CrCasRenterContractAlertNo).OrderByDescending(x => x.CrCasRenterContractBasicCopy).FirstOrDefault();
        //            if (contract != null)
        //            {
        //                contract.CrCasRenterContractBasicStatus = "E";
        //                _unitOfWork.CrCasRenterContractBasic.Update(contract);
        //                _unitOfWork.Complete();
        //            }
        //            IsTrue = false;
        //        }


        //        string reversedContractNumber = ReverseParts(item.CrCasRenterContractAlertNo);
        //        string EndDateContract = item.CrCasRenterContractAlertEndDate?.ToString("yyyy/MM/dd HH:mm");
        //        string message = item.CrCasRenterContractAlertArStatusMsg + " - " + reversedContractNumber + " - " + EndDateContract;
        //        string fromNumber = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJudW1iZXIiOiJKYXNlcjExIiwic2VyaWFsIjoiMTk5ZmUzYjFlYjc2MjNlIiwiaWF0IjoxNzA3NzMxNjI4LCJleHAiOjE3OTQxMzE2Mjh9.O_4RW4vYAays1ZL7D-OlOQh6C5P5xVYrT3pZ2Oi9Yak";
        //        string toNumber = user.CrMasUserInformationCallingKey + user.CrMasUserInformationMobileNo;
        //        if (!IsTrue) await SendMessageOnly(toNumber, fromNumber, message);




        //        //string pathInServer = "C:\\inetpub\\wwwroot\\BnanSystem\\wwwroot\\";
        //        //string pathInLocalHost = "E:\\Work\\8-3-2024\\Work\\BnanJordan\\Bnan.Ui\\wwwroot\\";
        //        //string imagePath = path.Replace("~", "").TrimStart('/');
        //        //string fullPathImage = System.IO.Path.Combine(pathInLocalHost , imagePath);
        //        //if (!IsTrue)
        //        //{
        //        //    try
        //        //    {
        //        //        await SendPhotoToWhatsup.SendMailBeforeOneDay(contract, fullPathImage, toNumber, fromNumber);

        //        //    }
        //        //    catch (HttpRequestException e)
        //        //    {
        //        //        Console.WriteLine($"Error: {e.Message}");
        //        //    }
        //        //}

        //    }
        //    _logger.LogInformation("Finished database update at {Time}", DateTime.Now);
        //}
        //private async Task SendMessageOnly(string toNumber, string fromNumber, string messageText)
        //{
        //    var client = _httpClientFactory.CreateClient();
        //    string url = $"https://business.enjazatik.com/api/v1/send-message?number={Uri.EscapeDataString(toNumber)}&message={Uri.EscapeDataString(messageText)}&token={Uri.EscapeDataString(fromNumber)}";
        //    try
        //    {
        //        HttpResponseMessage response = await client.GetAsync(url);
        //        response.EnsureSuccessStatusCode(); // Throw if not a success code

        //        string responseBody = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine(responseBody); // Output the response
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        Console.WriteLine($"Error: {e.Message}");
        //    }
        //}
        //private async Task SendPhotoBeforeOneDay(CrCasRenterContractBasic contract, string filePath, string number, string token)
        //{
        //    string imagePath = filePath;
        //    string pathSavedModified = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName).FullName;
        //    string savedModified = Path.Combine(pathSavedModified, "modified_image.jpg"); // Specify a proper file name for the modified image
        //    try
        //    {
        //        using (var bitmap = new Bitmap(imagePath))
        //        {
        //            using (var graphics = Graphics.FromImage(bitmap))
        //            {
        //                // Define text color
        //                Brush brush = new SolidBrush(Color.Black);

        //                // Define text font
        //                // Define custom font for Arabic text
        //                FontFamily arabicFontFamily = new FontFamily("Times New Roman");
        //                Font arabicFont = new Font(arabicFontFamily, 35, FontStyle.Bold);
        //                Font font = new Font("Arial", 35, FontStyle.Bold);
        //                Font fontContratNo = new Font("Arial", 27, FontStyle.Bold);
        //                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        //                // Text to display
        //                string arName = contract.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationArName;
        //                string enName = contract.CrCasRenterContractBasic5.CrCasRenterLessorNavigation.CrMasRenterInformationEnName;
        //                string contractNo = contract.CrCasRenterContractBasicNo;
        //                // Create StringFormat object with right-to-left direction and set character spacing
        //                StringFormat stringFormat = new StringFormat();
        //                stringFormat.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, arName.Length) });
        //                stringFormat.SetTabStops(0, new float[] { 0 });
        //                stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        //                stringFormat.SetDigitSubstitution(0, StringDigitSubstitute.National);
        //                // Define rectangle
        //                Rectangle arNameRec = new Rectangle(450, 435, 1000, 500);
        //                Rectangle enNameRec = new Rectangle(190, 700, 1000, 500);
        //                Rectangle ContractNoRec = new Rectangle(40, 220, 1000, 500);
        //                // Draw text on image
        //                graphics.DrawString(arName, arabicFont, brush, arNameRec, stringFormat);
        //                graphics.DrawString(enName, font, brush, enNameRec);
        //                graphics.DrawString(contractNo, fontContratNo, brush, ContractNoRec);

        //                // Save the output file
        //                bitmap.Save(savedModified);
        //            }
        //        }

        //        //Create HttpClient
        //        using (var client = new HttpClient())
        //        {
        //            // Create multipart form data
        //            var formData = new MultipartFormDataContent();

        //            // Add image file to the form data
        //            using (var fileStream = File.OpenRead(savedModified))
        //            {
        //                formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(@savedModified));
        //                formData.Add(new StringContent(" "), "message");

        //                // Add number to the form data
        //                formData.Add(new StringContent(number), "number");

        //                // Make the API request to send the image
        //                var url = $"https://business.enjazatik.com/api/v1/send-media?token={token}";
        //                var response = await client.PostAsync(url, formData);

        //                // Check if request was successful
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    var responseBody = await response.Content.ReadAsStringAsync();
        //                    Console.WriteLine(responseBody); // Output the response
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"Error: {response.StatusCode}");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex}");
        //        throw;
        //    }
        //    finally
        //    {
        //        // Delete the temporary file
        //        if (File.Exists(savedModified))
        //        {
        //            File.Delete(savedModified);
        //        }
        //    }
        //}
        //private string ReverseParts(string input)
        //{
        //    string[] parts = input.Split('-');
        //    Array.Reverse(parts);
        //    return string.Join("-", parts);
        //}
    }
}
