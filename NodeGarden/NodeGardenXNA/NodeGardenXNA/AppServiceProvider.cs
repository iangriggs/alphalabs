//-----------------------------------------------------------------------
// <copyright file="AppServiceProvider.cs" company="Studio Arcade Ltd">
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

namespace NodeGardenXNA
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements IServiceProvider for the application. This type is exposed through the App.Services
    /// property and can be used for ContentManagers or other types that need access to an IServiceProvider.
    /// </summary>
    public class AppServiceProvider : IServiceProvider
    {
        /// <summary>
        /// A map of service type to the services themselves
        /// </summary>
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// Adds a new service to the service provider.
        /// </summary>
        /// <param name="serviceType">The type of service to add.</param>
        /// <param name="service">The service object itself.</param>
        public void AddService(Type serviceType, object service)
        {
            // Validate the input
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            if (!serviceType.IsAssignableFrom(service.GetType()))
            {
                throw new ArgumentException("service does not match the specified serviceType");
            }

            // Add the service to the dictionary
            this.services.Add(serviceType, service);
        }

        /// <summary>
        /// Gets a service from the service provider.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve.</param>
        /// <returns>The service object registered for the specified type..</returns>
        public object GetService(Type serviceType)
        {
            // Validate the input
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            // Retrieve the service from the dictionary
            return this.services[serviceType];
        }

        /// <summary>
        /// Removes a service from the service provider.
        /// </summary>
        /// <param name="serviceType">The type of service to remove.</param>
        public void RemoveService(Type serviceType)
        {
            // Validate the input
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            // Remove the service from the dictionary
            this.services.Remove(serviceType);
        }
    }
}
