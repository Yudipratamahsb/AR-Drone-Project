using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Text;

namespace DroneController
{
    public static class MulticastHelper
    {
        // The address of the multicast group to join.
        // Must be in the range from 224.0.0.0 to 239.255.255.255
        private const string GROUP_ADDRESS = "224.1.1.1";

        // The port over which to communicate to the multicast group
        private const int GROUP_PORT = 5555;

        // A client receiver for multicast traffic from any source
        static UdpAnySourceMulticastClient _client = null;

        // true if we have joined the multicast group; otherwise, false
        static bool _joined = false;

        // Buffer for incoming data
        private static byte[] _receiveBuffer;

        // Maximum size of a message in this communication
        private const int MAX_MESSAGE_SIZE = 512;

        internal static void Join()
        {
            // Initialize the receive buffer
            _receiveBuffer = new byte[MAX_MESSAGE_SIZE];

            // Create the UdpAnySourceMulticastClient instance using the defined 
            // GROUP_ADDRESS and GROUP_PORT constants. UdpAnySourceMulticastClient is a 
            // client receiver for multicast traffic from any source, also known as Any Source Multicast (ASM)
            _client = new UdpAnySourceMulticastClient(IPAddress.Parse(GROUP_ADDRESS), GROUP_PORT);

            // Make a request to join the group.
            _client.BeginJoinGroup(
                result =>
                {
                    // Complete the join
                    _client.EndJoinGroup(result);

                    // The MulticastLoopback property controls whether you receive multicast 
                    // packets that you send to the multicast group. Default value is true, 
                    // meaning that you also receive the packets you send to the multicast group. 
                    // To stop receiving these packets, you can set the property following to false
                    _client.MulticastLoopback = true;

                    // Set a flag indicating that we have now joined the multicast group 
                    _joined = true;

                    Receive();
                }, null);
        }

        private static void Receive()
        {
            // Only attempt to receive if you have already joined the group
            if (_joined)
            {
                Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                    result =>
                    {
                        IPEndPoint source;

                        // Complete the asynchronous operation. The source field will 
                        // contain the IP address of the device that sent the message
                        _client.EndReceiveFromGroup(result, out source);


                        OnReceivedData(_receiveBuffer);

                        // Call receive again to continue to "listen" for the next message from the group
                        Receive();
                    }, null);
            }
        }

        private static void OnReceivedData(byte[] _receiveBuffer)
        {
            if (data_received != null)
            {
                data_received(null, new ReceivedEventArgs(_receiveBuffer));
            }
        }

        internal static event EventHandler<ReceivedEventArgs> data_received;


    }
}
