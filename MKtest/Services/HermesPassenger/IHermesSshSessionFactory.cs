using MKtest.Configs;

namespace MKtest.Services.HermesPassenger
{
    public interface IHermesSshSessionFactory
    {
        IHermesSshSession Create(HermesSSHConfig config);
    }
}
