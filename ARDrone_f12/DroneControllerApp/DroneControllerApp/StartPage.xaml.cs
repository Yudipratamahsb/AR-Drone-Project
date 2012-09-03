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
    public partial class StartPage : PhoneApplicationPage
    {
        public StartPage()
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

        private void FlyButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ControlPage.xaml", UriKind.Relative));
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ConfigPage.xaml", UriKind.Relative));
        }

        private void AutoNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AutoNavPage.xaml", UriKind.Relative));
        }

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            Uri website = new Uri("http://students.cse.tamu.edu/jabam89/wp7drone/");
            var task = new Microsoft.Phone.Tasks.WebBrowserTask
            {
                Uri = website
            };
            task.Show();
        }
    }
}