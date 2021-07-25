using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EnvironmentSensorDashboard.Data;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

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
                .Build();


            string keyvault_endpoint = configuration["KEYVAULT_ENDPOINT"];
            if (!string.IsNullOrEmpty(keyvault_endpoint))
            {
                ConsoleWrite("Loading configuration from Azure Key Vault: " + keyvault_endpoint);
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(
                                new KeyVaultClient.AuthenticationCallback(
                                    azureServiceTokenProvider.KeyVaultTokenCallback));

                configuration = new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .AddAzureKeyVault(keyvault_endpoint, keyVaultClient, new DefaultKeyVaultSecretManager())
                    .Build();
            }

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

                    PiEnvMonSensorResponse response = await GetPiData(device.IPAddress);
                    if (response == null) {
                        Console.WriteLine("FAIL");
                    } else {
                        ConsoleWrite($"Response from {response.System.Name}...");
                                                
                        // Update device info
                        device.LastSeenUTC = DateTime.Now.ToUniversalTime();
                        device.Name = response.System.Name;
                        device.Model = response.System.Model;
                        device.Description = response.System.Description;
                        device.Serial = response.System.Serial; 

                        
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

                        // Update the device record 
                        try {
                            deviceRepo.Update(device);
                        } catch(Exception ex) {
                            ConsoleWrite("EXCEPTION: " + ex.Message);
                        }                       
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
