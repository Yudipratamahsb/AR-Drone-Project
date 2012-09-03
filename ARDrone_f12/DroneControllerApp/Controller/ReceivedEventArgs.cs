using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DroneController
{
    internal class ReceivedEventArgs : EventArgs
    {
        internal byte[] buf;
        internal ReceivedEventArgs(byte[] b)
        {
            buf = b;
        }
    }
}
