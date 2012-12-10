//-----------------------------------------------------------------------
// <copyright file="Gardener.cs" company="Studio Arcade Ltd">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Microsoft.Devices;
    using Microsoft.Phone.Info;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using ShakeGestures;
    using SLARToolKit;
    using Color = System.Windows.Media.Color;

    /// <summary>
    /// The gardener manages tracking the nodes, detecting events and communicating between gardens (devices)
    /// </summary>
    public class Gardener : IDisposable
    {
        /// <summary>
        /// The camera used for color and image detection
        /// </summary>
        private PhotoCamera cam;

        /// <summary>
        /// Thread used to process the camera stream and try and detect specific color
        /// </summary>
        private Thread colorDetectThread;

        /// <summary>
        /// Flag used to notify to the background thread that is being closed
        /// </summary>
        private volatile bool cdtPleaseExit;

        /// <summary>
        /// Buffer for storing microphone data
        /// </summary>
        private byte[] microphoneBuffer;

        /// <summary>
        /// How many microphone buffers worth of data in a row have been over the volume threshold
        /// </summary>
        private int loudCount;

        /// <summary>
        /// Time to ignore the microphone until - so don't trigger multiple detections in quick succession
        /// </summary>
        private DateTime ignoreNoisesUntil;

        /// <summary>
        /// Dispatcher timer used to simulate XNA framework game loop
        /// </summary>
        private DispatcherTimer frameworkDispatcherTimer;

        /// <summary>
        /// Used to track initialization is only done once
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Volume that must be sustained for event to be triggered
        /// </summary>
        private int noiseThreshold = 1000;

        /// <summary>
        /// Duration that a noise above the threshold must be sustained for to trigger the event
        /// </summary>
        private int noiseDuration = 1;

        /// <summary>
        /// Id of the self node
        /// </summary>
        private string selfNodeId;

        /// <summary>
        /// Tracks if color is being detected
        /// </summary>
        private bool detectColors;

        /// <summary>
        /// Tracks if images are being detected
        /// </summary>
        private bool detectImages;

        /// <summary>
        /// Tracks if the camera has been activated
        /// </summary>
        private bool cameraActivated;

        /// <summary>
        /// The detector for SLART markers
        /// </summary>
        private GrayBufferMarkerDetector markerDetector;

        /// <summary>
        /// Timer used to trigger image detection
        /// </summary>
        private DispatcherTimer imageDetectTimer;

        /// <summary>
        /// If currently detecting markers
        /// </summary>
        private bool isDetecting;

        /// <summary>
        /// Time to stop image detection until - so don't trigger multiple detections in quick succession
        /// </summary>
        private DateTime ignoreImagesUntil;

        /// <summary>
        /// The brush used to display the camera output on - required to capture content
        /// Can be made invisible but must be in the visual tree
        /// </summary>
        private VideoBrush viewfinderBrush;

        /// <summary>
        /// Transform applied to the video display brush and used to ensure the camera is displayed at the correct orientation
        /// </summary>
        private RotateTransform viewfinderBrushTransform;

        /// <summary>
        /// Buffer used to store camera image
        /// </summary>
        private byte[] cameraBuffer;

        /// <summary>
        /// The color to detect
        /// </summary>
        private Color colorToDetect;

        /// <summary>
        /// Threshold from defined color is considered a match
        /// </summary>
        private int colorThreshold;

        /// <summary>
        /// Timer that triggers sending messages to other connected devices
        /// to let them know that not gone away
        /// </summary>
        private Timer imAliveNotificationTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gardener"/> class.
        /// </summary>
        public Gardener()
        {
            this.Nodes = new List<Node>();
        }

        /// <summary>
        /// Event handler for when a shake is detected
        /// </summary>
        public delegate void ShakeDetectedEventHandler();

        /// <summary>
        /// Event handler for when a noise is detected
        /// </summary>
        public delegate void NoiseDetectedEventHandler();

        /// <summary>
        /// Event handler for when notification of a node having changed is received.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        public delegate void NodeChangedEventHandler(string nodeId);

        /// <summary>
        /// Event handler for when a known image is detected.
        /// </summary>
        /// <param name="imageName">Name of the image that was detected.</param>
        public delegate void ImageDetectedEventHandler(string imageName);

        /// <summary>
        /// Event handler for when the specified color is detected
        /// </summary>
        public delegate void ColorDetectedEventHandler();

        /// <summary>
        /// Occurs when a shake is detected.
        /// </summary>
        public event ShakeDetectedEventHandler OnShakeDetected;

        /// <summary>
        /// Occurs when a sustained loud noise is detected.
        /// </summary>
        public event NoiseDetectedEventHandler OnNoiseDetected;

        /// <summary>
        /// Occurs when an image is detected.
        /// </summary>
        public event ImageDetectedEventHandler OnImageDetected;

        /// <summary>
        /// Occurs when the specified color is detected.
        /// </summary>
        public event ColorDetectedEventHandler OnColorDetected;

        /// <summary>
        /// Occurs when a node is changed.
        /// </summary>
        public event NodeChangedEventHandler OnNodeChanged;

        /// <summary>
        /// Gets all the nodes in the garden.
        /// </summary>
        /// <value>
        /// The nodes.
        /// </value>
        public List<Node> Nodes { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns><c>true</c> if initialization is successful; otherwise, <c>false</c>.</returns>
        public bool Initialize(GardenerSettings settings)
        {
            if (!this.isInitialized)
            {
                CommManager.Instance.SetCommunicationType(settings.CommType);
                CommManager.Instance.NotificationReceived += this.NodeChangeMessageReceived;

                if (settings.EnableColorDetection || settings.EnableImageDetection)
                {
                    if (settings.ViewFinderBrush == null)
                    {
                        throw new ArgumentNullException("settings.ViewFinderBrush");
                    }

                    if (settings.ViewFinderBrushTransform == null)
                    {
                        throw new ArgumentNullException("settings.ViewFinderBrushTransform");
                    }

                    this.viewfinderBrush = settings.ViewFinderBrush;
                    this.viewfinderBrushTransform = settings.ViewFinderBrushTransform;
                }

                if (settings.EnableColorDetection)
                {
                    this.colorToDetect = settings.ColorToDetect;
                    this.colorThreshold = settings.ColorDetectionThreshold;

                    this.InitializeColorDetection();
                }

                if (settings.EnableImageDetection)
                {
                    this.InitializeImageDetection();
                }

                if (settings.EnableShakeDetection)
                {
                    this.InitializeShakeDetection();
                }

                if (settings.EnableNoiseDetection)
                {
                    this.noiseThreshold = settings.NoiseThreshold < 1000 ? 2000 : settings.NoiseThreshold;

                    this.noiseDuration = settings.NoiseDuration < 1 ? 3 : settings.NoiseDuration;

                    this.InitializeNoiseDetection();
                }

                NetworkChange.NetworkAddressChanged += (s, args) => CommManager.Instance.Reconnect();

                this.imAliveNotificationTimer = new Timer(  
                    state =>
                    {
                        this.ForceSendingSelfNode();
                        this.RemoveDisconnectedNodes();
                    },
                    null,
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(15));

                this.isInitialized = true;
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Gardener can only be initialized once");
                return false;
            }
        }

        public Microsoft.Xna.Framework.Color GetXnaColour(System.Windows.Media.Color color)
        {
            return new Microsoft.Xna.Framework.Color(
                color.R * (byte)(color.A / 255.0f),
                color.G * (byte)(color.A / 255.0f),
                color.B * (byte)(color.A / 255.0f),
                color.A);
        }

        /// <summary>
        /// Adds the self node.
        /// </summary>
        /// <param name="xPosition">The x position.</param>
        /// <param name="yPosition">The y position.</param>
        public void AddSelfNode(double xPosition, double yPosition)
        {
            if (!string.IsNullOrWhiteSpace(this.selfNodeId))
            {
                throw new InvalidOperationException("Self node already added");
            }

            var deviceId = GetSimplifiedDeviceId();

            var node = new Node(deviceId, xPosition, yPosition)
            {
                NodeType = TypeOfNode.Self,
                LastUpdated = DateTime.UtcNow
            };

            this.Nodes.Add(node);

            this.selfNodeId = node.Id;
            CommManager.Instance.SetOwnNodeId(node.Id);
            CommManager.Instance.SendNode(node);
        }

        /// <summary>
        /// Gets the self node id.
        /// </summary>
        /// <returns>The Id</returns>
        public string GetSelfNodeId()
        {
            return this.selfNodeId;
        }

        /// <summary>
        /// Adds the default node.
        /// </summary>
        /// <param name="xPosition">The x position.</param>
        /// <param name="yPosition">The y position.</param>
        public void AddDefaultNode(double xPosition, double yPosition)
        {
            var node = new Node(Guid.NewGuid().ToString(), xPosition, yPosition)
            {
                NodeType = TypeOfNode.Default,
                LastUpdated = DateTime.UtcNow
            };

            this.Nodes.Add(node);
        }

        /// <summary>
        /// Updates the self node position.
        /// </summary>
        /// <param name="xPosition">The x position.</param>
        /// <param name="yPosition">The y position.</param>
        /// <param name="tag">The tag.</param>
        public void UpdateSelfNodePosition(double xPosition, double yPosition, string tag = null, bool forceSend = false)
        {
            var selfNode = this.Nodes.FirstOrDefault(n => n.NodeType == TypeOfNode.Self);

            if ((selfNode != null && (selfNode.X != xPosition || selfNode.Y != yPosition)) || forceSend)
            {
                selfNode.X = xPosition;
                selfNode.Y = yPosition;
                selfNode.Tag = tag;

                CommManager.Instance.SendNode(selfNode, forceSend);
            }
        }

        public void ClearSelfPing()
        {
            var selfNode = this.Nodes.FirstOrDefault(n => n.NodeType == TypeOfNode.Self);
            if (selfNode.Tag != null)
            {
                var pingTag = selfNode.Tag.Deserialize();
                pingTag.Ping = false;
                selfNode.Tag = pingTag.Serialize();
            }
        }

        /// <summary>
        /// Reconnects this instance.
        /// </summary>
        public void Reconnect()
        {
            if (this.isInitialized)
            {
                CommManager.Instance.Reconnect();
            }
        }

        /// <summary>
        /// Sends the "Where is every body" message.
        /// </summary>
        public void WhereIsEveryBody()
        {
            CommManager.Instance.WhereIsEveryBody();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.frameworkDispatcherTimer != null)
                {
                    this.frameworkDispatcherTimer.Stop();
                }

                if (this.cam != null)
                {
                    // Notify the background worker to stop processing.
                    this.cdtPleaseExit = true;

                    if (this.colorDetectThread != null)
                    {
                        this.colorDetectThread.Join();
                    }

                    if (this.imageDetectTimer != null)
                    {
                        this.imageDetectTimer.Stop();
                    }

                    // Dispose of the camera object to free memory.
                    if (this.cam != null)
                    {
                        this.cam.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the simplified device id.
        /// </summary>
        /// <returns>A simplified version of the unique device Id</returns>
        private static string GetSimplifiedDeviceId()
        {
            var deviceUniqueId = BitConverter.ToString((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId"));

            return deviceUniqueId.Replace("-", string.Empty);
        }

        /// <summary>
        /// Gets the RGB values from YCbCr ones.
        /// </summary>
        /// <param name="luma">The luma value.</param>
        /// <param name="chromaB">The chroma B value.</param>
        /// <param name="chromaR">The chroma R value.</param>
        /// <param name="red">The red value.</param>
        /// <param name="green">The green value.</param>
        /// <param name="blue">The blue value.</param>
        private static void GetRgb(byte luma, int chromaB, int chromaR, out int red, out int green, out int blue)
        {
            // Assumes Cb & Cr have been converted to signed values (ranging from 127 to -128).

            // Integer-only division.
            red = luma + chromaR + (chromaR >> 2) + (chromaR >> 3) + (chromaR >> 5);
            green = luma - ((chromaB >> 2) + (chromaB >> 4) + (chromaB >> 5)) - ((chromaR >> 1) + (chromaR >> 3) + (chromaR >> 4) + (chromaR >> 5));
            blue = luma + chromaB + (chromaB >> 1) + (chromaB >> 2) + (chromaB >> 6);

            // Clamp values to 8-bit RGB range between 0 and 255.
            red = red <= 255 ? red : 255;
            red = red >= 0 ? red : 0;
            green = green <= 255 ? green : 255;
            green = green >= 0 ? green : 0;
            blue = blue <= 255 ? blue : 255;
            blue = blue >= 0 ? blue : 0;
        }

        /// <summary>
        /// Handles a node change message being received.
        /// </summary>
        /// <param name="node">The node.</param>
        private void NodeChangeMessageReceived(Node node)
        {
            if (node.Id == CommManager.WhereIsEveryBodyMessage)
            {
                this.ForceSendingSelfNode();
                return;
            }

            var existingNode = this.Nodes.FirstOrDefault(n => n.Id == node.Id);

            if (existingNode == null)
            {
                node.LastUpdated = DateTime.UtcNow;
                this.Nodes.Add(node);
            }
            else
            {
                var idx = this.Nodes.IndexOf(existingNode);
                this.Nodes[idx].LastUpdated = DateTime.UtcNow;
                this.Nodes[idx].X = node.X;
                this.Nodes[idx].Y = node.Y;
                this.Nodes[idx].Tag = node.Tag;
            }

            if (this.OnNodeChanged != null)
            {
                this.OnNodeChanged(node.Id);
            }
        }

        /// <summary>
        /// Forces sending the self nodes position - so other devices know that still here, even if haven't moved recently.
        /// </summary>
        private void ForceSendingSelfNode()
        {
            var selfNode = this.Nodes.FirstOrDefault(n => n.NodeType == TypeOfNode.Self);
            CommManager.Instance.SendNode(selfNode, true);
        }

        /// <summary>
        /// Removes any disconnected nodes.
        /// </summary>
        private void RemoveDisconnectedNodes()
        {
            for (var index = this.Nodes.Count - 1; index >= 0; index--)
            {
                var node = this.Nodes[index];

                if (node.NodeType == TypeOfNode.Other
                 && node.LastUpdated <= DateTime.UtcNow.AddMinutes(-1))
                {
                    this.Nodes.Remove(node);

                    if (this.OnNodeChanged != null)
                    {
                        this.OnNodeChanged(node.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes noise detection.
        /// </summary>
        private void InitializeNoiseDetection()
        {
            // As the Microphone is from XNA we need to ensure that we are manually calling `FrameworkDispatcher.Update()` so that it works correctly.
            this.frameworkDispatcherTimer = new DispatcherTimer();
            this.frameworkDispatcherTimer.Tick += (sender, args) => FrameworkDispatcher.Update();
            this.frameworkDispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            this.frameworkDispatcherTimer.Start();

            FrameworkDispatcher.Update();

            // Configure the microphone with the smallest supported BufferDuration (.1)
            Microphone.Default.BufferDuration = TimeSpan.FromSeconds(.1);
            Microphone.Default.BufferReady += this.MicrophoneBufferReady;

            // Initialize the buffer for holding microphone data
            int size = Microphone.Default.GetSampleSizeInBytes(Microphone.Default.BufferDuration);
            this.microphoneBuffer = new byte[size];

            // Start listening
            Microphone.Default.Start();
        }

        /// <summary>
        /// Microphones the buffer ready.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MicrophoneBufferReady(object sender, EventArgs e)
        {
            if (DateTime.UtcNow < this.ignoreNoisesUntil)
            {
                return;
            }

            this.ignoreNoisesUntil = DateTime.MinValue;

            int size = Microphone.Default.GetData(this.microphoneBuffer);

            if (size > 0)
            {
                var currentVolume = this.GetAverageVolume(size);

                System.Diagnostics.Debug.WriteLine("Volume: " + currentVolume);

                if (currentVolume >= this.noiseThreshold)
                {
                    if (this.loudCount++ > this.noiseDuration)
                    {
                        if (this.OnNoiseDetected != null)
                        {
                            this.OnNoiseDetected();
                        }

                        // now ignore the microphone for the next few seconds
                        this.ignoreNoisesUntil = DateTime.UtcNow.AddMilliseconds(1000);
                        this.loudCount = 0;
                    }
                }
                else
                {
                    this.loudCount = 0;
                }
            }
        }

        /// <summary>
        /// Gets the average volume.
        /// </summary>
        /// <param name="numberOfBytes">The number of bytes.</param>
        /// <returns>
        /// The average volume for the specified number of bytes in the microphone buffer
        /// </returns>
        private int GetAverageVolume(int numberOfBytes)
        {
            long total = 0;

            // Buffer is an array of bytes, but we want to examine each 2-byte value.
            // [SampleDuration for 1 sec (32000) / SampleRate (16000) = 2 bytes]
            // Therefore, we iterate through the array 2 bytes at a time.
            for (int i = 0; i < numberOfBytes; i += 2)
            {
                // Cast from short to int to prevent -32768 from overflowing Math.Abs:
                int value = Math.Abs((int)BitConverter.ToInt16(this.microphoneBuffer, i));
                total += value;
            }

            return (int)(total / (numberOfBytes / 2));
        }

        /// <summary>
        /// Initializes the shake detection.
        /// </summary>
        private void InitializeShakeDetection()
        {
            ShakeGesturesHelper.Instance.ShakeGesture += (sender, args) =>
            {
                if (this.OnShakeDetected != null)
                {
                    this.OnShakeDetected();
                }
            };

            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 6;
            ShakeGesturesHelper.Instance.Active = true;
        }

        /// <summary>
        /// Initializes color detection.
        /// </summary>
        private void InitializeColorDetection()
        {
            this.ActivateCamera();
            this.detectColors = true;
        }

        /// <summary>
        /// Initializes image detection.
        /// </summary>
        private void InitializeImageDetection()
        {
            this.ActivateCamera();
            this.detectImages = true;
        }

        /// <summary>
        /// Activates the camera.
        /// </summary>
        private void ActivateCamera()
        {
            if (this.cameraActivated)
            {
                return;
            }

            this.cameraActivated = true;

            // TODO: test with a device with a front facing camera
            // Check to see if the camera is available on the device.
            if (Camera.IsCameraTypeSupported(CameraType.Primary) ||
                Camera.IsCameraTypeSupported(CameraType.FrontFacing))
            {
                this.cam = new PhotoCamera();
                this.cam.Initialized += this.CameraInitialized;
                this.viewfinderBrush.SetSource(this.cam);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Device does not have a camera");
            }
        }

        /// <summary>
        /// Handle successful camera initialization
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Microsoft.Devices.CameraOperationCompletedEventArgs"/> instance containing the event data.</param>
        private void CameraInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            if (this.cam != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    // Set the orientation of the viewfinder.
                    this.viewfinderBrushTransform.Angle = this.cam.Orientation;
                });

                if (this.detectColors)
                {
                    // Start the background worker thread that processes the camera preview buffer frames.
                    this.cdtPleaseExit = false;
                    this.colorDetectThread = new Thread(this.ColorDetectWorker);
                    this.colorDetectThread.Start();
                }

                if (this.detectImages)
                {
                    this.markerDetector = new GrayBufferMarkerDetector();

                    // TODO: add more markers
                    var marker = Marker.LoadFromResource("markers/wp8.pat", 16, 16, 80, "WP8");

                    // The perspective projection has the near plane at 1 and the far plane at 4000
                    this.markerDetector.Initialize((int)this.cam.PreviewResolution.Width, (int)this.cam.PreviewResolution.Height, 1, 4000, marker);

                    Deployment.Current.Dispatcher.BeginInvoke(
                        () =>
                        {
                            imageDetectTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                            imageDetectTimer.Tick += (s, args) => this.DetectImages();
                            imageDetectTimer.Start();
                        });
                }
            }
        }

        /// <summary>
        /// Detects images.
        /// </summary>
        private void DetectImages()
        {
            if (this.isDetecting || DateTime.UtcNow < this.ignoreImagesUntil)
            {
                return;
            }

            this.isDetecting = true;

            try
            {
                // Update buffer size
                var pixelWidth = (int)this.cam.PreviewResolution.Width;
                var pixelHeight = (int)this.cam.PreviewResolution.Height;

                if (this.cameraBuffer == null || this.cameraBuffer.Length != pixelWidth * pixelHeight)
                {
                    this.cameraBuffer = new byte[pixelWidth * pixelHeight];
                }

                try
                {
                    // Grab snapshot
                    this.cam.GetPreviewBufferY(this.cameraBuffer);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                // Detect
                var dr = this.markerDetector.DetectAllMarkers(this.cameraBuffer, pixelWidth, pixelHeight);

                if (dr.HasResults)
                {
                    this.ignoreImagesUntil = DateTime.UtcNow.AddSeconds(10);

                    if (this.OnImageDetected != null)
                    {
                        this.OnImageDetected(dr.MostConfidableResult.Marker.Name);
                    }
                }
            }
            finally
            {
                this.isDetecting = false;
            }
        }

        /// <summary>
        /// Worker method for detecting the specified color
        /// </summary>
        private void ColorDetectWorker()
        {
            // Obtain the YCbCr layout settings used by the camera buffer.
            var bufferLayout = this.cam.YCbCrPixelLayout;

            // Allocate the appropriately sized preview buffer.
            byte[] currentPreviewBuffer = new byte[bufferLayout.RequiredBufferSize];

            // Continue processing until asked to stop in OnNavigatingFrom.
            while (!this.cdtPleaseExit)
            {
                // Get the current preview buffer from the camera.
                this.cam.GetPreviewBufferYCbCr(currentPreviewBuffer);

                var colorA = this.GetColorFromPixel(210, 160, currentPreviewBuffer);
                var colorB = this.GetColorFromPixel(210, 320, currentPreviewBuffer);
                var colorC = this.GetColorFromPixel(480, 160, currentPreviewBuffer);
                var colorD = this.GetColorFromPixel(480, 320, currentPreviewBuffer);

                if (this.WithinDetectionThreshold(colorA) &&
                    this.WithinDetectionThreshold(colorB) &&
                    this.WithinDetectionThreshold(colorC) &&
                    this.WithinDetectionThreshold(colorD))
                {
                    if (this.OnColorDetected != null)
                    {
                        this.OnColorDetected();
                    }

                    // Big pause before we detect again
                    Thread.Sleep(10000);
                }

                // Pause before checking again
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Detects if the detected color is within the threshold of the defined color.
        /// </summary>
        /// <param name="detectedColor">Color of the detected.</param>
        /// <returns><c>true</c> if the color is within the defined threshold of the specified color, otherwise <c>false</c></returns>
        private bool WithinDetectionThreshold(Color detectedColor)
        {
            return Math.Abs(detectedColor.R - this.colorToDetect.R) <= this.colorThreshold &&
                   Math.Abs(detectedColor.G - this.colorToDetect.G) <= this.colorThreshold &&
                   Math.Abs(detectedColor.B - this.colorToDetect.B) <= this.colorThreshold;
        }

        /// <summary>
        /// Gets the color from the specified pixel.
        /// </summary>
        /// <param name="xPosition">The x position.</param>
        /// <param name="yPosition">The y position.</param>
        /// <param name="currentPreviewBuffer">The current preview buffer.</param>
        /// <returns>The color of the pixel</returns>
        private Color GetColorFromPixel(int xPosition, int yPosition, byte[] currentPreviewBuffer)
        {
            byte y;
            int cr;
            int cb;

            this.GetYCbCrFromPixel(this.cam.YCbCrPixelLayout, currentPreviewBuffer, xPosition, yPosition, out y, out cr, out cb);

            int r;
            int g;
            int b;

            GetRgb(y, cb, cr, out r, out g, out b);

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// Gets the Y, Cb and Cr values from pixel.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="currentPreviewBuffer">The current preview buffer.</param>
        /// <param name="xFramePos">The x frame pos.</param>
        /// <param name="yFramePos">The y frame pos.</param>
        /// <param name="y">The y.</param>
        /// <param name="cr">The cr.</param>
        /// <param name="cb">The cb.</param>
        private void GetYCbCrFromPixel(YCbCrPixelLayout layout, byte[] currentPreviewBuffer, int xFramePos, int yFramePos, out byte y, out int cr, out int cb)
        {
            // Find the bytes corresponding to the pixel location in the frame.
            int yBufferIndex = layout.YOffset + (yFramePos * layout.YPitch) + (xFramePos * layout.YXPitch);
            int crBufferIndex = layout.CrOffset + ((yFramePos / 2) * layout.CrPitch) + ((xFramePos / 2) * layout.CrXPitch);
            int cbBufferIndex = layout.CbOffset + ((yFramePos / 2) * layout.CbPitch) + ((xFramePos / 2) * layout.CbXPitch);

            // The luminance value is always positive.
            y = currentPreviewBuffer[yBufferIndex];

            // The preview buffer contains an unsigned value between 255 and 0.
            // The buffer value is cast from a byte to an integer.
            cr = currentPreviewBuffer[crBufferIndex];

            // Convert to a signed value between 127 and -128.
            cr -= 128;

            // The preview buffer contains an unsigned value between 255 and 0.
            // The buffer value is cast from a byte to an integer.
            cb = currentPreviewBuffer[cbBufferIndex];

            // Convert to a signed value between 127 and -128.
            cb -= 128;
        }
    }
}
