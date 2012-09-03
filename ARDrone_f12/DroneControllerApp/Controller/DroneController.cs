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

#define Video

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
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;



namespace DroneController
{
    /// <summary>
    /// This class provides the main interface to control the AR.Drone
    /// </summary>
    public class DroneController
    {
        /// <summary>
        /// Occurs when the contoller sends trace messages.
        /// </summary>
        public event EventHandler<TraceNotificationEventArgs> OnNotifyTraceMessage;
        /// <summary>
        /// Occurs when a new Video frame has been captured.
        /// </summary>
        public event EventHandler<VideoNotificationEventArgs> OnNotifyVideoMessage;
        /// <summary>
        /// Occurs when the ARDrone has received input that alters it's position. This can be used to synchronize sreen output with the Drone's current position.
        /// </summary>
        public event EventHandler<InputNotificationEventArgs> OnNotifyInputMessage;
        /// <summary>
        /// Occurs when the status of the ARDrone has changed.
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        /// <summary>
        /// Occurs when new information on the ARDrone is available. This is handled by a timer with elapses at a configurable interval.
        /// </summary>
        /// <see cref="ControllerConfig.DroneInfoTimerInterval"/>
        public event EventHandler<DroneInfoNotificationEventArgs> OnNotifyDroneInfoMessage;



        /// <summary>
        /// Gets or sets the trace notification level. Possible values are verbose, information, warning, error and none.
        /// </summary>
        /// <value>The trace notification level.</value>
        public TraceNotificationLevel TraceNotificationLevel { get; set; }
        /// <summary>
        /// Gets the connection status.
        /// </summary>
        /// <value>The connection status.</value>
        public ConnectionStatus ConnectionStatus { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ARDrone is flying.
        /// </summary>
        /// <value><c>true</c> if tte ARdrone is flying; otherwise, <c>false</c>.</value>
        public bool DroneIsFlying { get; set; }


        public bool IsMoving { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ARDrone needs to save a still image.
        /// </summary>
        /// <value><c>true</c> if image is requested; otherwise, <c>false</c>.</value>
        public bool PictureRequested { get; set; }


        private DroneControllerConfiguration ControllerConfig { get; set; }
        private DroneProxy DroneProxy { get; set; }

#if Video
        private VideoImage VideoImage { get; set; }
//        private VideoFileWriter VideoFileWriter { get; set; }
#endif


        private DispatcherTimer DroneInfoTimer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DroneController"/> class.
        /// </summary>
        public DroneController() : this(new DroneControllerConfiguration()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DroneController"/> class.
        /// </summary>
        /// <param name="droneControllerConfiguration">The drone controller configuration.</param>
        public DroneController(DroneControllerConfiguration droneControllerConfiguration)
        {

            ControllerConfig = droneControllerConfiguration;
            DroneProxy = DroneProxy.Create();

#if Video
            VideoImage = new VideoImage();
            VideoImage.ImageComplete += new EventHandler<ImageCompleteEventArgs>(VideoImage_ImageComplete);
#endif
            DroneInfoNotificationEventArgs = new DroneInfoNotificationEventArgs();
            DroneInfoTimer = new DispatcherTimer();
            DroneInfoTimer.Interval = TimeSpan.FromTicks(Constants.DroneInfoTimerInterval);
            DroneInfoTimer.Tick += new EventHandler(DroneInfoTimer_Elapsed);


        }

        void StartTimer()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    DroneInfoTimer.Start();
                });
        }

