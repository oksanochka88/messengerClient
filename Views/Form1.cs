using mACRON.Controllers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace mACRON
{
    public partial class Form1 : Form
    {
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private ConfigController configController = new ConfigController();
        private LoginUser userController = new LoginUser();
        private JWT jwtAutch = new JWT();
        private RegisterUser registerUser = new RegisterUser();

        public Form1()
        {
            InitializeComponent();
        }

        // Register
        private async void button1_Click_1(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string email = textBox2.Text;
            string password = textBox3.Text;
            string about = textBox5.Text;

            // Считываем фото из PictureBox
            byte[] photo = null;
            if (pictureBox1.Image != null)
            {
                photo = ImageToByteArray(pictureBox1.Image);
            }


            try
            {
                string responseBody = await registerUser.RegisterUserAsync(username, email, password, about, photo);
                MessageBox.Show("Registration successful: " + responseBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }

        // Load foto
        private void button2_Click_2(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                pictureBox1.Image = Image.FromFile(filePath);
            }
        }

        // ConfigController
        private void button4_Click(object sender, EventArgs e)
        {
            string ip = textBox7.Text;
            string port = textBox8.Text;

            configController.SaveServerConfig(ip, port);

            MessageBox.Show("Server configuration saved!");
        }

        // Ping
        private async void button5_Click(object sender, EventArgs e)
        {
            string serverUrl = configController.GetServerUrl() + "/ping"; // Ваш URL сервера
            var result = await configController.IsServerAvailable(serverUrl);

            if (result.isSuccess)
            {
                MessageBox.Show(result.message);
            }
            else
            {
                MessageBox.Show($"Error: {result.message}");
            }
        }

        //Login
        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = textBox4.Text;
            string password = textBox6.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string jwt = await userController.AuthenticateUser(username, password);

                if (!string.IsNullOrEmpty(jwt))
                {
                    // Сохранение JWT
                    jwtAutch.SaveJwtToConfig(jwt);
                    // Открываем ChatForm
                    Form2 form2 = new Form2(this);
                    form2.Show();

                    this.Hide(); // Скрываем форму входа
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аутентификации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}