using System;
using System.Text.Json;

namespace MKtest.Services.JSONRPCprotokol
{
    public static class JSONRPCprotokolPacketBuilder
    {
        public static string Build(int trCounter)
        {
            var packet = new
            {
                jsonrpc = "2.0",
                method = "sekop.swParams",
                @params = new
                {
                    route = "101",
                    order = "12345678",
                    trip = DateTime.Now.ToString("HH:mm:ss"),
                    trCounter = trCounter.ToString()
                },
                id = 13
            };

            return JsonSerializer.Serialize(packet);
        }
    }
}