        void StopTimer()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                DroneInfoTimer.Stop();
            });
        }



        /// <summary>
        /// Starts the engines of the ARDrone. The ARDrone will take of and start hovering at a predetermined height.
        /// </summary>
        public void StartEngines()
        {
            DroneProxy.StartEngines();
            DroneIsFlying = true;
        }
        /// <summary>
        /// Stops the ARDrone engines, the ARDrone will land smoothly.
        /// </summary>
        public void StopEngines()
        {
            DroneProxy.StopEngines();
            DroneIsFlying = false;
        }
        /// <summary>
        /// Starts issuing an emergency reset command to the ARDrone. The engines will stop immediately and ARDrone stops hovering. In most cases this will lead to a crash.
        /// </summary>
        public void StartReset()
        {
            DroneProxy.StartResetDrone();
        }
        /// <summary>
        /// Stops issuing an emergency reset command to the ARDrone.
        /// </summary>
        public void StopReset()
        {
            DroneProxy.StopResetDrone();
        }


        /// <summary>
        /// Makes the ARDrone animate its LED's.
        /// </summary>
        /// <param name="id">The id of the animation to play.</param>
        /// <param name="frequency">The blink frequency of the animation in times per second (Hz).</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        public void PlayLedAnimation(int id, int frequency, int duration)
        {
            CommandCenter.PlayLedAnimation((LedAnimation)id, frequency, duration);
        }
        /// <summary>
        /// Sets the flat trim. Has to be called before each new flight.
        /// </summary>
        public void SetFlatTrim()
        {
            CommandCenter.SetFlatTrim();
        }


        /// <summary>
        /// Displays the next video channel.
        /// </summary>
        public void DisplayNextVideoChannel()
        {
            CommandCenter.SwitchVideoChannel(VideoChannel.Next);
        }
        /// <summary>
        /// Switches to the selected video channel.
        /// </summary>
        public void SwitchVideoChannel(int videoChannel)
        {
            CommandCenter.SwitchVideoChannel((VideoChannel)videoChannel);
        }


        float roll = 0;

        public float Roll
        {
            get { return roll; }
            set
            {
                roll = value;
                SetFlightParameters(roll, pitch, height, yaw);
            }
        }
        float pitch = 0;

        public float Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                SetFlightParameters(roll, pitch, height, yaw);
            }
        }
        float height = 0;

        public float Gaz
        {
            get { return height; }
            set
            {
                height = value;
                SetFlightParameters(roll, pitch, height, yaw);
            }
        }
        float yaw = 0;

        public float Yaw
        {
            get { return yaw; }
            set
            {
                yaw = value;
                SetFlightParameters(roll, pitch, height, yaw);
            }
        }
        /// <summary>
        /// Sets the flight parameters. This allows to pilot the ARDrone.
        /// </summary>
        /// <param name="roll">The roll parameter (Tilt Left/Right - Phi angle).</param>
        /// <param name="pitch">The pitch parameter (Tilt Front/Back - Theta angle)</param>
        /// <param name="height">The height parameter. (Move Up/Down)</param>
        /// <param name="yaw">The yaw parameter. (Rotate Left/Right - Psi angle)</param>
        /// <remarks>All parameters have a value between -1 and 1.</remarks>
        public void SetFlightParameters(float roll, float pitch, float height, float yaw)
        {
            CommandCenter.SetProgressiveInputValues(roll, pitch, height, yaw);

            if (ControllerConfig.EnableInputFeedback)
            {
                OnFlightParametersChanged(roll, pitch, height, yaw);
            }
        }

        /// <summary>
        /// Makes the ARDrone animate its LED's.
        /// </summary>
        /// <param name="ledAnimation">The animation to play.</param>
        /// <param name="frequency">The blink frequency of the animation in times per second (Hz).</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        public void PlayLedAnimation(LedAnimation ledAnimation, int frequency, int duration)
        {
            CommandCenter.PlayLedAnimation(ledAnimation, frequency, duration);
        }

        /// <summary>
        /// Switches the video channel. The ARDrone has two cameras, a horizontal and a vertical one. 
        /// Calling this method with the appropriate parameter results in images from either one of the cameras
        /// or a composite image from both cameras at the same time.
        /// </summary>
        /// <param name="videoChannel">An enumerated value indicating what kind of image to produce..</param>
        public void SwitchVideoChannel(VideoChannel videoChannel)
        {
            CommandCenter.SwitchVideoChannel(videoChannel);
        }

        /// <summary>
        /// This method establishes a connection with the ARDrone.
        /// </summary>
        /// <remarks>
        /// In order to be able to provide real time feedback to the requester this call is asynchronous. 
        /// Feedback is provided via the 'OnNotifyTraceMessage' event.
        /// </remarks>
        public void Connect()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(ConnectAsync);
            backgroundWorker.RunWorkerAsync();
        }



        /// <summary>
        /// This method tears down the connection with the ARDrone.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                StopTimer();
                StopEngines();
                CommunicationCenter.Disconnect();
                ConnectionStatus = ConnectionStatus.Closed;
            }
            catch (DroneException droneException)
            {
                OnNotifyTraceMessage(this, new TraceNotificationEventArgs(droneException.Message, droneException.NotificationSource, TraceNotificationLevel.Error));
            }
            catch (Exception exception)
            {
                OnNotifyTraceMessage(this, new TraceNotificationEventArgs(exception.Message, NotificationSource.DroneController, TraceNotificationLevel.Error));
            }
            finally
            {
                if (ConnectionStatus != ConnectionStatus.Closed)
                {
                    ConnectionStatus = ConnectionStatus.Error;
                }
            }
        }

        /// <summary>
        /// In order to receive the drone onboard configuration this method needs to be called. The response will be sent via a TCP channel (e.g. port 5555).
        /// The Communication center contains the logic to receive TCP frames on this control channel. The response will be passed via the 'OnNotifyDroneInfoMessage'
        /// evcent.
        /// </summary>
        public void RequestConfiguredValues()
        {
            CommandCenter.RequestConfiguredValues();
        }

        /// <summary>
        /// Sets the ARDrone configuration for outdoor flights. These parameters can be adapted according to personal requirements.
        /// </summary>
        /// <remarks>
        /// More information can be found in developers guide, chapter 'Drone Configuration'.
        /// </remarks>
        public void SetOutdoorConfiguration()
        {
            SetConfiguration("control:outdoor", "TRUE");
            SetConfiguration("control:flight_without_shell", "FALSE");

            SetConfiguration("control:altitude_max", "2000");
            SetConfiguration("control:euler_angle_max", ".25");
            SetConfiguration("detect:enemy_colors", "1");
        }

        /// <summary>
        /// Sets the ARDrone configuration for indoor flights. These parameters can be adapted according to personal requirements.
        /// </summary>
        /// <remarks>
        /// More information can be found in developers guide, chapter 'Drone Configuration'.
        /// </remarks>
        public void SetIndoorConfiguration()
        {
            SetConfiguration("control:outdoor", "FALSE");
            SetConfiguration("control:flight_without_shell", "FALSE");
            SetConfiguration("control:altitude_max", "2000");
            SetConfiguration("control:euler_angle_max", ".25");
            SetConfiguration("detect:enemy_colors", "1");
        }

        /// <summary>
        /// Sets ARDrone configuration values.
        /// </summary>
        /// <remarks>
        /// More information can be found in developers guide, chapter 'Drone Configuration'.
        /// </remarks>
        public void SetConfiguration(string parameterName, string parameterValue)
        {
            CommandCenter.SetConfiguration(parameterName, parameterValue);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            StopTimer();
        }

        private int traceThrottler = 0;

        private void OnNotifiedTraceMessage(TraceNotificationEventArgs e)
        {
            if (OnNotifyTraceMessage != null && ConnectionStatus != ConnectionStatus.Closed)
            {
                if (e.NotificationLevel >= TraceNotificationLevel)
                {
                    //Verbose notification will be Throttled in order
                    //to avoid message flooding in the client
                    if (e.NotificationLevel == TraceNotificationLevel.Verbose)
                    {
                        traceThrottler++;

                        if (traceThrottler > 10)
                        {
                            OnNotifyTraceMessage(this, e);
                            traceThrottler = 0;
                        }
                    }
                    else
                    {
                        OnNotifyTraceMessage(this, e);
                    }
                }
            }
        }

        private void OnNotifiedTraceMessage(string notificationMessage, NotificationSource notificationSource, TraceNotificationLevel notificationLevel)
        {
            OnNotifiedTraceMessage(new TraceNotificationEventArgs(notificationMessage, notificationSource, notificationLevel));
        }

        private void OnNotifiedTraceMessage(string notificationMessage, Exception exception, NotificationSource notificationSource, TraceNotificationLevel notificationLevel)
        {
            string notificationMessageFinal = String.Format("{0} Error message is: '{1}'.", notificationMessage, exception.Message);

            OnNotifiedTraceMessage(new TraceNotificationEventArgs(notificationMessageFinal, notificationSource, notificationLevel));
        }

        private void OnFlightParametersChanged(Single roll, Single pitch, Single height, Single yaw)
        {
            if (OnNotifyInputMessage != null)
            {
                    OnNotifyInputMessage(this, new InputNotificationEventArgs(roll, pitch, height, yaw));
            }
        }

        private void OnNotifiedDroneInfo(DroneInfoNotificationEventArgs e)
        {
            if (OnNotifyTraceMessage != null && ConnectionStatus != ConnectionStatus.Closed)
            {
                OnNotifyDroneInfoMessage(this, e);
            }
        }

        private void OnGPSMessage(string message)
        {
            if (OnNotifyGPSMessage != null)
            {
                OnNotifyGPSMessage(this, new GPSNotificationEventArgs(message));
            }
        }

        private DroneInfoNotificationEventArgs DroneInfoNotificationEventArgs { get; set; }
        private int inputNotificationThrottler = 0;

        void DroneInfoTimer_Elapsed(object sender, EventArgs e)
        {
            #region Collect DroneInfo

            DroneInfoNotificationEventArgs.BatteryLevel = DroneProxy.BatteryLevel;
            DroneInfoNotificationEventArgs.DroneStatus = DroneProxy.DroneStatus;
            DroneInfoNotificationEventArgs.Height = DroneProxy.Height;
            DroneInfoNotificationEventArgs.Theta = DroneProxy.Theta;
            DroneInfoNotificationEventArgs.Phi = DroneProxy.Phi;
            DroneInfoNotificationEventArgs.Psi = DroneProxy.Psi;

            DroneInfoNotificationEventArgs.WorkerThreadATCommandsActive = CommunicationCenter.WorkerThreadATCommandsActive;
            DroneInfoNotificationEventArgs.WorkerThreadNavigationDataActive = CommunicationCenter.WorkerThreadNavigationDataActive;
            DroneInfoNotificationEventArgs.WorkerThreadVideoStreamActive = CommunicationCenter.WorkerThreadVideoStreamActive;
            DroneInfoNotificationEventArgs.WorkerThreadControlInfoActive = CommunicationCenter.WorkerThreadControlInfoActive;

            DroneInfoNotificationEventArgs.DroneConfiguration = DroneProxy.DroneConfiguration;

            #endregion

            OnNotifiedDroneInfo(DroneInfoNotificationEventArgs);
        }

