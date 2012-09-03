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

namespace DroneControllerApp
{
    public partial class ConfigPage : PhoneApplicationPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (!(e.Orientation == PageOrientation.LandscapeRight))
            {
                base.OnOrientationChanged(e);
            }
        }

        private void droneTilt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            droneTilt.Value = Math.Round(droneTilt.Value);
            droneTiltLabel.Text = string.Format("AR.Drone Tilt Max: {0}°",droneTilt.Value);
        }

        private void phoneTilt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            phoneTilt.Value = Math.Round(phoneTilt.Value);
            phoneTiltLabel.Text = string.Format("Phone Tilt Max: {0}°", phoneTilt.Value);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            droneTilt.Value = (App.Current as App).Config.EulerAngleMax;
            phoneTilt.Value = (App.Current as App).Config.WP7TiltMax;
            phoneTiltThreshold.Value = (App.Current as App).Config.PhoneTiltThreshold;
            YawSpeed.Value = (App.Current as App).Config.YawSpeed;
            VerticalSpeed.Value = (App.Current as App).Config.VerticalSpeed;
            MaxAltitude.Value = (App.Current as App).Config.MaxAltitude;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            (App.Current as App).Config.EulerAngleMax = (float)droneTilt.Value;
            (App.Current as App).Config.WP7TiltMax = (float)phoneTilt.Value;
            (App.Current as App).Config.PhoneTiltThreshold = (float)phoneTiltThreshold.Value;
            (App.Current as App).Config.YawSpeed = (float)YawSpeed.Value;
            (App.Current as App).Config.VerticalSpeed = (float)VerticalSpeed.Value;
            (App.Current as App).Config.MaxAltitude = (float)MaxAltitude.Value;
            base.OnNavigatedFrom(e);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/StartPage.xaml", UriKind.Relative));
        }

        private void phoneTiltThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            phoneTiltThreshold.Value = Math.Round(phoneTiltThreshold.Value);
            phoneTiltThresholdLabel.Text = string.Format("Phone Tilt Threshold: {0}°", phoneTiltThreshold.Value);
        }

        private void YawSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            YawSpeed.Value = Math.Round(YawSpeed.Value);
            YawSpeedLabel.Text = string.Format("Yaw Speed: {0}°/s", YawSpeed.Value);
        }

        private void VerticalSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VerticalSpeed != null)
                VerticalSpeed.Value = Math.Round(VerticalSpeed.Value);
            if (VerticalSpeedLabel != null)
                VerticalSpeedLabel.Text = string.Format("Vertical Speed: {0}mm/s", VerticalSpeed.Value);
        }

        private void MaxAltitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MaxAltitude != null)
                MaxAltitude.Value = Math.Round(MaxAltitude.Value);
            if (MaxAltitudeLabel != null)
                MaxAltitudeLabel.Text = string.Format("Max Altitude: {0}mm", MaxAltitude.Value);
        }
    }
}