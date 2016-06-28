using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OctanificationServer.Server;

namespace OctanificationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get the current directory.
                string resourcesPath = ResourceManager();
                int port = RandomPort();

                ServerListener serverListener = new ServerListener(port, resourcesPath);
                serverListener.StartServer();
            }
            catch (Exception e)
            {

                Console.WriteLine("An error occurred: '{0}'", e);
            }

        }


        private static string ResourceManager()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string staticFile = @"..\..\staticData\OctanUsers.json";
            return Path.Combine(currentDirectory, staticFile);
        }

        private static int RandomPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
