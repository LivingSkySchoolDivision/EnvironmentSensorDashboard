using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonExternalSensor
    {
        public string SystemId { get; set; }
        public string SensorId { get; set; }
        public string Description { get; set; }
    }
}