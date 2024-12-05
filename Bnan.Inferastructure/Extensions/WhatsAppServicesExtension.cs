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

            var content = new FormUrlEncodedContent(formData);

            try
            {
                // إرسال الطلب
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return "Message sent successfully.";
                }
                else
                {
                    throw new Exception($"Failed to send message: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending message: {ex.Message}", ex);
            }
        }
    }

}
