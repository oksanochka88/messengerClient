using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Controllers
{
    public class LoginUser
    {
        private ConfigController configController = new ConfigController();

        public async Task<string> AuthenticateUser(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                var loginData = new
                {
                    username,
                    password
                };

                string json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(configController.GetServerUrl() + "/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseContent);
                    return jsonResponse["token"]?.ToString();
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка: {response.StatusCode}, {errorContent}");
                }
            }
        }
    }
}
