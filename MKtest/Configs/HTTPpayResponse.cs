using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKtest.Configs
{
    public class HTTPpayResponse
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public int StatusCode { get; set; }

        public DateTime Timestamp { get; set; }

    }
}