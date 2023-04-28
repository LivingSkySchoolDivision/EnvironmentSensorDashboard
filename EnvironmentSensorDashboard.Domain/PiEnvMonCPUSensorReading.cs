using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonCPUSensorReading
    {
        public int SystemDatabaseId { get; set; }
        public string SystemId { get; set; }
        public string SensorId { get; set; }
        public decimal TemperatureCelsius { get; set; }      
        public DateTime ReadingTimestamp { get; set; }    
    }    
}