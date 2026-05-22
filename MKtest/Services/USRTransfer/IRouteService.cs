using System.Collections.Generic;

namespace MKtest.Services.USRTransfer
{
    public interface IRouteService
    {
        List<RouteConfig> LoadRoutes();
    }
}