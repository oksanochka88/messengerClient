using mACRON.Controllers;
using mACRON.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace mACRON
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        private WebSocket ws;

        private User _user;
        //private Dictionary<string, List<mACRON.Models.Message>> chatMessages;

        private JWT jwtAutch = new JWT();
        private List<Chat> _chats = new List<Chat>();
        private Chat _activeChat;

        private readonly ChatService _chatService;
        private readonly HttpClient _httpClient;

        public Form2(Form1 form1)
        {
            InitializeComponent();

            this.FormClosing += Form2_FormClosing;
            this.form1 = form1;

            _httpClient = new HttpClient();
            _chatService = new ChatService(_httpClient);

            LoadUserChats();
            //User userProfile = await GetUserProfileAsync(jwtToken);
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            _user = await GetUserProfileAsync(jwtAutch.GetJwtFromConfig());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                try
                {
                    // Отправляем сообщение через WebSocket
                    ws.Send(textBox1.Text);

                    // Отображаем отправленное сообщение на панели
                    //AddMessageToPanel("Me", textBox1.Text, DateTime.Now);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при отправке сообщения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Очищаем текстовое поле после отправки сообщения
                textBox1.Clear();
            }
            else
            {
                MessageBox.Show("Введите сообщение перед отправкой", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
        private void AddMessageToPanel(bool isMe, List<Models.Message> messages)
        {
            panel1.Controls.Clear();
            panel1.AutoScroll = true;

            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            try
            {
                if (messages == null || messages.Count == 0)
                    return;
                foreach (var message in messages)
                {
                    Label messageLabel = new Label
                    {
                        Text = $"{message.CreatedAt:G}: {message.Content}",
                        AutoSize = true,
                        MaximumSize = new Size(panel1.Width - 20, 0),
                        Padding = new Padding(10),
                        Margin = new Padding(5),
                        BackColor = isMe ? Color.LightBlue : Color.LightGray,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    flowLayoutPanel.Controls.Add(messageLabel);
                }

                panel1.Controls.Add(flowLayoutPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding message to panel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */
        private void AddMessageToPanel(int currentUserId, List<Models.Message> messages)
        {
            panel1.Controls.Clear();
            panel1.AutoScroll = true;

            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            try
            {
                if (messages == null || messages.Count == 0)
                {
                    Label noMessagesLabel = new Label
                    {
                        Text = "No messages to display.",
                        AutoSize = true,
                        MaximumSize = new Size(panel1.Width - 20, 0),
                        Padding = new Padding(10),
                        Margin = new Padding(5),
                        BackColor = Color.LightGray,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    flowLayoutPanel.Controls.Add(noMessagesLabel);
                }
                else
                {
                    foreach (var message in messages)
                    {
                        bool isMe = message.UserId == currentUserId.ToString();

                        Label messageLabel = new Label
                        {
                            Text = $"{message.CreatedAt:G}: {message.Content}",
                            AutoSize = true,
                            MaximumSize = new Size(panel1.Width - 20, 0),
                            Padding = new Padding(10),
                            Margin = new Padding(5),
                            BackColor = isMe ? Color.LightBlue : Color.LightGray,
                            TextAlign = isMe ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft
                        };

                        flowLayoutPanel.Controls.Add(messageLabel);
                    }
                }

                panel1.Controls.Add(flowLayoutPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding message to panel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void LoadUserChats()
        {
            panel2.Controls.Clear();
            panel2.AutoScroll = true;

            try
            {
                SetAuthorizationHeader(); // Устанавливаем заголовок авторизации

                // Получаем список чатов по сети
                string result = await _chatService.GetChats(); // Предполагается, что метод GetChats возвращает строку JSON
                //MessageBox.Show(result); // Показывает полученный JSON

                // Десериализуем JSON в корневой объект ChatResponse
                var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(result);

                // Получаем список чатов из корневого объекта
                var chatsResponse = chatResponse.Chats;

                _chats = chatsResponse;

                if (chatsResponse == null || !chatsResponse.Any())
                {
                    return;
                }

                FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false
                };

                foreach (var chat in chatsResponse)
                {
                    Panel chatPanel = new Panel
                    {
                        Height = 40,
                        Width = panel2.Width - 20,
                        Margin = new Padding(5),
                        BorderStyle = BorderStyle.FixedSingle,
                        Padding = new Padding(5)
                    };

                    Label chatLabel = new Label
                    {
                        Text = chat.Name,
                        Dock = DockStyle.Left,
                        AutoSize = true
                    };

                    Button chatButton = new Button
                    {
                        Text = "",
                        Tag = chat.Id,
                        Height = 40,
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat
                    };
                    chatButton.Click += ChatButton_Click;
                    chatPanel.Controls.Add(chatLabel);
                    chatPanel.Controls.Add(chatButton);

                    flowLayoutPanel.Controls.Add(chatPanel);
                }

                panel2.Controls.Add(flowLayoutPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке чатов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ChatButton_Click(object sender, EventArgs e)
        {
            Button chatButton = sender as Button;

            int _chatId = (int)chatButton.Tag;

            // Найдите чат в списке _chats по chatId и установите его как активный
            _activeChat = _chats.FirstOrDefault(chat => chat.Id == _chatId);

            if (chatButton != null && chatButton.Tag != null)
            {
                int chatId = (int)chatButton.Tag;
                //MessageBox.Show(chatId.ToString());

                try
                {
                    SetAuthorizationHeader();

                    // Получаем сообщения чата по сети
                    HttpResponseMessage response = await _chatService.GetMessages(chatId);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var messagesResponse = JsonConvert.DeserializeObject<MessagesResponse>(json);

                        //SaveJsonToFile(json, "messagejson.json");

                        //MessageBox.Show(messagesResponse.Messages.ToString());

                        //panel1.Controls.Clear();

                        AddMessageToPanel(int.Parse(_user.ID), messagesResponse.Messages);

                    }
                    else
                    {
                        MessageBox.Show($"Ошибка получения сообщений: {response.ReasonPhrase}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при загрузке сообщений чата: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Ошибка: Не удалось определить идентификатор чата.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateActiveChatUI()
        {
            // Пример обновления интерфейса, например, вывод имени активного чата в TextBox
            //textBoxActiveChatName.Text = _activeChat?.Name ?? "No active chat selected";
        }

        /*
        private void LoadChatMessages(string chatId)
        {
            //panel1.Controls.Clear();

            if (chatMessages.TryGetValue(chatId, out var messages))
            {
                foreach (var message in messages)
                {

                }
            }
        }
        */

        // Обновить сообщения в активном чате
        private async Task LoadChatMessages(string chatId)
        {
            try
            {
                SetAuthorizationHeader();

                HttpResponseMessage response = await _chatService.GetMessages(int.Parse(chatId));

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var messagesResponse = JsonConvert.DeserializeObject<MessagesResponse>(json);

                    if (messagesResponse != null && messagesResponse.Messages != null)
                    {
                        // Добавляем сообщения в панель
                        AddMessageToPanel(int.Parse(_user.ID), messagesResponse.Messages);
                    }
                    else
                    {
                        MessageBox.Show("No messages found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AddMessageToPanel(int.Parse(_user.ID), new List<Models.Message>()); // Отображаем пустую панель
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка получения сообщений: {response.ReasonPhrase}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке сообщений чата: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            form1.Close();
        }

        private void SetAuthorizationHeader()
        {
            string token = jwtAutch.GetJwtFromConfig();
            _chatService.SetAuthorizationHeader(token);
        }

        // Поиск чататов
        private async void button2_Click(object sender, EventArgs e)
        {
            SetAuthorizationHeader(); // Устанавливаем заголовок авторизации

            string chatName = "roma";
            List<string> participants = new List<string>
            {
                "pizda7897"
            };

            HttpResponseMessage response = await _chatService.CreateChat(chatName, participants);
            string responseBody = await response.Content.ReadAsStringAsync();

            MessageBox.Show(response.IsSuccessStatusCode ? "Chat created successfully" : "Error creating chat: " + responseBody);
        }

        private async Task CreateChatAsync(string chatName, List<string> participants, string jwtToken)
        {
            try
            {
                SetAuthorizationHeader(); // Устанавливаем заголовок авторизации

                HttpResponseMessage response = await _chatService.CreateChat(chatName, participants);
                string responseBody = await response.Content.ReadAsStringAsync();

                MessageBox.Show(response.IsSuccessStatusCode ? "Chat created successfully" : "Error creating chat: " + responseBody, "Chat Creation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while creating the chat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                SetAuthorizationHeader();

                if (_activeChat == null)
                {
                    MessageBox.Show("No active chat selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int chatId = _activeChat.Id; // Получаем идентификатор активного чата
                string content = textBox1.Text;

                HttpResponseMessage response = await _chatService.SendMessage(chatId, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Message sent successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();

                    // Обновляем сообщения в чате
                    await LoadChatMessages(chatId.ToString());
                }
                else
                {
                    MessageBox.Show("Error sending message: " + responseBody, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке сообщения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        // Выход
        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        /*
        // Получение сообщений
        private async void button8_Click(object sender, EventArgs e)
        {
            try
            {
                SetAuthorizationHeader(); // Устанавливаем заголовок авторизации

                int chatId = 6; // Пример ID чата, замените на нужный ID
                HttpResponseMessage response = await _chatService.GetMessages(chatId);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var messagesResponse = JsonConvert.DeserializeObject<MessagesResponse>(json);

                        //var message = messagesResponse.Messages;

                        //panel1.Controls.Clear();
                        AddMessageToPanel(true, messagesResponse.Messages);
                    }
                    catch (Exception deserializationException)
                    {
                        MessageBox.Show($"Ошибка десериализации: {deserializationException.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка получения сообщений: {response.ReasonPhrase}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */

        private void SaveJsonToFile(string json, string fileName)
        {
            try
            {
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<User> GetUserProfileAsync(string jwtToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8080/"); // Замените на ваш адрес
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                HttpResponseMessage response = await client.GetAsync("/profile");
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(responseBody);
                    return user;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Ошибка при получении профиля: " + error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            string username = textBox6.Text;
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Введите имя пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            User user = await GetUserByUsernameAsync(username);
            if (user != null)
            {
                MessageBox.Show($"ID: {user.ID}\nUsername: {user.Username}\nEmail: {user.Email}\nAbout: {user.About}", "Информация о пользователе", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Вызываем функцию создания чата
                string chatName = username;
                List<string> participants = new List<string> { username }; // Используем найденного пользователя

                await CreateChatAsync(chatName, participants, jwtAutch.GetJwtFromConfig());
            }
            else
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:8080/"); // Замените на ваш адрес
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtAutch.GetJwtFromConfig()); // Добавляем JWT токен

                HttpResponseMessage response = await client.GetAsync($"check/{username}");
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(responseBody);
                    return user;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Ошибка при получении профиля: " + error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при запросе: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }
    }
}