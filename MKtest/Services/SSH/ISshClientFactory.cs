using MKtest.Configs;
using Renci.SshNet;

namespace MKtest.Services.SSH
{
    /// <summary>Создание SSH-клиента отделено от логики сессии.</summary>
    public interface ISshClientFactory
    {
        SshClient Create(SSHConfig config);
    }
}
