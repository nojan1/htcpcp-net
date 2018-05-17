using htcpcp_net;
using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new HTCPCPListenerService("0.0.0.0", 9000);
            service.BrewingRequestRecieved += Service_BrewingRequestRecieved;

            service.Listen();
        }

        private static void Service_BrewingRequestRecieved(object sender, htcpcp_net.Model.BrewingRequestEventArgs e)
        {
            Console.WriteLine("Main program: Got brewing request");

            foreach(var addition in e.Additions)
            {
                Console.WriteLine($"{addition.Type}: {addition.Name}");
            }
        }
    }
}
