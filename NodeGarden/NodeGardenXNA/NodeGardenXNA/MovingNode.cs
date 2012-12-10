//-----------------------------------------------------------------------
// <copyright file="MovingNode.cs" company="Studio Arcade Ltd">
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
    using System.Globalization;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Generic;
    using NodeGardenXaml;
    using Microsoft.Xna.Framework.Audio;
    using System.Windows;

    /// <summary>
    /// A node which can move by itself
    /// </summary>
    public class MovingNode : VisualNode
    {
        /// <summary>
        /// the degree to which the node changes direction.
        /// </summary>
        protected const float AngleAdd = 0.1f;

        /// <summary>
        /// Minimum value of possible node size (which is based on connectedness)
        /// </summary>
        protected const int NodeSizeMin = 20;

        protected List<Ripple> ripples;

        private SoundEffect soundEffect;

        private List<SoundEffect> soundEffects;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovingNode"/> class.
        /// </summary>
        /// <param name="rand">The rand.</param>
        /// <param name="circleTex">The circle tex.</param>
        /// <param name="screenSize">Size of the screen.</param>
        public MovingNode(Random rand, Texture2D circleTex, Microsoft.Xna.Framework.Point screenSize)
        {
            // store the size of the draw area
            this.ScreenSize = screenSize;
            this.Rand = rand;

            this.NodeSizeMax = 50;

            // choose a random position, speed, and angle
            this.Position = new Vector2(
                rand.Next(this.NodeSizeMax, this.ScreenSize.X - this.NodeSizeMax),
                rand.Next(this.NodeSizeMax, this.ScreenSize.Y - this.NodeSizeMax));

            // store the circle texture passed in
            this.Texture = circleTex;

            ripples = new List<Ripple>();
            soundEffects = new List<SoundEffect>
            {
                (Application.Current as App).Content.Load<SoundEffect>("tweet1"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet2"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet3"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet4"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet5"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet6"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet7"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet8"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet9"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet10"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet11"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet12"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet13"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet14"),
                (Application.Current as App).Content.Load<SoundEffect>("tweet15")
            };
             
            soundEffect = (Application.Current as App).Content.Load<SoundEffect>("slowbullet");

            AccentColour = (System.Windows.Media.Color)Application.Current.Resources["PhoneAccentColor"];
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; protected set; }

        /// <summary>
        /// Gets or sets the Maximum value of possible node size (which is based on connectedness)
        /// </summary>
        /// <value>
        /// The maximum node size.
        /// </value>
        protected int NodeSizeMax { get; set; }

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        /// <value>
        /// The texture.
        /// </value>
        protected Texture2D Texture { get; set; }

        /// <summary>
        /// Gets the rand.
        /// </summary>
        protected Random Rand { get; private set; }

        /// <summary>
        /// Gets the size of the screen.
        /// </summary>
        /// <value>
        /// The size of the screen.
        /// </value>
        protected Microsoft.Xna.Framework.Point ScreenSize { get; private set; }

        public System.Windows.Media.Color AccentColour { get; set; }

        /// <summary>
        /// Where the main code for updating the nodes goes
        /// </summary>
        public override void Update()
        {
            // Randomly move one node every now and again
            if (!this.DisableVirtualMovement && this.Rand.Next(1, 500) == 499)
            {
                this.X = this.Rand.NextDouble() * 480;
                this.Y = this.Rand.NextDouble() * 800;
            }

            if (Math.Abs(this.CurrentX - this.X) > 0.5 || Math.Abs(this.CurrentY - this.Y) > 0.5)
            {
                const double FollowSpeed = 0.06;

                // the inertia calculation. Only move the ellipse a fraction of the distance in the direction of the lead node
                this.CurrentX += (float)((this.X - this.CurrentX) * FollowSpeed);
                this.CurrentY += (float)((this.Y - this.CurrentY) * FollowSpeed);
            }
        }

        /// <summary>
        /// Use the connectedness value to alter the visual attributes of the node
        /// </summary>
        public override void FinishConnection()
        {
            // calculate the node size from NormalisedConnectedness
            this.Size = (int)Global.Map(this.NormalisedConnectedness, 0, 1, NodeSizeMin, this.NodeSizeMax);
            base.FinishConnection();
        }

        /// <summary>
        /// Draw any textures for the node using the visual attributes
        /// </summary>
        /// <param name="sb">A sprite batch that has already called Begin and not yet called End</param>
        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(this.Texture, new Rectangle((int)this.CurrentX - (this.Size / 2), (int)this.CurrentY - (this.Size / 2), this.Size, this.Size), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
        }

        public void CreateRipple()
        {
            CreateRipple(GetXnaColour(AccentColour));
        }

        public void CreateRipple(Color rippleColour)
        {
            Ripple ripple = null;
            foreach (var existingRipple in ripples)
            {
                if (!existingRipple.IsVisible)
                {
                    ripple = existingRipple;
                    ripple.Init(rippleColour);
                    break;
                }
            }

            if (ripple == null)
            {
                ripple = new Ripple(rippleColour);
                ripples.Add(ripple);
            }

            int randomSoundEffectIndex = Rand.Next(15);
            soundEffects[randomSoundEffectIndex].Play();
        }

        protected Microsoft.Xna.Framework.Color GetXnaColour(System.Windows.Media.Color color)
        {
            return new Microsoft.Xna.Framework.Color(
                color.R * (byte)(color.A / 255.0f),
                color.G * (byte)(color.A / 255.0f),
                color.B * (byte)(color.A / 255.0f),
                color.A);
        }
    }
}