using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using EnvironmentSensorDashboard.Data;
using Microsoft.Extensions.Configuration;

namespace EnvironmentSensorDashboard.API
{
    public class PiEnvMonSensorDeviceService
    {
        private readonly PiEnvMonDeviceRepository _repository;

        public PiEnvMonSensorDeviceService(IConfiguration configuration)
        {
            string dbConnectionString = configuration.GetConnectionString("InternalDatabase");
            this._repository = new PiEnvMonDeviceRepository(dbConnectionString);
        }

        public IEnumerable<PiEnvMonSensorDevice> GetAll()
        {
            return _repository.GetAll();
        }

        public IEnumerable<PiEnvMonSensorDevice> GetEnabled()
        {
            return _repository.GetAllEnabled();
        }
    }
}