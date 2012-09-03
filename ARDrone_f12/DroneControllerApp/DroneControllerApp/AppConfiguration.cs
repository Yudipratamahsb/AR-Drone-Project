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
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace DroneControllerApp
{
    internal class AppConfigManager
    {

        public AppConfiguration Config { get; private set; }
        public void LoadFromFile()
        {
            TextReader reader = null;
            try
            {
                IsolatedStorageFile isoStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if ((!isoStorage.FileExists("AppConfig.xml")))
                {
                    Config = new AppConfiguration();
                }
                else
                {
                    IsolatedStorageFileStream file = isoStorage.OpenFile("AppConfig.xml", FileMode.OpenOrCreate);
                    reader = new StreamReader(file);
                    XmlSerializer xs = new XmlSerializer(typeof(AppConfiguration));
                    try
                    {
                        Config = (AppConfiguration)xs.Deserialize(reader);
                    }
                    catch (InvalidOperationException)
                    {
                        Config = new AppConfiguration();
                    }
                    reader.Close();
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }

        }
        public void SaveToFile()
        {
            TextWriter writer = null;
            try
            {
                IsolatedStorageFile isoStorage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream file = isoStorage.OpenFile("AppConfig.xml", FileMode.Create);
                writer = new StreamWriter(file);
                XmlSerializer xs = new XmlSerializer(typeof(AppConfiguration));
                xs.Serialize(writer, Config);
                writer.Close();
            }
            catch (Exception e)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // A navigation has failed; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }
            }
            finally
            {
                if (writer != null)
                    writer.Dispose();
            }
        }
    }
    public class AppConfiguration
    {
        public float EulerAngleMax { get; set; }
        public float WP7TiltMax { get; set; }
        public float PhoneTiltThreshold { get; set; }
        public float YawSpeed { get; set; }
        public float VerticalSpeed { get; set; }
        public float MaxAltitude { get; set; }
        public AppConfiguration()
        {
            EulerAngleMax = 10;
            WP7TiltMax = 30;
            PhoneTiltThreshold = 5;
            YawSpeed = 15;
            VerticalSpeed = 500;
            MaxAltitude = 2000;
        }
    }
}
