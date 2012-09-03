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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DroneController
{
    internal class Constants
    {
        /// <summary>
        /// The default interval at which the navigation data is read from the UDP channel.
        /// </summary>
        internal const int NavigationDataRefreshInterval = 60;
        /// <summary>
        /// The start sequence when captureing navigation data.
        /// </summary>
        internal const int DefaultNavigationDataSequence = 0;
        /// <summary>
        /// The interval at which to collect drone specific information and pass it to subscribers.
        /// </summary>
        internal const int DroneInfoTimerInterval = 1000;
        /// <summary>
        /// The default videoframerate used when creating video recordings. This value is chosen to be as luch in sync with the native image frame rate of the ARDrone.
        /// </summary>
        internal const int VideoFrameRate = 12;
        /// <summary>
        /// The default time out in millseconds for the navigation data channel to receive messages.
        /// </summary>
        internal const int SocketTimeoutNavigationData = 500;
        /// <summary>
        /// The default time out in millseconds for the video stream channel to receive messages.
        /// </summary>
        internal const int SocketTimeoutVideoStream = 500;
        /// <summary>
        /// The default time out in millseconds for the command channel to receive messages.
        /// </summary>
        internal const int SocketTimeoutATCommand = 500;
        /// <summary>
        /// The default time out in millseconds for the controle channel to receive acknowledgement messages.
        /// </summary>
        internal const int SocketTimeoutControlInfo = 0;
        /// <summary>
        /// The default size of the buffer used to receive control messages.
        /// </summary>
        internal const int ControlInfoBufferSize = 4096;
        /// <summary>
        /// A fixed byte sequence used to recognize the start of a navigation data message.
        /// </summary>
        internal const int NavigationDataHeader = 0x55667788;
        /// <summary>
        /// The default throttle value for the pitch parameter.
        /// </summary>
        internal const Single DefaultPitchThrottleValue = .3f;
        /// <summary>
        /// The default throttle value for the roll parameter.
        /// </summary>
        internal const Single DefaultRollThrottleValue = .3f;
        /// <summary>
        /// The default throttle value for the height parameter.
        /// </summary>
        internal const Single DefaultHeightThrottleValue = .2f;
        /// <summary>
        /// The default throttle value for the yaw parameter.
        /// </summary>
        internal const Single DefaultYawThrottleValue = .6f;
        /// <summary>
        /// The fixed part of the auto generated filename when saving a picture.
        /// </summary>
        internal const string DefaultPictureFileNamePattern = "DronePicture";
        /// <summary>
        /// The fixed part of the auto generated filename when saving a recorded video.
        /// </summary>
        internal const string DefaultVideoFileNamePattern = "DroneVideo";
    }
}
