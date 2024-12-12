using Bnan.Core.Extensions;
using Newtonsoft.Json;

namespace Bnan.Inferastructure.Extensions
{
    public static class WhatsAppServicesExtension
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string api = "http://62.84.187.79:3000";

        /// <summary>
        /// Generates a QR Code for a given company.
        /// </summary>
        public static async Task<string> GenerateQrCode(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            var url = $"{api}/api/generateQrCodeNew2/{companyId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to generate QR code. Status code: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Checks if the client is ready.
        /// </summary>
        public static async Task<string> IsClientReady(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));
            var url = $"{api}/api/isClientReady_data/{companyId}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to check client connection. Status code: {response.StatusCode}");
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error during IsClientReady execution.", ex);
            }
        }

        /// <summary>
        /// Logs out the client from WhatsApp by company ID.
        /// </summary>
        public static async Task<HttpResponseMessage> LogoutAsync(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

            var url = $"{api}/api/logout_whats/{companyId}";

            try
            {
                return await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                // Log exception (use logging library if available)
                throw new Exception("Error while calling the logout API.", ex);
            }
        }
        /// <summary>
        /// Send message from the company phone connected to client from WhatsApp by company ID.
        /// </summary>
        public static async Task<string> SendMessageAsync(string phone, string message, string companyId)
        {
            var url = $"{api}/api/sendMessage_text";

            // إعداد البيانات بتنسيق x-www-form-urlencoded
            var formData = new Dictionary<string, string>
        {
            { "phone", phone },
            { "message", message },
            { "apiToken", "Bnan_fgfghgfhnbbbmhhjhgmghhgghhgj" },
            { "id", companyId }
        };

            var data = new FormUrlEncodedContent(formData);

            try
            {
                var response = await _httpClient.PostAsync(url, data);
                if (!response.IsSuccessStatusCode)
                    return ApiResponseStatus.Failure;

                var content = await response.Content.ReadAsStringAsync();
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(content);

                // التحقق من الحالة
                if (jsonResult != null && (jsonResult.status == true || jsonResult.status.ToString().ToLower() == "true")) return ApiResponseStatus.Success;
                return ApiResponseStatus.Failure;
            }
            catch (HttpRequestException)
            {
                return ApiResponseStatus.ServerError;
            }
            catch (Exception)
            {
                return ApiResponseStatus.ServerError;
            }
        }

        /// <summary>
        /// Connects a lessor by adding a new device with the provided company ID and name.
        /// </summary>
        /// <param name="companyId">The ID of the company.</param>
        /// <param name="companyName">The name of the company.</param>
        /// <returns>A string indicating success or failure of the operation.</returns>
        public static async Task<string> ConnectLessor(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return ApiResponseStatus.ValidationError;
            string url = $"{api}/api/addNew_Device?id={companyId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(content);
                if (jsonResult != null && jsonResult.status == true) return ApiResponseStatus.Success;
                else return ApiResponseStatus.Failure;
            }
            catch (HttpRequestException)
            {
                // Specific error for HTTP request issues
                return ApiResponseStatus.ServerError;
            }
            catch (Exception)
            {
                // General error handling
                return ApiResponseStatus.ServerError;
            }
        }
        public static async Task<string> CheckIsClientInitialized(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return ApiResponseStatus.ValidationError;

            var url = $"{ApiUrl.IPWhatsService}/api/checkisClientInitialized/{companyId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(content);

                if (jsonResult != null && jsonResult.status == true)
                    return ApiResponseStatus.AlreadyExists;
                else
                    return ApiResponseStatus.NotFound;

            }
            catch (Exception ex)
            {
                return ApiResponseStatus.ServerError;
            }
        }
    }

}
