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

#region Imports

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

#endregion

namespace DroneController
{
    internal class DroneProxy
    {
        #region Private Static Fields

        private static DroneProxy instance = null;

        #endregion

        #region Private Properties

        private NavigationDataInfo NavigationDataInfo { get; set; }
        private NavigationDataInfo.NavDataDrone DroneData { get; set; }
        private NavigationDataInfo.NavVisionDetect VisionDetect { get; set; }

        #endregion

        #region internal Properties

        #region General

        internal Dictionary<String, String>   DroneConfiguration { get; set; }
        internal ConnectionStatus             ConnectionStatus { get; set; }
        internal DroneInputValueFlags         DroneInputValue { get; set; }
               
        internal byte[]                       NavigationDataBuffer
        {
            set { NavigationDataInfo.NavigationDataBuffer = value; }
        }
        internal UInt32                       NavigationDataHeader 
        {
            get { return NavigationDataInfo.Header; }
        }
        internal UInt32                       MessageIndex 
        {
            get { return NavigationDataInfo.Sequence; }
        }
        internal DroneStatusFlags DroneStatus 
        {
            get { return (DroneStatusFlags)NavigationDataInfo.DroneStatus; }
        }

        #region DroneData

        internal int BatteryLevel
        {
            get { return (int)DroneData.FlyingPercentage; }
        }
        internal int Height
        {
            get { return (int)DroneData.Altitude; }
        }
        internal int Theta
        {
            get { return (int)DroneData.Theta; }
        }

        internal int Phi
        {
            get { return (int)DroneData.Phi; }
        }

        internal int Psi
        {
            get { return (int)DroneData.Psi; }
        }

        #endregion

        #region VisionDetect

        internal int TagCount
        {
            get { return (int)VisionDetect.TagCount; }
        }

        internal int TagBoxHeight
        {
            get { return (int)VisionDetect.Tag1BoxHeight; }
        }

        internal int TagBoxWidth
        {
            get { return (int)VisionDetect.Tag1BoxWidth; }
        }

        internal int TagDistance
        {
            get { return (int)VisionDetect.Tag1Distance; }
        }

        internal int TagType
        {
            get { return (int)VisionDetect.Tag1Type; }
        }

        internal int TagBoxLeft
        {
            get { return (int)VisionDetect.Tag1X; }
        }

        internal int TagBoxTop
        {
            get { return (int)VisionDetect.Tag1Y; }
        }

        #endregion

        #endregion

        #region DroneStatus

        /// <summary>
        /// Refers to status mask AR_DRONE_FLY_MASK
        /// </summary>
        internal bool IsFlightEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_FLY_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_VIDEO_MASK
        /// </summary>
        internal bool IsVideoEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_VIDEO_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_VISION_MASK
        /// </summary>
        internal bool IsVisionEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_VISION_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_CONTROL_MASK
        /// </summary>
        internal bool IsAngulerSpeedControlEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_CONTROL_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_ALTITUDE_MASK
        /// </summary>
        internal bool IsAltitudeControlEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ALTITUDE_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_USER_FEEDBACK_START
        /// </summary>
        internal bool IsStartButtonEnabled
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_USER_FEEDBACK_START); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_COMMAND_MASK
        /// </summary>
        internal bool HasCommandReceived
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_COMMAND_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_TRIM_COMMAND_MASK
        /// </summary>
        internal bool HasFlatTrimCommandReceived
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_TRIM_COMMAND_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_TRIM_RUNNING_MASK
        /// </summary>
        internal bool IsFlatTrimRunning
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_TRIM_RUNNING_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_TRIM_RESULT_MASK
        /// </summary>
        internal bool HasFlatTrimSucceeded
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_TRIM_RESULT_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_NAVDATA_DEMO_MASK
        /// </summary>
        internal bool SendAllNavigationData
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_NAVDATA_DEMO_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_NAVDATA_BOOTSTRAP
        /// </summary>
        internal bool IsInitialized
        {
            get{return !GetDroneStatusMask(DroneStatusFlags.AR_DRONE_NAVDATA_BOOTSTRAP);}
        }

