using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Controllers
{
    public class RegisterUser
    {
        private ConfigController configController = new ConfigController();

        public async Task<string> RegisterUserAsync(string username, string email, string password, string about, byte[] photo)
        {
            var user = new
            {
                Username = username,
                Email = email,
                Password = password,
                About = about,
                Photo = photo
            };

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(configController.GetServerUrl() + "/register", content);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Error: " + ex.Message);
                }
            }
        }
    }
}
