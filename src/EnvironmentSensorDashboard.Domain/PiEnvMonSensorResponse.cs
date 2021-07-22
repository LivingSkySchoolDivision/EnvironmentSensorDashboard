using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonSensorResponse
    {
        PiEnvMonSensorDevice System { get; set; }
        PiEnvMonCPUSensorReading CPUSensorReading { get; set; }
        List<PiEnvMonHumiditySensorReading> HumidityReadings { get; set; }
        List<PiEnvMonTemperatureSensorReading> TemperatureReadings { get; set; }       
    }    
}