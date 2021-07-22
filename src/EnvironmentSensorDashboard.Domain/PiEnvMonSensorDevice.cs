using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public class PiEnvMonSensorDevice
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }        
        public string IPAddress { get; set; }
    }
}