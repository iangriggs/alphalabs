//-----------------------------------------------------------------------
// <copyright file="Connection.cs" company="Studio Arcade Ltd">
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
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Abstract connection
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        /// Forms the connection.
        /// </summary>
        /// <param name="node1">The node1.</param>
        /// <param name="node2">The node2.</param>
        /// <param name="distance">The distance.</param>
        public abstract void FormConnection(VisualNode node1, VisualNode node2, float distance);

        /// <summary>
        /// Breaks the connection.
        /// </summary>
        /// <param name="node1">The node1.</param>
        /// <param name="node2">The node2.</param>
        public abstract void BreakConnection(VisualNode node1, VisualNode node2);

        /// <summary>
        /// Draws the specified sprite batch.
        /// </summary>
        /// <param name="sb">The sprite batch.</param>
        public abstract void Draw(SpriteBatch sb);
    }
}
