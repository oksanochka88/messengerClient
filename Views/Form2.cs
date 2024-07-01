using mACRON.Controllers;
using mACRON.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using WebSocketSharp;
using System.IO;

namespace mACRON
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        private WebSocket ws;
        private List<Chat> chats;
        private Dictionary<string, List<mACRON.Models.Message>> chatMessages;
        private JWT jwtAutch = new JWT();

        private readonly ChatService _chatService;
        private readonly HttpClient _httpClient;

        public Form2(Form1 form1)
        {
            InitializeComponent();
            InitializeTestData();
            LoadUserChats();

            this.FormClosing += Form2_FormClosing;
            this.form1 = form1;

            _httpClient = new HttpClient();
            _chatService = new ChatService(_httpClient);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
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
                    AddMessageToPanel("Me", textBox1.Text, DateTime.Now);
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

        private void AddMessageToPanel(string sender, string content, DateTime timestamp)
        {
            Label messageLabel = new Label
            {
                Text = $"{timestamp:G} {sender}: {content}",
                AutoSize = true,
                MaximumSize = new Size(panel2.Width - 20, 0),
                Padding = new Padding(10),
                Margin = new Padding(5),
                BackColor = sender == "Me" ? Color.LightBlue : Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel2.Controls.Add(messageLabel);
            panel2.ScrollControlIntoView(messageLabel);
        }

        private void InitializeTestData()
        {
            chats = new List<Chat>
            {
                new Chat { Id = "1", Name = "Chat fffffffffffffff1", UnreadMessages = 3 },
                new Chat { Id = "2", Name = "Chat ffffffffffffffff2", UnreadMessages = 0 },
                new Chat { Id = "3", Name = "Chat 3", UnreadMessages = 1 },
                new Chat { Id = "4", Name = "Chat fffffffffffffff1", UnreadMessages = 3 },
                new Chat { Id = "5", Name = "Chat ffffffffffffffff2", UnreadMessages = 0 },
                new Chat { Id = "6", Name = "Chat 3", UnreadMessages = 1 },
                new Chat { Id = "7", Name = "Chat fffffffffffffff1", UnreadMessages = 3 },
                new Chat { Id = "8", Name = "Chat ffffffffffffffff2", UnreadMessages = 0 },
                new Chat { Id = "9", Name = "Chat 3", UnreadMessages = 1 },
                new Chat { Id = "10", Name = "Chat fffffffffffffff1", UnreadMessages = 3 },
                new Chat { Id = "11", Name = "Chat ffffffffffffffff2", UnreadMessages = 0 },
                new Chat { Id = "12", Name = "Chat 3", UnreadMessages = 1 },
                new Chat { Id = "13", Name = "Chat fffffffffffffff1", UnreadMessages = 3 },
                new Chat { Id = "14", Name = "Chat ffffffffffffffff2", UnreadMessages = 0 },
                new Chat { Id = "15", Name = "Chat 3", UnreadMessages = 1 },
            };

            chatMessages = new Dictionary<string, List<mACRON.Models.Message>>
            {
                {
                    "1", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User1", Content = "Hello from Chat 1", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hi there!", Timestamp = DateTime.Now.AddMinutes(-8) }
                    }
                },
                {
                    "2", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User2", Content = "Welcome to Chat 2", Timestamp = DateTime.Now.AddMinutes(-20) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Thanks!", Timestamp = DateTime.Now.AddMinutes(-18) }
                    }
                },
                {
                    "3", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User3", Content = "Chat 3 message", Timestamp = DateTime.Now.AddMinutes(-30) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hello!", Timestamp = DateTime.Now.AddMinutes(-28) }
                    }
                },
                        {
                    "4", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User1", Content = "Hello from Chat 1", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hi there!", Timestamp = DateTime.Now.AddMinutes(-8) }
                    }
                },
                {
                    "5", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User2", Content = "Welcome to Chat 2", Timestamp = DateTime.Now.AddMinutes(-20) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Thanks!", Timestamp = DateTime.Now.AddMinutes(-18) }
                    }
                },
                {
                    "6", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User3", Content = "Chat 3 message", Timestamp = DateTime.Now.AddMinutes(-30) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hello!", Timestamp = DateTime.Now.AddMinutes(-28) }
                    }
                },
                        {
                    "7", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User1", Content = "Hello from Chat 1", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hi there!", Timestamp = DateTime.Now.AddMinutes(-8) }
                    }
                },
                {
                    "8", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User2", Content = "Welcome to Chat 2", Timestamp = DateTime.Now.AddMinutes(-20) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Thanks!", Timestamp = DateTime.Now.AddMinutes(-18) }
                    }
                },
                {
                    "9", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User3", Content = "Chat 3 message", Timestamp = DateTime.Now.AddMinutes(-30) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hello!", Timestamp = DateTime.Now.AddMinutes(-28) }
                    }
                },
                        {
                    "10", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User1", Content = "Hello from Chat 1", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hi there!", Timestamp = DateTime.Now.AddMinutes(-8) }
                    }
                },
                {
                    "11", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User2", Content = "Welcome to Chat 2", Timestamp = DateTime.Now.AddMinutes(-20) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Thanks!", Timestamp = DateTime.Now.AddMinutes(-18) }
                    }
                },
                {
                    "12", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User3", Content = "Chat 3 message", Timestamp = DateTime.Now.AddMinutes(-30) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hello!", Timestamp = DateTime.Now.AddMinutes(-28) }
                    }
                },
                        {
                    "13", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User1", Content = "Hello from Chat 1", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hi there!", Timestamp = DateTime.Now.AddMinutes(-8) }
                    }
                },
                {
                    "14", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User2", Content = "Welcome to Chat 2", Timestamp = DateTime.Now.AddMinutes(-20) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Thanks!", Timestamp = DateTime.Now.AddMinutes(-18) }
                    }
                },
                {
                    "15", new List<mACRON.Models.Message>
                    {
                        new mACRON.Models.Message { Sender = "User3", Content = "Chat 3 message", Timestamp = DateTime.Now.AddMinutes(-30) },
                        new mACRON.Models.Message { Sender = "Me", Content = "Hello!", Timestamp = DateTime.Now.AddMinutes(-28) }
                    }
                },
            };
        }

        private void LoadUserChats()
        {
            panel2.Controls.Clear();
            panel2.AutoScroll = true;

            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown
            };

            foreach (var chat in chats)
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

                if (chat.UnreadMessages > 0)
                {
                    Label unreadLabel = new Label
                    {
                        Text = chat.UnreadMessages.ToString(),
                        ForeColor = Color.White,
                        BackColor = Color.Red,
                        AutoSize = true,
                        Padding = new Padding(5),
                        Margin = new Padding(5),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Right
                    };
                    chatPanel.Controls.Add(unreadLabel);
                }

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

        private void ChatButton_Click(object sender, EventArgs e)
        {
            Button chatButton = sender as Button;

            if (chatButton != null && chatButton.Tag != null)
            {
                string chatId = chatButton.Tag.ToString();
                LoadChatMessages(chatId);

                // Снять индикатор непрочитанных сообщений
                var chat = chats.Find(c => c.Id == chatId);
                if (chat != null)
                {
                    chat.UnreadMessages = 0;
                }

                // Перезагрузить список чатов для обновления индикатора
                LoadUserChats();
            }
            else
            {
                MessageBox.Show("Ошибка: Не удалось определить идентификатор чата.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChatMessages(string chatId)
        {
            panel1.Controls.Clear();

            if (chatMessages.TryGetValue(chatId, out var messages))
            {
                foreach (var message in messages)
                {
                    AddMessageToPanel(message.Sender, message.Content, message.Timestamp);
                }
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

        // Отправить сообщение, ПОЛУЧИТЬ ЧАТЫ
        private async void button1_Click_1(object sender, EventArgs e)
        {
            /*
            SetAuthorizationHeader(); // Устанавливаем заголовок авторизации

            try
            {
                string result = await _chatService.GetChats();
                MessageBox.Show(result, "Chats Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting chats: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */


            try
            {
                SetAuthorizationHeader();

                int chatId = 6; // Укажите идентификатор чата, куда нужно отправить сообщение
                string content = textBox1.Text; // Предположим, у вас есть текстовое поле для ввода сообщения

                HttpResponseMessage response = await _chatService.SendMessage(chatId, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Message sent successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();
                    LoadChatMessages(chatId.ToString()); // Обновляем сообщения в чате
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

                    // Сохранение JSON в файл
                    SaveJsonToFile(json, "chatMessages.json");

                    var messages = JsonConvert.DeserializeObject<List<Message>>(json);

                    panel1.Controls.Clear();
                    foreach (var message in messages)
                    {
                        AddMessageToPanel(message.UserId.ToString(), message.Content, message.CreatedAt);
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


    }

    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}