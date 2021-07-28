using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace EnvironmentSensorDashboard.Data
{
    public class PiEnvMonCPUSensorDataRespository
    {
        private readonly string _dbConnectionString;       

        public PiEnvMonCPUSensorDataRespository(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        private PiEnvMonCPUSensorReading dataReaderToObject(SqlDataReader dataReader)
        {
            return new PiEnvMonCPUSensorReading()
            {
                SystemDatabaseId = dataReader["system_database_id"].ToString().Trim().ToInt(),
                SystemId = dataReader["system_id"].ToString().Trim(),
                SensorId = dataReader["sensor_id"].ToString().Trim(),
                ReadingTimestamp = dataReader["scan_time_utc"].ToString().Trim().ToDateTime(),
                TemperatureCelsius = dataReader["temperature_celsius"].ToString().Trim().ToDecimal()
            };
        }

        public List<PiEnvMonCPUSensorReading> GetForSensor(PiEnvMonSensorDevice System, DateTime fromUTC, DateTime toUTC) 
        {
            List<PiEnvMonCPUSensorReading> returnMe = new List<PiEnvMonCPUSensorReading>();
            
            using (SqlConnection connection = new SqlConnection(_dbConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandType = CommandType.Text,
                    CommandText = "SELECT * FROM CPUSensorReadings WHERE system_database_id=@SYSTEMID AND scan_time_utc>=@DATEFROM AND scan_time_utc<=@DATETO;"
                })
                {
                    sqlCommand.Parameters.AddWithValue("SYSTEMID", System.DatabaseId);
                    sqlCommand.Parameters.AddWithValue("DATEFROM", fromUTC);
                    sqlCommand.Parameters.AddWithValue("DATETO", toUTC);
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


        public void Insert(PiEnvMonCPUSensorReading NewReading) 
        {            
            if (NewReading.SystemDatabaseId > 0) {
                using (SqlConnection connection = new SqlConnection(_dbConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand
                    {
                        Connection = connection,
                        CommandType = CommandType.Text,
                        CommandText = "INSERT INTO CPUSensorReadings(scan_time_utc,system_database_id,system_id,sensor_id,temperature_celsius) VALUES(@SCANTIME, @SYSDBID, @SYSID, @SENID, @TEMPC);"
                    })
                    {
                        sqlCommand.Parameters.AddWithValue("SCANTIME", NewReading.ReadingTimestamp.ToSQLSafeDate());
                        sqlCommand.Parameters.AddWithValue("SYSDBID", NewReading.SystemDatabaseId);
                        sqlCommand.Parameters.AddWithValue("SYSID", NewReading.SystemId);
                        sqlCommand.Parameters.AddWithValue("SENID", NewReading.SensorId);
                        sqlCommand.Parameters.AddWithValue("TEMPC", NewReading.TemperatureCelsius);
                        sqlCommand.Connection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlCommand.Connection.Close();
                    }
                }
            }
        }



    }
}