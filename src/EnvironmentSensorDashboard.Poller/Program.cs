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
        private static HttpClient _httpClient = new HttpClient();

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
            PiEnvMonDeviceRepository deviceRepo = new PiEnvMonDeviceRepository(dbConnectionString);



            // Test IPs to use
            List<PiEnvMonSensorDevice> enabledDevices = deviceRepo.GetAllEnabled();

            foreach(PiEnvMonSensorDevice device in enabledDevices) 
            {
                // Reach out and try to deserialize the data from the pi
                ConsoleWrite($"Polling {device.IPAddress}...");

                PiEnvMonSensorResponse response = await GetPiData(device.IPAddress);
                if (response == null) {
                    Console.WriteLine("FAIL");
                } else {
                    Console.Write(response.System.Name + " ");
                    Console.Write(response.CPUSensorReading.ReadingTimestamp.ToShortDateString() + " " + response.CPUSensorReading.ReadingTimestamp.ToLongTimeString());

                    if (response.TemperatureReadings.Count > 0) {
                        Console.WriteLine(" " + response.TemperatureReadings[0].TemperatureCelsius + "C " + response.HumidityReadings[0].HumidityPercent + "%");
                    } else {
                        Console.WriteLine(" NO DATA");
                    }
                }

            }

        }
    }
}
