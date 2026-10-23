namespace MKtest.Managers
{
    public interface IJsonRpcOperations
    {
        bool IsRunning { get; }
        void Start(int counter, int intervalSeconds);
        void Stop();
        void UpdateValues(int counter, int intervalSeconds);
    }
}
