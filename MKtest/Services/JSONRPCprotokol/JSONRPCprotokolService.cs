using MKtest.Configs;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.JSONRPCprotokol
{
    public class JSONRPCprotokolService
    {
        private readonly JSONRPCprotokolConfig _config;

        public JSONRPCprotokolService(
            JSONRPCprotokolConfig config)
        {
            _config = config;
        }

        public async Task SendAsync(
            int trCounter,
            CancellationToken token)
        {
            string json =
                JSONRPCprotokolPacketBuilder.Build(trCounter);

            await JSONRPCprotokolSender.SendAsync(
                _config.IP,
                _config.Port,
                json,
                token);
        }
    }
}