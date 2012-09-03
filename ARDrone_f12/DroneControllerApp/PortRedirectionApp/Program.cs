using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace PortRedirectionApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input Desired Port:");
            string input = Console.ReadLine();
            short port = Int16.Parse(input.Split(' ')[0]);
            IPEndPoint cont = null;
            IPEndPoint copter = new IPEndPoint(IPAddress.Parse("192.168.1.1"), port);

            UdpClient cli = new UdpClient(port);
            IPEndPoint remoteHost = null;
            while (true)
            {
                byte[] recieved = cli.Receive(ref remoteHost);
                if (remoteHost.Equals(copter))
                {
                    Console.WriteLine("Packet recieved from copter at ({0})", remoteHost);
                    if (cont != null)
                    {
                        Console.WriteLine("Sending to ({0})", cont);
                        cli.Send(recieved, recieved.Length, cont);
                    }
                }
                else
                {
                    cont = remoteHost;
                    Console.WriteLine("Packet recieved from controller at ({0})", remoteHost);
                    Console.WriteLine("Sending to ({0})", copter);
                    cli.Send(recieved, recieved.Length, copter);
                }
            }
        }
    }
}
