//-----------------------------------------------------------------------
// <copyright file="MyNode.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenXNA
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Audio;

    using NodeGardenLib;

    using Point = Microsoft.Xna.Framework.Point;
    using System.Diagnostics;
    using NodeGardenXaml;
    using System.Collections.Generic;

    /// <summary>
    /// Node representing the current device and which can be dragged
    /// </summary>
    public class MyNode : ShadowNode
    {
        /// <summary>
        /// Reference to gardener managing the garden this node is in
        /// </summary>
        private readonly Gardener gardener;

        /// <summary>
        /// Color of the node
        /// </summary>
        private readonly Color color;

        /// <summary>
        /// Tracks if currently being dragged
        /// </summary>
        private bool isBeingDragged;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MyNode"/> class.
        /// </summary>
        /// <param name="rand">The rand.</param>
        /// <param name="circleTex">The circle tex.</param>
        /// <param name="screenSize">Size of the screen.</param>
        /// <param name="gardener">The gardener.</param>
        public MyNode(Random rand, Texture2D circleTex, Point screenSize, Gardener gardener)
            : base(rand, circleTex, screenSize)
        {
            this.NodeSizeMax = 70;

            this.gardener = gardener;

            var accentColor = (System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"];
            this.color = GetXnaColour(accentColor);

            // set the controllable node to take touch events
            Touch.FrameReported += this.TouchFrameReported;

            this.DisableVirtualMovement = true;

            var pingTag = new PingTag { AccentColour = accentColor };
            this.Tag = pingTag.Serialize();
        }

        /// <summary>
        /// Where the main code for updating the nodes goes
        /// </summary>
        public override void Update()
        {
            // Don't use the base update to reflect changes
            // Position will be updated by manipulation
            ripples.ForEach(r => r.Update());
        }

        /// <summary>
        /// Draw any textures for the node using the visual attributes
        /// </summary>
        /// <param name="sb">A sprite batch that has already called Begin and not yet called End</param>
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            sb.Draw(this.Texture, new Rectangle((int)this.Position.X - (this.Size / 2), (int)this.Position.Y - (this.Size / 2), this.Size, this.Size), null, this.color, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            ripples.ForEach(r => r.Draw(sb, new Vector2(Position.X, Position.Y), this.Size / 2));
        }

        Vector2 heldAt = new Vector2();

        /// <summary>
        /// Handles the FrameReported event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TouchFrameEventArgs"/> instance containing the event data.</param>
        private void TouchFrameReported(object sender, TouchFrameEventArgs e)
        {
            TouchPoint tp = e.GetPrimaryTouchPoint(null);
            switch (tp.Action)
            {
                case TouchAction.Down:
                    // when the user touches the screen find out if they hit the node, if so start dragging
                    if (Vector2.DistanceSquared(new Vector2((float)tp.Position.X, (float)tp.Position.Y), this.Position) < Global.TouchPointSize * Global.TouchPointSize)
                    {
                        this.isBeingDragged = true;
                        heldAt = new Vector2((float)tp.Position.X, (float)tp.Position.Y);
                    }

                    break;

                case TouchAction.Move:
                    // if dragging move the node to the touch position
                    if (this.isBeingDragged)
                    {
                        var x = tp.Position.X;
                        var y = tp.Position.Y;

                        this.Position = new Vector2((float)x, (float)y);

                        var accentColour = (System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"];
                        var tag = new PingTag { AccentColour = accentColour, Ping = false }; 
                        this.gardener.UpdateSelfNodePosition(x, y, tag.Serialize());
                    }

                    break;

                case TouchAction.Up:
                    // release finger and stop dragging
                    this.isBeingDragged = false;
                    if (heldAt == new Vector2((float)tp.Position.X, (float)tp.Position.Y))
                    {
                        Debug.WriteLine("Ping");
                        var accentColour = (System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"];
                        CreateRipple(GetXnaColour(accentColour));
                        var tag = new PingTag { AccentColour = accentColour, Ping = true };
                        gardener.UpdateSelfNodePosition(tp.Position.X, tp.Position.Y, tag.Serialize());
                        gardener.ClearSelfPing();
                    }
                    break;
                default:
                    break;
            }

        }
    }
}