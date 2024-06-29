using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mACRON.Controllers
{
    public class JWT
    {
        public void SaveJwtToConfig(string jwt)
        {
            // Сохранение JWT в настройки пользователя
            Properties.Settings.Default.JWT = jwt;
            Properties.Settings.Default.Save();
        }

        public string GetJwtFromConfig()
        {
            return Properties.Settings.Default.JWT;
        }
    }
}
