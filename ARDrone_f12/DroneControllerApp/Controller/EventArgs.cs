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
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace DroneController
{
    /// <summary>
    /// This class passes the captured image from the ARDrone.
    /// </summary>
    public class VideoNotificationEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the current image.
        /// </summary>
        /// <value>The current image.</value>
        public WriteableBitmap CurrentImage { get; set; }

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoNotificationEventArgs"/> class.
        /// </summary>
        /// <param name="imageSource">The image to send.</param>
        public VideoNotificationEventArgs(WriteableBitmap imageSource)
        {
            this.CurrentImage = imageSource;
        }

        #endregion
    }



    /// <summary>
    /// This class passes information on received navigation commands by a inputprovider. 
    /// The event subscriber can use this info to show onscreen input handling.
    /// </summary>
    public class InputNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the roll value.
        /// </summary>
        /// <value>The roll value.</value>
        public Single Roll { get; set; }
        /// <summary>
        /// Gets or sets the pitch value.
        /// </summary>
        /// <value>The pitch value.</value>
        public Single Pitch { get; set; }
        /// <summary>
        /// Gets or sets the height value.
        /// </summary>
        /// <value>The height value.</value>
        public Single Height { get; set; }
        /// <summary>
        /// Gets or sets the yaw value.
        /// </summary>
        /// <value>The yaw value.</value>
        public Single Yaw { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputNotificationEventArgs"/> class.
        /// </summary>
        /// <param name="roll">The roll value.</param>
        /// <param name="pitch">The pitch value.</param>
        /// <param name="height">The height value.</param>
        /// <param name="yaw">The yaw value.</param>
        public InputNotificationEventArgs(Single roll, Single pitch, Single height, Single yaw)
        {
            this.Roll = roll;
            this.Pitch = pitch;
            this.Height = height;
            this.Yaw = yaw;
        }
    }

    /// <summary>
    /// This class notifies consumers with the latest connection status of the ARDrone.
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// This property provides information on the connection status of the ARDrone.
        /// </summary>
        /// <value>Returns a <see cref="ConnectionStatus"/> value.</value>
        public ConnectionStatus ConnectionStatus { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStatusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="connectionStatus">The connection status.</param>
        public ConnectionStatusChangedEventArgs(ConnectionStatus connectionStatus)
        {
            this.ConnectionStatus = connectionStatus;
        }
    }


    /// <summary>
    /// This class serves to notify event subscribers about messages coming from the DroneController.
    /// </summary>
    public class TraceNotificationEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        /// <value>The notification message.</value>
        public string NotificationMessage { get; set; }
        /// <summary>
        /// Gets or sets the notification source.
        /// </summary>
        /// <value>The notification source.</value>
        public NotificationSource NotificationSource { get; set; }
        /// <summary>
        /// Gets or sets the notification level.
        /// </summary>
        /// <value>The notification level.</value>
        public TraceNotificationLevel NotificationLevel { get; set; }

        #endregion

        #region Constuction

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceNotificationEventArgs"/> class.
        /// </summary>
        /// <param name="notificationMessage">The notification message.</param>
        /// <param name="notificationSource">The notification source.</param>
        /// <param name="notificationLevel">The notification level.</param>
        public TraceNotificationEventArgs(string notificationMessage, NotificationSource notificationSource, TraceNotificationLevel notificationLevel)
        {
            this.NotificationMessage = notificationMessage;
            this.NotificationSource = notificationSource;
            this.NotificationLevel = notificationLevel;
        }

        #endregion
    }



    

    /// <summary>
    /// This class passes ARDrone specific information to the event subscribers (e.g. BatteryLevel, Current Height, ...)
    /// </summary>
    public class DroneInfoNotificationEventArgs : EventArgs
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the battery level.
        /// </summary>
        /// <value>The battery level.</value>
        public int BatteryLevel { get; set; }
        /// <summary>
        /// Gets or sets the current height of the ARDrone.
        /// </summary>
        /// <value>The height of the ARDrone.</value>
        public int Height { get; set; }
        /// <summary>
        /// Gets or sets the theta (Pitch).
        /// </summary>
        /// <value>The theta value (Pitch).</value>
        public int Theta { get; set; }
        /// <summary>
        /// Gets or sets the phi value (Roll).
        /// </summary>
        /// <value>The phi value (Roll).</value>
        public int Phi { get; set; }
        /// <summary>
        /// Gets or sets the psi value (Yaw).
        /// </summary>
        /// <value>The psi value (Yaw).</value>
        public int Psi { get; set; }
        /// <summary>
        /// Gets or sets the drone status.
        /// </summary>
        /// <value>The drone status.</value>
        public DroneStatusFlags DroneStatus { get; set; }
        /// <summary>
        /// Gets or sets the drone configuration.
        /// </summary>
        /// <value>The drone configuration.</value>
        /// <emremarks>This property returns a name-value pair dictionary.</emremarks>
        public Dictionary<String, String> DroneConfiguration { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the video stream worker thread is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the video stream worker thread is active; otherwise, <c>false</c>.
        /// </value>
        public bool WorkerThreadVideoStreamActive { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the AT command worker thread is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the AT command worker thread is active; otherwise, <c>false</c>.
        /// </value>
        public bool WorkerThreadATCommandsActive { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the navigation data worker thread is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the navigation data worker thread is active; otherwise, <c>false</c>.
        /// </value>
        public bool WorkerThreadNavigationDataActive { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the control info worker thread is active].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the control info worker thread is active; otherwise, <c>false</c>.
        /// </value>
        public bool WorkerThreadControlInfoActive { get; set; }

        #endregion
    }

    public class ImageCompleteEventArgs : EventArgs
    {
        public ImageSource ImageSource { get; set; }

        public ImageCompleteEventArgs(ImageSource imageSource)
        {
            this.ImageSource = imageSource;
        }
    }
}
