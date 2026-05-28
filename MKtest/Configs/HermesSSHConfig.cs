using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKtest.Configs
{
    public class HermesSSHConfig
    {
        public string IP { get; set; } = "172.16.8.12";
        public int Port { get; set; } = 1022;
        public string User { get; set; } = "root";
        public string Password { get; set; } = "rEbxZdz.VSDpNA9xc";
        public string RemoteFilePath { get; set; } = "/tmp/total_cnt.txt";
        public int KillIntervalSeconds { get; set; } = 1;
        public int FileUpdateIntervalSeconds { get; set; } = 5;

    }
}