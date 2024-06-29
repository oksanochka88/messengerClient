using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Controllers
{
    public class ConfigController
    {
        private static readonly HttpClient client = new HttpClient();

        public void SaveServerConfig(string ip, string port)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ServerIP"].Value = ip;
            config.AppSettings.Settings["ServerPort"].Value = port;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public string GetServerIP()
        {
            return ConfigurationManager.AppSettings["ServerIP"];
        }

        public string GetServerPort()
        {
            return ConfigurationManager.AppSettings["ServerPort"];
        }

        public string GetServerUrl()
        {
            string ip = GetServerIP();
            string port = GetServerPort();
            return $"http://{ip}:{port}";
        }

        // Ping server
        public async Task<(bool isSuccess, string message)> IsServerAvailable(string serverUrl)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(serverUrl);
                response.EnsureSuccessStatusCode();
                return (true, "Server is available.");
            }
            catch (HttpRequestException httpRequestException)
            {
                return (false, $"HTTP Error: {httpRequestException.Message}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                return (false, $"Request Timed Out: {taskCanceledException.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected Error: {ex.Message}");
            }
        }
    }
}