#if Video
        private void VideoImage_ImageComplete(object sender, ImageCompleteEventArgs e)
        {
            if (OnNotifyVideoMessage != null && ConnectionStatus != ConnectionStatus.Closed)
            {
                OnNotifyVideoMessage(this, new VideoNotificationEventArgs(VideoImage.ImageSource));

                if (PictureRequested)
                {
                    SavePicture();
                    PictureRequested = false;
                }
            }
        }

        private void SavePicture()
        {
            String tempJPEG = "TempJPEG";

            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(tempJPEG))
            {
                myStore.DeleteFile(tempJPEG);
            }

            IsolatedStorageFileStream myFileStream = myStore.CreateFile(tempJPEG);
            VideoImage.ImageSource.SaveJpeg(myFileStream, VideoImage.ImageSource.PixelWidth, VideoImage.ImageSource.PixelHeight, 0, 85);
            myFileStream.Close();


            // Create a new stream from isolated storage, and save the JPEG file to the media library on Windows Phone.
            myFileStream = myStore.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);

            // Save the image to the camera roll or saved pictures album.
            MediaLibrary library = new MediaLibrary();
            // Save the image to the camera roll album.
            Picture pic = library.SavePictureToCameraRoll("SavedPicture.jpg", myFileStream);
            MessageBox.Show("Image saved to camera roll album");
            myFileStream.Close();

        }

