using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonHumiditySensorReading
    {
        public string SystemId { get; set; }
        public string SensorId { get; set; }
        public decimal HumidityPercent { get; set; }     
        public DateTime ReadingTimestamp { get; set; }   
    }    
}