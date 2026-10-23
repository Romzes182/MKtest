using MKtest.Configs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.JSONRPCprotokol
{
    /// <summary>Coordinates an unchanged JSON-RPC payload and a TCP transport.</summary>
    public sealed class JSONRPCprotokolService : IJsonRpcSender
    {
        private readonly JSONRPCprotokolConfig _config;
        private readonly IJsonRpcPayloadBuilder _builder;
        private readonly IJsonRpcTransport _transport;

        public JSONRPCprotokolService(JSONRPCprotokolConfig config)
            : this(config, new JsonRpcPayloadBuilder(), new TcpJsonRpcTransport()) { }

        public JSONRPCprotokolService(
            JSONRPCprotokolConfig config,
            IJsonRpcPayloadBuilder builder,
            IJsonRpcTransport transport)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task SendAsync(int trCounter, CancellationToken token) =>
            _transport.SendAsync(_config.IP, _config.Port, _builder.Build(trCounter), token);
    }
}
