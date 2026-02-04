using System.Collections.Generic;

namespace MKtest.Configs
{
    public class DemoConfig
    {
        public string ScenariosPath { get; set; } = "DemoScenarios";
        public string TemplatesPath { get; set; } = "JsonTemplates";
        public List<string> AvailableScenarios { get; set; } = new();

        public DemoConfig()
        {
            AvailableScenarios = new List<string>
            {
                "demo-1",
                "demo-2",
                "demo-3",
                "demo-4",
                "demo-5",
                "demo-45"
            };
        }
    }
}