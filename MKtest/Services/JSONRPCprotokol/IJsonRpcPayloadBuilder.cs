namespace MKtest.Services.JSONRPCprotokol
{
    public interface IJsonRpcPayloadBuilder
    {
        string Build(int transactions);
    }

    public sealed class JsonRpcPayloadBuilder : IJsonRpcPayloadBuilder
    {
        public string Build(int transactions) =>
            JSONRPCprotokolPacketBuilder.Build(transactions);
    }
}
