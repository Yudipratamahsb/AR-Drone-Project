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

    /// <summary>
    /// Specifies the image format of the returned image.
    /// </summary>
    internal enum PictureFormats
    {
        /// <summary>
        /// 176px x 144px
        /// </summary>
        Cif = 1,
        /// <summary>
        /// 320px x 240px
        /// </summary>
        Vga = 2
    }

    /// <summary>
    /// This enumeration represnets the ar.drone connection status
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// No connection established with the ARDrone.
        /// </summary>
        Closed,
        /// <summary>
        /// A request has been made to close the connection with the ARDrone.
        /// </summary>
        CloseRequested,
        /// <summary>
        /// A connection with the ARDrone is being set up.
        /// </summary>
        Initializing,
        /// <summary>
        /// A connection with the ARDrone has been established.
        /// </summary>
        Open,
        /// <summary>
        /// An erro occurred during a connection set up with the ARDrone.
        /// </summary>
        Error
    }

    internal enum NavigationDataTag : uint
    {
        NavDataDemo = 0,
        NavDataTime,
        NavDataRawMeasures,
        NavDataPhysicalMeasures,
        NavDayrosOffsetsS,
        NavDataEulerAngles,
        NavDataReferences,
        NavDataTrims,
        NavDataRcReferences,
        NavDataPwm,
        NavDataAltitude,
        NavDataVisionRaw,
        NavDataVisionOf,
        NavDataVision,
        NavDataVisionPerf,
        NavDataTrackersSend,
        NavDataVisionDetect,
        NavDataWatchDog,
        NavDataAdcDataFrame,
        NavDataCks = 0xFFFF
    }


    /// <summary>
    /// Identifies input values sent to the ARDrone.
    /// </summary>
    /// <remarks>Currently only bits 8 and 9 are used and supported.</remarks>
    [Flags]
    internal enum DroneInputValueFlags : int
    {
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_AG = 0, /* Button turn to left */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_AB = 1, /* Button altitude down (ah - ab)*/
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_AD = 2, /* Button turn to right */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_AH = 3, /* Button altitude up (ah - ab)*/
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_L1 = 4, /* Button - z-axis (r1 - l1) */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_R1 = 5, /* Not used */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_L2 = 6, /* Button + z-axis (r1 - l1) */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_R2 = 7, /* Not used */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_SELECT = 8, /* Button emergency reset all */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_START = 9, /* Button Takeoff / Landing */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_TRIM_THETA = 18, /* y-axis trim +1 (Trim increase at +/- 1??/s) */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_TRIM_PHI = 20, /* x-axis trim +1 (Trim increase at +/- 1??/s) */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_TRIM_YAW = 22, /* z-axis trim +1 (Trim increase at +/- 1??/s) */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_X = 24, /* x-axis +1 */
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_UI_BIT_Y = 28, /* y-axis +1 */
    }


    /// <summary>
    /// Used to pass the status of the ARDrone. See ARDrone developer's guide for more information.
    /// </summary>
    [Flags]
    public enum DroneStatusFlags : uint
    {
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_FLY_MASK = 1,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_VIDEO_MASK = 2,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_VISION_MASK = 4,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_CONTROL_MASK = 8,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ALTITUDE_MASK = 16,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_USER_FEEDBACK_START = 32,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_COMMAND_MASK = 64,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_TRIM_COMMAND_MASK = 128,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_TRIM_RUNNING_MASK = 256,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_TRIM_RESULT_MASK = 512,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_NAVDATA_DEMO_MASK = 1024,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_NAVDATA_BOOTSTRAP = 2048,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_MOTORS_BRUSHED = 4096,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_COM_LOST_MASK = 8192,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_GYROS_ZERO = 16384,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_VBAT_LOW = 32768,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_VBAT_HIGH = 65536,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_TIMER_ELAPSED = 131072,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_NOT_ENOUGH_POWER = 262144,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ANGLES_OUT_OF_RANGE = 524288,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_WIND_MASK = 1048576,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ULTRASOUND_MASK = 2097152,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_CUTOUT_MASK = 4194304,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_PIC_VERSION_MASK = 8388608,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ATCODEC_THREAD_ON = 16777216,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_NAVDATA_THREAD_ON = 33554432,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_VIDEO_THREAD_ON = 67108864,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ACQ_THREAD_ON = 134217728,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_CTRL_WATCHDOG_MASK = 268435456,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_ADC_WATCHDOG_MASK = 536870912,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_COM_WATCHDOG_MASK = 1073741824,
        /// <summary>
        /// 
        /// </summary>
        AR_DRONE_EMERGENCY_MASK = 2147483648
    }


    /// <summary>
    /// Identifies which camera captures the images.
    /// </summary>
    public enum VideoChannel
    {
        /// <summary>
        /// Captured images are coming from the horizontal (forward) camera.
        /// </summary>
        Horizontal = 0,
        /// <summary>
        /// Captured images are coming from the vertical (downward) camera.
        /// </summary>
        Vertical,
        /// <summary>
        /// Captured images are coming from both the vertical and horizontal camera. The vertical image is shown in upper left corner.
        /// </summary>
        VerticalInHorizontal,
        /// <summary>
        /// Captured images are coming from both the vertical and horizontal camera. The horizontal image is shown in upper left corner.
        /// </summary>
        HorizontalInVertical,
        /// <summary>
        /// Captured images are coming the next videochannel determined by this enumeration.
        /// </summary>
        Next
    }

    /// <summary>
    /// Indicates the LED animation to perform.
    /// </summary>
    public enum LedAnimation
    {
        /// <summary>
        /// 
        /// </summary>
        BlinkGreenRed,
        /// <summary>
        /// 
        /// </summary>
        BlinkGreen,
        /// <summary>
        /// 
        /// </summary>
        BlinkRed,
        /// <summary>
        /// 
        /// </summary>
        SnakeGreenRed,
        /// <summary>
        /// 
        /// </summary>
        Fire,
        /// <summary>
        /// 
        /// </summary>
        Standard,
        /// <summary>
        /// 
        /// </summary>
        Red,
        /// <summary>
        /// 
        /// </summary>
        Green
    }


    /// <summary>
    /// Indicates the source of the notification message.
    /// </summary>
    public enum NotificationSource
    {
        /// <summary>
        /// The notification message comes from the DroneController.
        /// </summary>
        DroneController,
        /// <summary>
        /// The notification message comes from the DroneProxy (intermediate to the ARDrone).
        /// </summary>
        DroneProxy,
        /// <summary>
        /// The notification message comes from the communicationcenter.
        /// </summary>
        CommunicationCenter,
        /// <summary>
        /// The notification message comes from the commandcenter.
        /// </summary>
        CommandCenter,
    }

    /// <summary>
    /// This enumeration can be used to trim the trace messages.
    /// </summary>
    public enum TraceNotificationLevel
    {
        /// <summary>
        /// Trace all messages.
        /// </summary>
        Verbose,
        /// <summary>
        /// Trace messages with at least information level.
        /// </summary>
        Information,
        /// <summary>
        /// Trace messages with at least warning level.
        /// </summary>
        Warning,
        /// <summary>
        /// Trace messages with at least error level.
        /// </summary>
        Error,
        /// <summary>
        /// Block all trace messages.
        /// </summary>
        None
    }


    /// <summary>
    /// Identifies the different communication channels.
    /// </summary>
    internal enum CommunicationChannel
    {
        /// <summary>
        /// Ths communication channel is used to send AT commands.
        /// </summary>
        Command,
        /// <summary>
        /// This communication channel is used to receive navigation data.
        /// </summary>
        NavigationData,
        /// <summary>
        /// This communication channel is used to receive video images.
        /// </summary>
        VideoStream,
        /// <summary>
        /// This communication channel is used to send control messages.
        /// </summary>
        ControlInfo,
        GPSData
    }


    /// <summary>
    /// Static class containing the AT commands currently supported by the DroneController.
    /// </summary>
    internal static class ATCommands
    {
        /// <summary>
        /// This AT command is used to switch between different camera views.
        /// </summary>
        internal static readonly string SwitchVideoChannel = "AT*ZAP={0},{1}\r";
        /// <summary>
        /// This AT Command is used for take off/land and emergency reset.
        /// </summary>
        internal static readonly string SetInputValue = "AT*REF={0},{1}\r";
        /// <summary>
        /// This AT command sets a reference of the horizontal plane for the drone internal control system.
        /// </summary>
        internal static readonly string SetFlatTrim = "AT*FTRIM={0}\r";
        /// <summary>
        /// This AT Command sets an configurable option on the drone.
        /// </summary>
        internal static readonly string SetConfiguration = "AT*CONFIG={0},\"{1}\",\"{2}\"\r";
        /// <summary>
        /// This AT Command is used when communicating with the control communication channel.
        /// </summary>
        internal static readonly string SetControlMode = "AT*CTRL={0},{1},{2}\r";
        /// <summary>
        /// This AT Command makes the ARDrone animate its LED's according to a selectable pattern.
        /// </summary>
        internal static readonly string PlayLedAnimation = "AT*LED={0},{1},{2},{3}\r";
        /// <summary>
        /// This AT Command is used to provide the ARDrone with piloting instructions.
        /// </summary>
        internal static readonly string SetProgressiveInputValues = "AT*PCMD={0},{1},{2},{3},{4},{5}\r";
        /// <summary>
        /// This AT Command activates/deactivates the detection of coloured patterns.
        /// </summary>
        internal static readonly string SetTagDetection = "AT*CAD={0},{1},{2}\r";
        /// <summary>
        /// This AT Command resets the internal ARDrone communication system.
        /// </summary>
        internal static readonly string ResetCommunicationHub = "AT*COMWDG={0}\r";
    }

    internal enum ControlMode
    {
        NO_CONTROL_MODE = 0,          // Doing nothing
        ARDRONE_UPDATE_CONTROL_MODE,  // Ardrone software update reception (update is done next run)
        // After event completion, card should power off
        PIC_UPDATE_CONTROL_MODE,      // Ardrone pic software update reception (update is done next run)
        // After event completion, card should power off
        LOGS_GET_CONTROL_MODE,        // Send previous run's logs
        CFG_GET_CONTROL_MODE,         // Send activ configuration
        ACK_CONTROL_MODE              // Reset command mask in navdata
    }


    /// <summary>
    /// Identitifies whether the ARDrone detects predetermined coloured patterns.
    /// </summary>
    internal enum DetectionType
    {
        /// <summary>
        /// 
        /// </summary>
        CAD_TYPE_HORIZONTAL = 0,
        /// <summary>
        /// 
        /// </summary>
        CAD_TYPE_VERTICAL,
        /// <summary>
        /// Makes the ARDrone recognize predetermined coloured patterns.
        /// </summary>
        CAD_TYPE_VISION_DETECT,
        /// <summary>
        /// Stops the detection of predetermined coloured patterns.
        /// </summary>
        CAD_TYPE_NONE,
        /// <summary>
        /// 
        /// </summary>
        CAD_TYPE_NUM
    }



}
