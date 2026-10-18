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
