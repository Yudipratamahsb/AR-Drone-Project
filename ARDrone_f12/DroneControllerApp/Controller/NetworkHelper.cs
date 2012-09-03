#region Copyright Notice

//Copyright © 2007-2011, PARROT SA, all rights reserved. 

//DISCLAIMER 
//The APIs is provided by PARROT and contributors "AS IS" and any express or implied warranties, including, but not limited to, the implied warranties of merchantability 
//and fitness for a particular purpose are disclaimed. In no event shall PARROT and contributors be liable for any direct, indirect, incidental, special, exemplary, or 
//consequential damages (including, but not limited to, procurement of substitute goods or services; loss of use, data, or profits; or business interruption) however 
//caused and on any theory of liability, whether in contract, strict liability, or tort (including negligence or otherwise) arising in any way out of the use of this 
//software, even if advised of the possibility of such damage. 

//Author            : Wilke Jansoone
//Email             : wilke.jansoone@digitude.net
//Publishing date   : 28/11/2010 

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions
//are met:
//    - Redistributions of source code must retain the above copyright notice, this list of conditions, the disclaimer and the original author of the source code.
//    - Neither the name of the PixVillage Team, nor the names of its contributors may be used to endorse or promote products derived from this software without 
//      specific prior written permission.

#endregion

