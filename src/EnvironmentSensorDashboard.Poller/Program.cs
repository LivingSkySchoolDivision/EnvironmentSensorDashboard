using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnvironmentSensorDashboard.Poller
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();

        private static async Task<PiEnvMonSensorResponse> GetPiData(string ip)
        {
            try {
                string results = await _httpClient.GetStringAsync(ip);
                return JsonSerializer.Deserialize<PiEnvMonSensorResponse>(results);
            } catch {}

            return null;
        }

        static async Task Main(string[] args)
        {
            // Test IPs to use
            List<string> IPAddresses = new List<string>() {
                "http://10.177.64.243",
                "http://10.177.76.243",
                "http://10.177.70.243",
                "http://10.177.8.243",
                "http://10.177.8.244",
                "http://10.177.54.243",
                "http://10.177.54.244",
                "http://10.177.30.243",
                "http://10.177.81.243",
                "http://10.177.11.243",
                "http://10.177.72.243",
                "http://10.177.20.243",
                "http://10.177.23.243",
                "http://10.177.0.243",
                "http://10.177.0.244",
                "http://10.177.74.243",
                "http://10.177.16.243",
                "http://10.177.16.244",
                "http://10.177.18.243",
                "http://10.177.26.243",
                "http://10.177.203.206",
                "http://10.177.203.207",
                "http://10.177.203.208",
                "http://10.177.203.209",
                "http://10.177.14.243",
                "http://10.177.14.244",
                "http://10.177.68.243",
                "http://10.177.68.244",
                "http://10.177.7.243",
                "http://10.177.7.244",
                "http://10.177.31.243",
                "http://10.177.24.244"
            };

            // Reach out and try to deserialize the data from the pi


            foreach(string ip in IPAddresses) {
                Console.Write(ip + ": ");
                PiEnvMonSensorResponse response = await GetPiData(ip);
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
