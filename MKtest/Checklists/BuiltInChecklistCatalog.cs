using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MKtest.Checklists
{
    /// <summary>
    /// Built from the supplied Android, Linux, MChS and fare-evasion checklists.
    /// 246 unique scenarios, with their original stable IDs and expected results.
    /// No previous test result is baked into this catalog.
    /// </summary>
    public static class BuiltInChecklistCatalog
    {
        private const string GzipBase64 = "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";

        public static List<ChecklistCase> Load()
        {
            using var compressed = new MemoryStream(Convert.FromBase64String(GzipBase64));
            using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
            var cases = JsonSerializer.Deserialize<List<string[]>>(gzip)
                ?? throw new InvalidDataException("Не удалось загрузить каталог проверок.");

            var result = new List<ChecklistCase>(cases.Count);
            foreach (var fields in cases)
            {
                if (fields.Length < 5) continue;
                result.Add(new ChecklistCase
                {
                    Id = fields[0],
                    Category = fields[1],
                    Platforms = fields[2],
                    Action = fields[3],
                    Expected = fields[4],
                    Section = "Общий",
                    Result = "Не проверено"
                });
            }
            return result;
        }
    }
}