        /// <summary>
        /// Refers to status mask AR_DRONE_MOTORS_BRUSHED
        /// </summary>
        internal bool IsUsingBrushedMotors
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_MOTORS_BRUSHED); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_COM_LOST_MASK
        /// </summary>
        internal bool HasCommunicationLost
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_COM_LOST_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_GYROS_ZERO
        /// </summary>
        internal bool HasGyrosProblem
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_GYROS_ZERO); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_VBAT_LOW
        /// </summary>
        internal bool IsBatteryChargeTooLow
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_VBAT_LOW); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_VBAT_HIGH
        /// </summary>
        internal bool IsBatteryChargeTooHigh
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_VBAT_HIGH); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_TIMER_ELAPSED
        /// </summary>
        internal bool HasTimerElapsed
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_TIMER_ELAPSED); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_NOT_ENOUGH_POWER
        /// </summary>
        internal bool HasNotEnoughPower
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_NOT_ENOUGH_POWER); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_ANGLES_OUT_OF_RANGE
        /// </summary>
        internal bool AreAnglesOutOfRange
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ANGLES_OUT_OF_RANGE); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_WIND_MASK
        /// </summary>
        internal bool TooMuchWind
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_WIND_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_ULTRASOUND_MASK
        /// </summary>
        internal bool HasUltraSoundProblem
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ULTRASOUND_MASK); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_ATCODEC_THREAD_ON
        /// </summary>
        internal bool HasActiveATCommandThread
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ATCODEC_THREAD_ON); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_NAVDATA_THREAD_ON
        /// </summary>
        internal bool HasActiveNavigationDataThread
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_NAVDATA_THREAD_ON); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_VIDEO_THREAD_ON
        /// </summary>
        internal bool HasActiveVideoStreamThread
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_VIDEO_THREAD_ON); }
        }
        /// <summary>
        /// Refers to status mask AR_DRONE_COM_WATCHDOG_MASK
        /// </summary>
        internal bool HasCommunicationProblem
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_COM_WATCHDOG_MASK); }
        }

        /// <summary>
        /// Refers to status mask AR_DRONE_EMERGENCY_MASK
        /// </summary>
        internal bool HasEmergency
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_EMERGENCY_MASK); }
        }

        #region Attributes not yet understood

        internal bool AR_DRONE_CUTOUT_MASK
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_CUTOUT_MASK); }
        }
        internal bool AR_DRONE_PIC_VERSION_MASK
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_PIC_VERSION_MASK); }
        }
        internal bool AR_DRONE_ACQ_THREAD_ON
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ACQ_THREAD_ON); }
        }
        internal bool AR_DRONE_CTRL_WATCHDOG_MASK
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_CTRL_WATCHDOG_MASK); }
        }
        internal bool AR_DRONE_ADC_WATCHDOG_MASK
        {
            get{ return GetDroneStatusMask(DroneStatusFlags.AR_DRONE_ADC_WATCHDOG_MASK); }
        }

        #endregion

        #endregion

        #region InputValue

        /// <summary>
        /// Indicates whether the drone has its engines running.
        /// </summary>
        internal bool HasEnginesRunning
        {
            get{ return GetDroneInputValueFlagBit(DroneInputValueFlags.AR_DRONE_UI_BIT_START); }
        }

        #endregion

        #endregion

        #region Construction

        private DroneProxy()
        {
            NavigationDataInfo = new NavigationDataInfo();
            DroneConfiguration = new Dictionary<string, string>();
        }

        #endregion

        #region internal Methods

        internal static DroneProxy Create()
        {
            if (instance == null)
            {
                instance = new DroneProxy();
            }

            instance.NavigationDataInfo = new NavigationDataInfo();
            instance.DroneInputValue = (DroneInputValueFlags)((1 << (int)DroneInputValueFlags.AR_DRONE_UI_BIT_TRIM_THETA) |
                                                              (1 << (int)DroneInputValueFlags.AR_DRONE_UI_BIT_TRIM_PHI) |
                                                              (1 << (int)DroneInputValueFlags.AR_DRONE_UI_BIT_TRIM_YAW) |
                                                              (1 << (int)DroneInputValueFlags.AR_DRONE_UI_BIT_X) |
                                                              (1 << (int)DroneInputValueFlags.AR_DRONE_UI_BIT_Y));

            return instance;
        }

        internal void StartEngines()
        {
            SetDroneInputValueFlagBit(DroneInputValueFlags.AR_DRONE_UI_BIT_START);
        }
        internal void StopEngines()
        {
            ClearDroneInputValueFlagBit(DroneInputValueFlags.AR_DRONE_UI_BIT_START);
        }
        internal void StartResetDrone()
        {
            SetDroneInputValueFlagBit(DroneInputValueFlags.AR_DRONE_UI_BIT_SELECT);
        }
        internal void StopResetDrone()
        {
            ClearDroneInputValueFlagBit(DroneInputValueFlags.AR_DRONE_UI_BIT_SELECT);
        }     

        internal void UpdateDroneConfiguration(string configuredValues)
        {
            if (!String.IsNullOrEmpty(configuredValues))
            {
                DroneConfiguration.Clear();

                string[] configuredValuesPairs = configuredValues.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string configuredValuesPair in configuredValuesPairs)
                {
                    if (configuredValuesPair.Contains("="))
                    {
                        string[] configurationEntry = configuredValuesPair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                        DroneConfiguration.Add(configurationEntry[0].Trim(), configurationEntry[1].Trim());
                    }
                }
            }
        }

        #endregion

        #region Internal Methods

        internal void LoadNavigationDataStructures()
        {
            NavigationDataInfo.LoadNavigationDataStructures();
            DroneData = NavigationDataInfo.droneData;
            VisionDetect = NavigationDataInfo.VisionDetect;
        }

        internal bool ValidateNavigationData()
        {
            return NavigationDataInfo.IsCheckSumValid();
        }

        #endregion

        #region Private Methods

        private void SetDroneInputValueFlagBit(DroneInputValueFlags droneInputValueFlags)
        {
            DroneInputValue = (DroneInputValueFlags)CommonHelper.SetBitValue((int)DroneInputValue, (int)droneInputValueFlags);
        }

        private void ClearDroneInputValueFlagBit(DroneInputValueFlags droneInputValueFlags)
        {
            DroneInputValue = (DroneInputValueFlags)CommonHelper.ClearBitValue((int)DroneInputValue, (int)droneInputValueFlags);
        }

        private void FlipDroneInputValueFlagBit(DroneInputValueFlags droneInputValueFlags)
        {
            DroneInputValue = (DroneInputValueFlags)CommonHelper.FlipBitValue((int)DroneInputValue, (int)droneInputValueFlags);
        }

        private bool GetDroneStatusMask(DroneStatusFlags droneStatusFlags)
        {
            return (((DroneStatusFlags)DroneStatus & droneStatusFlags) == droneStatusFlags);
        }

        private bool GetDroneInputValueFlagBit(DroneInputValueFlags droneInputValueFlags)
        {
            return ((int)DroneInputValue & (1 << (int)droneInputValueFlags)) != 0;
        }

        #endregion
    }
}
