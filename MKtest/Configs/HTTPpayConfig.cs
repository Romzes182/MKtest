using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKtest.Configs
{
    public class HTTPpayConfig
    {
        public string IP { get; set; } = "172.16.8.5";

        public int Port { get; set; } = 60002;

        public string Terminal { get; set; } = "51000200030004";
        public string Route { get; set; } = "4000000000020016";

        public int Trip { get; set; } = 2;

        public DateTime TripDate { get; set; } = new DateTime(2021, 1, 1, 5, 0, 0);

        public int CurrentPayments { get; set; } = 6;

        public bool IncludeTimestamp { get; set; } = true;

        public int IntervalSeconds { get; set; } = 5;

        public int TotalPayments { get; set; } = 35;

       
    }
}