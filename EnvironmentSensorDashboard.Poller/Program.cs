using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EnvironmentSensorDashboard.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace EnvironmentSensorDashboard.Poller
{
    class Program
    {
        private const int _sleepMinutes = 10;

        private static HttpClient _httpClient = new HttpClient() {
            Timeout = new TimeSpan(0,0,5)
        };

        private static async Task<PiEnvMonSensorResponse> GetPiData(string ip)
        {
            try {
                string results = await _httpClient.GetStringAsync($"http://{ip}");
                return JsonSerializer.Deserialize<PiEnvMonSensorResponse>(results);
            } catch {}

            return null;
        }
        
        private static void ConsoleWrite(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm K") + ": " + message);
        }

        static async Task Main(string[] args)
        {

            // Load configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
        

            string dbConnectionString = configuration.GetConnectionString("InternalDatabase") ?? string.Empty;


            // Load stuff from database

            // Main loop
            while (true) 
            {
                PiEnvMonDeviceRepository deviceRepo = new PiEnvMonDeviceRepository(dbConnectionString);
                PiEnvMonCPUSensorDataRespository cpuTempRepo = new PiEnvMonCPUSensorDataRespository(dbConnectionString);
                PiEnvMonTemperatureSensorReadingRepository tempRepo = new PiEnvMonTemperatureSensorReadingRepository(dbConnectionString);
                PiEnvMonHumiditySensorDataRepository humidityRepo = new PiEnvMonHumiditySensorDataRepository(dbConnectionString);

                ConsoleWrite($"Refreshing list of enabled devices...");
                List<PiEnvMonSensorDevice> enabledDevices = deviceRepo.GetAllEnabled();

                foreach(PiEnvMonSensorDevice device in enabledDevices) 
                {
                    // Reach out and try to deserialize the data from the pi
                    ConsoleWrite($"Polling {device.IPAddress}...");

                    device.LastScanAttemptUTC = DateTime.Now.ToUniversalTime();

                    PiEnvMonSensorResponse response = await GetPiData(device.IPAddress);
                    if (response == null) {
                        Console.WriteLine("FAIL");
                        device.WasLastPollSuccessful = false;
                        device.LastFailureUTC = DateTime.Now.ToUniversalTime();
                    } else {
                        ConsoleWrite($"> Response from {response.System.Name}...");
                        device.WasLastPollSuccessful = true;
                        device.LastSuccessUTC = DateTime.Now.ToUniversalTime();
                                                
                        // Update device info
                        device.Name = response.System.Name;
                        device.Model = response.System.Model;
                        device.Description = response.System.Description;
                        device.Serial = response.System.Serial; 

                        Console.WriteLine($"   CPUSensorReading.TemperatureCelsius: {response.CPUSensorReading.TemperatureCelsius}");
                        Console.WriteLine($"   CPUSensorReading.SystemDatabaseId: {response.CPUSensorReading.SystemDatabaseId}");
                        Console.WriteLine($"   device.LastCPUTemp: {device.LastCPUTemp}");
                        Console.WriteLine($"   device.LastCPUTempTimeUTC: {device.LastCPUTempTimeUTC}");

                        foreach (var reading in response.TemperatureReadings)
                        {
                            Console.WriteLine($"    reading.TemperatureCelsius: {reading.TemperatureCelsius}");
                            Console.WriteLine($"    reading.SystemDatabaseId: {reading.SystemDatabaseId}");
                            Console.WriteLine($"    device.LastTempCelsius: {device.LastTempCelsius}");
                            Console.WriteLine($"    device.LastTempTimeUTC: {device.LastTempTimeUTC}");
                        }

                        foreach (var reading in response.HumidityReadings)
                        {
                            Console.WriteLine($"    reading.HumidityPercent: {reading.HumidityPercent}");
                            Console.WriteLine($"    reading.SystemDatabaseId: {reading.SystemDatabaseId}");
                        }
                                                

                        
                        // Record CPU temp reading
                        if (response.CPUSensorReading.TemperatureCelsius > -999) 
                        {
                            response.CPUSensorReading.SystemDatabaseId = device.DatabaseId;

                            // Update last known info on device record
                            if (response.CPUSensorReading.ReadingTimestamp > device.LastCPUTempTimeUTC)
                            {
                                device.LastCPUTemp = response.CPUSensorReading.TemperatureCelsius;
                                device.LastCPUTempTimeUTC = response.CPUSensorReading.ReadingTimestamp;
                            }
                            
                            try {
                                cpuTempRepo.Insert(response.CPUSensorReading);
                            } catch(Exception ex) {
                                ConsoleWrite("EXCEPTION: " + ex.Message);
                            }
                        }

                        // Record temp reading(s)
                        foreach(var reading in response.TemperatureReadings) 
                        {
                            if (reading.TemperatureCelsius > -999) 
                            {
                                reading.SystemDatabaseId = device.DatabaseId;

                                // Update last known info on device record
                                if (reading.ReadingTimestamp > device.LastTempTimeUTC)
                                {
                                    device.LastTempCelsius = reading.TemperatureCelsius;
                                    device.LastTempTimeUTC = reading.ReadingTimestamp;
                                }

                                try {
                                    tempRepo.Insert(reading);
                                } catch(Exception ex) {
                                    ConsoleWrite("EXCEPTION: " + ex.Message);
                                }
                            }

                        }

                        // Record humidity reading(s)
                        foreach(var reading in response.HumidityReadings) 
                        {
                            if (reading.HumidityPercent > -999) 
                            {
                                reading.SystemDatabaseId = device.DatabaseId;

                                // Update last known info on device record
                                if (reading.ReadingTimestamp > device.LastHumidityTimeUTC)
                                {
                                    device.LastHumidityPercent = reading.HumidityPercent;
                                    device.LastHumidityTimeUTC = reading.ReadingTimestamp;
                                }

                                try {
                                    humidityRepo.Insert(reading);
                                } catch(Exception ex) {
                                    ConsoleWrite("EXCEPTION: " + ex.Message);
                                }
                            }
                        }                                              
                    }

                    // Update the device record 
                    try {
                        ConsoleWrite("Updating device " + device.DatabaseId + "...");
                        deviceRepo.Update(device);
                    } catch(Exception ex) {
                        ConsoleWrite("EXCEPTION: " + ex.Message);
                    } 
                }

                ConsoleWrite("Done!");
                
                // Sleep
                ConsoleWrite($"Sleeping for {_sleepMinutes} minutes...");
                Task.Delay(_sleepMinutes * 60 * 1000).Wait();
            } // Main loop

        }
    }
}
