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
    public class DroneControllerConfiguration
    {
        #region Public Properties


        /// <summary>
        /// Gets or sets the ARDrone ip address.
        /// </summary>
        /// <value>The ARDrone ip address.</value>
        public string   DroneIpAddress { get; set; }
        /// <summary>
        /// Gets or sets the ip address of the guest that controls the ARDrone.
        /// </summary>
        /// <value>The ip address of the guest controlling the ARDrone.</value>
        public string GuestIpAddress { get; set; }
        /// <summary>
        /// Gets or sets the command port number.
        /// </summary>
        /// <value>The command port number.</value>
        public int      CommandPort { get; set; }
        /// <summary>
        /// Gets or sets the navigation data port number.
        /// </summary>
        /// <value>The navigation data port number.</value>
        public int      NavigationDataPort { get; set; }
        /// <summary>
        /// Gets or sets the video stream port number.
        /// </summary>
        /// <value>The video stream port number.</value>
        public int      VideoStreamPort { get; set; }
        /// <summary>
        /// Gets or sets the control info port number.
        /// </summary>
        /// <value>The control info port number.</value>
        public int      ControlInfoPort { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the AT command worker thread should be activated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the AT command worker thread should be activated; otherwise, <c>false</c>.
        /// </value>
        public bool     EnableATCommandThread { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the video stream worker thread should be activated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the video stream worker thread should be activated; otherwise, <c>false</c>.
        /// </value>
        public bool     EnableVideoStreamThread { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the navigation data stream worker thread should be activated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the navigation data worker thread should be activated; otherwise, <c>false</c>.
        /// </value>
        public bool     EnableNavigationDataThread{ get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the control info worker thread should be activated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the control info worker thread should be activated; otherwise, <c>false</c>.
        /// </value>
        public bool     EnableControlInfoThread { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether input feedback should be given to subscribers.
        /// </summary>
        /// <value><c>true</c> if input feedback should be given; otherwise, <c>false</c>.</value>
        public bool     EnableInputFeedback { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the AT commands should be simulated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the AT commands should be simulated; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>If simulation is active the AT commands are not sent through to the ARDrone.</remarks>
        public bool     EnableATCommandSimulation { get; set; }
        /// <summary>
        /// Gets or sets the default video file path where to save video recordings.
        /// </summary>
        /// <value>The default video file path.</value>
        public string   VideoFilePath { get; set; }
        /// <summary>
        /// Gets or sets the default picture file path where to save snapshots.
        /// </summary>
        /// <value>The default picture file path.</value>
        public string   PictureFilePath { get; set; }
        /// <summary>
        /// Gets or sets the roll throttle value.
        /// </summary>
        /// <value>The roll throttle value.</value>
        public Single RollThrottle { get; set; }
        /// <summary>
        /// Gets or sets the pitch throttle value.
        /// </summary>
        /// <value>The pitch throttle value.</value>
        public Single PitchThrottle { get; set; }
        /// <summary>
        /// Gets or sets the height throttle value.
        /// </summary>
        /// <value>The height throttle value.</value>
        public Single HeightThrottle { get; set; }
        /// <summary>
        /// Gets or sets the yaw throttle value.
        /// </summary>
        /// <value>The yaw throttle value.</value>
        public Single YawThrottle { get; set; }

        /// <summary>
        /// Gets or sets the drone info timer interval. The timer event will collect information on the ARDrone.
        /// </summary>
        /// <value>The drone info timer interval in milliseconds.</value>
        public int DroneInfoTimerInterval { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DroneControllerConfiguration"/> class.
        /// </summary>
        public DroneControllerConfiguration()
        {
            this.DroneIpAddress             = "192.168.1.1";
            this.GuestIpAddress             = "192.168.1.2";
            this.NavigationDataPort         = 5554;
            this.VideoStreamPort            = 6665;
            this.CommandPort                = 5556;
            this.ControlInfoPort            = 5559;
            this.GPSDataPort = 6666;
            
            this.EnableATCommandThread      = true;
            this.EnableGPSStreamThread      = true;
            this.EnableNavigationDataThread = true;
            this.EnableVideoStreamThread    = true;
            this.EnableControlInfoThread    = true;
            this.EnableInputFeedback        = false;

            this.RollThrottle               = Constants.DefaultRollThrottleValue;
            this.PitchThrottle              = Constants.DefaultPitchThrottleValue;
            this.HeightThrottle             = Constants.DefaultHeightThrottleValue;
            this.YawThrottle                = Constants.DefaultYawThrottleValue;

            DroneInfoTimerInterval          = Constants.DroneInfoTimerInterval;
        }

        public bool EnableGPSStreamThread { get; set; }

        public int GPSDataPort { get; set; }
    }
}
