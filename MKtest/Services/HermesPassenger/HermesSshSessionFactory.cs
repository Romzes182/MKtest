using MKtest.Configs;
using Renci.SshNet;
using System;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>Creates an SSH.NET session using the existing Hermes settings.</summary>
    public sealed class HermesSshSessionFactory : IHermesSshSessionFactory
    {
        public IHermesSshSession Create(HermesSSHConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            var connectionInfo = new ConnectionInfo(
                config.IP,
                config.Port,
                config.User,
                new PasswordAuthenticationMethod(config.User, config.Password))
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            return new HermesSshSession(new SshClient(connectionInfo));
        }
    }
}
