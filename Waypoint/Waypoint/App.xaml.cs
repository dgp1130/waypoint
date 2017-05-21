using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Waypoint
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Start at MainPage, using it as base navigation
            MainPage = new NavigationPage(new Waypoint.MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
