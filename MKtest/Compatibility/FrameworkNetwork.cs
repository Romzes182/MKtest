using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Compatibility
{
    internal static class FrameworkNetwork
    {
        // Framework socket APIs cannot cancel in-flight I/O using a token alone.
        // Each caller owns its connection, so closing it is safe and unblocks I/O.
        public static async Task ConnectAsync(this TcpClient client, string host, int port, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using (token.Register(client.Close))
            {
                try
                {
                    await client.ConnectAsync(host, port).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }
                catch (Exception ex) when (token.IsCancellationRequested && IsIoException(ex))
                {
                    throw new OperationCanceledException("Connection canceled.", ex, token);
                }
            }
        }

        public static async Task WriteAsync(this NetworkStream stream, byte[] bytes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using (token.Register(stream.Dispose))
            {
                try
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }
                catch (Exception ex) when (token.IsCancellationRequested && IsIoException(ex))
                {
                    throw new OperationCanceledException("Write canceled.", ex, token);
                }
            }
        }

        public static async Task<int> ReadAsync(this NetworkStream stream, byte[] bytes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using (token.Register(stream.Dispose))
            {
                try
                {
                    int count = await stream.ReadAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    return count;
                }
                catch (Exception ex) when (token.IsCancellationRequested && IsIoException(ex))
                {
                    throw new OperationCanceledException("Read canceled.", ex, token);
                }
            }
        }

        private static bool IsIoException(Exception ex) =>
            ex is IOException or SocketException or ObjectDisposedException;
    }
}
