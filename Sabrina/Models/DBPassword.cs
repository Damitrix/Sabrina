using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Models
{
    static class DBPassword
    {
        private static string _password;
        public static string Password
        {
            get
            {
                if(_password == null)
                {
                    _password = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "DBPassword.cfg"));
                }

                return _password;
            }
        }
    }
}
