using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UDP_echo_app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input Desired Port:");
            string input = Console.ReadLine();
            short port = Int16.Parse(input.Split(' ')[0]);
            IPEndPoint cont = null;

            UdpClient cli = new UdpClient(port);
            IPEndPoint remoteHost = null;
            while (true)
            {
                byte[] recieved = cli.Receive(ref remoteHost);
                cont = remoteHost;
                Console.WriteLine("Packet recieved from controller at ({0})", remoteHost);
                Console.WriteLine("Sending to ({0})", remoteHost);
                cli.Send(recieved, recieved.Length, remoteHost);
            }
        }
    }
}
