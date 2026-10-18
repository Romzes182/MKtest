using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MKtest.Checklists
{
    /// <summary>
    /// Every walk-through is an independent session. Closing the program or
    /// creating a new checklist cannot erase results of an earlier session.
    /// </summary>
    public sealed class ChecklistRunStore
    {
        private readonly string _directory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public ChecklistRunStore(string? directory = null)
        {
            _directory = directory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MKtest", "Checklists", "runs");
            Directory.CreateDirectory(_directory);
        }

        public void Save(ChecklistDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            if (string.IsNullOrWhiteSpace(document.RunId))
                document.RunId = Guid.NewGuid().ToString("N");

            // Do not allow unexpected paths from externally edited JSON.
            if (!Guid.TryParseExact(document.RunId, "N", out _))
                throw new InvalidDataException("Некорректный идентификатор прохода.");

            string file = Path.Combine(_directory, document.RunId + ".json");
            string temporary = file + ".tmp";
            File.WriteAllText(temporary,
                JsonSerializer.Serialize(document, _jsonOptions));
            File.Move(temporary, file, true);
        }

        public ChecklistDocument Load(string runId)
        {
            if (!Guid.TryParseExact(runId, "N", out _))
                throw new InvalidDataException("Некорректный идентификатор прохода.");
            string file = Path.Combine(_directory, runId + ".json");
            return JsonSerializer.Deserialize<ChecklistDocument>(File.ReadAllText(file))
                ?? throw new InvalidDataException("Не удалось прочитать проход.");
        }

        public IReadOnlyList<ChecklistDocument> List()
        {
            var result = new List<ChecklistDocument>();
            foreach (string file in Directory.EnumerateFiles(_directory, "*.json"))
            {
                try
                {
                    var run = JsonSerializer.Deserialize<ChecklistDocument>(
                        File.ReadAllText(file));
                    if (run != null) result.Add(run);
                }
                catch (Exception ex) when (ex is IOException or JsonException)
                {
                    // Keep other sessions accessible if a file is damaged.
                }
            }

            return result.OrderByDescending(x => x.CreatedAt).ToList();
        }
    }
}