using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace DroneController
{
    internal static class NetworkHelper
    {
        private static int TimeOut = 0;

        const int MAX_BUFFER_SIZE = 1024*2;

        static Dictionary<Socket, ManualResetEvent> _clientDone = new Dictionary<Socket, ManualResetEvent>();
        static Dictionary<Socket, IPEndPoint> _local_end_points = new Dictionary<Socket, IPEndPoint>();


        internal static int SendUdp(Socket s, byte[] msg, int len, IPEndPoint EP)
        {
            string response = "Operation Timeout";
            int sent = 0;

            if (s != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                // Set properties on context object
                socketEventArg.RemoteEndPoint = EP;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object ss, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();
                    sent = e.BytesTransferred;
                    // Unblock the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                });

                // Add the data to be sent into the buffer
                socketEventArg.SetBuffer(msg, 0, len);

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone[s].Reset();

                // Make an asynchronous Send request over the socket
                if (s.SendToAsync(socketEventArg) == false)
                {
                    response = socketEventArg.SocketError.ToString();
                    sent = socketEventArg.BytesTransferred;
                    // Unblock the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone[s].WaitOne(TimeOut);
            }
            return sent;
        }

        internal static void SendTcp(Socket s, byte[] msg, int len, IPEndPoint EP)
        {
            string response = "Operation Timeout";

            if (s != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                // Set properties on context object
                socketEventArg.RemoteEndPoint = EP;
                socketEventArg.UserToken = null;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object ss, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    // Unblock the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                });

                // Add the data to be sent into the buffer
                socketEventArg.SetBuffer(msg, 0, len);

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone[s].Reset();

                // Make an asynchronous Send request over the socket
                if (s.SendAsync(socketEventArg) == false)
                {
                    response = socketEventArg.SocketError.ToString();

                    // Unblock the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone[s].WaitOne(TimeOut);
            }
        }

        internal static byte[] UdpRecieve(Socket s, ref IPEndPoint EP)
        {
            string response = "Operation Timeout";

            byte[] msg = null;
            if (s != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = EP;

                // Setup the buffer to receive the data
                socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);

                // Inline event handler for the Completed event.
                // Note: This even handler was implemented inline in order to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object ss, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        msg = new byte[e.BytesTransferred];
                        Array.Copy(e.Buffer, e.Offset, msg, 0, e.BytesTransferred);
                    }
                    else
                    {
                        response = e.SocketError.ToString();
                    }

                    try 
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                });

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone[s].Reset();

                // Make an asynchronous Receive request over the socket
                if (s.ReceiveFromAsync(socketEventArg) == false)
                {
                    if (socketEventArg.SocketError == SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        msg = new byte[socketEventArg.BytesTransferred];
                        Array.Copy(socketEventArg.Buffer, socketEventArg.Offset, msg, 0, socketEventArg.BytesTransferred);
                    }
                    else
                    {
                        response = socketEventArg.SocketError.ToString();
                    }
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone[s].WaitOne();//(TimeOut);
            }

            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="buf"></param>
        /// <param name="begin"></param>
        /// <param name="len"></param>
        /// <returns>byte count</returns>
        internal static int TcpRecieve(Socket s, byte[] buf, int begin, int len)
        {
            string response = "Operation Timeout";
            int read_bytes = 0;

            if (s != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = s.RemoteEndPoint;

                // Setup the buffer to receive the data
                socketEventArg.SetBuffer(new Byte[len], 0, len);

                // Inline event handler for the Completed event.
                // Note: This even handler was implemented inline in order to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object ss, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        read_bytes = e.BytesTransferred;
                        Array.Copy(e.Buffer, e.Offset, buf, begin, e.BytesTransferred);
                    }
                    else
                    {
                        response = e.SocketError.ToString();
                    }

                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                });

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone[s].Reset();

                // Make an asynchronous Receive request over the socket
                if (s.ReceiveAsync(socketEventArg) == false)
                {
                    if (socketEventArg.SocketError == SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        read_bytes = socketEventArg.BytesTransferred;
                        Array.Copy(socketEventArg.Buffer, socketEventArg.Offset, buf, begin, socketEventArg.BytesTransferred);
                    }
                    else
                    {
                        response = socketEventArg.SocketError.ToString();
                    }

                    try 
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone[s].WaitOne();//(TimeOut);
            }
            return read_bytes;
        }

        internal static void TcpConnect(Socket s, IPEndPoint EP)
        {
            if (s != null)
            {
                string result = string.Empty;


                // Create a SocketAsyncEventArgs object to be used in the connection request
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = EP;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object ss, SocketAsyncEventArgs e)
                {
                    // Retrieve the result of this request
                    result = e.SocketError.ToString();

                    // Signal that the request is complete, unblocking the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                });

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone[s].Reset();

                // Make an asynchronous Connect request over the socket
                if (s.ConnectAsync(socketEventArg) == false)
                {
                    // Retrieve the result of this request
                    result = socketEventArg.SocketError.ToString();

                    // Signal that the request is complete, unblocking the UI thread
                    try
                    {
                        _clientDone[s].Set();
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                }

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone[s].WaitOne(TimeOut);


            }
        }

        internal static Socket CreateUdpSocket(string ipString, int port, int timeOut)
        {

            Socket  udpClient = null;
            
            try
            {
                udpClient = CreateUdpSocket(ipString, port);
                _clientDone.Add(udpClient, new ManualResetEvent(false));
                TimeOut = timeOut;
            }
            catch
            {
                throw;
            }

            return udpClient;
        }

        internal static Socket CreateUdpSocket(string ipString, int port)
        {
            Socket udpClient = null;

            try
            {
                IPAddress ipLocalAddress = IPAddress.Parse(ipString);
                IPEndPoint ipLocalEndPoint = new IPEndPoint(ipLocalAddress, port);
                udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            }
            catch
            {
                throw;
            }

            return udpClient;
        }

        internal static Socket CreateTcpSocket(string ipString, int port, int timeOut)
        {

            Socket tcpClient = null;

            try
            {
                tcpClient = CreateTcpSocket(ipString, port);
                _clientDone.Add(tcpClient, new ManualResetEvent(false));
                if (timeOut != 0) TimeOut = timeOut;
            }
            catch
            {
                throw;
            }

            return tcpClient;
        }

        internal static Socket CreateTcpSocket(string ipString, int port)
        {
            Socket tcpClient = null;

            try
            {
                IPAddress ipLocalAddress = IPAddress.Parse(ipString);
                IPEndPoint ipLocalEndPoint = new IPEndPoint(ipLocalAddress, port);
                tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//new TcpClient(ipLocalEndPoint);
            }
            catch
            {
                throw;
            }

            return tcpClient;
        }

        internal static IPEndPoint CreateRemoteEndPoint(string remoteIPString, int port)
        {
            IPEndPoint ipEndPoint = null;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(remoteIPString);
                ipEndPoint = new IPEndPoint(ipAddress, port);
            }
            catch { }

            return ipEndPoint;
        }

        internal static void CloseUdpConnection(Socket socket)
        {
            if (socket != null)
            {
                _clientDone.Remove(socket);
                socket.Close();
            }
        }

        internal static void CloseTcpConnection(Socket socket)
        {
            if (socket != null)
            {
                _clientDone.Remove(socket);
                socket.Close();
            }
        }

    }
}
