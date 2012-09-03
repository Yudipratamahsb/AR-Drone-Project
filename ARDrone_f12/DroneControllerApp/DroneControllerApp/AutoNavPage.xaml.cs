using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using DroneController;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Device.Location;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Net;

namespace DroneControllerApp
{
    public partial class AutoNavPage : PhoneApplicationPage
    {
        internal DroneController.DroneController droneController;

        internal ObservableCollection<string> Traces;
        GeoCoordinate defaultStartLocation = new GeoCoordinate(30.61903827, -96.33579344); //admin
        //GeoCoordinate defaultStartLocation = new GeoCoordinate(30.61958532, -96.33625477); //field
        //GeoCoordinate defaultStartLocation = new GeoCoordinate(30.619417, -96.338724); //street
        GeoCoordinate current;

        DispatcherTimer time;
        internal float speedf = 0.7f;
        //internal float verticalSpeed;
        //internal float yawSpeed;
        internal bool moving;
        Motion motion;
        Queue<GeoCoordinate> waypoints;

        MapLayer imageLayer;

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
        public AutoNavPage()
        {
            InitializeComponent();
            Traces = new ObservableCollection<string>();
            //TraceBox.DataContext = Traces;
            time = new DispatcherTimer();
            motion = new Motion();
            imageLayer = new MapLayer();
            navMap.Children.Add(imageLayer);

            current = defaultStartLocation;     // Default location - middle of field
            current.Course = 0.0;               // Start facing North for reference

            motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged);

            waypoints = new Queue<GeoCoordinate>();

            gpsBox.Visibility = System.Windows.Visibility.Collapsed;

            Join();
            setMap();

            try
            {
                motion.Start();
            }
            catch (InvalidOperationException)
            {
                //MessageBox.Show("Unable to start the motion API");
            }
        }

