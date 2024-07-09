using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Controllers
{
    public class ChatService
    {
        private ConfigController _configController = new ConfigController();
        
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<HttpResponseMessage> CreateChat(string chatName, List<string> participantUsernames)
        {
            var chatData = new
            {
                name = chatName,
                participants = participantUsernames
            };

            var json = JsonConvert.SerializeObject(chatData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync(_configController.GetServerUrl() + "/chats", content);
        }

        public async Task<string> GetChats()
        {
            HttpResponseMessage response = await _httpClient.GetAsync(_configController.GetServerUrl() + "/chats");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> SendMessage(int chatId, string content)
        {
            var messageData = new
            {
                content = content
            };

            var json = JsonConvert.SerializeObject(messageData);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            var serverUrl = _configController.GetServerUrl();
            return await _httpClient.PostAsync($"{serverUrl}/chats/{chatId}/messages", contentData);
        }

        public async Task<HttpResponseMessage> GetMessages(int chatId)
        {
            var serverUrl = _configController.GetServerUrl();
            return await _httpClient.GetAsync($"{serverUrl}/chats/{chatId}/messages");
        }
    }
}
