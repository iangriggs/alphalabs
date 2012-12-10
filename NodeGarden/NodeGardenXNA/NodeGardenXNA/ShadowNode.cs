//-----------------------------------------------------------------------
// <copyright file="ShadowNode.cs" company="Studio Arcade Ltd">
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
    using NodeGardenLib;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Representation of a default node
    /// </summary>
    public class ShadowNode : MovingNode
    {
        /// <summary>
        /// shadow 1 size
        /// </summary>
        protected const int Shadow1Multiplier = 85;

        /// <summary>
        /// shadow 2 size
        /// </summary>
        protected const int Shadow2Multiplier = 110;

        /// <summary>
        /// Minimum thickness for the ellipse outline. Mapped using Connectedness
        /// </summary>
        protected const int EllipseOutlineMin = 4;

        /// <summary>
        /// Maximum thickness for the ellipse outline. Mapped using Connectedness
        /// </summary>
        protected const int EllipseOutlineMax = 12;

        /// <summary>
        /// Thickness of the outline
        /// </summary>
        private int outlineSize;

        /// <summary>
        /// Size of the inner shadow
        /// </summary>
        private int shadow1Size;

        /// <summary>
        /// Size of the outer shadow
        /// </summary>
        private int shadow2Size;

        private Color color = Color.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowNode"/> class.
        /// </summary>
        /// <param name="rand">The rand.</param>
        /// <param name="circleTex">The circle tex.</param>
        /// <param name="screenSize">Size of the screen.</param>
        public ShadowNode(Random rand, Texture2D circleTex, Point screenSize)
            : base(rand, circleTex, screenSize)
        {
        }

        /// <summary>
        /// Finishes the connection.
        /// </summary>
        public override void FinishConnection()
        {
            // calculate the shadow sizes from NormalisedConnectedness
            this.outlineSize = this.Size + (int)Global.Map(this.NormalisedConnectedness, 0, 1, EllipseOutlineMin, EllipseOutlineMax);
            this.shadow1Size = (int)(this.NormalisedConnectedness * Shadow1Multiplier);
            this.shadow2Size = (int)(this.NormalisedConnectedness * Shadow2Multiplier);

            base.FinishConnection();
        }

        /// <summary>
        /// Draw any textures for the node using the visual attributes
        /// </summary>
        /// <param name="sb">A sprite batch that has already called Begin and not yet called End</param>
        public override void Draw(SpriteBatch sb)
        {
            // center all the ellipses on our position taking in to account their sizes
            var halfSize = this.Size / 2;

            sb.Draw(this.Texture, new Rectangle((int)this.CurrentX - halfSize, (int)this.CurrentY - halfSize, this.Size, this.Size), null, color * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);

            halfSize -= (this.Size - this.outlineSize) / 2;
            sb.Draw(this.Texture, new Rectangle((int)this.CurrentX - halfSize, (int)this.CurrentY - halfSize, this.outlineSize, this.outlineSize), null, color * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, 0.2f);

            halfSize -= (this.outlineSize - this.shadow1Size) / 2;
            sb.Draw(this.Texture, new Rectangle((int)this.CurrentX - halfSize, (int)this.CurrentY - halfSize, this.shadow1Size, this.shadow1Size), null, color * 0.2f, 0f, Vector2.Zero, SpriteEffects.None, 0.3f);

            halfSize -= (this.shadow1Size - this.shadow2Size) / 2;
            sb.Draw(this.Texture, new Rectangle((int)this.CurrentX - halfSize, (int)this.CurrentY - halfSize, this.shadow2Size, this.shadow2Size), null, color * 0.1f, 0f, Vector2.Zero, SpriteEffects.None, 0.4f);

            ripples.ForEach(r => r.Draw(sb, new Vector2(this.CurrentX, this.CurrentY), this.Size / 2));
        }

        public override void Update()
        {
            base.Update();

            if (!string.IsNullOrEmpty(Tag))
            {
                var tag = Tag.Deserialize();
                if (tag.Ping)
                {
                    CreateRipple(GetXnaColour(tag.AccentColour));
                }

                color = GetXnaColour(tag.AccentColour);

                Tag = "";
            }

            ripples.ForEach(r => r.Update());
        }

        public Microsoft.Xna.Framework.Color GetXnaColour(System.Windows.Media.Color color)
        {
            return new Microsoft.Xna.Framework.Color(
                color.R * (byte)(color.A / 255.0f),
                color.G * (byte)(color.A / 255.0f),
                color.B * (byte)(color.A / 255.0f),
                color.A);
        }
    }
}