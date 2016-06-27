// Debugger Suspend button fail test code.
// This app demonstrates the debuggers failure to suspend a System.Threading.Timer when the App-Lifecycle Suspend button is pressed.
// The non-indented comments show where the changes from the default blank app are.
// All changes are in this file.
namespace DebugSuspendBug
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Microsoft.ApplicationInsights;

    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
//Timer object.
        private Timer _systemThreadingTimer;
        private DispatcherTimer _dispatcherTimer;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
        }

// A simple toast method.
        private void Toast(string text)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
            toastXml.GetElementsByTagName("text")[0].AppendChild(toastXml.CreateTextNode(text));
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
//Timer that runs when the app is suspended by the debugger button but not when suspended when run from the start menu.
            _systemThreadingTimer = new Timer(x =>
            {
                var s = $"ThreadTimer: {DateTime.Now.Second.ToString()}";
                Toast(s);
                Debug.WriteLine(s);
            }, null, 0, 2000);
//A DispatcherTimer that does the same thing. This will suspend properly when the debugger suspend button is pressed.
            _dispatcherTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            _dispatcherTimer.Tick += ((x,y) =>
            {
                var s = $"DispatcherTimer: {DateTime.Now.Second.ToString()}";
                Toast(s);
                Debug.WriteLine(s);
            });
            _dispatcherTimer.Start();

            #region unmodified-code

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            #endregion
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
//Toast to make it obvious that the app is being suspended
            Toast("SuspendingNow");
            deferral.Complete();
        }
    }
}