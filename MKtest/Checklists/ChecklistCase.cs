using System;
using System.Collections.Generic;
using System.Linq;

namespace MKtest.Checklists
{
    public sealed class ChecklistCase
    {
        public bool Selected { get; set; }
        public string Id { get; set; } = "";
        public string Section { get; set; } = "";
        public string Category { get; set; } = "";
        public string Algorithm { get; set; } = "";
        public string Platforms { get; set; } = "";
        public string Module { get; set; } = "";
        public string Action { get; set; } = "";
        public string Expected { get; set; } = "";
        public string Result { get; set; } = "Не проверено";
        public string Bug { get; set; } = "";
        public string Notes { get; set; } = "";

        public ChecklistCase NewBlankResult() => new()
        {
            Id = Id, Section = Section, Category = Category,
            Algorithm = Algorithm,
            Platforms = Platforms, Module = Module,
            Action = Action, Expected = Expected
        };
    }

    public sealed class ChecklistDocument
    {
        /// <summary>Stable ID links a printed sheet and the editable electronic run.</summary>
        public string RunId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Title { get; set; } = "Общий чек-лист медиакомплекса";
        public string Environment { get; set; } = "";
        public string Version { get; set; } = "";
        public string Tester { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Today;
        public List<ChecklistCase> Cases { get; set; } = new();

        public ChecklistDocument CreateSubset(IEnumerable<ChecklistCase> selected,
            string title) => new()
        {
            Title = title,
            Environment = Environment,
            Version = Version,
            Tester = Tester,
            Date = DateTime.Today,
            Cases = selected.Select(x => x.NewBlankResult()).ToList()
        };
    }
}
