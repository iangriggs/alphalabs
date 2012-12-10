//-----------------------------------------------------------------------
// <copyright file="Node.cs" company="Studio Arcade Ltd">
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

    using Newtonsoft.Json;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// A class representing a node in the garden
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Node(string id, double x, double y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        public Node()
        {
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>
        /// The X.
        /// </value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>
        /// The Y.
        /// </value>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        [JsonIgnore]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        [JsonIgnore]
        public TypeOfNode NodeType { get; set; }

        /// <summary>
        /// Deserializes the instance.
        /// </summary>
        /// <param name="serialized">The serialized.</param>
        /// <returns>The deserialized instance</returns>
        public static Node DeSerialize(string serialized)
        {
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => System.Diagnostics.Debug.WriteLine(args.ErrorContext.Error)
            };

            try
            {
                var node = JsonConvert.DeserializeObject<Node>(serialized, settings);

                node.NodeType = TypeOfNode.Other;

                return node;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns>A serialized string of this instance</returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}: {1},{2}", this.Id, this.X, this.Y);
        }
    }
}