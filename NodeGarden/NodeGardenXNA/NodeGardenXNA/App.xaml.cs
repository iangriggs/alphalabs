//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Studio Arcade Ltd">
// Copyright © Studio Arcade Ltd 2012.
// All rights reserved.
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// This code is made available under the Ms-PL or GPL as appropriate.
// Please see LICENSE.txt for more details
// </copyright>
// <Author>Matt Lacey</Author>
//-----------------------------------------------------------------------

namespace NodeGardenXNA
{
    using System;
    using System.Windows;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// The XNA application
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Avoid double-initialization
        /// </summary>
        private bool phoneApplicationInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            this.UnhandledException += this.Application_UnhandledException;

            // Standard Silverlight initialization
            this.InitializeComponent();

            // Phone-specific initialization
            this.InitializePhoneApplication();

            // XNA initialization
            this.InitializeXnaApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                ////Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                ////Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                ////PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            // Run under lock & prevent lock from being triggered
            // For demo purposes we don't want lock to kick in
            // But it'll be nice if changes on other devices are reflected while locked
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        /// <summary>
        /// Gets the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Gets access to a ContentManager for the application.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Gets access to a GameTimer that is set up to pump the FrameworkDispatcher.
        /// </summary>
        public GameTimer FrameworkDispatcherTimer { get; private set; }

        /// <summary>
        /// Gets access to the AppServiceProvider for the application.
        /// </summary>
        public AppServiceProvider Services { get; private set; }

        /// <summary>
        /// Handles the Launching event of the Application control.
        /// Code to execute when the application is launching (e.g., from Start)
        /// This code will not execute when the application is reactivated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Phone.Shell.LaunchingEventArgs"/> instance containing the event data.</param>
        private void ApplicationLaunching(object sender, LaunchingEventArgs e)
        {
        }

        /// <summary>
        /// Handles the Activated event of the Application control.
        /// Code to execute when the application is activated (brought to foreground)
        /// This code will not execute when the application is first launched
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Phone.Shell.ActivatedEventArgs"/> instance containing the event data.</param>
        private void ApplicationActivated(object sender, ActivatedEventArgs e)
        {
        }

        /// <summary>
        /// Applications the deactivated.
        /// Code to execute when the application is deactivated (sent to background)
        /// This code will not execute when the application is closing
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Phone.Shell.DeactivatedEventArgs"/> instance containing the event data.</param>
        private void ApplicationDeactivated(object sender, DeactivatedEventArgs e)
        {
        }

        /// <summary>
        /// Applications the closing.
        /// Code to execute when the application is closing (e.g., user hit Back)
        /// This code will not execute when the application is deactivated
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Phone.Shell.ClosingEventArgs"/> instance containing the event data.</param>
        private void ApplicationClosing(object sender, ClosingEventArgs e)
        {
        }

        /// <summary>
        /// Handles the NavigationFailed event of the RootFrame control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.NavigationFailedEventArgs"/> instance containing the event data.</param>
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Handles the UnhandledException event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.ApplicationUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        /// <summary>
        /// Initializes the phone application.
        /// Do not add any additional code to this method
        /// </summary>
        private void InitializePhoneApplication()
        {
            if (this.phoneApplicationInitialized)
            {
                return;
            }

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            this.RootFrame = new PhoneApplicationFrame();
            this.RootFrame.Navigated += this.CompleteInitializePhoneApplication;

            // Handle navigation failures
            this.RootFrame.NavigationFailed += this.RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            this.phoneApplicationInitialized = true;
        }

        /// <summary>
        /// Completes the initialize phone application.
        /// Do not add any additional code to this method
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.NavigationEventArgs"/> instance containing the event data.</param>
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (this.RootVisual != this.RootFrame)
            {
                this.RootVisual = this.RootFrame;
            }

            // Remove this handler since it is no longer needed
            this.RootFrame.Navigated -= this.CompleteInitializePhoneApplication;
        }

        #endregion

        #region XNA application initialization

        /// <summary>
        /// Performs initialization of the XNA types required for the application.
        /// </summary>
        private void InitializeXnaApplication()
        {
            // Create the service provider
            this.Services = new AppServiceProvider();

            // Add the SharedGraphicsDeviceManager to the Services as the IGraphicsDeviceService for the app
            foreach (object obj in this.ApplicationLifetimeObjects)
            {
                if (obj is IGraphicsDeviceService)
                {
                    this.Services.AddService(typeof(IGraphicsDeviceService), obj);
                }
            }

            // Create the ContentManager so the application can load precompiled assets
            this.Content = new ContentManager(this.Services, "Content");

            // Create a GameTimer to pump the XNA FrameworkDispatcher
            this.FrameworkDispatcherTimer = new GameTimer();
            this.FrameworkDispatcherTimer.FrameAction += this.FrameworkDispatcherFrameAction;
            this.FrameworkDispatcherTimer.Start();
        }

        /// <summary>
        /// An event handler that pumps the FrameworkDispatcher each frame.
        /// FrameworkDispatcher is required for a lot of the XNA events and
        /// for certain functionality such as SoundEffect playback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void FrameworkDispatcherFrameAction(object sender, EventArgs e)
        {
            FrameworkDispatcher.Update();
        }

        #endregion
    }
}