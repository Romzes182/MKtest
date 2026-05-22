using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MKtest.Services.USRTransfer
{
    public class RouteConfig
    {
        public string RouteNumber { get; set; } = string.Empty;
        public string SelectedSvcFilePath { get; set; } = string.Empty;
        public List<string> SvcFilePaths { get; set; } = new();
        public List<string> InFilePaths { get; set; } = new();
    }
}