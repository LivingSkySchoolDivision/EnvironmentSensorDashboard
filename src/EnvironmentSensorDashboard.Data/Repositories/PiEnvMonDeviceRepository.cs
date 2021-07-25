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
                DatabaseId = dataReader["id"].ToString().Trim().ToInt(),
                Name = dataReader["name"].ToString().Trim(),
                Description = dataReader["description"].ToString().Trim(),
                Model = dataReader["model"].ToString().Trim(),
                Serial = dataReader["serial"].ToString().Trim(),
                IPAddress = dataReader["ip_address"].ToString().Trim(),
                IsEnabled = dataReader["is_enabled"].ToString().Trim().ToBool(),
                LastCPUTemp = dataReader["last_cpu_temp_celsius"].ToString().Trim().ToDecimal(),
                LastCPUTempTimeUTC = dataReader["last_cpu_temp_time"].ToString().Trim().ToDateTime(),
                LastTempCelsius = dataReader["last_temp_celsius"].ToString().Trim().ToDecimal(),
                LastTempTimeUTC = dataReader["last_temp_time"].ToString().Trim().ToDateTime(),
                LastHumidityPercent = dataReader["last_humidity_percent"].ToString().Trim().ToDecimal(),
                LastHumidityTimeUTC = dataReader["last_humidity_time"].ToString().Trim().ToDateTime()
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

        public List<PiEnvMonSensorDevice> GetAllEnabled() 
        {
            List<PiEnvMonSensorDevice> returnMe = new List<PiEnvMonSensorDevice>();
            
            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText = "SELECT * FROM SensorDevices WHERE is_enabled=1;"
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

        public void Update(PiEnvMonSensorDevice device) 
        {            
            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText = 
                        "UPDATE SensorDevices SET " +
                            "ip_address=@DIP, " +
                            "is_enabled=@ISENABLED, " +
                            "name=@DNAME, " +
                            "description=@DDESC, " +
                            "model=@DMODEL, " +
                            "serial=@DSERIAL, " +
                            "last_seen_utc=@DLASTSEEN, " +
                            "last_cpu_temp_celsius=@LASTCPUTEMP, " +
                            "last_cpu_temp_time=@LASTCPUTEMPTIME, " +
                            "last_temp_celsius=@LASTTEMP, " +
                            "last_temp_time=@LASTTEMPTIME, " +
                            "last_humidity_percent=@LASTHUMID, " +
                            "last_humidity_time=@LASTHUMIDTIME " +
                        "WHERE id=@DEVICEID;"
                })
                {
                    sqlCommand.Parameters.AddWithValue("DEVICEID", device.DatabaseId);
                    sqlCommand.Parameters.AddWithValue("DIP", device.IPAddress);
                    sqlCommand.Parameters.AddWithValue("ISENABLED", device.IsEnabled);
                    sqlCommand.Parameters.AddWithValue("DNAME", device.Name);
                    sqlCommand.Parameters.AddWithValue("DDESC", device.Description);
                    sqlCommand.Parameters.AddWithValue("DMODEL", device.Model);
                    sqlCommand.Parameters.AddWithValue("DSERIAL", device.Serial);
                    sqlCommand.Parameters.AddWithValue("DLASTSEEN", device.LastSeenUTC);
                    
                    sqlCommand.Parameters.AddWithValue("LASTCPUTEMP", device.LastCPUTemp);
                    sqlCommand.Parameters.AddWithValue("LASTCPUTEMPTIME", device.LastCPUTempTimeUTC);
                    sqlCommand.Parameters.AddWithValue("LASTTEMP", device.LastTempCelsius);
                    sqlCommand.Parameters.AddWithValue("LASTTEMPTIME", device.LastTempTimeUTC);
                    sqlCommand.Parameters.AddWithValue("LASTHUMID", device.LastHumidityPercent);
                    sqlCommand.Parameters.AddWithValue("LASTHUMIDTIME", device.LastHumidityTimeUTC);
                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
            }
        }



    }
}