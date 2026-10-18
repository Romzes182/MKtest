using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Configs;
using MKtest.Services.JSONRPCprotokol;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MKtest.Tests;

[TestClass]
public sealed class JsonRpcProtocolTests
{
    [TestMethod]
    [DataRow(0, "0")]
    [DataRow(12, "12")]
    [DataRow(65535, "65535")]
    public void Build_ProducesExactJsonRpcShapeAndStringCounter(int transactions, string expected)
    {
        string payload = JSONRPCprotokolPacketBuilder.Build(transactions);
        using JsonDocument doc = JsonDocument.Parse(payload);
        JsonElement root = doc.RootElement;

        Assert.AreEqual(JsonValueKind.Object, root.ValueKind);
        Assert.AreEqual(4, root.EnumerateObject().Count());
        Assert.AreEqual("2.0", root.GetProperty("jsonrpc").GetString());
        Assert.AreEqual("sekop.swParams", root.GetProperty("method").GetString());
        Assert.AreEqual(13, root.GetProperty("id").GetInt32());

        JsonElement parameters = root.GetProperty("params");
        Assert.AreEqual(4, parameters.EnumerateObject().Count());
        Assert.AreEqual("101", parameters.GetProperty("route").GetString());
        Assert.AreEqual("12345678", parameters.GetProperty("order").GetString());
        Assert.AreEqual(JsonValueKind.String, parameters.GetProperty("trCounter").ValueKind);
        Assert.AreEqual(expected, parameters.GetProperty("trCounter").GetString());

        string? trip = parameters.GetProperty("trip").GetString();
        Assert.IsNotNull(trip);
        StringAssert.Matches(trip, new Regex(@"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$"));
    }

    [TestMethod]
    public async Task SendAsync_ForwardsJsonAndConfiguredEndpointToTransport()
    {
        var config = new JSONRPCprotokolConfig
        {
            IP = "192.0.2.44",
            Port = 60003
        };
        var fake = new RecordingTransport();
        var service = new JSONRPCprotokolService(config, new JsonRpcPayloadBuilder(), fake);
        using var source = new CancellationTokenSource();

        await service.SendAsync(73, source.Token);

        Assert.AreEqual(1, fake.SendCount);
        Assert.AreEqual("192.0.2.44", fake.IP);
        Assert.AreEqual(60003, fake.Port);
        Assert.AreEqual(source.Token, fake.Token);
        Assert.IsNotNull(fake.Payload);

        using JsonDocument parsed = JsonDocument.Parse(fake.Payload);
        Assert.AreEqual("73",
            parsed.RootElement.GetProperty("params").GetProperty("trCounter").GetString());
    }

    private sealed class RecordingTransport : IJsonRpcTransport
    {
        public string? IP { get; private set; }
        public int Port { get; private set; }
        public string? Payload { get; private set; }
        public CancellationToken Token { get; private set; }
        public int SendCount { get; private set; }

        public Task SendAsync(string ip, int port, string json, CancellationToken token)
        {
            SendCount++;
            IP = ip;
            Port = port;
            Payload = json;
            Token = token;
            return Task.CompletedTask;
        }
    }
}
