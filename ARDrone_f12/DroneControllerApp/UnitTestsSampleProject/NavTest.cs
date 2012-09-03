using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WatermarkedTextBoxControl;
using DroneController;
using DroneControllerApp;

namespace UnitTestsSampleProject
{
	[TestClass]
	public class NavTests : SilverlightTest
    {
        //protected mock = new Moq.Mock<AutoNavPage>() ;
        protected DroneControllerApp.AutoNavPage navApp;
        protected float latDiff, lonDiff;
        protected GeoCoordinate current;
        protected GeoCoordinate destination;

        [TestInitialize]
        public void Init()
        {
            current = new GeoCoordinate(30.623665, -96.334171);
            destination = new GeoCoordinate(30.62505, -96.335362);
            navApp = new DroneControllerApp.AutoNavPage();
            navApp.connectToDrone();
        }
        
            /*[TestMethod]
            public void TestLocationConstructor()
            {
                Assert.Equals(destination.Latitude, 30.62505);
                Assert.Equals(destination.Longitude, -96.335362);
                Assert.Equals(current.Latitude, 30.623665);
                Assert.Equals(current.Longitude, -96.334171);
            }*/

            [TestMethod]
            public void TestDestinationFound()
            {
                Assert.IsFalse(navApp.destinationFound(current, destination));
                Assert.IsTrue(navApp.destinationFound(current, current));
            }

            /*[TestMethod]
            public void TestFindLocation_NW()
            {
                GeoCoordinate destinationFound = navApp.findDestination(current, destination);
                Assert.IsTrue(navApp.destinationFound(destination, destinationFound));
            }

            [TestMethod]
            public void TestFindLocation_NE()
            {
                destination.Longitude = -96.333178;
                GeoCoordinate destinationFound = navApp.findDestination(current, destination);
                Assert.IsTrue(navApp.destinationFound(destination, destinationFound));
            }
        
            [TestMethod]
            public void TestFindLocation_SW()
            {
                destination.Latitude = 30.622601;
                GeoCoordinate destinationFound = navApp.findDestination(current, destination);
                Assert.IsTrue(navApp.destinationFound(destination, destinationFound));
            }
        
            [TestMethod]
            public void TestFindLocation_SE()
            {
                destination.Longitude = -96.333178;
                destination.Latitude = 30.622601;
                GeoCoordinate destinationFound = navApp.findDestination(current, destination);
                Assert.IsTrue(navApp.destinationFound(destination, destinationFound));
            }*/

            /*[TestMethod]
            [Tag("InOrderTest")]
            public void TestInOrder()
            {
                GeoCoordinate d1 = new GeoCoordinate(30.625204, -96.336547);
                GeoCoordinate d2 = new GeoCoordinate(30.623432, -96.330668);
                Queue<GeoCoordinate> route = new Queue<GeoCoordinate>();
                route.Enqueue(d1);
                GeoCoordinate destination = navApp.followRoute(route, "converge", true);
                Assert.IsTrue(navApp.destinationFound(d1, destination));
                
                route.Enqueue(d1);
                route.Enqueue(d2);
                destination = navApp.followRoute(route, "converge", true);
                Assert.IsTrue(navApp.destinationFound(d2, destination));
                
                route.Enqueue(d1);
                route.Enqueue(d2);
                route.Enqueue(d1);
                route.Enqueue(d2);
                destination = navApp.followRoute(route, "converge", true);
                Assert.IsTrue(navApp.destinationFound(d2, destination));
            }
            
            [TestMethod]
            [Tag("ShortestTest")]
            public void TestShortest()
            {
                GeoCoordinate d1 = new GeoCoordinate(30.624059, -96.33541);
                GeoCoordinate d2 = new GeoCoordinate(30.625204, -96.336547);
                GeoCoordinate d3 = new GeoCoordinate(30.623432, -96.330668);
                Queue<GeoCoordinate> route = new Queue<GeoCoordinate>();
                
                route.Enqueue(d1);
                GeoCoordinate destination = navApp.followRoute(route, "converge", false);
                Assert.IsTrue(navApp.destinationFound(d1, destination));
                
                route.Enqueue(d1);
                route.Enqueue(d2);
                destination = navApp.followRoute(route, "converge", false);
                Assert.IsTrue(navApp.destinationFound(d2, destination));
                
                route.Enqueue(d2);
                route.Enqueue(d1);
                destination = navApp.followRoute(route, "converge", false);
                Assert.IsTrue(navApp.destinationFound(d2, destination));
                
                route.Enqueue(d3);
                route.Enqueue(d2);
                route.Enqueue(d1);
                destination = navApp.followRoute(route, "converge", false);
                Assert.IsTrue(navApp.destinationFound(d3, destination));
            }*/

            /*[TestMethod]
            public void TestSaved()
            {
                destination = navApp.savedAutoRoute();
                //GeoCoordinate test = new GeoCoordinate(30.622483, -96.333924);
                //GeoCoordinate test = new GeoCoordinate(30.622250, -96.333414);
                //GeoCoordinate d2 = new GeoCoordinate(30.623432, -96.330668);
                GeoCoordinate d3 = new GeoCoordinate(30.61955531, -96.33615285);
                Assert.IsTrue(navApp.destinationFound(destination, d3));
            }*/

            [TestMethod]
            public void TestAngleToDestination()
            { 
                double angle = navApp.angleToDestination(new GeoCoordinate(30.001,-96.001), new GeoCoordinate(30.002, -96.002));
                Assert.Equals(-45.0, angle);    // Quad II 
                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.001, -96.002));
                Assert.Equals(0.0, angle);      // 270* N
                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.000, -96.000));
                Assert.Equals(-45.0, angle);    // Quad IV 
                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.002, -96.000));
                Assert.Equals(45.0, angle);     // Quad I
            }
            
            [TestMethod]
            public void TestCalc360Angle()
            { 
                double angle = navApp.angleToDestination(new GeoCoordinate(30.001,-96.001), new GeoCoordinate(30.002, -96.002));
                double angle360 = navApp.calc360Angle(AutoNavPage.DirectionX.West, AutoNavPage.DirectionY.North, angle);
                Assert.Equals(315.0, angle360);

                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.001, -96.002));
                angle360 = navApp.calc360Angle(AutoNavPage.DirectionX.West, AutoNavPage.DirectionY.North, angle);
                Assert.Equals(270.0, angle);
                
                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.000, -96.000));
                angle360 = navApp.calc360Angle(AutoNavPage.DirectionX.East, AutoNavPage.DirectionY.South, angle);
                Assert.Equals(135.0, angle);
                
                angle = navApp.angleToDestination(new GeoCoordinate(30.001, -96.001), new GeoCoordinate(30.002, -96.000));
                angle360 = navApp.calc360Angle(AutoNavPage.DirectionX.East, AutoNavPage.DirectionY.North, angle);
                Assert.Equals(45.0, angle);
            }

        [TestCleanup]
        public void NavTestCleanup()
        {
            navApp.droneController.Disconnect();
            navApp.droneController.Dispose();
            current = null;
            destination = null;
            navApp = null;
        }
    
    }
}
