namespace StudentManagement.Services
{
    public class SMSSender
    {
        private readonly HttpClient _httpClient;       //HttpClient used to send HTTP requests and receive HTTP responses from a resource identified by a URI.
        public SMSSender(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SendOTP(string otp, string mobile)
        {
            try
            {
                string authkey = "370038A9phBOW9czQ68e3ee20P1";
                string template_id = "1307164706435757762";
                string message = "Your OTP Code is " + otp + ". Do not share it with anyone. From DigiCoders . #TeamDigiCoders";

                string encodedMessage = Uri.EscapeDataString(message);

                mobile = "91" + mobile;

                string url = "http://sms.digicoders.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + mobile + "&message=" + encodedMessage + "&sender=DIGICO&route=4&country=91&DLT_TE_ID=" + template_id;

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "Error";
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }

        }

    }
}
