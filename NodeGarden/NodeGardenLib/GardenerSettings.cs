//-----------------------------------------------------------------------
// <copyright file="GardenerSettings.cs" company="Studio Arcade Ltd">
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
    using System.Windows.Media;

    /// <summary>
    /// Settings used to configure a NodeGardener
    /// </summary>
    public class GardenerSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable color detection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if color detection should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableColorDetection { get; set; }

        /// <summary>
        /// Gets or sets the color to detect.
        /// </summary>
        /// <value>
        /// The color to detect.
        /// </value>
        public Color ColorToDetect { get; set; }

        /// <summary>
        /// Gets or sets the color detection threshold.
        /// </summary>
        /// <value>
        /// The color detection threshold.
        /// </value>
        public int ColorDetectionThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable image detection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if image detection should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableImageDetection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable shake detection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if shake detection should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableShakeDetection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable noise detection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if noise detection should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableNoiseDetection { get; set; }

        /// <summary>
        /// Gets or sets the noise threshold.
        /// </summary>
        /// <value>
        /// The noise threshold.
        /// </value>
        public int NoiseThreshold { get; set; }

        /// <summary>
        /// Gets or sets the duration of the noise, over the specified volume, which will trigger detection.
        /// Value is in tenths of a second.
        /// Defaults to 3
        /// </summary>
        /// <value>
        /// The duration of the noise.
        /// </value>
        public int NoiseDuration { get; set; }

        /// <summary>
        /// Gets or sets the view finder brush.
        /// </summary>
        /// <value>
        /// The view finder brush.
        /// </value>
        public VideoBrush ViewFinderBrush { get; set; }

        /// <summary>
        /// Gets or sets the view finder brush transform.
        /// </summary>
        /// <value>
        /// The view finder brush transform.
        /// </value>
        public RotateTransform ViewFinderBrushTransform { get; set; }

        /// <summary>
        /// Gets or sets the type of communication used.
        /// </summary>
        /// <value>
        /// The type of communication.
        /// </value>
        public CommType CommType { get; set; }
    }
}
