//-----------------------------------------------------------------------
// <copyright file="CommManager.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenLib
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows;

    using SignalR.Client.Hubs;

    /// <summary>
    /// Manages communication between application instances / devices
    /// </summary>
    internal class CommManager : IDisposable
    {
        /// <summary>
        /// IP Address used for communication
        /// </summary>
        private const string GroupAddress = "224.224.224.224";

        /// <summary>
        /// Port used for communication
        /// </summary>
        private const int GroupPort = 54545;

        /// <summary>
        /// The address of the SignalR server
        /// </summary>
        private const string WebAddress = "http://nodegarden.azurewebsites.net/";

        /// <summary>
        /// Actual Communication Manager instance
        /// </summary>
        private static CommManager commMgr;

        /// <summary>
        /// The type of communication being used
        /// </summary>
        private CommType commType;

        /// <summary>
        /// SignalR connection
        /// </summary>
        private HubConnection hubConnection;

        /// <summary>
        /// Interface for communicating with SignalR
        /// </summary>
        private IHubProxy signalRHub;

        /// <summary>
        /// Track whether a web connection has been made
        /// </summary>
        private bool webConnectionMade;

        /// <summary>
        /// Track own Id
        /// </summary>
        private string selfNodeId;

        /// <summary>
        /// The last X value sent
        /// </summary>
        private double lastXSent;

        /// <summary>
        /// The last Y value sent
        /// </summary>
        private double lastYSent;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommManager"/> class.
        /// Prevents a default instance of the <see cref="CommManager"/> class from being created.
        /// </summary>
        private CommManager()
        {
        }

        /// <summary>
        /// Event raised when a notification is received
        /// </summary>
        /// <param name="node">The node.</param>
        public delegate void NotificationReceivedEventHandler(Node node);

        /// <summary>
        /// Occurs when a notification is received.
        /// </summary>
        public event NotificationReceivedEventHandler NotificationReceived;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static CommManager Instance
        {
            get
            {
                if (commMgr == null)
                {
                    commMgr = new CommManager();
                }

                return commMgr;
            }
        }

        /// <summary>
        /// Gets the message sent to ask where other connected devices are
        /// </summary>
        internal static string WhereIsEveryBodyMessage
        {
            get
            {
                return "WIEB";
            }
        }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>
        /// The channel.
        /// </value>
        private UdpAnySourceMulticastChannel Channel { get; set; }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.Channel.Close();
        }

        /// <summary>
        /// Sends the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="forceSend">if set to <c>true</c> force sending the node, even if it's within the threshold for suppressing duplicate sending.</param>
        public void SendNode(Node node, bool forceSend = false)
        {
            // Avoid sending very small changes in node position
            if (!forceSend && Math.Abs(this.lastXSent - node.X) < 5 && Math.Abs(this.lastYSent - node.Y) < 5)
            {
                return;
            }

            this.lastXSent = node.X;
            this.lastYSent = node.Y;

            this.Send(node.Serialize());
        }

        /// <summary>
        /// Res the open.
        /// </summary>
        public void ReOpen()
        {
            if (!this.Channel.IsJoined)
            {
                this.Channel.Open();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.UnregisterEvents();
        }

        /// <summary>
        /// Sets the own node id.
        /// </summary>
        /// <param name="id">The id.</param>
        public void SetOwnNodeId(string id)
        {
            this.selfNodeId = id;
        }

        /// <summary>
        /// Sets the type of the communication.
        /// </summary>
        /// <param name="type">The type.</param>
        public void SetCommunicationType(CommType type)
        {
            this.commType = type;

            switch (type)
            {
                case CommType.Udp:
                    this.CreateUdpChannel();
                    break;
                case CommType.Web:
                    this.CreateWebConnection();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        /// <summary>
        /// Reconnects this instance.
        /// </summary>
        public void Reconnect()
        {
            if (this.commType == CommType.Udp)
            {
                this.ReconnectUdp();
            }
            else
            {
                this.ReconnectWeb();
            }
        }

        /// <summary>
        /// Sends the "Where is every body" message.
        /// </summary>
        public void WhereIsEveryBody()
        {
            this.Send(WhereIsEveryBodyMessage);
        }

        /// <summary>
        /// Creates the web connection.
        /// </summary>
        private void CreateWebConnection()
        {
            this.hubConnection = new HubConnection(WebAddress);

            this.signalRHub = this.hubConnection.CreateProxy("node");

            this.hubConnection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.WriteLine("task");
                    }
                    else
                    {
                        this.webConnectionMade = true;
                    }
                });

            this.signalRHub.On<string>(
                "received",
                message =>
                {
                    Debug.WriteLine("RECD (web): " + message);

                    var node = Node.DeSerialize(message);

                    if (message == WhereIsEveryBodyMessage)
                    {
                        Debug.WriteLine("WEIB Received (web)");
                        node = new Node(WhereIsEveryBodyMessage, 0, 0);
                    }

                    // Don't notify about received message that we sent
                    if (node != null && this.NotificationReceived != null && node.Id != this.selfNodeId)
                    {
                        this.NotificationReceived(node);
                    }
                });
        }

        /// <summary>
        /// Creates the channel.
        /// </summary>
        private void CreateUdpChannel()
        {
            this.Channel = new UdpAnySourceMulticastChannel(GroupAddress, GroupPort);

            this.RegisterEvents();

            this.Channel.Open();
        }

        /// <summary>
        /// Reconnects the web connection.
        /// </summary>
        private void ReconnectWeb()
        {
            this.hubConnection.Stop();

            this.CreateWebConnection();
        }

        /// <summary>
        /// Reconnects the UDP connection .
        /// </summary>
        private void ReconnectUdp()
        {
            if (this.Channel != null)
            {
                this.Channel.Dispose();
            }

            this.CreateUdpChannel();
        }

        /// <summary>
        /// Registers the events.
        /// </summary>
        private void RegisterEvents()
        {
            // Register for events from the multicast channel
            this.Channel.Joined += this.ChannelJoined;
            this.Channel.PacketReceived += this.ChannelPacketReceived;
            this.Channel.ErrorOccurred += this.ChannelErrorOccurred;
        }

        /// <summary>
        /// Handles the ErrorOccurred event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionEventArgs"/> instance containing the event data.</param>
        private void ChannelErrorOccurred(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null && e.Exception is SocketException && (e.Exception as SocketException).SocketErrorCode == SocketError.OperationAborted)
            {
                this.CreateUdpChannel();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Debug.WriteLine(e.Message);
                        if (e.Exception != null)
                        {
                            Debug.WriteLine(e.Exception.Message);
                        }

                        ThreadPool.QueueUserWorkItem(
                            state =>
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                                this.Reconnect();
                            });
                    });
            }
        }

        /// <summary>
        /// Unregisters the events.
        /// </summary>
        private void UnregisterEvents()
        {
            if (this.Channel != null)
            {
                // unregister for events from the multicast channel
                this.Channel.Joined -= this.ChannelJoined;
                this.Channel.PacketReceived -= this.ChannelPacketReceived;
                this.Channel.ErrorOccurred -= this.ChannelErrorOccurred;
            }
        }

        /// <summary>
        /// Handles the Joined event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ChannelJoined(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles the PacketReceived event of the Channel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UdpPacketReceivedEventArgs"/> instance containing the event data.</param>
        private void ChannelPacketReceived(object sender, UdpPacketReceivedEventArgs e)
        {
            string message = e.Message.Trim('\0');

            Debug.WriteLine("RECD: " + message);

            var node = message == WhereIsEveryBodyMessage ? new Node(WhereIsEveryBodyMessage, 0, 0)
                                                          : Node.DeSerialize(message);

            if (node != null && this.NotificationReceived != null)
            {
                this.NotificationReceived(node);
            }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void Send(string message)
        {
            Debug.WriteLine("SEND: " + message);

            switch (this.commType)
            {
                case CommType.Udp:
                    this.Channel.Send(message);
                    break;
                case CommType.Web:
                    if (this.webConnectionMade)
                    {
                        try
                        {
                            this.signalRHub.Invoke("Send", message);
                        }
                        catch (Exception)
                        {
                            this.webConnectionMade = false;
                            this.ReconnectWeb();
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
