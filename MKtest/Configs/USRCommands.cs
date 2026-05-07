namespace MKtest.Configs
{
    public static class USRCommands
    {
        private static readonly byte[] CommandPreamble = { 0x55, 0xAA, 0x55 };

        public static byte[] CreateBaudRate4800Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x12, 0xC0, 0x03, 0xD5 };
        }

        public static byte[] CreateBaudRate9600Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x25, 0x80, 0x03, 0xE8 };
        }

        public static byte[] CreateBaudRate115200Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x01, 0xC2, 0x00, 0x03, 0xC6 };
        }

        public static byte[] CreateBaudRate19200Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x4B, 0x00, 0x03, 0xF3 };
        }

        public static byte[] CreateBaudRate38400Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x96, 0x00, 0x03, 0x3E };
        }

        public static byte[] CreateBaudRate57600Command()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0xE1, 0x00, 0x03, 0x89 };
        }

        public static byte[] GetBaudRateCommand(int baudRate)
        {
            return baudRate switch
            {
                4800 => CreateBaudRate4800Command(),
                9600 => CreateBaudRate9600Command(),
                19200 => CreateBaudRate19200Command(),
                38400 => CreateBaudRate38400Command(),
                57600 => CreateBaudRate57600Command(),
                115200 => CreateBaudRate115200Command(),
                _ => CreateBaudRate9600Command()
            };
        }

        public static byte[] CreateSaveSettingsCommand()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x00, 0x00, 0x08, 0xFD };
        }

        public static byte[] CreateRebootCommand()
        {
            return new byte[] { 0x55, 0xAA, 0x55, 0x00, 0x00, 0x00, 0x05, 0xFA };
        }
    }
}