#endif

        private void ConnectAsync(object sender, DoWorkEventArgs e)
        {
            try
            {
                ConnectionStatus = ConnectionStatus.Initializing;

                OnNotifiedTraceMessage("Initializing DroneController.", NotificationSource.DroneController, TraceNotificationLevel.Information);

                #region Register DroneController

                CommunicationCenter.RegisterController(this);
                CommandCenter.RegisterController(this);

                #endregion

                bool success = CommunicationCenter.Connect();

                if (success)
                {

                    StartTimer();
                    ConnectionStatus = ConnectionStatus.Open;
                    OnNotifiedTraceMessage("DroneController initialized.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                }
            }
            catch (DroneException droneException)
            {
                OnNotifiedTraceMessage(droneException.Message, droneException.NotificationSource, TraceNotificationLevel.Error);
            }
            catch (Exception exception)
            {
                OnNotifiedTraceMessage(exception.Message, NotificationSource.DroneController, TraceNotificationLevel.Error);
            }
            finally
            {
                switch (ConnectionStatus)
                {
                    case ConnectionStatus.Open:
                        break;
                    case ConnectionStatus.Initializing:
                    case ConnectionStatus.Closed:
                    case ConnectionStatus.CloseRequested:
                        ConnectionStatus = ConnectionStatus.Error;
                        OnNotifiedTraceMessage("DroneController not initialized.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        break;
                }

                if (OnConnectionStatusChanged != null)
                {
                    OnConnectionStatusChanged(this, new ConnectionStatusChangedEventArgs(ConnectionStatus));
                }
            }
        }

        private void UpdateDroneConfigurationValues(string configuredValues)
        {
            DroneProxy.UpdateDroneConfiguration(configuredValues);
        }





        private static class CommunicationCenter
        {
            private static byte[] _receiveBuffer;
            private static UdpAnySourceMulticastClient _client;
            private static bool _joined;
            private static Thread WorkerThreadReceiveGPSData;
            #region Private Properties

            private static DroneController Controller { get; set; }
            private static DroneProxy DroneProxy { get; set; }

            private static Thread WorkerThreadReceiveNavigationData { get; set; }
            private static Thread WorkerThreadReceiveControlInfo { get; set; }
            private static Thread WorkerThreadReceiveVideoStream { get; set; }
            private static Thread WorkerThreadSendATCommands { get; set; }

            private static UInt32 LocalMessageIndex { get; set; }

            private static ManualResetEvent WaitHandle { get; set; }
            private static string LocalIPAddress { get; set; }

            private static Dictionary<CommunicationChannel, Socket> UdpSockets { get; set; }
            private static Dictionary<CommunicationChannel, Socket> TcpSockets { get; set; }
            private static Dictionary<CommunicationChannel, IPEndPoint> EndPoints { get; set; }

            #endregion

            #region Public Properties

            public static bool WorkerThreadVideoStreamActive { get; private set; }
            public static bool WorkerThreadATCommandsActive { get; private set; }
            public static bool WorkerThreadNavigationDataActive { get; private set; }
            public static bool WorkerThreadControlInfoActive { get; private set; }

            #endregion

            #region Construction

            static CommunicationCenter()
            {
                UdpSockets = new Dictionary<CommunicationChannel, Socket>();
                TcpSockets = new Dictionary<CommunicationChannel, Socket>();
                EndPoints = new Dictionary<CommunicationChannel, IPEndPoint>();
            }

            #endregion

            #region Internal Methods

            internal static void RegisterController(DroneController droneController)
            {
                Controller = droneController;
                DroneProxy = droneController.DroneProxy;
            }

            internal static bool Connect()
            {
                bool continueProcess = false;

                try
                {
                    if (Controller.ControllerConfig.EnableATCommandSimulation)
                    {
                        LocalIPAddress = "127.0.0.1";
                        continueProcess = true;
                    }
                    else
                    {

                        LocalIPAddress = Controller.ControllerConfig.GuestIpAddress;

                    }

                    continueProcess = true;

                    if (continueProcess)
                    {
                        #region Create Communication Channels and Worker Threads

                        #region AT commands

                        if (Controller.ControllerConfig.EnableATCommandThread)
                        {
                            Controller.OnNotifiedTraceMessage("Setting up AT command communication channel.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                            UdpSockets[CommunicationChannel.Command] = NetworkHelper.CreateUdpSocket(LocalIPAddress, Controller.ControllerConfig.CommandPort, Constants.SocketTimeoutATCommand);
                            EndPoints[CommunicationChannel.Command] = NetworkHelper.CreateRemoteEndPoint(Controller.ControllerConfig.DroneIpAddress, Controller.ControllerConfig.CommandPort);

                            WorkerThreadSendATCommands = new Thread(new ThreadStart(ThreadMethodSendATCommands));
                            WorkerThreadSendATCommands.Name = "ATCommandThread";
                            WorkerThreadSendATCommands.Start();

                            Controller.OnNotifiedTraceMessage("AT command worker thread started.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("ATCommand communication not enabled.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        }

                        #endregion

                        #region Navigation Data

                        if (Controller.ControllerConfig.EnableNavigationDataThread)
                        {
                            Controller.OnNotifiedTraceMessage("Setting up Navigation Data communication channel.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                            UdpSockets[CommunicationChannel.NavigationData] = NetworkHelper.CreateUdpSocket(LocalIPAddress, Controller.ControllerConfig.NavigationDataPort, Constants.SocketTimeoutNavigationData);
                            EndPoints[CommunicationChannel.NavigationData] = NetworkHelper.CreateRemoteEndPoint(Controller.ControllerConfig.DroneIpAddress, Controller.ControllerConfig.NavigationDataPort);

                            WorkerThreadReceiveNavigationData = new Thread(new ThreadStart(ThreadMethodReceiveNavigationData));
                            WorkerThreadReceiveNavigationData.Name = "NavigationDataThread";
                            WorkerThreadReceiveNavigationData.Start();

                            Controller.OnNotifiedTraceMessage("Navigation Data worker thread started.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("Navigation Data communication not enabled.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        }

                        #endregion

                        #region Video Stream

                        if (Controller.ControllerConfig.EnableVideoStreamThread)
                        {
                            Controller.OnNotifiedTraceMessage("Setting up Video Stream communication channel.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                            UdpSockets[CommunicationChannel.VideoStream] = NetworkHelper.CreateUdpSocket(LocalIPAddress, Controller.ControllerConfig.VideoStreamPort, Constants.SocketTimeoutVideoStream);
                            EndPoints[CommunicationChannel.VideoStream] = NetworkHelper.CreateRemoteEndPoint(Controller.ControllerConfig.DroneIpAddress, Controller.ControllerConfig.VideoStreamPort);

                            WorkerThreadReceiveVideoStream = new Thread(new ThreadStart(ThreadMethodReceiveVideoStream));
                            WorkerThreadReceiveVideoStream.Name = "VideoStreamThread";
                            WorkerThreadReceiveVideoStream.Start();

                            Controller.OnNotifiedTraceMessage("Video Stream worker thread started.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("Video Stream communication not enabled.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        }

                        #endregion

                        #region GPS Data

                        if (Controller.ControllerConfig.EnableGPSStreamThread)
                        {
                            Controller.OnNotifiedTraceMessage("Setting up GPS data communication channel.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                            UdpSockets[CommunicationChannel.GPSData] = NetworkHelper.CreateUdpSocket(LocalIPAddress, Controller.ControllerConfig.GPSDataPort, Constants.SocketTimeoutVideoStream);
                            EndPoints[CommunicationChannel.GPSData] = NetworkHelper.CreateRemoteEndPoint(Controller.ControllerConfig.DroneIpAddress, Controller.ControllerConfig.GPSDataPort);

                            WorkerThreadReceiveGPSData = new Thread(new ThreadStart(ThreadMethodReceiveGPSData));
                            WorkerThreadReceiveGPSData.Name = "GPS Data Thread";
                            WorkerThreadReceiveGPSData.Start();

                            Controller.OnNotifiedTraceMessage("GPS Data worker thread started.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("GPS Data communication not enabled.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        }

                        #endregion


                        #region Control Info

                        if (Controller.ControllerConfig.EnableControlInfoThread)
                        {
                            Controller.OnNotifiedTraceMessage("Setting up Control Info communication channel.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                            TcpSockets[CommunicationChannel.ControlInfo] = NetworkHelper.CreateTcpSocket(LocalIPAddress, Controller.ControllerConfig.ControlInfoPort, Constants.SocketTimeoutControlInfo);
                            EndPoints[CommunicationChannel.ControlInfo] = NetworkHelper.CreateRemoteEndPoint(Controller.ControllerConfig.DroneIpAddress, Controller.ControllerConfig.ControlInfoPort);

                            WorkerThreadReceiveControlInfo = new Thread(new ThreadStart(ThreadMethodReceiveControlInfo));
                            WorkerThreadReceiveControlInfo.Name = "ControlInfoThread";
                            WorkerThreadReceiveControlInfo.Start();

                            Controller.OnNotifiedTraceMessage("Control Info worker thread started.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("Control Info communication not enabled.", NotificationSource.DroneController, TraceNotificationLevel.Warning);
                        }

                        #endregion

                        #endregion
                    }

                    return continueProcess;
                }
                catch (Exception exception)
                {
                    throw new DroneException(NotificationSource.CommunicationCenter, exception.Message);
                }
            }

            internal static void Disconnect()
            {
                try
                {
                    if (Controller.ConnectionStatus == ConnectionStatus.Open)
                    {
                        WorkerThreadSendATCommands = null;
                        WorkerThreadReceiveNavigationData = null;
                        WorkerThreadReceiveVideoStream = null;

                        Controller.ConnectionStatus = ConnectionStatus.Closed;

                        foreach (KeyValuePair<CommunicationChannel, Socket> entry in UdpSockets)
                        {
                            NetworkHelper.CloseUdpConnection(entry.Value);
                        }

                        foreach (KeyValuePair<CommunicationChannel, Socket> entry in TcpSockets)
                        {
                            NetworkHelper.CloseTcpConnection(entry.Value);
                        }

                        CommandCenter.ResetCommandBatchSequence();

                        WorkerThreadSendATCommands = null;
                        WorkerThreadReceiveNavigationData = null;
                        WorkerThreadReceiveVideoStream = null;
                        WorkerThreadReceiveControlInfo = null;
                    }
                }
                catch (Exception exception)
                {
                    throw new DroneException(NotificationSource.CommunicationCenter, exception.Message);
                }
            }

            #endregion


            #region Private Methods

            private static void SendMessage(CommunicationChannel communicationChannel, int message)
            {
                try
                {
                    SendMessage(communicationChannel, BitConverter.GetBytes(message));
                }
                catch (Exception e)
                {
                }
            }

            private static void SendMessage(CommunicationChannel communicationChannel, string message)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    SendMessage(communicationChannel, buffer);
                }
                catch (Exception e)
                { 
                }
            }

            private static void SendMessage(CommunicationChannel communicationChannel, byte[] message)
            {
                try
                {
                    int bytesSent = NetworkHelper.SendUdp(UdpSockets[communicationChannel],message, message.Length, EndPoints[communicationChannel]);
                }
                catch (Exception e)
                { 
                }
            }

            private static void InitializeDrone()
            {
                if (!DroneProxy.IsInitialized)
                {
                    Controller.OnNotifiedTraceMessage("Initializing Drone.", NotificationSource.CommunicationCenter, TraceNotificationLevel.Information);

                    SendMessage(CommunicationChannel.Command, CommandCenter.ComposeCommandString(ATCommands.SetConfiguration, "general:navdata_demo", "TRUE"));

                    int retry = 20;
                    bool bcontinue = true;
                    int next = 0;

                    while (bcontinue && retry > 0)
                    {
                        if (next == 0)
                        {
                            if (DroneProxy.HasCommandReceived)
                            {
                                Controller.OnNotifiedTraceMessage("Processing 'Initialize Drone Command'.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                                next++;
                            }
                        }
                        else
                        {
                            SendMessage(CommunicationChannel.Command, CommandCenter.ComposeCommandString(ATCommands.SetControlMode, (int)ControlMode.ACK_CONTROL_MODE, 0));

                            if (!DroneProxy.HasCommandReceived)
                            {
                                Controller.OnNotifiedTraceMessage("'Initialize Drone Command' processed successfully.", NotificationSource.DroneController, TraceNotificationLevel.Information);
                                bcontinue = false;
                            }
                        }
                        Thread.Sleep(1000);
                        retry--;
                    }
                }
            }

            #region Thread Methods

            private static void ThreadMethodSendATCommands()
            {
                string commandBatch = null;
                Socket socket = UdpSockets[CommunicationChannel.Command];
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                StopWatch stopWatch = new StopWatch();

                do
                {
                    try
                    {
                        stopWatch.Restart();

                        if (!DroneProxy.IsInitialized)
                        {
                            InitializeDrone();
                        }

                        commandBatch = CommandCenter.GetNextCommandBatch();

                        //Simple way to be able to only view the commands that would be sent and not
                        //actually send them. Maybe to be finetuned at a later stage for some selctivity. 
                        if (!Controller.ControllerConfig.EnableATCommandSimulation)
                        {
                            SendMessage(CommunicationChannel.Command, commandBatch);
                        }

                        Controller.OnNotifiedTraceMessage(commandBatch.Remove(commandBatch.Length -1), NotificationSource.CommandCenter, TraceNotificationLevel.Information);

                        stopWatch.Stop();

                        if (stopWatch.ElapsedMilliseconds < Constants.NavigationDataRefreshInterval)
                        {
                            Thread.Sleep((int)(Constants.NavigationDataRefreshInterval - stopWatch.ElapsedMilliseconds));
                        }

                        if (!WorkerThreadATCommandsActive)
                        {
                            WorkerThreadATCommandsActive = true;
                        }
                    }
                    catch (SocketException socketException)
                    {
                        switch (socketException.ErrorCode)
                        {
                            case 10060: //Socket timeout
                                Controller.OnNotifiedTraceMessage(String.Format("AT :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                WorkerThreadATCommandsActive = false;
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Controller.OnNotifiedTraceMessage("AT :" + exception.Message, NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                    }

                } while (Controller.ConnectionStatus != ConnectionStatus.Closed);
            }


            private static void ThreadMethodReceiveGPSData()
            {
                Socket socket = UdpSockets[CommunicationChannel.GPSData];
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                SendMessage(CommunicationChannel.GPSData, 1);


                do
                {
                    try
                    {
                        byte[] receiveBuffer = NetworkHelper.UdpRecieve(socket,ref endPoint);
                        if (receiveBuffer == null)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        else if (receiveBuffer.Length == 0)
                        {
                            SendMessage(CommunicationChannel.GPSData, 1);
                        }
                        else
                        {
                            Controller.OnGPSMessage(Encoding.UTF8.GetString(receiveBuffer, 0, receiveBuffer.Length));
                        }
                    }
                    catch (SocketException socketException)
                    {
                        switch (socketException.ErrorCode)
                        {
                            case 10060: //Socket timeout
                                Controller.OnNotifiedTraceMessage(String.Format("Nav :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                WorkerThreadNavigationDataActive = false;
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Controller.OnNotifiedTraceMessage("GPS " + exception.ToString(), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                    }
                } while (Controller.ConnectionStatus != ConnectionStatus.Closed);
            } 

            private static void ThreadMethodReceiveNavigationData()
            {
                Socket socket = UdpSockets[CommunicationChannel.NavigationData];
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                #region Activate NavigationData Stream

                SendMessage(CommunicationChannel.NavigationData, 1);

                #endregion

                do
                {
                    try
                    {
                        byte[] receiveBuffer = NetworkHelper.UdpRecieve(socket,ref endPoint);
                        if (receiveBuffer == null)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        if (receiveBuffer.Length == 0)
                        {
                            #region Activate NavigationData Stream

                            SendMessage(CommunicationChannel.NavigationData, 1);

                            #endregion

                            LocalMessageIndex = Constants.DefaultNavigationDataSequence;
                        }
                        else
                        {
                            DroneProxy.NavigationDataBuffer = receiveBuffer;

                            if (DroneProxy.NavigationDataHeader == Constants.NavigationDataHeader)
                            {
                                if (DroneProxy.HasCommunicationProblem)
                                {
                                    LocalMessageIndex = Constants.DefaultNavigationDataSequence;

                                    if (DroneProxy.IsInitialized)
                                    {
                                        SendMessage(CommunicationChannel.Command, CommandCenter.ComposeCommandString(ATCommands.ResetCommunicationHub));
                                        Controller.OnNotifiedTraceMessage("Sending reset watchdog", NotificationSource.CommunicationCenter, TraceNotificationLevel.Verbose);
                                    }
                                }

                                if (DroneProxy.MessageIndex > LocalMessageIndex)
                                {
                                    DroneProxy.LoadNavigationDataStructures();

                                    

                                    if (!DroneProxy.ValidateNavigationData())
                                    {
                                        Controller.OnNotifiedTraceMessage("Navigation data checksum failed. [Preliminary message, might need reviewing.]", NotificationSource.CommunicationCenter, TraceNotificationLevel.Warning);
                                    }
                                }
                                else
                                {
                                    Controller.OnNotifiedTraceMessage(String.Format("Sequence out of order. Local: {0} - Drone: {1}.[Preliminary message, might need reviewing.]", LocalMessageIndex, DroneProxy.MessageIndex), NotificationSource.CommunicationCenter, TraceNotificationLevel.Warning);
                                }

                                if (!WorkerThreadNavigationDataActive)
                                {
                                    WorkerThreadNavigationDataActive = true;
                                }
                            }

                            LocalMessageIndex = DroneProxy.MessageIndex;
                        }
                    }
                    catch (SocketException socketException)
                    {
                        switch (socketException.ErrorCode)
                        {
                            case 10060: //Socket timeout
                                Controller.OnNotifiedTraceMessage(String.Format("Nav :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                WorkerThreadNavigationDataActive = false;
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Controller.OnNotifiedTraceMessage("Nav " + exception.Message, NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                    }

                } while (Controller.ConnectionStatus != ConnectionStatus.Closed);
            }





            private static void ThreadMethodReceiveVideoStream()
            {
                Socket socket = UdpSockets[CommunicationChannel.VideoStream];
                IPEndPoint remoteEndPoint = EndPoints[CommunicationChannel.VideoStream];

                #region Activate Video Stream

                SendMessage(CommunicationChannel.VideoStream, 0);

  
                #endregion



/*
                MulticastHelper.Join();
                MulticastHelper.data_received += new EventHandler<ReceivedEventArgs>((sender, e) =>
                {
                    if (e.buf.Length > 0)
                    {
                        Controller.VideoImage.AddImageStream(e.buf);
                    }
                });

                */

                do
                {
                    try
                    {
                        byte[] receiveBuffer = NetworkHelper.UdpRecieve(socket, ref remoteEndPoint);
                        if (receiveBuffer == null)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        else if (receiveBuffer.Length == 0)
                        {
                            #region Activate NavigationData Stream

                            SendMessage(CommunicationChannel.NavigationData, 1);

                            #endregion
                        }
                        else
                        {
                            Controller.VideoImage.AddImageStream(receiveBuffer);

                            if (!WorkerThreadVideoStreamActive)
                            {
                                WorkerThreadVideoStreamActive = true;
                            }
                        }
                        
                    }
                    catch (SocketException socketException)
                    {
                        switch (socketException.ErrorCode)
                        {
                            case 10060: //Socket timeout
                                Controller.OnNotifiedTraceMessage(String.Format("Video :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                WorkerThreadNavigationDataActive = false;
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        Controller.OnNotifiedTraceMessage("Video " + exception.Message, NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                    }

                } while (Controller.ConnectionStatus != ConnectionStatus.Closed);


            }

            private static void ThreadMethodReceiveControlInfo()
            {
                Socket socket = null;
                //NetworkStream networkStream = null;
                StringBuilder stringBuffer = new StringBuilder();
                byte[] buffer = new byte[Constants.ControlInfoBufferSize];

                do
                {
                    try
                    {
                        socket = TcpSockets[CommunicationChannel.ControlInfo];
                        NetworkHelper.TcpConnect(socket,EndPoints[CommunicationChannel.ControlInfo]);
                        //networkStream = socket.GetStream();

                        int byteCount = NetworkHelper.TcpRecieve(socket,buffer, 0, Constants.ControlInfoBufferSize);

                        if (byteCount > 0)
                        {
                            do
                            {
                                byte[] message = new byte[byteCount];
                                Buffer.BlockCopy(buffer, 0, message, 0, byteCount);
                                stringBuffer.Append(Encoding.UTF8.GetString(message,0,message.Length));
                                byteCount = 0;
                                //if (networkStream.DataAvailable)
                                //{
                                byteCount = NetworkHelper.TcpRecieve(socket, buffer, 0, Constants.ControlInfoBufferSize);
                                //}
                            } while (byteCount > 0);

                            Controller.UpdateDroneConfigurationValues(stringBuffer.ToString());
                        }

                        if (!WorkerThreadControlInfoActive)
                        {
                            WorkerThreadControlInfoActive = true;
                        }
                    }
                    catch (SocketException socketException)
                    {
                        switch (socketException.ErrorCode)
                        {
                            case 10060: //Socket timeout
                                Controller.OnNotifiedTraceMessage(String.Format("Ctl :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                WorkerThreadControlInfoActive = false;
                                break;
                        }
                    }
                    catch (IOException ioException)
                    {
                        if (ioException.InnerException != null)
                        {
                            SocketException socketException = ioException.InnerException as SocketException;

                            if (socketException != null)
                            {
                                switch (socketException.ErrorCode)
                                {
                                    case 10060: //Socket timeout
                                        Controller.OnNotifiedTraceMessage(String.Format("Ctl :No activity on worker thread for {0} milliseconds.", Constants.SocketTimeoutNavigationData), NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                                        WorkerThreadControlInfoActive = false;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Controller.OnNotifiedTraceMessage("Ctl " + ioException.Message, NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                        }
                    }
                    catch (Exception exception)
                    {
                        Controller.OnNotifiedTraceMessage("Ctl " + exception.Message, NotificationSource.CommunicationCenter, TraceNotificationLevel.Error);
                    }
                    finally
                    {
                        stringBuffer.Length = 0;

                        /*if (networkStream != null)
                        {
                            networkStream.Close();
                        }*/

                        if (socket != null)
                        {
                            socket.Close();
                        }

                        if (Controller.ConnectionStatus != ConnectionStatus.Closed)
                        {
                            TcpSockets[CommunicationChannel.ControlInfo] = NetworkHelper.CreateTcpSocket(LocalIPAddress, Controller.ControllerConfig.ControlInfoPort, Constants.SocketTimeoutControlInfo);
                        }
                    }

                } while (Controller.ConnectionStatus != ConnectionStatus.Closed);
            }

            #endregion

            #endregion
        }

        private static class CommandCenter
        {
            #region Private Properties

            private static DroneController DroneController { get; set; }
            private static DroneProxy DroneProxy { get; set; }
            private static Queue<String> Commands { get; set; }
            private static StringBuilder CommandBatch { get; set; }
            private static UInt32 CommandBatchSequence { get; set; }

            #endregion

            #region Construction

            static CommandCenter()
            {
                CommandBatch = new StringBuilder();
                Commands = new Queue<String>();
            }

            #endregion

            #region Private Methods

            private static void EnqueueCommand(string command)
            {
                EnqueueCommand(command, null);
            }

            private static void EnqueueCommand(string command, params object[] parameters)
            {
                Commands.Enqueue(ComposeCommandString(command, parameters));
            }

            #endregion

            #region Internal Methods

            internal static void ResetCommandBatchSequence()
            {
                CommandBatchSequence = 0;
            }

            internal static void RegisterController(DroneController droneController)
            {
                DroneController = droneController;
                DroneProxy = droneController.DroneProxy;
            }

            internal static string ComposeCommandString(string command, params object[] parameters)
            {
                if (parameters == null)
                {
                    return String.Format(command, CommandBatchSequence++);
                }
                else
                {
                    List<object> list = new List<object>(parameters);
                    list.Insert(0, CommandBatchSequence++);
                    return String.Format(command, list.ToArray());
                }
            }

            /// <summary>
            /// This method dequeues all messages currently in the queue and concatenates them.
            /// The sequence is automatically added at the beginning of the string.
            /// </summary>
            /// <returns>A string containing a chain of AT commands.</returns>
            /// <remarks>
            /// I do not currently take into account the 1024 character limit of
            /// a concatenated command string. Testing will indicate whether it is 
            /// possible that the string becomes larger than the limit between 
            /// two sweeps of the command queue. If so I then take measures to cope.
            /// </remarks>
            internal static string GetNextCommandBatch()
            {
                CommandBatch.Length = 0;

                int commandCount = Commands.Count;

                #region Add CommandQueue Commands

                if (commandCount > 0)
                {
                    for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
                    {
                        CommandBatch.Append(Commands.Dequeue());
                    }
                }

                #endregion

                #region Add Sequence & InputValue

                //
                
                int moving = 0;
                if (DroneController.IsMoving)
                {
                    if (DroneController.Roll != 0)
                    {
                        moving = 1;
                    }
                    if (DroneController.Pitch != 0)
                    {
                        moving = 1;
                    }
                    if (DroneController.Yaw != 0)
                    {
                        moving = 1;
                    }
                    if (DroneController.Gaz != 0)
                    {
                        moving = 1;
                    }
                }

                if (moving == 1)
                {
                    CommandBatch.Append(ComposeCommandString(ATCommands.ResetCommunicationHub));
                    CommandBatch.Append(ComposeCommandString(ATCommands.SetProgressiveInputValues, 1, BitConverter.ToInt32(BitConverter.GetBytes(DroneController.Roll), 0), BitConverter.ToInt32(BitConverter.GetBytes(DroneController.Pitch), 0), BitConverter.ToInt32(BitConverter.GetBytes(DroneController.Gaz), 0), BitConverter.ToInt32(BitConverter.GetBytes(DroneController.Yaw), 0)));
                    if (Debugger.IsAttached)
                        Debug.WriteLine("Pitch: {0}, Roll: {1}, Yaw: {2}, Gaz: {3}, Moving: 1", DroneController.Pitch, DroneController.Roll, DroneController.Yaw, DroneController.Gaz);
                }
                else
                {
                    CommandBatch.Append(ComposeCommandString(ATCommands.SetInputValue, (uint)DroneProxy.DroneInputValue));
                    if (Debugger.IsAttached)
                        Debug.WriteLine("Pitch: {0}, Roll: {1}, Yaw: {2}, Gaz: {3}, Moving: 0", DroneController.Pitch, DroneController.Roll, DroneController.Yaw, DroneController.Gaz);

                }

                #endregion

                return CommandBatch.ToString();
            }

            internal static void SwitchVideoChannel(VideoChannel videoChannel)
            {
                EnqueueCommand(ATCommands.SwitchVideoChannel, (int)videoChannel);
            }

            internal static void SetProgressiveInputValues(float roll, float pitch, float height, float yaw)
            {
                int newPitch = 0;
                int newRoll = 0;
                int newGaz = 0;
                int newYaw = 0;

                roll = (Math.Abs(roll) > 1) ? 1 : roll;
                pitch = (Math.Abs(pitch) > 1) ? 1 : pitch;
                height = (Math.Abs(height) > 1) ? 1 : height;
                yaw = (Math.Abs(yaw) > 1) ? 1 : yaw;

                newPitch = BitConverter.ToInt32(BitConverter.GetBytes(pitch),0);
                newRoll = BitConverter.ToInt32(BitConverter.GetBytes(roll), 0);
                newGaz = BitConverter.ToInt32(BitConverter.GetBytes(height), 0);
                newYaw = BitConverter.ToInt32(BitConverter.GetBytes(yaw), 0);


                EnqueueCommand(ATCommands.SetProgressiveInputValues, 1, newRoll, newPitch, newGaz, newYaw);
            }

            internal static void SetFlatTrim()
            {
                EnqueueCommand(ATCommands.SetFlatTrim);
            }

            internal static void StartVisonDetect()
            {
                int newValue;
                newValue = BitConverter.ToInt32(BitConverter.GetBytes((float)0), 0);
                EnqueueCommand(ATCommands.SetTagDetection, (int)DetectionType.CAD_TYPE_VISION_DETECT, newValue);
            }

            internal static void StopVisonDetect()
            {
                int newValue;
                newValue = BitConverter.ToInt32(BitConverter.GetBytes((float)0), 0);
                EnqueueCommand(ATCommands.SetTagDetection, (int)DetectionType.CAD_TYPE_NONE, newValue);
            }

            internal static void SetConfiguration(string parameterName, string parameterValue)
            {
                EnqueueCommand(ATCommands.SetConfiguration, parameterName, parameterValue);
            }

            internal static void SetControlMode(ControlMode controlMode)
            {
                EnqueueCommand(ATCommands.SetControlMode, controlMode, 0);
            }

            internal static void SetControlMode(ControlMode controlMode, int length)
            {
                EnqueueCommand(ATCommands.SetControlMode, controlMode, length);
            }

            internal static void ResetCommunicationHub()
            {
                EnqueueCommand(ATCommands.ResetCommunicationHub);
            }

            internal static void PlayLedAnimation(LedAnimation ledAnimation, int frequency, int duration)
            {
                int newFrequency;
                newFrequency = BitConverter.ToInt32(BitConverter.GetBytes((float)frequency), 0);
                EnqueueCommand(ATCommands.PlayLedAnimation, (int)ledAnimation, newFrequency, duration);
            }

            internal static void RequestConfiguredValues()
            {
                EnqueueCommand(ATCommands.SetControlMode, ControlMode.CFG_GET_CONTROL_MODE, 0);
            }

            #endregion
        }

        public event EventHandler<GPSNotificationEventArgs> OnNotifyGPSMessage;
    }
}
