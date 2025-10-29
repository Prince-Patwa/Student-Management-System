using StudentManagement.Models;
using System.Text.Json;

namespace StudentManagement.Services
{
    public class PaymentLink
    {

        private readonly HttpClient _httpClient;
        public PaymentLink(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CreatePaymentLink(double amount, string name, string email, string mobile, string callback_url)
        {

            try
            {
                string apiurl = "https://phpcrud.himanshukashyap.com/rzp/createLink.php?mode=test&amount=" + amount.ToString() + "&name=" + name + "&mobile=" + mobile + "&email=" + email + "&callback_url=" + callback_url;

                HttpResponseMessage response = await _httpClient.GetAsync(apiurl);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    ApiResult data = JsonSerializer.Deserialize<ApiResult>(result);
                    return data.payment_link;
                }
                else
                {
                    return "Error2";
                }

            }
            catch (Exception ex)
            {
                return "Error1 - " + ex.Message;
            }

        }
    }
}
