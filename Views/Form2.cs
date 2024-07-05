using mACRON.Controllers;
using mACRON.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mACRON
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        private User _user;

        private ConfigController configController = new ConfigController();
        private JWT _jwtAutch = new JWT();
        private List<Chat> _chats = new List<Chat>();
        private Chat _activeChat;

        private readonly ChatService _chatService;
        private readonly HttpClient _httpClient;

        private ClientWebSocket ws;
        private CancellationTokenSource cancellationTokenSource;

        public Form2(Form1 form1)
        {
            InitializeComponent();

            this.FormClosing += Form2_FormClosing;
            this.form1 = form1;

            _httpClient = new HttpClient();
            _chatService = new ChatService(_httpClient);

            LoadUserChats();
        }

        private async void Form2_Load(object sender, EventArgs e)
        {
            _user = await GetUserProfileAsync(_jwtAutch.GetJwtFromConfig());
            LoadUserProfile(_user);

            cancellationTokenSource = new CancellationTokenSource();
            ListenForWebSocketMessages(_jwtAutch.GetJwtFromConfig());
        }

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
            cancellationTokenSource.Cancel();
            if (ws != null)
            {
                ws.Dispose();
            }

            form1.Close();
        }

        private void SetAuthorizationHeader()
        {
            string token = _jwtAutch.GetJwtFromConfig();
            _chatService.SetAuthorizationHeader(token);
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
                    //MessageBox.Show("Message sent successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            this.Close(); // Закрывает текущую форму
        }

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
                client.BaseAddress = new Uri(configController.GetServerUrl()+"/"); // Замените на ваш адрес
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

        private void LoadUserPhoto(User user, PictureBox pictureBox)
        {
            if (user.Photo != null && user.Photo.Length > 0)
            {
                using (var ms = new MemoryStream(user.Photo))
                {
                    pictureBox.Image = Image.FromStream(ms);
                }
            }
            else
            {
                pictureBox.Image = null; // Или установить изображение по умолчанию
            }
        }

        // Начать ообщение
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
                //MessageBox.Show($"ID: {user.ID}\nUsername: {user.Username}\nEmail: {user.Email}\nAbout: {user.About}", "Информация о пользователе", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Вызываем функцию создания чата
                string chatName = username;
                List<string> participants = new List<string> { username }; // Используем найденного пользователя

                await CreateChatAsync(chatName, participants, _jwtAutch.GetJwtFromConfig());

                LoadUserChats();

                DisplayUserProfile(panel3, user);
            }
            else
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayUserProfile(Panel panel, User user)
        {
            panel.Controls.Clear(); // Очистка панели от предыдущих данных

            int padding = 10;
            int labelWidth = 100;
            int controlHeight = 25;
            int yPosition = 10;
            int controlWidth = panel.Width - padding * 2 - labelWidth; // Максимальная ширина для текстовых полей

            yPosition += controlHeight + padding;

            // Label для имени пользователя
            Label lblUsername = new Label
            {
                Text = "Username:",
                AutoSize = true,
                Location = new Point(padding, yPosition)
            };
            panel.Controls.Add(lblUsername);

            TextBox txtUsername = new TextBox
            {
                Text = user.Username,
                Location = new Point(padding + labelWidth, yPosition),
                Width = controlWidth
            };
            panel.Controls.Add(txtUsername);

            yPosition += controlHeight + padding;

            // Label для email пользователя
            Label lblEmail = new Label
            {
                Text = "Email:",
                AutoSize = true,
                Location = new Point(padding, yPosition)
            };
            panel.Controls.Add(lblEmail);

            TextBox txtEmail = new TextBox
            {
                Text = user.Email,
                Location = new Point(padding + labelWidth, yPosition),
                Width = controlWidth
            };
            panel.Controls.Add(txtEmail);

            yPosition += controlHeight + padding;

            // Label для описания пользователя (About)
            Label lblAbout = new Label
            {
                Text = "About:",
                AutoSize = true,
                Location = new Point(padding, yPosition)
            };
            panel.Controls.Add(lblAbout);

            TextBox txtAbout = new TextBox
            {
                Text = user.About,
                Location = new Point(padding + labelWidth, yPosition),
                Width = controlWidth,
                Height = 50,
                Multiline = true
            };
            panel.Controls.Add(txtAbout);

            yPosition += 50 + padding;

            // Label для фото пользователя
            Label lblPhoto = new Label
            {
                Text = "Photo:",
                AutoSize = true,
                Location = new Point(padding, yPosition)
            };
            panel.Controls.Add(lblPhoto);

            PictureBox pictureBox = new PictureBox
            {
                Location = new Point(padding + labelWidth, yPosition),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            if (user.Photo != null)
            {
                using (var ms = new MemoryStream(user.Photo))
                {
                    pictureBox.Image = Image.FromStream(ms);
                }
            }
            panel.Controls.Add(pictureBox);
        }

        private async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(configController.GetServerUrl()+"/"); // Замените на ваш адрес
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtAutch.GetJwtFromConfig()); // Добавляем JWT токен

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

        private void LoadUserProfile(User user)
        {
            // Устанавливаем значения для TextBox
            textBox2.Text = user.Username;
            textBox3.Text = user.Email;
            textBox5.Text = user.About;

            LoadUserPhoto(_user, pictureBox1);
        }

        // Удалить профиль
        private void button7_Click(object sender, EventArgs e)
        {

        }

        private async void ListenForWebSocketMessages(string jwtToken)
        {
            var serverUrl = configController.GetServerUrl();
            var wsUrl = serverUrl.Replace("http", "ws") + "/ws";

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    using (ws = new ClientWebSocket())
                    {
                        ws.Options.SetRequestHeader("Authorization", "Bearer " + jwtToken);
                        await ws.ConnectAsync(new Uri(wsUrl), cancellationTokenSource.Token);

                        var buffer = new byte[1024 * 4];
                        while (ws.State == WebSocketState.Open)
                        {
                            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                            if (message == "new_message")
                            {
                                int chatId = _activeChat.Id; // Получаем идентификатор активного чата
                                // Получить обновленный список сообщений через REST API и обновить интерфейс
                                await LoadChatMessages(chatId.ToString());
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    // Обработка ошибки соединения и попытка переподключения
                    MessageBox.Show($"WebSocket connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await Task.Delay(5000); // Задержка перед попыткой переподключения
                }
                catch (OperationCanceledException)
                {
                    // Обработка отмены операции (например, закрытие приложения)
                    break;
                }
            }
        }

        // Поиск человека по нику, дублирование
        private async void button5_Click(object sender, EventArgs e)
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
                DisplayUserProfile(panel3, user);
            }
            else
            {
                MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}