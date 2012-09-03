using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Windows.Threading;

namespace GPSTracker
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher watcher;
        const string SEND_IP = "192.168.1.2";
        const int SEND_PORT = 5558;

        Compass compass;
        DispatcherTimer timer;

        double magneticHeading;
        double trueHeading;
        double headingAccuracy;
        Vector3 rawMagnetometerReading;
        bool isDataValid;
        Accelerometer accelerometer;

        double heading;

        bool calibrating = false;

        // The address of the multicast group to join.
        // Must be in the range from 224.0.0.0 to 239.255.255.255
        private const string GROUP_ADDRESS = "224.0.0.1";

        // The port over which to communicate to the multicast group
        private const int GROUP_PORT = 5558;

        // A client receiver for multicast traffic from any source
        UdpAnySourceMulticastClient _client = null;

        // true if we have joined the multicast group; otherwise, false
        bool _joined = false;

        // Buffer for incoming data
        private byte[] _receiveBuffer;

        // Maximum size of a message in this communication
        private const int MAX_MESSAGE_SIZE = 512;


        // Constructor
        public MainPage()
        {
            InitializeComponent();
            try
            {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            catch { MessageBox.Show("Couldnt disable idleDetection"); }
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 0;

            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

            if (!Compass.IsSupported)
            {
                MessageBox.Show("Compass Not Supported");
            }
            else
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(30);
                timer.Tick += new EventHandler(timer_Tick);
            }
        }

        private void startCompass()
        {
            if (compass != null && compass.IsDataValid)
            {
                compass.Stop();
                timer.Stop();
                accelerometer.Stop();
            }
            else
            {
                if (compass == null)
                {
                    compass = new Compass();
                    compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);
                    compass.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<CompassReading>>(compass_CurrentValueChanged);
                    compass.Calibrate += new EventHandler<CalibrationEventArgs>(compass_Calibrate);
                }
            
            }

                try
                {
                  compass.Start();
                  timer.Start();
                  accelerometer = new Accelerometer();   
                  // Start accelerometer for detecting compass axis
                  accelerometer = new Accelerometer();
                  accelerometer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(accelerometer_CurrentValueChanged);
                  accelerometer.Start();
                  heading = 0.0;
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Error with sensor data");
                }

        }

        void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Vector3 v = e.SensorReading.Acceleration;

            bool isCompassUsingNegativeZAxis = false;

            if (Math.Abs(v.Z) < Math.Cos(Math.PI / 4) &&
                          (v.Y < Math.Sin(7 * Math.PI / 4)))
            {
                isCompassUsingNegativeZAxis = true;
            }

            //Dispatcher.BeginInvoke(() => { orientationTextBlock.Text = (isCompassUsingNegativeZAxis) ? "portrait mode" : "flat mode"; });
        }

        void compass_CurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            isDataValid = compass.IsDataValid;

            trueHeading = e.SensorReading.TrueHeading;
            magneticHeading = e.SensorReading.MagneticHeading;
            headingAccuracy = Math.Abs(e.SensorReading.HeadingAccuracy);
            rawMagnetometerReading = e.SensorReading.MagnetometerReading;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!calibrating)
            {
                if (isDataValid)
                {
                    //statusTextBlock.Text = "receiving data from compass.";
                }

                // Update the textblocks with numeric heading values
                heading = magneticHeading;
                //Log("Mag heading = "+ heading.ToString("0.0"));
                /*magneticTextBlock.Text = magneticHeading.ToString("0.0");
                trueTextBlock.Text = trueHeading.ToString("0.0");
                accuracyTextBlock.Text = headingAccuracy.ToString("0.0");

                // Update the line objects to graphically display the headings
                double centerX = headingGrid.ActualWidth / 2.0;
                double centerY = headingGrid.ActualHeight / 2.0;
                magneticLine.X2 = centerX - centerY * Math.Sin(MathHelper.ToRadians((float)magneticHeading));
                magneticLine.Y2 = centerY - centerY * Math.Cos(MathHelper.ToRadians((float)magneticHeading));
                trueLine.X2 = centerX - centerY * Math.Sin(MathHelper.ToRadians((float)trueHeading));
                trueLine.Y2 = centerY - centerY * Math.Cos(MathHelper.ToRadians((float)trueHeading));

                // Update the textblocks with numeric raw magnetometer readings
                xTextBlock.Text = rawMagnetometerReading.X.ToString("0.00");
                yTextBlock.Text = rawMagnetometerReading.Y.ToString("0.00");
                zTextBlock.Text = rawMagnetometerReading.Z.ToString("0.00");

                // Update the line objects to graphically display raw data
                xLine.X2 = xLine.X1 + rawMagnetometerReading.X * 4;
                yLine.X2 = yLine.X1 + rawMagnetometerReading.Y * 4;
                zLine.X2 = zLine.X1 + rawMagnetometerReading.Z * 4;*/
            }
            else
            {
                if (headingAccuracy <= 10)
                {
                    calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    calibrationTextBlock.Text = "Complete!";
                }
                else
                {
                    calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    calibrationTextBlock.Text = headingAccuracy.ToString("0.0");
                }
            }
        }

        void compass_Calibrate(object sender, CalibrationEventArgs e)
        {
            Dispatcher.BeginInvoke(() => { calibrationStackPanel.Visibility = Visibility.Visible; });
            calibrating = true;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Create a new UdpAnySourceMulticastClient instance and join the group.
            Join();
        }

        private void calibrationButton_Click(object sender, RoutedEventArgs e)
        {
            calibrationStackPanel.Visibility = Visibility.Collapsed;
            calibrating = false;
        }



        private void Join()
        {
            // Initialize the receive buffer
            _receiveBuffer = new byte[MAX_MESSAGE_SIZE];

            // Create the UdpAnySourceMulticastClient instance using the defined 
            // GROUP_ADDRESS and GROUP_PORT constants. UdpAnySourceMulticastClient is a 
            // client receiver for multicast traffic from any source, also known as Any Source Multicast (ASM)
            _client = new UdpAnySourceMulticastClient(IPAddress.Parse(GROUP_ADDRESS), GROUP_PORT);

            // Make a request to join the group.
            _client.BeginJoinGroup(
                result =>
                {
                    // Complete the join
                    _client.EndJoinGroup(result);

                    // The MulticastLoopback property controls whether you receive multicast 
                    // packets that you send to the multicast group. Default value is true, 
                    // meaning that you also receive the packets you send to the multicast group. 
                    // To stop receiving these packets, you can set the property following to false
                    _client.MulticastLoopback = true;

                    // Set a flag indicating that we have now joined the multicast group 
                    _joined = true;

                    // Let others know we have joined by sending out a message to the group.
                    //Send("Joined the group");

                    // Wait for data from the group. This is an asynchronous operation 
                    // and will not block the UI thread.
                    //Receive();
                }, null);
        }

        private void Send(string message)
        {
            // Attempt the send only if you have already joined the group.
            if (_joined)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _client.BeginSendToGroup(data, 0, data.Length,
                    result =>
                    {
                        _client.EndSendToGroup(result);

                        // Log what we just sent
                        Log(message);

                    }, null);
            }
            else
            {
                MessageBox.Show("Cant receive - not connected to Multicast group");
            }
        }

        private void Log(string message)
        { 
            Deployment.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    listBox.Items.Add(message);

                    // Make sure that the item we added is visible to the user.
                    listBox.ScrollIntoView(message);
                });
        }

        // Event handler for the GeoCoordinateWatcher.StatusChanged event.
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.
                    //startLocationButton.IsEnabled = false;
                    break;

                case GeoPositionStatus.NoData:
                    stopLocationButton.IsEnabled = true;
                    break;

                case GeoPositionStatus.Ready:
                    stopLocationButton.IsEnabled = true;
                    break;
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            string latitude = e.Position.Location.Latitude.ToString("0.00000000");
            string longitude = e.Position.Location.Longitude.ToString("0.00000000");
            latitudeTextBlock.Text = latitude;
            longitudeTextBlock.Text = longitude;

            string packetMsg = latitude + "," + longitude + "," + heading.ToString("0.0");

            Send(packetMsg);
            /*byte[] packetBMsg = System.Text.Encoding.UTF8.GetBytes (packetMsg);
 
            Socket socket = DroneController.NetworkHelper.CreateUdpSocket(SEND_IP, SEND_PORT);
            IPEndPoint IPEP = DroneController.NetworkHelper.CreateRemoteEndPoint(SEND_IP, SEND_PORT);

            DroneController.NetworkHelper.SendUdp(socket, packetBMsg, packetBMsg.Length, IPEP);

            DroneController.NetworkHelper.CloseUdpConnection(socket);*/
        }

        private void startLocationButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Start();
            startCompass();
            startLocationButton.IsEnabled = false;
            stopLocationButton.IsEnabled = true;
        }

        // Click the event handler for the “Start Location” button.
        private void stopLocationButton_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            compass.Stop();
            timer.Stop();
            accelerometer.Stop();

            startLocationButton.IsEnabled = true;
            stopLocationButton.IsEnabled = false;
        }

    }
}