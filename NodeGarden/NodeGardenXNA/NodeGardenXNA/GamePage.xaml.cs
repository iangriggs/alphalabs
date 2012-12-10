//-----------------------------------------------------------------------
// <copyright file="GamePage.xaml.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenXNA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Navigation;

    using Microsoft.Phone.Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    using NodeGardenLib;

    /// <summary>
    /// The page showing the XNA version of the simple node garden
    /// </summary>
    public partial class GamePage : PhoneApplicationPage
    {
        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly Random rand = new Random();

        /// <summary>
        /// The content manager
        /// </summary>
        private readonly ContentManager contentManager;

        /// <summary>
        /// Timer managing the game loop
        /// </summary>
        private readonly GameTimer timer;

        /// <summary>
        /// Looks after the garden, detects events and manages communication between gardens/devices
        /// </summary>
        private Gardener gardener;

        /// <summary>
        /// The sprite batch
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// All the nodes used in the visualization
        /// </summary>
        private List<MovingNode> nodes;

        /// <summary>
        /// all the connections used
        /// </summary>
        private List<Connection> connections;

        /// <summary>
        /// the currently selected line
        /// </summary>
        private int currentLine = 0;

        /// <summary>
        /// The node image
        /// </summary>
        private Texture2D circleTex;

        /// <summary>
        /// The line image
        /// </summary>
        private Texture2D lineTex;

        /// <summary>
        /// Renderer for Silverlight elements
        /// </summary>
        private UIElementRenderer elementRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePage"/> class.
        /// </summary>
        public GamePage()
        {
            this.InitializeComponent();

            // Get the content manager from the application
            this.contentManager = (Application.Current as App).Content;

            this.LayoutUpdated += this.GamePage_LayoutUpdated;

            // Create a timer for this page
            this.timer = new GameTimer();
            this.timer.UpdateInterval = TimeSpan.FromTicks(333333);
            this.timer.Update += this.OnUpdate;
            this.timer.Draw += this.OnDraw;

            this.DisplayedVersionNumber.Text = Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0];
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

            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);

            this.circleTex = this.contentManager.Load<Texture2D>("node");
            this.lineTex = this.contentManager.Load<Texture2D>("line");

            this.nodes = new List<MovingNode>();

            this.connections = new List<Connection>();

            // Start the timer
            this.timer.Start();

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            this.timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            if (this.gardener != null)
            {
                this.gardener.Dispose();
            }

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the GamePage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GamePage_LayoutUpdated(object sender, EventArgs e)
        {
            // make sure page size is valid
            if (this.ActualWidth == 0 || this.ActualHeight == 0)
            {
                return;
            }

            // see if we already have the right sized renderer
            if (this.elementRenderer != null &&
                this.elementRenderer.Texture != null &&
                this.elementRenderer.Texture.Width == (int)ActualWidth &&
                this.elementRenderer.Texture.Height == (int)ActualHeight)
            {
                return;
            }

            // dispose the current renderer
            if (this.elementRenderer != null)
            {
                this.elementRenderer.Dispose();
            }

            // create the renderer
            this.elementRenderer = new UIElementRenderer(this, (int)ActualWidth, (int)ActualHeight);
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
            this.gardener.OnNodeChanged += nodeId => this.Dispatcher.BeginInvoke(
                () =>
                {
                    var gardenerNode = this.gardener.Nodes.FirstOrDefault(n => n.Id == nodeId);

                    var visualNode = this.nodes.FirstOrDefault(n => n.Id == nodeId);

                    if (gardenerNode != null)
                    {
                        if (visualNode == null)
                        {
                            var on = new ShadowNode(
                                rand,
                                this.circleTex,
                                new Microsoft.Xna.Framework.Point(
                                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                                    GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height));

                            on.X = gardenerNode.X;
                            on.Y = gardenerNode.Y;
                            on.Tag = gardenerNode.Tag;
                            on.Id = gardenerNode.Id;
                            on.DisableVirtualMovement = true;

                            this.nodes.Add(on);
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
                            this.nodes.Remove(visualNode);
                        }
                    }
                });
        }

        /// <summary>
        /// Adds the handlers for detected events.
        /// </summary>
        private void AddHandlersForDetectedEvents()
        {
            this.gardener.OnShakeDetected += () => this.Dispatcher.BeginInvoke(
                () =>
                {
                    Ping();
                });

            this.gardener.OnNoiseDetected += () => this.Dispatcher.BeginInvoke(
                () =>
                {
                    Ping();
                });
        }

        private void Ping()
        {
            var selfNode = this.nodes.FirstOrDefault(n => n.NodeType == TypeOfNode.Self);
            selfNode.CreateRipple();
            var tag = new PingTag { AccentColour = selfNode.AccentColour, Ping = true };
            gardener.UpdateSelfNodePosition(selfNode.X, selfNode.Y, tag.Serialize());
            gardener.ClearSelfPing();
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Xna.Framework.GameTimerEventArgs"/> instance containing the event data.</param>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            this.currentLine = 0;

            for (var i = 0; i < this.nodes.Count; ++i)
            {
                // update the node's position
                this.nodes[i].Update();

                for (var j = i + 1; j < this.nodes.Count; ++j)
                {
                    // calculate the distance between each 2 nodes
                    var distance = Vector2.Distance(this.nodes[i].CurrentPosition, this.nodes[j].CurrentPosition);

                    // if distance is within the threshold
                    if (distance < Global.MinDist)
                    {
                        // add a mapped value between 1-0 to each node's connectedness value
                        var connectedness = Global.Map(distance, 0, Global.MinDist, 1, 0);
                        this.nodes[i].ApplyConnection(connectedness, this.nodes[j]);
                        this.nodes[j].ApplyConnection(connectedness, this.nodes[i]);

                        if (this.currentLine < Global.NumberOfLines)
                        {
                            this.connections[this.currentLine++].FormConnection(this.nodes[i], this.nodes[j], distance);
                        }
                    }
                }

                this.nodes[i].FinishConnection();
            }

            if (this.currentLine < this.connections.Count)
            {
                for (int i = this.currentLine; i < Global.NumberOfLines; i++)
                {
                    this.connections[i].BreakConnection(null, null);
                }
            }
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Xna.Framework.GameTimerEventArgs"/> instance containing the event data.</param>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.Black);

            // begin drawing sprites
            this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            // draw all nodes
            foreach (var node in this.nodes)
            {
                node.Draw(this.spriteBatch);
            }

            // draw all connections
            foreach (var con in this.connections)
            {
                con.Draw(this.spriteBatch);
            }

            this.elementRenderer.Render();
            this.spriteBatch.Draw(this.elementRenderer.Texture, Vector2.Zero, Color.White);

            // end drawing sprites
            this.spriteBatch.End();
        }

        /// <summary>
        /// Starts the clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.ConfigureGardener();

            // add one controllable node at the beginning of the collection
            var myNode = new MyNode(
                this.rand,
                this.circleTex,
                new Microsoft.Xna.Framework.Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height),
                this.gardener);

            this.gardener.AddSelfNode(myNode.X, myNode.Y);

            myNode.Id = this.gardener.GetSelfNodeId();

            this.nodes.Add(myNode);

            this.gardener.UpdateSelfNodePosition(myNode.X, myNode.Y, myNode.Tag, true);

            this.gardener.WhereIsEveryBody();

            Global.InitialNumberOfNodes = 1 + Convert.ToInt32(Math.Floor(this.AdditionalNodesCount.Value));

            for (int i = 1; i < Global.InitialNumberOfNodes; i++)
            {
                this.nodes.Add(
                    new ShadowNode(
                        this.rand,
                        this.circleTex,
                        new Microsoft.Xna.Framework.Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)));
            }

            for (int i = 0; i < Global.NumberOfLines; i++)
            {
                this.connections.Add(new LineConnection(this.lineTex));
            }

            this.DebugConfigOptions.Visibility = Visibility.Collapsed;
        }
    }
}