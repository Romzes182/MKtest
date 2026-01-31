using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKtest.Configs
{
    public class AppConfig
    {
        public SSHConfig SSHBeelink { get; set; } = new SSHConfig();
        // В будущем добавите здесь другие конфиги:
        // public SSHConfig AnotherDevice { get; set; } = new SSHConfig();
    }
}