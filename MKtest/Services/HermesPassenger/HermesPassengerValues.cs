namespace MKtest.Services.HermesPassenger
{
    /// <summary>An atomic snapshot of both passenger counters.</summary>
    public sealed record HermesPassengerValues(int Entered, int Exited);
}
