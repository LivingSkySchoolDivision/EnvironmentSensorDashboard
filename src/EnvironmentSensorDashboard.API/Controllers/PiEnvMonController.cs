using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnvironmentSensorDashboard.API.Controllers
{
    [ApiController]
    [Route("/")]
    public class PiEnvMonController : ControllerBase
    {
        private readonly PiEnvMonSensorDeviceService _repository;

        private readonly ILogger<PiEnvMonController> _logger;

        public PiEnvMonController(ILogger<PiEnvMonController> logger, PiEnvMonSensorDeviceService service)
        {
            _logger = logger;
            _repository = service;
        }

        [HttpGet]
        public IEnumerable<PiEnvMonSensorDevice> Get()
        {
            return _repository.GetEnabled().ToList();
        }
    }
}
