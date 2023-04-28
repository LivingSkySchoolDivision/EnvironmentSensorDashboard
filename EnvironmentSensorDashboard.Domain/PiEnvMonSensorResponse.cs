using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonSensorResponse
    {
        public PiEnvMonSensorDevice System { get; set; }
        public PiEnvMonCPUSensorReading CPUSensorReading { get; set; }
        public List<PiEnvMonHumiditySensorReading> HumidityReadings { get; set; }
        public List<PiEnvMonTemperatureSensorReading> TemperatureReadings { get; set; }       
    }    
}