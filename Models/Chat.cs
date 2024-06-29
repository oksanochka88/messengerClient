using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Models
{
    public class Chat
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int UnreadMessages { get; set; } // Новое свойство для непрочитанных сообщений
    }
}
