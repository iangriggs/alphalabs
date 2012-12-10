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
// <Author>Laurie Brown</Author>
//-----------------------------------------------------------------------

namespace NodeGardenSl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    using Microsoft.Phone.Controls;

    using NodeGardenLib;

    /// <summary>
    /// The page displaying the node garden
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Maximum value for the line width, based on connectedness
        /// </summary>
        private const float StrokeWeightMin = 1.0f;

        /// <summary>
        /// Minimum value for the line width, based on connectedness
        /// </summary>
        private const float StrokeWeightMax = 7.0f;

        /// <summary>
        /// minimum distance between 2 nodes for a line to be drawn between the 2
        /// </summary>
        private const int MinDist = 300;

        /// <summary>
        /// Number of lines created
        /// </summary>
        private const int NumberOfAvailableLines = 150;

        /// <summary>
        /// Timer used to trigger periodic node movement
        /// </summary>
        private readonly DispatcherTimer nodeMovementTimer = new DispatcherTimer();

        /// <summary>
        /// Looks after the garden, detects events and manages communication between gardens/devices
        /// </summary>
        private Gardener gardener;

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random rand = new Random();

        /// <summary>
        /// All the nodes used in the visualization
        /// </summary>
        private List<VisualNode> nodes;

        /// <summary>
        /// All the lines used to connect nodes
        /// </summary>
        private List<Line> lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.DisplayedVersionNumber.Text = Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
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
        /// The start button was clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.gardener = new Gardener();

            var comm = CommType.Udp;

            if (this.WebComms.IsChecked ?? false)
            {
                comm = CommType.Web;
            }

            // Define the way the gardener should behave
            var settings = new GardenerSettings
            {
                CommType = comm,
                EnableColorDetection = this.EnableColorDetection.IsChecked ?? false,
                EnableImageDetection = this.EnableImageDetection.IsChecked ?? false,
                EnableShakeDetection = this.EnableShakeDetection.IsChecked ?? false,
                EnableNoiseDetection = this.EnableNoiseDetection.IsChecked ?? false,
                NoiseThreshold = 1500,
                NoiseDuration = 2,
                ColorToDetect = Colors.Blue,
                ColorDetectionThreshold = 40,

                // These two properties must be set like this for the camera to be
                // able to be used to do color and image detection
                ViewFinderBrush = this.viewfinderBrush,
                ViewFinderBrushTransform = this.viewfinderBrushTransformation,
            };

            this.gardener.Initialize(settings);

            this.AddHandlersForDetectedEvents();

            // Handle notification, from another device, that a node has been added/changed
            this.gardener.OnNodeChanged += nodeId => this.Dispatcher.BeginInvoke(() =>
            {
                var gardenerNode = this.gardener.Nodes.FirstOrDefault(n => n.Id == nodeId);

                var visualNode = this.nodes.FirstOrDefault(n => n.Id == nodeId);

                if (gardenerNode != null)
                {
                    if (visualNode == null)
                    {
                        var vn = new VisualNode(rand, MainCanvas)
                            {
                                X = gardenerNode.X,
                                Y = gardenerNode.Y,
                                Tag = gardenerNode.Tag,
                                Id = gardenerNode.Id,
                                DisableVirtualMovement = true
                            };

                        this.nodes.Add(vn);
                    }
                    else
                    {
                        var idx = this.nodes.IndexOf(visualNode);

                        this.nodes[idx].X = gardenerNode.X;
                        this.nodes[idx].Y = gardenerNode.Y;
                        this.nodes[idx].Tag = gardenerNode.Tag;
                    }
                }
                else
                {
                    if (visualNode != null)
                    {
                        visualNode.Remove(this.MainCanvas);
                        this.nodes.Remove(visualNode);
                    }
                }
            });

            this.ConfigureOtherOptions();

            this.nodes = new List<VisualNode>();

            this.CreateOwnNode();

            this.gardener.WhereIsEveryBody();

            this.CreateDefaultNodes(Convert.ToInt32(Math.Floor(this.AdditionalNodesCount.Value)));

            this.CreateLines();

            this.nodeMovementTimer.Interval = TimeSpan.FromMilliseconds(10);
            this.nodeMovementTimer.Tick += this.UpdateDisplay;
            this.nodeMovementTimer.Start();
        }

        /// <summary>
        /// Creates the default nodes.
        /// </summary>
        /// <param name="numberOfNodes">The number of nodes.</param>
        private void CreateDefaultNodes(int numberOfNodes)
        {
            for (int i = 0; i < numberOfNodes; i++)
            {
                this.nodes.Add(new VisualNode(this.rand, MainCanvas));
            }
        }

        /// <summary>
        /// Creates the lines.
        /// </summary>
        private void CreateLines()
        {
            this.lines = new List<Line>(NumberOfAvailableLines);

            for (int i = 0; i < NumberOfAvailableLines; i++)
            {
                this.lines.Add(new Line());
                this.lines[i].StrokeThickness = 2.5f;
                this.lines[i].Stroke = new SolidColorBrush(Colors.Black);
                this.lines[i].X1 = 50;
                this.lines[i].Y1 = (i * 7) % 720;
                this.lines[i].X2 = 400;
                this.lines[i].Y2 = (i * 5) % 720;

                MainCanvas.Children.Add(this.lines[i]);
            }
        }

        /// <summary>
        /// Updates the display.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void UpdateDisplay(object sender, EventArgs e)
        {
            int currentLine = 0;

            // hide lines in case they aren't used
            foreach (var t in this.lines)
            {
                t.X1 = -1;
                t.Y1 = -1;
                t.X2 = -1;
                t.Y2 = -1;
            }

            for (int i = 0; i < this.nodes.Count; ++i)
            {
                // update the node's position
                this.nodes[i].Update(MainCanvas.RenderSize);

                for (int j = 0; j < this.nodes.Count; ++j)
                {
                    if (i != j)
                    {
                        // calculate the distance between each 2 nodes
                        float distance =
                            (float)Math.Sqrt(
                                Math.Pow(this.nodes[j].CurrentY - this.nodes[i].CurrentY, 2)
                                + Math.Pow(this.nodes[j].CurrentX - this.nodes[i].CurrentX, 2));

                        // if distance is within the threshold
                        if (distance < MinDist)
                        {
                            // add a mapped value between 1-0 to each node's connectedness value
                            float connectedness = VisualNode.Map(distance, 0, MinDist, 1, 0);
                            this.nodes[i].Connectedness += connectedness;
                            this.nodes[j].Connectedness += connectedness;

                            if (distance > Math.Max(this.nodes[i].NodeSize, this.nodes[j].NodeSize))
                            {
                                // draw a line between 2 nodes. The thickness/alpha varies depending on distance
                                this.lines[currentLine].StrokeThickness = VisualNode.Map(distance, 0, MinDist, StrokeWeightMax, StrokeWeightMin);
                                this.lines[currentLine].Stroke = new SolidColorBrush(Color.FromArgb((byte)VisualNode.Map(distance, 0, MinDist, 255, 0), 255, 255, 255));
                                this.lines[currentLine].X1 = this.nodes[i].CurrentX;
                                this.lines[currentLine].Y1 = this.nodes[i].CurrentY;
                                this.lines[currentLine].X2 = this.nodes[j].CurrentX;
                                this.lines[currentLine].Y2 = this.nodes[j].CurrentY;

                                // switch the currently used line in the pool
                                currentLine = (currentLine + 1) % NumberOfAvailableLines;
                            }
                        }
                    }
                }

                // draw the node using the Connectedness
                this.nodes[i].Draw();
            }
        }

        /// <summary>
        /// Configures the other options defined before starting the app.
        /// </summary>
        private void ConfigureOtherOptions()
        {
            // This is commented as it relates to a UIElement that is commented out on the page
            ////if (this.ShowVisualDebugger.IsChecked ?? false)
            ////{
            ////    this.CameraView.Opacity = 1;
            ////}

            this.DebugConfigOptions.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Adds the handlers for detected events.
        /// </summary>
        private void AddHandlersForDetectedEvents()
        {
            this.gardener.OnShakeDetected += () => this.Dispatcher.BeginInvoke(
                () =>
                {
                    // This is where you could do something in response to a shake being detected
                });

            this.gardener.OnNoiseDetected += () => this.Dispatcher.BeginInvoke(
                () =>
                {
                    // This is where you could do something in response to a noise being detected
                });

            this.gardener.OnColorDetected += () => this.Dispatcher.BeginInvoke(
                () =>
                {
                    // This is where you could do something in response to a colour being detected with the camera
                });

            this.gardener.OnImageDetected += img => this.Dispatcher.BeginInvoke(
                () =>
                {
                    // This is where you could do something in response to an image being detected
                });
        }

        /// <summary>
        /// Creates the node that can primarily be interacted with and will be reflected on other devices.
        /// </summary>
        private void CreateOwnNode()
        {
            var myNode = new MyVisualNode(this.rand, MainCanvas, this.gardener);

            this.gardener.AddSelfNode(myNode.X, myNode.Y);

            myNode.Id = this.gardener.GetSelfNodeId();

            this.nodes.Add(myNode);
        }
    }
}