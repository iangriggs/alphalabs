//-----------------------------------------------------------------------
// <copyright file="CommType.cs" company="Studio Arcade Ltd">
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
    /// <summary>
    /// Inter-device communication options
    /// </summary>
    public enum CommType
    {
        /// <summary>
        /// UDP communication should be used between clients
        /// </summary>
        Udp,

        /// <summary>
        /// The web should be used for communication
        /// </summary>
        Web
    }
}