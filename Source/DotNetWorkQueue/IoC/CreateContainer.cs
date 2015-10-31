﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2015 Brian Lehnen
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// ---------------------------------------------------------------------

using System;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Exceptions;
using DotNetWorkQueue.Logging;
using DotNetWorkQueue.Queue;
using SimpleInjector;
namespace DotNetWorkQueue.IoC
{
    internal static class ContainerLocker
    {
        /// <summary>
        /// The locker for creating new containers
        /// </summary>
        public static readonly object Locker = new object();
    }

    /// <summary>
    /// Creates the IoC container
    /// </summary>
    /// <typeparam name="T">The transport registration module</typeparam>
    internal class CreateContainer<T>: ICreateContainer<T> 
        where T : ITransportInit, new()
    {
        /// <summary>
        /// Creates the IoC container
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="registerService">The user defined service overrides.</param>
        /// <param name="queue">The queue.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="register">The transport init module.</param>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="registerServiceInternal">The internal registrations.</param>
        /// <returns>
        /// a new container
        /// </returns>
        public IContainer Create(QueueContexts queueType, Action<IContainer> registerService,
            string queue, string connection, T register, ConnectionTypes connectionType, Action<IContainer> registerServiceInternal)
        {
            lock (ContainerLocker.Locker) //thread safe issue with registration of decorators; should not be needed, but have found no other solution
            {
                var container = new Container();
                var containerWrapper = new ContainerWrapper(container);

                containerWrapper.Register(() => new QueueContext(queueType), LifeStyles.Singleton);

                var type = GetRegistrationType(register);

                if (!string.IsNullOrWhiteSpace(queue) && !string.IsNullOrWhiteSpace(connection))
                {
                    ComponentRegistration.RegisterDefaults(containerWrapper, type);
                }
                else
                {
                    ComponentRegistration.RegisterDefaultsForScheduler(containerWrapper);
                }

                // Enable overriding
                container.Options.AllowOverridingRegistrations = true;

                //register transport specific objects
                register.RegisterImplementations(containerWrapper, type);

                //register our internal overrides from outside this container
                registerServiceInternal(containerWrapper);

                //register caller overrides
                registerService(containerWrapper);

                //register conditional fallbacks
                container.Options.AllowOverridingRegistrations = false;
                ComponentRegistration.RegisterFallbacks(containerWrapper, type);

                //allow specific warnings to be disabled
                register.SuppressWarningsIfNeeded(containerWrapper, type);

                //create the connection
                if (!string.IsNullOrWhiteSpace(queue) && !string.IsNullOrWhiteSpace(connection))
                {
                    ComponentRegistration.SuppressWarningsIfNeeded(containerWrapper, type);
                    CreateConnection.Create(containerWrapper, queue, connection);
                }

                //set the log provider, if one was provided
                //if no explicit log provider was set, we will use liblog defaults
                var logProvider = container.GetInstance<ILogProvider>();
                if (!(logProvider is NoSpecifiedLogProvider))
                {
                    LogProvider.SetCurrentLogProvider(logProvider);
                    var factory = container.GetInstance<ILogFactory>();
                    factory.Create();
                }

                //verify the container configuration.
                container.Verify();

                //allow the transport to set defaults if needed
                register.SetDefaultsIfNeeded(containerWrapper, type, connectionType);

                return containerWrapper;
            }
        }

        /// <summary>
        /// Creates the IoC container
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="registerService">The user defined service overrides.</param>
        /// <param name="register">The transport init module.</param>
        /// <param name="registerServiceInternal">The internal registrations.</param>
        /// <returns>
        /// a new container
        /// </returns>
        public IContainer Create(QueueContexts queueType, Action<IContainer> registerService, T register, Action<IContainer> registerServiceInternal)
        {
            return Create(queueType, registerService, string.Empty, string.Empty, register, ConnectionTypes.NotSpecified, registerServiceInternal);
        }

        /// <summary>
        /// Gets the type of the registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>
        /// Default, Send, Receive
        /// </returns>
        /// <exception cref="DotNetWorkQueueException">A transport init module should inherit from ITransportInitSend, ITransportInitReceive or ITransportInitDuplex</exception>
        private RegistrationTypes GetRegistrationType(ITransportInit registration)
        {
            if (registration is ITransportInitDuplex ||
                (registration is ITransportInitReceive && registration is ITransportInitSend))
            {
                return RegistrationTypes.Send | RegistrationTypes.Receive;
            }
            if (registration is ITransportInitReceive)
            {
                return RegistrationTypes.Receive;
            }
            if (registration is ITransportInitSend)
            {
                return RegistrationTypes.Send;
            }
            throw new DotNetWorkQueueException(
                "A transport init module should inherit from ITransportInitSend, ITransportInitReceive or ITransportInitDuplex");
        }

        /// <summary>
        /// Creates a new singleton connection registration
        /// </summary>
        internal static class CreateConnection
        {
            /// <summary>
            /// Creates a connection information object for receiving messages.
            /// </summary>
            /// <param name="rootContainer">The root container.</param>
            /// <param name="queue">The queue.</param>
            /// <param name="connection">The connection.</param>
            public static void Create(IContainer rootContainer, string queue, string connection)
            {
                var createScope = rootContainer.GetInstance<ICreateConnectionFactory>();
                createScope.Register(queue, connection);
            }
        }
    }
}