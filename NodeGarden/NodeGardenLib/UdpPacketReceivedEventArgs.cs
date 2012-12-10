//-----------------------------------------------------------------------
// <copyright file="UdpPacketReceivedEventArgs.cs" company="Microsoft Corporation">
// This code is made available under the Ms-PL or GPL as appropriate.
// Please see LICENSE.txt for more details
//  Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
//  Use of this sample source code is subject to the terms of the Microsoft license 
//  agreement under which you licensed this sample source code and is provided AS-IS.
//  If you did not accept the terms of the license agreement, you are not authorized 
//  to use this sample source code.  For the terms of the license, please see the 
//  license agreement between you and Microsoft.
//
//  To see all Code Samples for Windows Phone, visit http://go.microsoft.com/fwlink/?LinkID=219604 
// </copyright>
//-----------------------------------------------------------------------

namespace NodeGardenLib
{
    using System;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Event arguments passed when a UDP packet is received
    /// </summary>
    public class UdpPacketReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPacketReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="source">The source.</param>
        public UdpPacketReceivedEventArgs(byte[] data, IPEndPoint source)
        {
            this.Message = Encoding.UTF8.GetString(data, 0, data.Length);
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IPEndPoint Source { get; set; }
    }
}