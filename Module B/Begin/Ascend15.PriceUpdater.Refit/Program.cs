using System;
using System.Collections.Generic;
using System.Net;

namespace Ascend15.PriceUpdater.Refit
{
    internal class Program
    {
        private static void Main()
        {
            // GREAT SUCCESS! :)
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            var newPrices = new Dictionary<string, decimal>
            {
                {
                    "Awesome-Glasses--Black-L", 289.99M
                },
                {
                    "Awesome-Glasses--Blue-L", 255.55M
                },
                {
                    "Awesome-Glasses--Green-L", 244.44M
                },
                {
                    "Awesome-Glasses--Red-L", 211.11M
                },
                {
                    "Another-Awesome-Glasses--Black-M", 899.99M
                },
                {
                    "Another-Awesome-Glasses--Blue-M", 855.55M
                },
                {
                    "Another-Awesome-Glasses--Red-M", 844.44M
                },
            };

            var service = new PriceUpdateService();

            Console.WriteLine("Starting price import...");
            service.ImportPricesAsync(newPrices).Wait();
            Console.WriteLine("Price import done.");

            Console.ReadLine();
        }
    }
}
