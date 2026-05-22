namespace MKtest.Configs
{
    public class USRTransferConfig
    {
        public string IP { get; set; } = "172.16.8.5";
        public int Port { get; set; } = 20108;
        public string RoutesPath { get; set; } = "Routes";
        public int SvcBaudRate { get; set; } = 115200;
        public int InBaudRate { get; set; } = 4800;
        public int DelayBetweenInSeconds { get; set; } = 15;
        public int DelayAfterLastInMs { get; set; } = 2000;

    }
}