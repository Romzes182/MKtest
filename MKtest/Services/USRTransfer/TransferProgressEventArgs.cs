using System;

namespace MKtest.Services.USRTransfer
{
    public class TransferProgressEventArgs : EventArgs
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public int CountdownSeconds { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}