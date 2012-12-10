//-----------------------------------------------------------------------
// <copyright file="MyVisualNode.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenXaml
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Visual node representing this device and can be dragged around
    /// </summary>
    public class MyVisualNode : VisualNode
    {
        /// <summary>
        /// Reference to gardener managing this garden
        /// </summary>
        private readonly NodeGardenLib.Gardener gardener;

        /// <summary>
        /// Is the node currently being dragged
        /// </summary>
        private bool isBeingDragged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyVisualNode"/> class.
        /// </summary>
        /// <param name="rand">The rand.</param>
        /// <param name="mainCanvas">The main canvas.</param>
        /// <param name="gardener">The gardener.</param>
        public MyVisualNode(Random rand, Canvas mainCanvas, NodeGardenLib.Gardener gardener)
            : base(rand, mainCanvas)
        {
            this.gardener = gardener;

            var color = (Color)Application.Current.Resources["PhoneAccentColor"];
            this.Center.Fill = new SolidColorBrush(color);
            Touch.FrameReported += this.TouchFrameReported;
        }

        /// <summary>
        /// Updates the specified rand.
        /// </summary>
        /// <param name="canvasSize">Size of the canvas.</param>
        public override void Update(Size canvasSize)
        {
            // The base implementation will move automatically
            // We don't want this as the user will manipulate the position of this node directly
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            Canvas.SetZIndex(this.Center, 14);
        }

        /// <summary>
        /// Gets the Maximum possible node size
        /// </summary>
        /// <returns>The maximum size the node could be</returns>
        protected override int NodeSizeMax()
        {
            return 50;
        }

        /// <summary>
        /// Touches the frame reported.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TouchFrameEventArgs"/> instance containing the event data.</param>
        private void TouchFrameReported(object sender, TouchFrameEventArgs e)
        {
            TouchPoint tp = e.GetPrimaryTouchPoint((UIElement)sender);

            switch (tp.Action)
            {
                case TouchAction.Down:
                    this.isBeingDragged = true;
                    this.X = tp.Position.X;
                    this.Y = tp.Position.Y;

                    this.CurrentX = this.X;
                    this.CurrentY = this.Y;
                    break;

                case TouchAction.Move:
                    if (this.isBeingDragged)
                    {
                        this.X = tp.Position.X;
                        this.Y = tp.Position.Y;

                        this.CurrentX = this.X;
                        this.CurrentY = this.Y;

                        this.gardener.UpdateSelfNodePosition(this.X, this.Y);
                    }

                    break;

                case TouchAction.Up:
                    this.isBeingDragged = false;
                    break;
                default:
                    break;
            }
        }
    }
}