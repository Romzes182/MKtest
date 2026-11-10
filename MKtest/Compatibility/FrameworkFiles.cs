using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Compatibility
{
    internal static class FrameworkFiles
    {
        public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var output = new MemoryStream();
            await input.CopyToAsync(output, 81920, token).ConfigureAwait(false);
            return output.ToArray();
        }

        public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read,
                81920, FileOptions.Asynchronous);
            await output.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
        }

        public static async Task<string> ReadAllTextAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        // Keep the previous saved document intact if replacing it fails.
        public static void MoveReplacing(string source, string destination)
        {
            if (File.Exists(destination))
                File.Replace(source, destination, null);
            else
                File.Move(source, destination);
        }
    }
}
