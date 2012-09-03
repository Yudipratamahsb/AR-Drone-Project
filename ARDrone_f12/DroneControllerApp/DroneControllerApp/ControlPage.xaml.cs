using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DroneController;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace DroneControllerApp
{
    public partial class ControlPage : PhoneApplicationPage
    {
        internal DroneController.DroneController droneController;

        internal ObservableCollection<string> Traces;

        DispatcherTimer time;
        internal float speedf = 0.2f;
        internal float verticalSpeed;
        internal float yawSpeed;
        internal bool moving;
        Motion motion;

        // Constructor
        public ControlPage()
        {
            InitializeComponent();
            Traces = new ObservableCollection<string>();
            //TraceBox.DataContext = Traces;
            time = new DispatcherTimer();
            motion = new Motion();

            motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged);

            try
            {
                motion.Start();
            }
            catch (InvalidOperationException)
            {
#if UNITTEST
#else
                MessageBox.Show("Unable to start the motion API");
#endif
            }
        }

        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {
            if (moving)
            {
                if (droneController != null)
                {
                    var PhonePitch = MathHelper.ToDegrees(e.SensorReading.Attitude.Pitch);
                    var PhoneRoll = MathHelper.ToDegrees(e.SensorReading.Attitude.Roll);
                    Debug.WriteLine("Phone Pitch: {0}, Roll: {1}", PhonePitch, PhoneRoll);
                    PhonePitch = PhonePitch > 0 ? PhonePitch - (App.Current as App).Config.PhoneTiltThreshold : PhonePitch + (App.Current as App).Config.PhoneTiltThreshold;
                    PhoneRoll = PhoneRoll > 0 ? PhoneRoll - (App.Current as App).Config.PhoneTiltThreshold : PhoneRoll +(App.Current as App).Config.PhoneTiltThreshold;
                    var divider =  (App.Current as App).Config.WP7TiltMax - (App.Current as App).Config.PhoneTiltThreshold;
                    Debug.WriteLine("New Phone Pitch: {0}, Roll: {1}/nDivider: {2}", PhonePitch, PhoneRoll, divider);
                    droneController.Roll = MathHelper.Clamp(PhonePitch / divider, -1.0f, 1.0f);
                    droneController.Pitch = -MathHelper.Clamp(PhoneRoll / divider, -1.0f, 1.0f);
                }
            }
        }


        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (!(e.Orientation == PageOrientation.LandscapeRight))
            {
                base.OnOrientationChanged(e);
            }
        }

        internal void connectToDrone()
        {
            DroneControllerConfiguration droneControllerConfiguration = new DroneControllerConfiguration();

            droneControllerConfiguration.EnableNavigationDataThread = false;
            droneControllerConfiguration.EnableVideoStreamThread = true;
            droneControllerConfiguration.EnableGPSStreamThread = false;
            droneControllerConfiguration.EnableATCommandThread = true;
            droneControllerConfiguration.EnableControlInfoThread = false;
            droneControllerConfiguration.EnableATCommandSimulation = false;
            droneControllerConfiguration.DroneIpAddress = IPBox.Text;

            droneController = null;  // ensure null for creation (removed set = null in disconect)
            droneController = new DroneController.DroneController(droneControllerConfiguration);

            droneController.IsMoving = true;

            droneController.TraceNotificationLevel = TraceNotificationLevel.Verbose;
            droneController.OnNotifyTraceMessage += new EventHandler<TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage);
            droneController.OnNotifyVideoMessage += new EventHandler<VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage);
            droneController.OnNotifyGPSMessage += new EventHandler<GPSNotificationEventArgs>(droneController_OnNotifyGPSMessage);
            droneController.OnNotifyDroneInfoMessage += new EventHandler<DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage);
            droneController.OnConnectionStatusChanged += new EventHandler<ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged);
            droneController.Connect();

            while (droneController.ConnectionStatus != ConnectionStatus.Open) Thread.Sleep(100);

            droneController.SetConfiguration("control:euler_angle_max", MathHelper.ToRadians((App.Current as App).Config.EulerAngleMax).ToString());
            droneController.SetConfiguration("control:control_iphone_tilt", "1");
            droneController.SetConfiguration("control:altitude_max", (App.Current as App).Config.MaxAltitude.ToString());
            droneController.SetConfiguration("control:control_vz_max", (App.Current as App).Config.VerticalSpeed.ToString());
            droneController.SetConfiguration("control:control_yaw", MathHelper.ToRadians(200).ToString());
            verticalSpeed = 1.0f;
            yawSpeed = MathHelper.ToRadians((App.Current as App).Config.YawSpeed) / MathHelper.ToRadians(200);
        }

        void droneController_OnNotifyGPSMessage(object sender, GPSNotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        void droneController_OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged), sender, e);
            }
            else
            {
                //labelStatus.Content = e.ConnectionStatus.ToString();
            }
        }

        void droneController_OnNotifyDroneInfoMessage(object sender, DroneInfoNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage), sender, e);
            }
            else
            {
                //handle e
                /*if (e.DroneConfiguration.Count != 0)
                {
                    takeOff();


                    droneController.SetConfiguration("control:euler_angle_max", "0.52");
                    droneController.SetConfiguration("control:control_iphone_tilt", "1");
                    time.Interval = TimeSpan.FromSeconds(4);
                    time.Tick += new EventHandler(takeOffDone);
                    time.Start();
                }*/
            }
            
        }

        void droneController_OnNotifyVideoMessage(object sender, VideoNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage), sender, e);
            }
            else
            {
                VideoImg.ImageSource = e.CurrentImage;
                ///VideoFeedImage.Source = e.CurrentImage;
            }
        }

        void droneController_OnNotifyTraceMessage(object sender, TraceNotificationEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action<object, TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage), sender, e);
            }
            else
            {
                //Traces.Add(e.NotificationMessage);
                //TraceBox.ScrollIntoView(TraceBox.Items[TraceBox.Items.Count-1]);
            }
        }

        void takeOffDone(object sender, EventArgs e)
        {
            time.Tick -= new EventHandler(takeOffDone);
            time.Stop();

            animLeds();

            time.Interval = TimeSpan.FromSeconds(6);
            time.Tick += new EventHandler(animDone);
            time.Start();
        }

        void animDone(object sender, EventArgs e)
        {
            time.Tick -= new EventHandler(animDone);
            time.Stop();

            land();

            time.Interval = TimeSpan.FromSeconds(4);
            time.Tick += new EventHandler(landingDone);
            time.Start();
        }


        void landingDone(object sender, EventArgs e)
        {
            time.Tick -= new EventHandler(landingDone);
            time.Stop();

            disconnectFromDrone();
        }

        internal void disconnectFromDrone()
        {
            droneController.Disconnect();
            //droneController = null;
        }

        internal void takeOff()
        {
            if (droneController != null && droneController.ConnectionStatus == ConnectionStatus.Open)
            {
                droneController.SetFlatTrim();
                droneController.StartEngines();
            }
        }

        internal void land()
        {
            if (droneController != null)
            {
                droneController.StopEngines();
            }
        }

        internal void flatTrim()
        {
            if (droneController != null)
            {
                droneController.SetFlatTrim();
            }
        }

        internal void reset()
        {
            if (droneController != null)
            {
                droneController.StartReset();

                Thread.Sleep(20);

                droneController.StopReset();
            }
        }

        internal void animLeds()
        {
            if (droneController != null && droneController.ConnectionStatus == ConnectionStatus.Open)
            {
                droneController.PlayLedAnimation(LedAnimation.BlinkGreen, 2, 5);
            }
        }


        // Drone Controller value set functions
        internal void setRoll(float speed)
        {
            droneController.Roll = speed;
        }
        internal void setPitch(float speed)
        {
            droneController.Pitch = speed;
        }
        internal void setGaz(float speed)
        {
            droneController.Gaz = speed;
        }
        internal void setYaw(float speed)
        {
            droneController.Yaw = speed;
        }
        internal void setMoving(bool val)
        {
            moving = val;
        }


        // Button Handlers
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)TestButton.Content == "Connect")
            {
                connectToDrone();
                TestButton.Content = "Disconnect";
            }
            else
            {
                disconnectFromDrone();
                TestButton.Content = "Connect";
            }

            /*droneController.RequestConfiguredValues();

            
            takeOff();
            
            time.Interval = TimeSpan.FromSeconds(4);
            time.Tick += new EventHandler(takeOffDone);
            time.Start();*/

        }
        
        private void buttonTakeOff_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            
        }

        private void buttonTakeOff_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            takeOff();
        }

        private void buttonLeft_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setRoll(-yawSpeed);
        }

        private void buttonLeft_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setRoll(0);
        }

        private void buttonUp_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setPitch(-speedf);
        }

        private void buttonUp_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setPitch(0);
        }

        private void buttonDown_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setPitch(speedf);
        }

        private void buttonDown_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setPitch(0);
        }

        private void buttonRight_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setRoll(yawSpeed);
        }

        private void buttonRight_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setRoll(0);
        }

        private void buttonGazUp_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setGaz(verticalSpeed);
        }

        private void buttonGazUp_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setGaz(0);
        }

        private void buttonGazDown_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setGaz(-verticalSpeed);
        }

        private void buttonGazDown_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setGaz(0);
        }

        private void buttonLand_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {

        }

        private void buttonLand_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            land();
        }

        private void ButtonAcceleroControl_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            setMoving(true);
        }

        private void ButtonAcceleroControl_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            setMoving(false);
            if (droneController != null)
            {
                setRoll(0);
                setPitch(0);
                setYaw(0);
                setGaz(0);
            }
        }
    }
}
