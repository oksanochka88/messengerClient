using System;

namespace mACRON
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public byte[] ProfilePhoto { get; set; }
        public string UniqueId { get; set; }
        public DateTime RegistrationDate { get; set; }

        public User()
        {
            // Конструктор без параметров
        }

        public User(string username, string email, int age, byte[] profilePhoto, string uniqueId, DateTime registrationDate)
        {
            Username = username;
            Email = email;
            Age = age;
            ProfilePhoto = profilePhoto;
            UniqueId = uniqueId;
            RegistrationDate = registrationDate;
        }

        // Дополнительные методы или свойства могут быть добавлены по необходимости
    }
}