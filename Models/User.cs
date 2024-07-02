using System;

namespace mACRON
{
    public class User
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public byte[] Photo { get; set; }
        public string UniqueId { get; set; }
        public string About { get; set; }
    }
}