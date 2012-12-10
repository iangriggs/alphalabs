//-----------------------------------------------------------------------
// <copyright file="NodeHub.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenWeb
{
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;

    using NodeGardenLib;

    using SignalR.Hubs;

    /// <summary>
    /// The SignalR hub for relaying messages between clients
    /// </summary>
    [HubName("node")]
    public class NodeHub : Hub
    {
        /// <summary>
        /// Sends the specified message.
        /// This is a really simple use of SignalR - we have a single method
        /// and anything sent to it will be resent to all clients (including the one that sent it)
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(string message)
        {
            SaveToDb(message);

            // broadcast to all connected devices
            Clients.received(message);
        }

        /// <summary>
        /// Saves to database.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void SaveToDb(string message)
        {
            string connStr = ConfigurationManager.ConnectionStrings["azureDbServer"].ConnectionString;

            var conn = new SqlConnection(connStr);

            var sqlQry = "INSERT INTO dbo.log (DeviceId, X, Y, Tag) VALUES (@id,@x,@y,@tag)";

            try
            {
                conn.Open();

                var cmd = new SqlCommand(sqlQry, conn);

                var msg = Node.DeSerialize(message);

                cmd.Parameters.AddWithValue("@id", msg.Id);
                cmd.Parameters.AddWithValue("@x", msg.X);
                cmd.Parameters.AddWithValue("@y", msg.Y);
                cmd.Parameters.AddWithValue("@tag", msg.Tag ?? string.Empty);

                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                string msg = "Insert Error: " + ex.Message;

                Debug.WriteLine(msg);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}