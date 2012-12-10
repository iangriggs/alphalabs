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

namespace NodeGardenXNA
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    using NodeGardenLib;

    /// <summary>
    /// Basic node definition
    /// </summary>
    public abstract class VisualNode : Node
    {
        /// <summary>
        /// each node tracks how connected (distance) it is to other nodes of the maximum Connectedness value
        /// this can be used to Global.Map various values such as node size
        /// </summary>
        private static float maxConnectedness = 0.1f;

        /// <summary>
        /// Gets or sets Position.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2((float)this.X, (float)this.Y);
            }

            set
            {
                this.X = value.X;
                this.Y = value.Y;

                this.CurrentX = value.X;
                this.CurrentY = value.Y;
            }
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
        public float CurrentX { get; set; }

        /// <summary>
        /// Gets or sets the current Y.
        /// </summary>
        /// <value>
        /// The current Y.
        /// </value>
        public float CurrentY { get; set; }

        /// <summary>
        /// Gets the current position.
        /// </summary>
        public Vector2 CurrentPosition
        {
            get
            {
                return new Vector2(this.CurrentX, this.CurrentY);
            }
        }

        /// <summary>
        /// Gets or sets the connectedness.
        /// </summary>
        /// <value>
        /// The connectedness.
        /// </value>
        public float Connectedness { get; set; }

        /// <summary>
        /// Gets or sets the normalized connectedness.
        /// </summary>
        /// <value>
        /// The normalized connectedness.
        /// </value>
        public float NormalisedConnectedness { get; protected set; }

        /// <summary>
        /// Where the main code for updating the nodes goes
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Compute the connectedness value and make a normalized version also create visual connections between nodes
        /// </summary>
        /// <param name="connectedness">The amount of connectedness between this and node2</param>
        /// <param name="node2">The node that is connected to this</param>
        public virtual void ApplyConnection(float connectedness, VisualNode node2)
        {
            // increase the connectedness
            this.Connectedness += connectedness;

            // this allows us to get a reliable value for MaxConnectedness. Used for Mapping the Connectedness value
            if (this.Connectedness > maxConnectedness)
            {
                maxConnectedness = this.Connectedness;
            }

            // create a normalised version of the Connectedness variable
            this.NormalisedConnectedness = Global.Map(this.Connectedness, 0, maxConnectedness, 0, 1);
        }

        /// <summary>
        /// Use the connectedness value to alter the visual attributes of the node
        /// </summary>
        public virtual void FinishConnection()
        {
            // reset the connectedness for the next fram
            this.Connectedness = 0;
        }

        /// <summary>
        /// Draw any textures for the node using the visual attributes
        /// </summary>
        /// <param name="sb">A sprite batch that has already called Begin and not yet called End</param>
        public abstract void Draw(SpriteBatch sb);
    }
}
