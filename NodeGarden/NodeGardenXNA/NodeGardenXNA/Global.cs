//-----------------------------------------------------------------------
// <copyright file="Global.cs" company="Studio Arcade Ltd">
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
    /// <summary>
    /// Global values and settings
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Minimum distance between 2 nodes for a connection
        /// </summary>
        private static int minDist = 300;

        /// <summary>
        /// Number of nodes
        /// </summary>
        private static int nodeNum = 1;

        /// <summary>
        /// Initializes static members of the <see cref="Global"/> class.
        /// </summary>
        static Global()
        {
            TouchPointSize = 50;
            NumberOfLines = 400;
        }

        /// <summary>
        /// Gets the minimum distance between nodes for which there is some connectedness.
        /// </summary>
        public static int MinDist
        {
            get { return minDist; }
            private set { minDist = value; }
        }

        /// <summary>
        /// Gets the size of the touch point.
        /// </summary>
        /// <value>
        /// The size of the touch point.
        /// </value>
        public static float TouchPointSize { get; private set; }

        /// <summary>
        /// Gets or sets the number of nodes.
        /// </summary>
        /// <value>
        /// The node number.
        /// </value>
        public static int InitialNumberOfNodes
        {
            get
            {
                return nodeNum;
            }

            set
            {
                nodeNum = value;

                // when there are fewer nodes the minimum distance is larger
                MinDist = 300 - (3 * InitialNumberOfNodes);
            }
        }

        /// <summary>
        /// Gets or sets the number of lines.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public static int NumberOfLines { get; set; }

        /// <summary>
        /// Map a value from one range to another
        /// </summary>
        /// <param name="value">The value to map</param>
        /// <param name="start1">Original range start</param>
        /// <param name="end1">Original range end</param>
        /// <param name="start2">New range start</param>
        /// <param name="end2">New range end</param>
        /// <returns>value mapped to new range</returns>
        public static float Map(float value, float start1, float end1, float start2, float end2)
        {
            float a = (value - start1) / (end1 - start1);
            return start2 + (a * (end2 - start2));
        }
    }
}