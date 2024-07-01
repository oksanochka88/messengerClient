using mACRON.Controllers;
using mACRON.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;
using WebSocketSharp;

namespace mACRON
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        private WebSocket ws;
        //private List<Chat> chats;
        private Dictionary<string, List<mACRON.Models.Message>> chatMessages;
        private JWT jwtAutch = new JWT();

        private readonly ChatService _chatService;
        private readonly HttpClient _httpClient;

        public Form2(Form1 form1)
        {
            InitializeComponent();
            //InitializeTestData();
            //LoadUserChats();

            this.FormClosing += Form2_FormClosing;
            this.form1 = form1;

            _httpClient = new HttpClient();
            _chatService = new ChatService(_httpClient);

            LoadUserChats();
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

        private void AddMessageToPanel(string sender, string content, DateTime timestamp)
        {
            try
            {
                if (sender == null)
                {
                    throw new ArgumentNullException(nameof(sender), "Sender is null");
                }

                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content), "Content is null");
                }

                Label messageLabel = new Label
                {
                    Text = $"{timestamp:G} {sender}: {content}",
                    AutoSize = true,
                    MaximumSize = new Size(panel1.Width - 20, 0),
                    Padding = new Padding(10),
                    Margin = new Padding(5),
                    BackColor = sender == "Me" ? Color.LightBlue : Color.LightGray,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Добавляем сообщение в начало панели
                panel1.Controls.Add(messageLabel); // Добавляем в конец
                panel1.Controls.SetChildIndex(messageLabel, 0); // Помещаем в начало
                //panel1.ScrollControlIntoView(messageLabel); // Прокручиваем к добавленному сообщению
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
                MessageBox.Show(result); // Показывает полученный JSON

                // Десериализуем JSON в корневой объект ChatResponse
                var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(result);

                // Получаем список чатов из корневого объекта
                var chatsResponse = chatResponse.Chats;

                FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown
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

            if (chatButton != null && chatButton.Tag != null)
            {
                string chatId = chatButton.Tag.ToString();

                try
                {
                    SetAuthorizationHeader();

                    // Получаем сообщения чата по сети
                    HttpResponseMessage response = await _chatService.GetMessages(6);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var messagesResponse = JsonConvert.DeserializeObject<MessagesResponse>(json);

                        panel1.Controls.Clear();

                        foreach (var message in messagesResponse.Messages)
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
                    MessageBox.Show($"Произошла ошибка при загрузке сообщений чата: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    //AddMessageToPanel(message.Sender, message.Content, message.Timestamp);
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

                    // Сохранение JSON в файл
                    //SaveJsonToFile(json, "chatMessages.json");

                    try
                    {
                        var messagesResponse = JsonConvert.DeserializeObject<MessagesResponse>(json);

                        panel1.Controls.Clear();

                        foreach (var message in messagesResponse.Messages)
                        {
                            MessageBox.Show(message.Content);

                            //AddMessageToPanel(1.ToString(), message.Content, message.CreatedAt);

                        }
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

    public class Message1
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MessagesResponse
    {
        public List<Message1> Messages { get; set; }
    }


}