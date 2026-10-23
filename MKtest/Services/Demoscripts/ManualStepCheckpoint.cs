using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>
    /// Snapshot of one destination file immediately before a manual step.
    /// "Previous" restores it, even when adjacent steps write different JSON files.
    /// </summary>
    public sealed class ManualStepCheckpoint
    {
        private readonly string _path;
        private readonly byte[]? _previousBytes;

        private ManualStepCheckpoint(string path, byte[]? previousBytes)
        {
            _path = path;
            _previousBytes = previousBytes;
        }

        public static async Task<ManualStepCheckpoint> CaptureAsync(
            string targetFile, CancellationToken token)
        {
            var path = Path.Combine("JsonTemplates", targetFile);
            byte[]? before = File.Exists(path)
                ? await File.ReadAllBytesAsync(path, token)
                : null;

            return new ManualStepCheckpoint(path, before);
        }

        public async Task RestoreAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (_previousBytes == null)
            {
                if (File.Exists(_path))
                    File.Delete(_path);
            }
            else
            {
                await File.WriteAllBytesAsync(_path, _previousBytes, token);
            }
        }
    }
}
