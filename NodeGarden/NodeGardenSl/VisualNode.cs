//-----------------------------------------------------------------------
// <copyright file="VisualNode.cs" company="Studio Arcade Ltd">
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
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using NodeGardenLib;

    /// <summary>
    /// A visual representation of the node
    /// </summary>
    public class VisualNode : Node
    {
        /// <summary>
        /// Minimum possible node size
        /// </summary>
        private const int NodeSizeMin = 20;

        /// <summary>
        /// The size of inner shadow
        /// </summary>
        private const int Shadow1Multiplier = 85;

        /// <summary>
        /// The size of the outer shadow
        /// </summary>
        private const int Shadow2Multiplier = 110;

        /// <summary>
        /// Minimum thickness for the ellipse outline. Mapped using Connectedness
        /// </summary>
        private const int EllipseOutlineMin = 4;

        /// <summary>
        /// Maximum thickness for the ellipse outline. Mapped using Connectedness
        /// </summary>
        private const int EllipseOutlineMax = 12;

        /// <summary>
        /// each node keeps tracks how connected (distance) it is to other nodes of the maximum Connectedness value
        /// this is used to Map various values such as node size and outline weight 
        /// </summary>
        private static float maxConnectedness = 0.1f;

        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly Random rand;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualNode"/> class with random Position, speed and direction.
        /// </summary>
        /// <param name="rand">The rand.</param>
        /// <param name="canvas">The canvas.</param>
        public VisualNode(Random rand, Canvas canvas)
        {
            this.rand = rand;

            // choose a random position
            this.X = rand.Next(this.NodeSizeMax(), (int)canvas.Width - this.NodeSizeMax());
            this.Y = rand.Next(this.NodeSizeMax(), (int)canvas.Height - this.NodeSizeMax());

            this.CurrentX = this.X;
            this.CurrentY = this.Y;

            this.Connectedness = 0;

            // create ellipses that make the node, it's outline and 2 shadows
            this.Center = new Ellipse { Fill = new SolidColorBrush(Color.FromArgb(105, 255, 255, 255)) };
            this.Outline = new Ellipse { Fill = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255)) };
            this.Shadow1 = new Ellipse { Fill = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)) };
            this.Shadow2 = new Ellipse { Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)) };

            // add the shapes to the canvas UIElement
            canvas.Children.Add(this.Center);
            canvas.Children.Add(this.Outline);
            canvas.Children.Add(this.Shadow1);
            canvas.Children.Add(this.Shadow2);

            // set the ZIndex so the Center is in front of the outline and shadows
            Canvas.SetZIndex(this.Center, 10);
            Canvas.SetZIndex(this.Outline, 9);
            Canvas.SetZIndex(this.Shadow1, 8);
            Canvas.SetZIndex(this.Shadow2, 7);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [disable virtual movement].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable virtual movement]; otherwise, <c>false</c>.
        /// </value>
        public bool DisableVirtualMovement { get; set; }

        /// <summary>
        /// Gets or sets the current X.
        /// </summary>
        /// <value>
        /// The current X.
        /// </value>
        public double CurrentX { get; set; }

        /// <summary>
        /// Gets or sets the current Y.
        /// </summary>
        /// <value>
        /// The current Y.
        /// </value>
        public double CurrentY { get; set; }

        /// <summary>
        /// Gets or sets a shape representing the center of the node
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        internal Ellipse Center { get; set; }

        /// <summary>
        /// Gets or sets a shape representing the outline of the node
        /// </summary>
        internal Ellipse Outline { get; set; }

        /// <summary>
        /// Gets or sets a shape representing the nearest part of the nodes shadow
        /// </summary>
        internal Ellipse Shadow1 { get; set; }

        /// <summary>
        /// Gets or sets a shape representing the outer part of the nodes shadow
        /// </summary>
        internal Ellipse Shadow2 { get; set; }

        /// <summary>
        /// Gets or sets the size of node (varies dependent upon connectedness)
        /// </summary>
        internal float NodeSize { get; set; }

        /// <summary>
        /// Gets or sets the level of connectedness (to nearby other nodes)
        /// The Connectedness value determines the stroke width of the node outline.
        /// This number is between strokeWeightMin and strokeWeightMax
        /// </summary>
        internal float Connectedness { get; set; }

        /// <summary>
        /// Maps the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>Determines connectedness between two points</returns>
        public static float Map(float value, float x1, float y1, float x2, float y2)
        {
            float a = (value - x1) / (y1 - x1);
            return x2 + (a * (y2 - x2));
        }

        /// <summary>
        /// Updates the specified rand.
        /// </summary>
        /// <param name="canvasSize">Size of the canvas.</param>
        public virtual void Update(Size canvasSize)
        {
            // Randomly move one node every now and again
            if (!this.DisableVirtualMovement && this.rand.Next(1, 500) == 499)
            {
                this.X = this.rand.NextDouble() * 480;
                this.Y = this.rand.NextDouble() * 800;
            }

            if (Math.Abs(this.CurrentX - this.X) > 0.5 || Math.Abs(this.CurrentY - this.Y) > 0.5)
            {
                const double FollowSpeed = 0.06;

                // the inertia calculation. Only move the ellipse a fraction of the distance in the direction of the lead node
                this.CurrentX += (this.X - this.CurrentX) * FollowSpeed;
                this.CurrentY += (this.Y - this.CurrentY) * FollowSpeed;
            }
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public virtual void Draw()
        {
            // this allows us to get a reliable value for MaxConnectedness. Used for Mapping the Connectedness value
            if (this.Connectedness > maxConnectedness)
            {
                maxConnectedness = this.Connectedness;
            }

            // create a normalised version of the Connectedness variable
            var normalisedConnectedness = Map(this.Connectedness, 0, maxConnectedness, 0, 1);

            // calculate the node size from NormalisedConnectedness
            this.NodeSize = Map(normalisedConnectedness, 0, 1, NodeSizeMin, this.NodeSizeMax());

            this.Center.Width = this.NodeSize;
            this.Center.Height = this.NodeSize;

            this.Outline.Width = this.NodeSize + Map(normalisedConnectedness, 0, 1, EllipseOutlineMin, EllipseOutlineMax);
            this.Outline.Height = this.Outline.Width;

            this.Shadow1.Width = normalisedConnectedness * Shadow1Multiplier;
            this.Shadow1.Height = this.Shadow1.Width;

            this.Shadow2.Width = normalisedConnectedness * Shadow2Multiplier;
            this.Shadow2.Height = this.Shadow2.Width;

            // reset Connectedness ready for next frame
            this.Connectedness = 0;

            // center all the ellipses on our position taking into account their sizes
            var halfSize = this.NodeSize / 2;
            Canvas.SetLeft(this.Center, this.CurrentX - halfSize);
            Canvas.SetTop(this.Center, this.CurrentY - halfSize);

            halfSize -= (float)(this.Center.Width - this.Outline.Width) / 2;
            Canvas.SetLeft(this.Outline, this.CurrentX - halfSize);
            Canvas.SetTop(this.Outline, this.CurrentY - halfSize);

            halfSize -= (float)(this.Outline.Width - this.Shadow1.Width) / 2;
            Canvas.SetLeft(this.Shadow1, this.CurrentX - halfSize);
            Canvas.SetTop(this.Shadow1, this.CurrentY - halfSize);

            halfSize -= (float)(this.Shadow1.Width - this.Shadow2.Width) / 2;
            Canvas.SetLeft(this.Shadow2, this.CurrentX - halfSize);
            Canvas.SetTop(this.Shadow2, this.CurrentY - halfSize);

            maxConnectedness -= 0.00001f;
        }

        /// <summary>
        /// Removes the specified main canvas.
        /// </summary>
        /// <param name="mainCanvas">The main canvas.</param>
        public void Remove(Canvas mainCanvas)
        {
            // remove the elements from the canvas
            mainCanvas.Children.Remove(this.Center);
            mainCanvas.Children.Remove(this.Outline);
            mainCanvas.Children.Remove(this.Shadow1);
            mainCanvas.Children.Remove(this.Shadow2);
        }

        /// <summary>
        /// Gets the Maximum possible node size
        /// </summary>
        /// <returns>The maximum size the node could be</returns>
        protected virtual int NodeSizeMax()
        {
            return 30;
        }
    }
}