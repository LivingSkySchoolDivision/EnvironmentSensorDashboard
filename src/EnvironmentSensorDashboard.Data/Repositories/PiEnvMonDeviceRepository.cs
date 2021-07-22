using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace EnvironmentSensorDashboard.Data
{
    public class PiEnvMonDeviceRepository
    {
        private readonly string _dbConnectionString;       

        public PiEnvMonDeviceRepository(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        private PiEnvMonSensorDevice dataReaderToObject(SqlDataReader dataReader)
        {
            return new PiEnvMonSensorDevice()
            {
                Name = dataReader["name"].ToString().Trim(),
                Description = dataReader["description"].ToString().Trim(),
                Model = dataReader["model"].ToString().Trim(),
                Serial = dataReader["serial"].ToString().Trim(),
                IPAddress = dataReader["ip_address"].ToString().Trim()
            };
        }

        public List<PiEnvMonSensorDevice> GetAll() 
        {
            List<PiEnvMonSensorDevice> returnMe = new List<PiEnvMonSensorDevice>();
            
            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText = "SELECT * FROM SensorDevices;"
                })
                {
                    sqlCommand.Connection.Open();
                    SqlDataReader dbDataReader = sqlCommand.ExecuteReader();

                    if (dbDataReader.HasRows)
                    {
                        while (dbDataReader.Read())
                        {
                            var u = dataReaderToObject(dbDataReader);
                            if (u != null)
                            {
                                returnMe.Add(u);
                            }
                        }
                    }

                    sqlCommand.Connection.Close();
                }
            }

            return returnMe;
        }



    }
}