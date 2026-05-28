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
        public WebServerConfig WebServer { get; set; } = new WebServerConfig();
        public USRTransferConfig USRTransfer { get; set; } = new USRTransferConfig();
        public HermesSSHConfig HermesSSH { get; set; } = new HermesSSHConfig();
        public JSONRPCprotokolConfig HTTPprotokol { get; set; } = new JSONRPCprotokolConfig();
                public HTTPpayConfig HTTPpay { get; set; } = new HTTPpayConfig();
        // В будущем добавите здесь другие конфиги:
        // public SSHConfig AnotherDevice { get; set; } = new SSHConfig();
    }
}