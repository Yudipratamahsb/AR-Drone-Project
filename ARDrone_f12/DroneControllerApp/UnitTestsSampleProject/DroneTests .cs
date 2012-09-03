using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
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
	public class DroneTests : SilverlightTest
    {
        protected DroneControllerApp.ControlPage droneControllerApp;
        
        [TestInitialize]
        public void Init()
        {
            droneControllerApp = new DroneControllerApp.ControlPage();
            droneControllerApp.connectToDrone();
        }
            [TestMethod]
            public void TestConnectToDrone()
            {
                // connectToDrone is called in Init (setup) - assert on conditions expected
                Assert.IsTrue(droneControllerApp.droneController.IsMoving);
                Assert.Equals(ConnectionStatus.Open, droneControllerApp.droneController.ConnectionStatus);
            }

            [TestMethod]
            public void TestDisconectFromDrone()
            {
                droneControllerApp.disconnectFromDrone();
                Assert.Equals(ConnectionStatus.Closed, droneControllerApp.droneController.ConnectionStatus);
            }

            [TestMethod]
            public void TestTakeOff()
            {
                droneControllerApp.takeOff();
                Assert.IsTrue(droneControllerApp.droneController.DroneIsFlying);
            }

            [TestMethod]
            public void TestLand()
            {
                droneControllerApp.land();
                Assert.IsFalse(droneControllerApp.droneController.DroneIsFlying);
            }
           
            [TestMethod]
            public void TestSetPitch()
            {
                droneControllerApp.setPitch(droneControllerApp.speedf);
                Assert.Equals(droneControllerApp.speedf, droneControllerApp.droneController.Pitch);
                droneControllerApp.setPitch(0);
                Assert.Equals(0, droneControllerApp.droneController.Pitch);
            }
            
            [TestMethod]
            public void TestSetRoll()
            {
                droneControllerApp.setRoll(droneControllerApp.speedf);
                Assert.Equals(droneControllerApp.speedf, droneControllerApp.droneController.Roll);
                droneControllerApp.setRoll(0);
                Assert.Equals(0, droneControllerApp.droneController.Roll);
            }

            [TestMethod]
            public void TestSetGaz()
            {
                droneControllerApp.setGaz(droneControllerApp.speedf);
                Assert.Equals(droneControllerApp.speedf, droneControllerApp.droneController.Gaz);
                droneControllerApp.setGaz(0);
                Assert.Equals(0, droneControllerApp.droneController.Gaz);
            }
            
            [TestMethod]
            public void TestSetYaw()
            {
                droneControllerApp.setYaw(droneControllerApp.speedf);
                Assert.Equals(droneControllerApp.speedf, droneControllerApp.droneController.Yaw);
                droneControllerApp.setYaw(0);
                Assert.Equals(0, droneControllerApp.droneController.Yaw);
            }
            
            [TestMethod]
            public void TestSetMoving()
            {
                droneControllerApp.setMoving(true);
                Assert.IsTrue(droneControllerApp.moving);
                droneControllerApp.setMoving(false);
                Assert.IsFalse(droneControllerApp.moving);
            }

        }
}
