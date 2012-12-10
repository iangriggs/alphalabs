//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenHtml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    using Microsoft.Phone.Controls;

    using NodeGardenLib;

    /// <summary>
    /// Page hosting the web version of the NodeGarden
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Looks after the garden, detects events and manages communication between gardens/devices
        /// </summary>
        private Gardener gardener;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random rand = new Random();

        /// <summary>
        /// The default nodes
        /// </summary>
        private List<Node> defaultNodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.StartButton.IsEnabled = false;

            this.DisplayedVersionNumber.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var wbh = new WebBrowserHelper(this.embeddedBrowser) { ScrollDisabled = true };
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.gardener != null)
            {
                this.gardener.Reconnect();
            }

            base.OnNavigatedTo(e);

            if (this.gardener == null)
            {
                this.embeddedBrowser.LoadCompleted += (sender, args) => Dispatcher.BeginInvoke(
                    () =>
                        {
                            this.StartButton.IsEnabled = true;
                        });

                var content = this.LoadHtmlContent();

                this.embeddedBrowser.NavigateToString(content);
            }
        }

        /// <summary>
        /// Called just before a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (this.gardener != null)
            {
                this.gardener.Dispose();
            }
        }

        /// <summary>
        /// Loads the HTML content.
        /// </summary>
        /// <returns>the HTML</returns>
        private string LoadHtmlContent()
        {
            var s = Application.GetResourceStream(new Uri("/NodeGardenHtml;component/NodeGarden.html", UriKind.Relative)).Stream;

            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// The start button was clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.InitializeAndConfigureGardener();

            this.DebugConfigOptions.Visibility = Visibility.Collapsed;

            this.CreateDefaultNodes(Convert.ToInt32(Math.Floor(this.AdditionalNodesCount.Value)));
        }

        /// <summary>
        /// Initializes and configures the gardener.
        /// </summary>
        private void InitializeAndConfigureGardener()
        {
            this.gardener = new Gardener();

            var comm = CommType.Udp;

            if (this.WebComms.IsChecked ?? false)
            {
                comm = CommType.Web;
            }

            var settings = new GardenerSettings
            {
                CommType = comm,
                EnableColorDetection = false,
                EnableImageDetection = false,
                EnableShakeDetection = this.EnableShakeDetection.IsChecked ?? false,
                EnableNoiseDetection = this.EnableNoiseDetection.IsChecked ?? false,
                NoiseThreshold = 1500,
                NoiseDuration = 2,
            };

            this.gardener.Initialize(settings);

            this.gardener.OnNodeChanged += nodeId => this.Dispatcher.BeginInvoke(() =>
            {
                var node = this.gardener.Nodes.FirstOrDefault(n => n.Id == nodeId);

                if (node != null)
                {
                    this.TellJsAboutNode(node);
                }
            });

            this.gardener.OnNoiseDetected += () => Dispatcher.BeginInvoke(() => this.embeddedBrowser.InvokeScript("NoiseDetected"));
            this.gardener.OnShakeDetected += () => Dispatcher.BeginInvoke(() => this.embeddedBrowser.InvokeScript("ShakeDetected"));

            var x = this.rand.Next(50, 430);
            var y = this.rand.Next(50, 750);

            this.gardener.AddSelfNode(x, y);
            this.gardener.WhereIsEveryBody();

            this.TellJsAboutAccentColor();
            this.TellJsAboutNode(this.gardener.Nodes.FirstOrDefault(n => n.NodeType == TypeOfNode.Self));
        }

        /// <summary>
        /// Creates the default nodes.
        /// </summary>
        /// <param name="numberOfNodes">The number of nodes.</param>
        private void CreateDefaultNodes(int numberOfNodes)
        {
            if (numberOfNodes > 0)
            {
                this.defaultNodes = new List<Node>(numberOfNodes);

                for (int i = 0; i < numberOfNodes; i++)
                {
                    var node = new Node(
                                        Guid.NewGuid().ToString(),
                                        this.rand.Next(50, 430),
                                        this.rand.Next(50, 750));
                    node.NodeType = TypeOfNode.Default;

                    this.defaultNodes.Add(node);

                    this.TellJsAboutNode(node);
                }

                var simulateMovementOfDefaultNodesTimer = new DispatcherTimer();
                simulateMovementOfDefaultNodesTimer.Tick += (sender, args) =>
                    {
                        if (this.rand.Next(1, 5) == 1)
                        {
                            var newX = this.rand.Next(50, 430);
                            var newY = this.rand.Next(50, 750);
                            var index = this.rand.Next(0, this.defaultNodes.Count);

                            this.defaultNodes[index].X = newX;
                            this.defaultNodes[index].Y = newY;

                            this.TellJsAboutNode(this.defaultNodes[index]);
                        }
                    };
                simulateMovementOfDefaultNodesTimer.Interval = TimeSpan.FromMilliseconds(300);
                simulateMovementOfDefaultNodesTimer.Start();
            }
        }

        /// <summary>
        /// Tell java script on page about accent color.
        /// </summary>
        private void TellJsAboutAccentColor()
        {
            var color = (Color)Application.Current.Resources["PhoneAccentColor"];

            this.embeddedBrowser.InvokeScript(
                "SetAccentColor",
                color.R.ToString(CultureInfo.InvariantCulture),
                color.G.ToString(CultureInfo.InvariantCulture),
                color.B.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Tell java script on page about a node.
        /// </summary>
        /// <param name="node">The node.</param>
        private void TellJsAboutNode(Node node)
        {
            this.embeddedBrowser.InvokeScript(
                "UpdateNode",
                node.Id,
                node.NodeType.ToString(),
                node.X.ToString(CultureInfo.InvariantCulture),
                node.Y.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Called when a script in the browser notifies back to the page
        /// </summary>
        /// <param name="sender">The sender (web browser).</param>
        /// <param name="e">The <see cref="NotifyEventArgs" /> instance containing the event data.</param>
        private void ScriptNotifying(object sender, NotifyEventArgs e)
        {
            var values = e.Value.Split(':');

            if (values[0] == "DEBUG")
            {
                System.Diagnostics.Debug.WriteLine(values[1]);
            }
            else
            {
                this.gardener.UpdateSelfNodePosition(double.Parse(values[0]), double.Parse(values[1]));
            }
        }
    }
}