        protected void setMap()
        {
            GeoCoordinate center = getCurrentGPS();
            navMap.Center = center;
            navMap.ZoomLevel = 15;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Create a new UdpAnySourceMulticastClient instance and join the group.
            Join();
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
                    _client.MulticastLoopback = false;

                    // Set a flag indicating that we have now joined the multicast group 
                    _joined = true;

                    // Let others know we have joined by sending out a message to the group.
                    //Send("Joined the group");

                    // Wait for data from the group. This is an asynchronous operation 
                    // and will not block the UI thread.
                    Receive();
                }, null);
        }

        private void Receive()
        {
            // Only attempt to receive if you have already joined the group
            if (_joined)
            {
                Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                _client.BeginReceiveFromGroup(_receiveBuffer, 0, _receiveBuffer.Length,
                    result =>
                    {
                        IPEndPoint source;

                        // Complete the asynchronous operation. The source field will 
                        // contain the IP address of the device that sent the message
                        _client.EndReceiveFromGroup(result, out source);

                        // Get the received data from the buffer.
                        string dataReceived = Encoding.UTF8.GetString(_receiveBuffer, 0, _receiveBuffer.Length);
                        string[] msg = dataReceived.Split(',');
                        
                        double latitude;
                        double longitude;
                        //double heading;

                        try
                        {
                            if (Double.TryParse(msg[0], out latitude))
                                current.Latitude = latitude;
                            if (Double.TryParse(msg[1], out longitude))
                                current.Longitude = longitude;
                            //if (Double.TryParse(msg[2], out heading))
                            //current.Course = heading;
                        }
                        catch { }
                        // Create a log entry.
                        //string message = String.Format("[{0}]: {1}", source.Address.ToString(), dataReceived);

                        // Log it.
                        Log(dataReceived, "sub");

                        // Call receive again to continue to "listen" for the next message from the group
                        Receive();
                    }, null);
            }
            else
            {
                MessageBox.Show("Cant receive - not connected to Multicast group");
            }
        }

        private void Log(string message, string box)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
                () =>
                {
                    if (box == "sub")
                    {
                        listBox.Items.Add(message);
                        listBox.ScrollIntoView(message);
                    }
                    else if (box == "main")
                    {
                        gpsBox.Items.Add(message);
                        gpsBox.ScrollIntoView(message);
                    }
                    // Make sure that the item we added is visible to the user.                    
                });
        }

        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e)
        {
            if (moving)
            {
                if (droneController != null)
                {
                    var PhonePitch = MathHelper.ToDegrees(e.SensorReading.Attitude.Pitch);
                    var PhoneRoll = MathHelper.ToDegrees(e.SensorReading.Attitude.Roll);
                    PhonePitch = PhonePitch > 0 ? PhonePitch - (App.Current as App).Config.PhoneTiltThreshold : PhonePitch + (App.Current as App).Config.PhoneTiltThreshold;
                    PhoneRoll = PhoneRoll > 0 ? PhoneRoll = (App.Current as App).Config.PhoneTiltThreshold : PhoneRoll +(App.Current as App).Config.PhoneTiltThreshold;
                    var divider =  (App.Current as App).Config.WP7TiltMax - (App.Current as App).Config.PhoneTiltThreshold;
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
            droneControllerConfiguration.EnableATCommandThread = true; ;
            droneControllerConfiguration.EnableControlInfoThread = false;
            droneControllerConfiguration.EnableATCommandSimulation = false;
            droneControllerConfiguration.DroneIpAddress = "192.168.1.1";

            droneController = null;  // ensure null for creation (removed set = null in disconect)
            droneController = new DroneController.DroneController(droneControllerConfiguration);

            droneController.IsMoving = true;

            droneController.TraceNotificationLevel = TraceNotificationLevel.Verbose;
            droneController.OnNotifyTraceMessage += new EventHandler<TraceNotificationEventArgs>(droneController_OnNotifyTraceMessage);
            droneController.OnNotifyVideoMessage += new EventHandler<VideoNotificationEventArgs>(droneController_OnNotifyVideoMessage);
            droneController.OnNotifyDroneInfoMessage += new EventHandler<DroneInfoNotificationEventArgs>(droneController_OnNotifyDroneInfoMessage);
            droneController.OnConnectionStatusChanged += new EventHandler<ConnectionStatusChangedEventArgs>(droneController_OnConnectionStatusChanged);
            droneController.Connect();

            while (droneController.ConnectionStatus != ConnectionStatus.Open) Thread.Sleep(100);

            droneController.SetConfiguration("control:euler_angle_max", MathHelper.ToRadians(10).ToString());
            droneController.SetConfiguration("control:control_iphone_tilt", "1");
            droneController.SetConfiguration("control:altitude_max", (App.Current as App).Config.MaxAltitude.ToString());
            droneController.SetConfiguration("control:control_vz_max", (App.Current as App).Config.VerticalSpeed.ToString());
            droneController.SetConfiguration("control:control_yaw", MathHelper.ToRadians(200).ToString());
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
            droneController = null;
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

        // --------------------------------------------------------
        // Auto Navigation Algorithms and Functions
        // --------------------------------------------------------
        internal double latitudeAccuracy = 0.00006;  //set with options box??
        internal double longitudeAccuracy = 0.00008; //set with options box??
        internal double distAccuracy = 15.0;
         
        internal int numSteps = 5; // Coarse measure of how quickly to approach target (lower = more accuracy)
        internal int threadMultiple = 500; //Set for accuracy of step size (smaller with more accuracy)
        internal enum DirectionX { East, West };
        internal enum DirectionY { North, South };
        bool breakRoute = false;
        private object flag_lock = new object();

        // --- Caluculation Functions and Route Finder Helpers ---
        // -------------------------------------------------------
        private static void getDir(double latDiff, double lonDiff, out DirectionX dirTravelX, out DirectionY dirTravelY)
        {
            // ---- Y direction of travel ----
            if (latDiff >= 0)
                dirTravelY = DirectionY.North;
            else
                dirTravelY = DirectionY.South;

            // ---- X direction of travel ----
            if (lonDiff >= 0)
            {
                if (dirTravelY == DirectionY.North)
                    dirTravelX = DirectionX.West;
                else
                    dirTravelX = DirectionX.East;
                // Might need a turn?? not sure?
            }
            else
            {
                if (dirTravelY == DirectionY.North)
                    dirTravelX = DirectionX.East;
                else
                    dirTravelX = DirectionX.West;
            }
        }

        private static void getDirAngles(double latDiff, double lonDiff, out DirectionX dirTravelX, out DirectionY dirTravelY)
        {
            // ---- Y direction of travel ----
            if (latDiff >= 0)
                dirTravelY = DirectionY.North;
            else
                dirTravelY = DirectionY.South;

            // ---- X direction of travel ----
            if (lonDiff >= 0)
                dirTravelX = DirectionX.West;
            else
                dirTravelX = DirectionX.East;
                // Might need a turn?? not sure?
        }

        internal double calcLatDiff(GeoCoordinate current, GeoCoordinate destination)
        {
            return (destination.Latitude - current.Latitude);
        }

        internal double calcLonDiff(GeoCoordinate current, GeoCoordinate destination)
        {
            return (Math.Abs(destination.Longitude) - Math.Abs(current.Longitude));
        }

        internal bool destinationFound(GeoCoordinate current, GeoCoordinate destination)
        {
            // Destination reached alert
            // Allow user to make selection on what to do at this point
            // Activate manual control screen??
            double currDist = current.GetDistanceTo(destination);
            Log("Dist: " + currDist.ToString(), "main");
            //Log("Dir: " + current.Course.ToString(), "main"); 
            return (currDist <= distAccuracy);
        }

        internal GeoCoordinate getCurrentGPS()
        {
            //if(!Double.IsNaN(current.Latitude) && !Double.IsNaN(current.Longitude)
            return current;
        }

        internal GeoCoordinate getGPSforTest(GeoCoordinate old, int forward, DirectionY dirY, int side, DirectionX dirX)
        {
            double lat = old.Latitude;
            double lon = old.Longitude;

            if (dirY == DirectionY.North)
            {
                lat += ((double)forward / (100000.0 ));
                if (dirX == DirectionX.West)
                    lon = lon - ((double)side / (100000.0 ));
                else
                    lon = lon + ((double)side / (100000.0 ));
            }
            else  //South
            {
                lat = lat - ((double)forward / (100000.0 ));

                if (dirX == DirectionX.West)
                    lon = lon + ((double)side / (100000.0));
                else
                    lon = lon - ((double)side / (100000.0 ));
            }

            return new GeoCoordinate(lat, lon);  // Return new location
        }

        internal double angleToDestination(GeoCoordinate current, GeoCoordinate destination)
        {
            double theta;
            double latDiff = calcLatDiff(current, destination);
            double lonDiff = calcLonDiff(current, destination);

            lonDiff = lonDiff * (-1.0); // Compensate for long diff compare being opp expected
            if (lonDiff == 0)
                theta = latDiff > 0 ? 0 : 180;
            else
                theta = MathHelper.ToDegrees((float)Math.Atan(latDiff / lonDiff));

            return theta;
        }

        internal double calc360Angle(DirectionX xDir, DirectionY yDir, double theta)
        {
            double angle;
            if (xDir == DirectionX.East)
            {
                angle = 90 - theta;   // North or south since eastSouth = neg Tan theta
                if (angle < 0)
                    angle += 360;
            }
            else
            {
                if (yDir == DirectionY.North)
                    angle = 270 + theta;
                else
                    angle = 270 - theta;    // South
            }
            return angle;
        }


        // --------------------------------------------------------
        // Step-based Route Finding (stair-step convergence)
        // --------------------------------------------------------
        internal GeoCoordinate findDestination(GeoCoordinate current, GeoCoordinate destination)
        {
            double latDiff = calcLatDiff(current, destination);
            double lonDiff = calcLonDiff(current, destination);

            DirectionX dirTravelX;
            DirectionY dirTravelY;

            while(!destinationFound(current, destination) && !breakRoute)  // location not found
            {
                getDir(latDiff, lonDiff, out dirTravelX, out dirTravelY);

                double travelRatio = Math.Abs(lonDiff / latDiff);

                // Move North / South
                if (dirTravelY == DirectionY.North)
                    setPitch(-speedf);
                else
                    setPitch(speedf);
                Thread.Sleep(threadMultiple * numSteps);  // Move forward - do I need?
                setPitch(0);

                // Move East / West [Dependent on whether dirTravel N/S]
                if (dirTravelY == DirectionY.North)
                {
                    if (dirTravelX == DirectionX.East)
                        setRoll(speedf);
                    else
                        setRoll(-speedf);
                }
                else
                {
                    if (dirTravelX == DirectionX.East)
                        setRoll(-speedf);
                    else
                        setRoll(speedf);
                }
                // Use ratio of N/S to E/W travel to travel on diag. rather than simple square/box travel
                int multiplier = (int)(threadMultiple * Math.Round(travelRatio, 1));
                if ((multiplier * numSteps) > 2 * threadMultiple * numSteps)
                    Thread.Sleep(2 * threadMultiple * numSteps);
                else 
                    Thread.Sleep(multiplier * numSteps);
                setRoll(0);

                // For production use:
                current = getCurrentGPS();  // Get current GPS location of Drone after move
                
                // For testing use:
                //current = getGPSforTest(current, threadMultiple * numSteps, dirTravelY, multiplier * numSteps, dirTravelX);
                
                // Calculate and update lat and lon differences
                latDiff = calcLatDiff(current, destination);
                lonDiff = calcLonDiff(current, destination);
            }
            return current;
        }


        // --------------------------------------------------------
        // Angle-Based Route Finding
        // --------------------------------------------------------
        internal GeoCoordinate findDestinationAngles(GeoCoordinate current, GeoCoordinate destination)
        {
            double latDiff = calcLatDiff(current, destination);
            double lonDiff = calcLonDiff(current, destination);

            DirectionX dirTravelX;
            DirectionY dirTravelY;
            double angle;
            double angle360;
            double angleChange;
            GeoCoordinate oldLoc = current;

            double distMultiple; 

            while (!destinationFound(current, destination) && !breakRoute)  // location not found
            {
                getDirAngles(latDiff, lonDiff, out dirTravelX, out dirTravelY);

                angle = angleToDestination(current, destination);
                angle360 = calc360Angle(dirTravelX, dirTravelY, angle);

                angleChange = angle360 - current.Course;
                oldLoc = current;
                // Change angle of drone to align with path to destination
                if (Math.Abs(angleChange) <= 180)
                    setYaw((float)(angleChange / 200.0));
                else
                    setYaw((float)(-1.0 * (360.0 - angleChange) / 200.0));
                Thread.Sleep(1000);
                setYaw(0);
                current.Course = angle360;
                Log("AngleNeeded: " + angleChange.ToString(), "main");
                Log("Course: " + current.Course.ToString(), "main");
                Log("Dist: " + current.GetDistanceTo(destination).ToString(), "main");
                // Distance multiple for steps toward target
                distMultiple = ((current.GetDistanceTo(destination)) / 10);
                
                // Move Forward (angle should be set toward destination)
                setPitch(speedf);
                Thread.Sleep(threadMultiple * (int)distMultiple);  // Move forward
                setPitch(0);

                // For production use:
                //current = getCurrentGPS();  // Get current GPS location of Drone after move

                double travelAngle = angleToDestination(oldLoc, current);
                Log("Tavel: " + travelAngle.ToString(), "main");
                if (!destinationFound(oldLoc, current)) current.Course = calc360Angle(dirTravelX, dirTravelY, travelAngle);
                // For testing use:
                current = getGPSforTest(current, threadMultiple * numSteps, dirTravelY, (int)distMultiple * numSteps, dirTravelX);

                // Calculate and update lat and lon differences
                latDiff = calcLatDiff(current, destination);
                lonDiff = calcLonDiff(current, destination);
            }
            return current;
        }


        // --------------------------------------------------------
        // Route Following Algorithm
        // --------------------------------------------------------
        internal GeoCoordinate followRoute(Queue<GeoCoordinate> waypoints, string method, bool inOrder)
        {
            GeoCoordinate current = new GeoCoordinate();

            if (inOrder)    // Route in-order of placement
            {
                if (waypoints.Count != 0)
                {
                    // Waypoints present
                    current = getCurrentGPS();
                    while (waypoints.Count > 0)
                    {
                        if (method == "converge")
                            current = findDestination(current, waypoints.Dequeue());
                        else
                            current = findDestinationAngles(current, waypoints.Dequeue());
                    }
                    Log("***Destination FOUND***", "main");
                    return current;
                }
                else    // No Waypoints - Error Message
                {
                    MessageBox.Show("No Waypoints Selected");
                    return null;
                }
            }

            else
            {
                // Shortest Path - Greedy Dijkstra
                GeoCoordinate[] waypointArray = waypoints.ToArray();
                current = getCurrentGPS();
                double minDist = current.GetDistanceTo(waypointArray[0]);
                double tempDist = minDist;

                int index = 0;
                bool destinationFound = false;

                while (!destinationFound)
                {
                    destinationFound = true;  // set flag = false -> if no destinations, end while

                    int j = 0;
                    while (j < waypointArray.Length)  // find first non-null location 
                    {
                        if (waypointArray[j] != null)
                        {
                            minDist = current.GetDistanceTo(waypointArray[j]);
                            destinationFound = false;  // at least one destination remaining
                            break;
                        }
                        j++;
                    }

                    if (destinationFound)
                        break;

                    else
                    {
                        for (int i = 0; i < waypointArray.Length; i++)
                        {
                            if (waypointArray[i] != null)
                            {
                                tempDist = current.GetDistanceTo(waypointArray[i]);
                                if (tempDist <= minDist)
                                {
                                    minDist = tempDist;
                                    index = i;
                                }
                            }
                        }
                        if (method == "converge")
                            current = findDestination(current, waypointArray[index]);
                        else
                            current = findDestinationAngles(current, waypointArray[index]);

                        waypointArray[index] = null;
                    }
                }
                Log("***Destination FOUND***", "main");
                return current;
            }
        }
        // --------------------------------------------------------

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

        internal GeoCoordinate savedAutoRoute()
        {
            /*GeoCoordinate d1 = new GeoCoordinate(30.622250, -96.333414);
            GeoCoordinate d2 = new GeoCoordinate(30.622954, -96.332969);
            GeoCoordinate d3 = new GeoCoordinate(30.623406, -96.334203);
            GeoCoordinate d4 = new GeoCoordinate(30.622483, -96.333924);
            Queue<GeoCoordinate> route = new Queue<GeoCoordinate>();
            GeoCoordinate destination;

            route.Enqueue(d4);
            route.Enqueue(d2);
            route.Enqueue(d1);
            //route.Enqueue(d4);
            destination = followRoute(route, true);
            return destination;*/

            // EAST LAWN Coordinates (new 5-2-12 update)
            //GeoCoordinate p1 = new GeoCoordinate(30.61923216, -96.33598789);
            //GeoCoordinate p2 = new GeoCoordinate(30.61947221, -96.33536562);
            GeoCoordinate p3 = new GeoCoordinate(30.619038267, -96.33579344);
            GeoCoordinate end = new GeoCoordinate(30.61903596, -96.33617565);

            //GeoCoordinate p5 = new GeoCoordinate(30.61867587, -96.33585513);
            //GeoCoordinate end = new GeoCoordinate(30.61919754, -96.33635804);
            //GeoCoordinate p4 = new GeoCoordinate(30.61983461, -96.33597583);
            //GeoCoordinate p5 = new GeoCoordinate(30.61955531, -96.33615285);

            //GeoCoordinate star = new GeoCoordinate(30.619027, -96.339359);
            Queue<GeoCoordinate> route = new Queue<GeoCoordinate>();

            GeoCoordinate destination;
            //route.Enqueue(p1);
            //route.Enqueue(p2);
            route.Enqueue(p3);
            //route.Enqueue(p5);
            route.Enqueue(end);
            destination = followRoute(route, "converge", true);

            return destination;
        }

        // Button Handlers
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)ConnectButton.Content == "Connect")
            {
                connectToDrone();
                ConnectButton.Content = "Disconnect";
            }
            else
            {
                disconnectFromDrone();
                ConnectButton.Content = "Connect";
            }
        }

        private void LandButton_Click(object sender, RoutedEventArgs e)
        {
            breakRoute = true;
            gpsBox.Visibility = System.Windows.Visibility.Collapsed;
            land();
            disconnectFromDrone();
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (droneController == null)
                disconnectFromDrone();
            base.OnNavigatedFrom(e);
        }
        private GeoCoordinate mapCenter;

        private void PushPinButton_Click (object sender, RoutedEventArgs e)
        {
            mapCenter = navMap.Center;
            waypoints.Enqueue(navMap.Center);

            Image pinImg = new Image();
            pinImg.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pushPinSmall.png", UriKind.Relative));
            pinImg.Opacity = 0.8;
            pinImg.Stretch = System.Windows.Media.Stretch.None;

            //Pushpin pin1 = new Pushpin();
            //pin1.Location = mapCenter;
            PositionOrigin pos = PositionOrigin.Center;
            imageLayer.AddChild(pinImg, mapCenter, pos);
            //navMap.Children.Add(pin1);
        }

        private void AutoNavButton_Click(object sender, RoutedEventArgs e)
        {
            connectToDrone();

            gpsBox.Visibility = System.Windows.Visibility.Visible;
            takeOff();
            current.Course = 0.0;
            ThreadPool.QueueUserWorkItem(o => {
                // (bool)ShortestPath.IsChecked
                GeoCoordinate destination = new GeoCoordinate();
                destination = followRoute(waypoints, "converge", (bool)InOrder.IsChecked);
                Thread.Sleep(3000);
                //savedAutoRoute();
                //Thread.Sleep(10000);
                land();
                gpsBox.Dispatcher.BeginInvoke(() => {
                gpsBox.Visibility = System.Windows.Visibility.Collapsed;
                });
            });
        }
               /* private void buttonLeft_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
                {
                    setYaw(-speedf);
                }

                private void buttonLeft_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
                {
                    setYaw(0);
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
                    setYaw(yawSpeed);
                }

                private void buttonRight_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
                {
                    setYaw(0);
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
                */
        private void ButtonAutoNav_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            //setMoving(true);
        }

        private void ButtonAutoNav_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
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
