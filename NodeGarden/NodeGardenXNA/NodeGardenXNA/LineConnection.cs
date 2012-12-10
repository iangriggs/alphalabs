//-----------------------------------------------------------------------
// <copyright file="LineConnection.cs" company="Studio Arcade Ltd">
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

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A line connecting nodes
    /// </summary>
    public class LineConnection : Connection
    {
        /// <summary>
        /// Minimum stroke weight for a connecting line
        /// </summary>
        protected const float StrokeWeightMin = 1.0f;

        /// <summary>
        /// Maximum stroke weight for a connecting line
        /// </summary>
        protected const float StrokeWeightMax = 7.0f;

        /// <summary>
        /// The texture to draw
        /// </summary>
        private readonly Texture2D pixelTexture;

        /// <summary>
        /// Actual stroke thickness of connecting line
        /// </summary>
        private int strokeThickness;

        /// <summary>
        /// Starting position
        /// </summary>
        private Vector2 start;

        /// <summary>
        /// Ending position
        /// </summary>
        private Vector2 end;

        /// <summary>
        /// Color of the line
        /// </summary>
        private Color color;

        /// <summary>
        /// Is this line visible
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineConnection"/> class.
        /// </summary>
        /// <param name="pixelTexture">The pixel texture.</param>
        public LineConnection(Texture2D pixelTexture)
        {
            this.pixelTexture = pixelTexture;
        }

        /// <summary>
        /// Forms the connection.
        /// </summary>
        /// <param name="node1">The node1.</param>
        /// <param name="node2">The node2.</param>
        /// <param name="distance">The distance.</param>
        public override void FormConnection(VisualNode node1, VisualNode node2, float distance)
        {
            this.isVisible = true;

            // draw a line between 2 nodes. The thickness/alpha varies depending on distance
            this.strokeThickness = (int)Global.Map(distance, 0, Global.MinDist, StrokeWeightMax, StrokeWeightMin);
            this.color = Color.White * Global.Map(distance, 0, Global.MinDist, 1.0f, 0);
            this.start = node1.CurrentPosition;
            this.end = node2.CurrentPosition;
        }

        /// <summary>
        /// Breaks the connection.
        /// </summary>
        /// <param name="node1">The node1.</param>
        /// <param name="node2">The node2.</param>
        public override void BreakConnection(VisualNode node1, VisualNode node2)
        {
            this.isVisible = false;
        }

        /// <summary>
        /// Draws the specified sprite batch.
        /// </summary>
        /// <param name="sb">The sprite batch.</param>
        public override void Draw(SpriteBatch sb)
        {
            if (this.isVisible)
            {
                sb.Draw(
                    this.pixelTexture,
                    new Rectangle(
                        (int)this.start.X,
                        (int)this.start.Y,
                        this.strokeThickness,
                        (int)Vector2.Distance(this.start, this.end)),
                    null,
                    this.color,
                    MathHelper.PiOver2 - (float)Math.Atan2(this.end.Y - this.start.Y, this.start.X - this.end.X),
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.5f);
            }
        }
    }
}