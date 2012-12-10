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
// <Author>Laurie Brown</Author>
// <Author>Matt Lacey</Author>
//-----------------------------------------------------------------------

namespace NodeGardenDirect3D
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using Microsoft.Phone.Controls;
    using NodeGardenDirect3DComp;
    using NodeGardenLib;

    public partial class MainPage : PhoneApplicationPage
    {
        private Direct3DInterop m_d3dInterop = new Direct3DInterop();

        private Gardener gardener;

        /// <summary>
        /// An integer represtation of each nodeId
        /// </summary>
        public Dictionary<string, int> NativeManagedNodeIdMap = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            this.DisplayedVersionNumber.Text = Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
        }

        private void DrawingSurface_Loaded(object sender, RoutedEventArgs e)
        {
            // Set window bounds in dips
            m_d3dInterop.WindowBounds = new Windows.Foundation.Size(
                (float)DrawingSurface.ActualWidth,
                (float)DrawingSurface.ActualHeight
                );

            // Set native resolution in pixels
            m_d3dInterop.NativeResolution = new Windows.Foundation.Size(
                (float)Math.Floor(DrawingSurface.ActualWidth * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f),
                (float)Math.Floor(DrawingSurface.ActualHeight * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f)
                );

            // Set render resolution to the full native resolution
            m_d3dInterop.RenderResolution = m_d3dInterop.NativeResolution;

            // Hook-up native component to DrawingSurface
            DrawingSurface.SetContentProvider(m_d3dInterop.CreateContentProvider());
            DrawingSurface.SetManipulationHandler(m_d3dInterop);

            DispatcherTimer _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromMilliseconds(100);

            _timer.Tick += new EventHandler(delegate(object s, EventArgs ev)
            {
                if (gardener != null && m_d3dInterop != null)
                {
                    try
                    {
                        var pos = m_d3dInterop.GetMyNodePosition();
                        this.gardener.UpdateSelfNodePosition(pos.X, pos.Y);
                    }
                    catch (AccessViolationException)
                    {
                        // Can happen when closing the app
                    }
                }
            });

            _timer.Start();
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.gardener != null)
            {
                this.gardener.Reconnect();
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (this.gardener != null)
            {
                this.gardener.Dispose();
            }

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Configures the gardener.
        /// </summary>
        private void ConfigureGardener()
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

                // Not specifying the other options for camera related detection
                // If wanting to add them a videobrush must be added to the page also
                // - See the Silverlight project for an example
                EnableColorDetection = false,
                EnableImageDetection = false,
                EnableShakeDetection = this.EnableShakeDetection.IsChecked ?? false,
                EnableNoiseDetection = this.EnableNoiseDetection.IsChecked ?? false,
                NoiseThreshold = 1500,
                NoiseDuration = 2,
            };

            this.gardener.Initialize(settings);

            this.AddHandlersForDetectedEvents();

            // Handle notification, from another device, that a node has been added/changed
            this.gardener.OnNodeChanged += nodeId => this.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    var gardenerNode = this.gardener.Nodes.FirstOrDefault(n => n.Id == nodeId);

                    var nativeId = this.GetNodeIdInt(nodeId);

                    if (gardenerNode != null)
                    {
                        if (nativeId < 0)
                        {
                            nativeId = m_d3dInterop.CreateNode((float)gardenerNode.X, (float)gardenerNode.Y);
                            this.MapNodeIds(gardenerNode.Id, nativeId);
                        }
                        else
                        {
                            m_d3dInterop.UpdateNodePosition(nativeId, (float)gardenerNode.X, (float)gardenerNode.Y);
                        }
                    }
                    else
                    {
                        if (nativeId >= 0)
                        {
                            m_d3dInterop.RemoveNode(nativeId);
                            this.NativeManagedNodeIdMap.Remove(nodeId);
                        }
                    }
                }));
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
        }

        /// <summary>
        /// Start button on debug pane is clicked
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments<see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.ConfigureGardener();

            Windows.Foundation.Point nodePos = m_d3dInterop.CreateMyNode();

            this.gardener.AddSelfNode(nodePos.X, nodePos.Y);

            this.gardener.WhereIsEveryBody();

            var initialNumberOfNodes = 1 + Convert.ToInt32(Math.Floor(this.AdditionalNodesCount.Value));

            m_d3dInterop.CreateNodes(initialNumberOfNodes);

            this.DebugConfigOptions.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Adds integer version of ids to dictionary
        /// </summary>
        /// <param name="managedNodeId">The managed node id.</param>
        /// <param name="nativeNodeId">The native node id.</param>
        public void MapNodeIds(string managedNodeId, int nativeNodeId)
        {
            if (!this.NativeManagedNodeIdMap.ContainsKey(managedNodeId))
            {
                this.NativeManagedNodeIdMap.Add(managedNodeId, nativeNodeId);
            }
        }

        /// <summary>
        /// Gets integer version of Id
        /// </summary>
        /// <param name="nodeId">The managed/string version of the node id.</param>
        /// <returns>Native Node Id</returns>
        public int GetNodeIdInt(string nodeId)
        {
            return this.NativeManagedNodeIdMap.ContainsKey(nodeId) ? this.NativeManagedNodeIdMap[nodeId]
                                                                   : -1;
        }
    }
}