using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKtest.Configs
{
    public class SSHConfig
    {
        public string IP { get; set; } = "172.16.8.5";
        public int Port { get; set; } = 2323;
        public string User { get; set; } = "user";
        public string PasswordUser { get; set; } = "user";
        public string PasswordRoot { get; set; } = "admin";
    }
}