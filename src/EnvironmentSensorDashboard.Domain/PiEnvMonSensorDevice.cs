using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonSensorDevice
    {
        public int DatabaseId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }        
        public string IPAddress { get; set; }
        public DateTime LastScanAttemptUTC { get;set; }
        public DateTime LastSuccessUTC { get; set; }
        public DateTime LastFailureUTC { get; set; }
        public bool WasLastPollSuccessful { get; set; }
        public bool IsEnabled { get; set; }

        public DateTime LastCPUTempTimeUTC { get; set; }
        public DateTime LastTempTimeUTC { get; set; }
        public DateTime LastHumidityTimeUTC { get; set; }

        public decimal LastCPUTemp { get; set; }
        public decimal LastTempCelsius { get; set; }
        public decimal LastHumidityPercent { get; set; }
    }
}