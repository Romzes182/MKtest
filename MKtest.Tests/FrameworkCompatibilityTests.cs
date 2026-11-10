using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Compatibility;
using MKtest.Services.HTTPpay;
using MKtest.Services.JSONRPCprotokol;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MKtest.Tests;

[TestClass]
public sealed class FrameworkCompatibilityTests
{
    [TestMethod]
    public async Task CancelingWait_DoesNotCancelProducer_AndPreservesCompletedResult()
    {
        var producer = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        Task<int> wait = producer.Task.WaitAsync(cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => Bounded(wait));
        Assert.IsFalse(producer.Task.IsCompleted);
        producer.SetResult(42);
        Assert.AreEqual(42, await producer.Task.WaitAsync(cancellation.Token));
    }

    [TestMethod]
    public async Task RawHttpTransport_PreservesRequestAndReadsCompleteResponse()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            const string response = "HTTP/1.1 200 OK\r\nContent-Length: 4\r\n\r\ntest";
            Task<string> exchange = new RawHttpPayTransport().GetRawAsync(
                $"http://127.0.0.1:{port}/payments?pTotal=42", cancellation.Token);
            using (var peer = await Bounded(listener.AcceptTcpClientAsync()))
            using (var stream = peer.GetStream())
            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true))
            {
                var headers = new List<string>();
                string? line;
                while (!string.IsNullOrEmpty(line = await Bounded(reader.ReadLineAsync())))
                    headers.Add(line!);
                CollectionAssert.AreEqual(new[] {
                    "GET /payments?pTotal=42 HTTP/1.1",
                    $"Host: 127.0.0.1:{port}", "Connection: close", "Accept: */*"
                }, headers);
                byte[] bytes = Encoding.ASCII.GetBytes(response);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
            Assert.AreEqual(response, await Bounded(exchange));
        }
        finally { listener.Stop(); }
    }

    [TestMethod]
    public async Task RawHttpTransport_CancelUnblocksAReadFromASilentPeer()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Task<string> exchange = new RawHttpPayTransport().GetRawAsync(
                $"http://127.0.0.1:{port}/payments", cancellation.Token);
            using var peer = await Bounded(listener.AcceptTcpClientAsync());
            using var stream = peer.GetStream();
            using var reader = new StreamReader(stream);
            // Wait until the full request arrived; send no response.
            while (!string.IsNullOrEmpty(await Bounded(reader.ReadLineAsync()))) { }
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => Bounded(exchange));
        }
        finally { listener.Stop(); }
    }

    [TestMethod]
    public async Task JsonRpcSender_PreservesUtf8BytesAndClosesConnection()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            const string json = "{\"method\":\"пассажиры\",\"count\":42}";
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Task sending = JSONRPCprotokolSender.SendAsync("127.0.0.1", port, json, cancellation.Token);
            using var peer = await Bounded(listener.AcceptTcpClientAsync());
            using var stream = peer.GetStream();
            using var received = new MemoryStream();
            await Bounded(stream.CopyToAsync(received));
            await Bounded(sending);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(json), received.ToArray());
        }
        finally { listener.Stop(); }
    }

    [TestMethod]
    public async Task PreCanceledConnection_DoesNotOpenASocket()
    {
        using var client = new TcpClient();
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            FrameworkNetwork.ConnectAsync(client, "127.0.0.1", 1, canceled.Token));
        Assert.IsFalse(client.Connected);
    }

    [TestMethod]
    public async Task FileOperations_PreserveBytesAndDoNotTruncateOnPreCancellation()
    {
        string root = Path.Combine(Path.GetTempPath(), "MKtest-Framework-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string path = Path.Combine(root, "coord.json");
            byte[] expected = Encoding.UTF8.GetBytes("{\"маршрут\":42}");
            await FrameworkFiles.WriteAllBytesAsync(path, expected, CancellationToken.None);
            CollectionAssert.AreEqual(expected, await FrameworkFiles.ReadAllBytesAsync(path, CancellationToken.None));
            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                FrameworkFiles.WriteAllBytesAsync(path, new byte[0], canceled.Token));
            CollectionAssert.AreEqual(expected, File.ReadAllBytes(path));

            string temporary = Path.Combine(root, "coord.tmp");
            File.WriteAllText(temporary, "replacement");
            FrameworkFiles.MoveReplacing(temporary, path);
            Assert.AreEqual("replacement", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(temporary));
            Assert.Throws<FileNotFoundException>(() => FrameworkFiles.MoveReplacing(temporary, path));
            Assert.AreEqual("replacement", File.ReadAllText(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static async Task Bounded(Task task)
    {
        if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))) != task)
            throw new TimeoutException("The operation did not finish in time.");
        await task;
    }

    private static async Task<T> Bounded<T>(Task<T> task)
    {
        await Bounded((Task)task);
        return await task;
    }
}
