using MKtest.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MKtest.Services.USRTransfer
{
    public class RouteService : IRouteService
    {
        public List<RouteConfig> LoadRoutes()
        {
            var routes = new List<RouteConfig>();
            var root = GetRoutesRootPath();
            if (!Directory.Exists(root)) return routes;
            foreach (var dir in Directory.GetDirectories(root).OrderBy(Path.GetFileName))
                routes.Add(BuildRouteConfig(dir));
            return routes;
        }

        private static string GetRoutesRootPath()
        {
            var path = ConfigService.Config.USRTransfer.RoutesPath;
            return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        }

        private static RouteConfig BuildRouteConfig(string dir)
        {
            var route = new RouteConfig { RouteNumber = Path.GetFileName(dir) };
            route.SvcFilePaths = LoadFiles(Path.Combine(dir, "SVC"), false);
            route.InFilePaths = LoadFiles(Path.Combine(dir, "IN"), true);
            route.SelectedSvcFilePath = route.SvcFilePaths.FirstOrDefault() ?? string.Empty;
            return route;
        }

        private static List<string> LoadFiles(string dir, bool sortByPrefix)
        {
            if (!Directory.Exists(dir)) return new List<string>();
            var files = Directory.GetFiles(dir, "*.*")
                .Where(f => !f.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
            return sortByPrefix ? SortIn(files) : files.OrderBy(Path.GetFileName).ToList();
        }

        private static List<string> SortIn(IEnumerable<string> files)
        {
            return files.OrderBy(f => ParsePrefix(Path.GetFileNameWithoutExtension(f)))
                .ThenBy(Path.GetFileName).ToList();
        }

        private static int ParsePrefix(string name)
        {
            var part = name.Split('_').FirstOrDefault();
            return int.TryParse(part, out var n) ? n : int.MaxValue;
        }
    }
}