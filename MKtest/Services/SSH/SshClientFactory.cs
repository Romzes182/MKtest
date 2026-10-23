using MKtest.Configs;
using Renci.SshNet;
using System;
using System.Text;

namespace MKtest.Services.SSH
{
    public sealed class SshClientFactory : ISshClientFactory
    {
        public SshClient Create(SSHConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            var connectionInfo = new ConnectionInfo(
                config.IP,
                config.Port,
                config.User,
                new PasswordAuthenticationMethod(config.User, config.PasswordUser))
            {
                Encoding = Encoding.UTF8,
                Timeout = TimeSpan.FromSeconds(30)
            };

            return new SshClient(connectionInfo);
        }
    }
}
