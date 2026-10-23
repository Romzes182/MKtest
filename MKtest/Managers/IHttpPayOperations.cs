using System;

namespace MKtest.Managers
{
    public interface IHttpPayOperations
    {
        bool IsRunning { get; }
        void Start(Func<int> readTotal, int intervalSeconds);
        void Stop();
    }
}
