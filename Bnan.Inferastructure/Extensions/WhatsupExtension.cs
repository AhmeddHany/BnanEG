using Microsoft.AspNetCore.Hosting;

namespace Bnan.Inferastructure.Extensions
{
    public static class WhatsupExtension
    {
        public static async Task<bool?> SendFile(this IWebHostEnvironment webHostEnvironment, string PathFile, string number, string Message)
        {
            string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJudW1iZXIiOiJKYXNlcjExIiwic2VyaWFsIjoiMTk5ZmUzYjFlYjc2MjNlIiwiaWF0IjoxNzA3NzMxNjI4LCJleHAiOjE3OTQxMzE2Mjh9.O_4RW4vYAays1ZL7D-OlOQh6C5P5xVYrT3pZ2Oi9Yak";
            string wwwroot = webHostEnvironment.WebRootPath;
            PathFile = PathFile.Substring(2).Replace("/", @"\");
            var path = Path.Combine(wwwroot, PathFile);

            //Create HttpClient
            using (var client = new HttpClient())
            {
                // Create multipart form data
                var formData = new MultipartFormDataContent();

                // Add image file to the form data
                using (var fileStream = File.OpenRead(path))
                {
                    formData.Add(new StreamContent(fileStream), "file", Path.GetFileName(path));
                    formData.Add(new StringContent(Message), "message");

                    // Add number to the form data
                    formData.Add(new StringContent(number), "number");

                    // Make the API request to send the image
                    var url = $"https://business.enjazatik.com/api/v1/send-media?token={Token}";
                    var response = await client.PostAsync(url, formData);

                    // Check if request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody); // Output the response
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
            }
            return true;
        }
        public static async Task<bool?> SendBase64StringAsImageToWhatsUp(string base64Image, string number, string message)
        {
            string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJudW1iZXIiOiJKYXNlcjExIiwic2VyaWFsIjoiMTk5ZmUzYjFlYjc2MjNlIiwiaWF0IjoxNzA3NzMxNjI4LCJleHAiOjE3OTQxMzE2Mjh9.O_4RW4vYAays1ZL7D-OlOQh6C5P5xVYrT3pZ2Oi9Yak";
            base64Image = FileExtensions.CleanAndCheckBase64StringPdf(base64Image);
            if (base64Image != null)
            {
                // Create HttpClient
                using (var client = new HttpClient())
                {
                    try
                    {
                        // Create multipart form data
                        var formData = new MultipartFormDataContent();

                        // Convert Base64 string to byte array
                        byte[] imageBytes = Convert.FromBase64String(base64Image);

                        // Add image byte array to the form data
                        formData.Add(new ByteArrayContent(imageBytes), "file", "ExtensionContractWhatUp.png");

                        // Add message and number to the form data
                        formData.Add(new StringContent(message), "message");
                        formData.Add(new StringContent(number), "number");

                        // Make the API request to send the file
                        var url = $"https://business.enjazatik.com/api/v1/send-media?token={Token}";
                        var response = await client.PostAsync(url, formData);

                        // Check if request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseBody); // Output the response
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending image: {ex.Message}");
                        return false;
                    }
                }

            }
            return false;
        }
        // Function to get the appropriate message based on PDF type
        public static string GetMessage(string pdfPath, string renterArName, string renterEnName)
        {
            string message = "";

            if (pdfPath == "Contract")
            {
                message = $"عزيزي / {renterArName} , يمكنك مراجعة العقد عن طريق تحميل الملف المرفق.\n\nDear {renterEnName}, you can review the contract by downloading the attached file.";
            }
            else if (pdfPath == "Invoice")
            {
                message = $"عزيزي / {renterArName} , يمكنك مراجعة الفاتورة عن طريق تحميل الملف المرفق.\n\nDear {renterEnName}, you can review the invoice by downloading the attached file.";
            }
            else if (pdfPath == "Receipt")
            {
                message = $"عزيزي / {renterArName} , يمكنك مراجعة السند عن طريق تحميل الملف المرفق.\n\nDear {renterEnName}, you can review the receipt by downloading the attached file.";
            }

            return message;
        }
    }
}
