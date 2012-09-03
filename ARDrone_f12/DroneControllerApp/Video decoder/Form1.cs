using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Video_decoder
{
    public partial class Form1 : Form
    {
        VideoImage i;
        public Form1()
        {
            InitializeComponent();
            i = new VideoImage();
            i.ImageComplete += new EventHandler<ImageCompleteEventArgs>(i_ImageComplete);
        }

        void i_ImageComplete(object sender, ImageCompleteEventArgs e)
        {
            pictureBox1.Image = e.ImageSource;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                IPEndPoint copter = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 5555);
                IPEndPoint copterAtCommands = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 5556);

                UdpClient cli = new UdpClient(5555);
                UdpClient cli2 = new UdpClient(5556);
                IPEndPoint remoteHost = null;
                
                string send_msg = "AT*REF=1,290717696\r";
                byte[] bytes = Encoding.ASCII.GetBytes(send_msg);
                cli2.Send(bytes, bytes.Length, copterAtCommands);
                cli.Send(new byte[] { 0 }, 1, copter);
                while (true)
                {
                    byte[] recieved = cli.Receive(ref remoteHost);
                    if (remoteHost.Equals(copter))
                    {
                        i.AddImageStream(recieved);
                    }
                }
            });
        }
    }
}
