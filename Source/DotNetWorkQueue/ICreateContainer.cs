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

namespace DotNetWorkQueue
{
    /// <summary>
    /// Creates new instance of <see cref="IContainer"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICreateContainer<in T> 
        where T : ITransportInit, new()
    {
        /// <summary>
        /// Creates the IoC container
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="registerService">The user defined service overrides</param>
        /// <param name="queue">The queue.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="register">The transport registration module.</param>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="registerServiceInternal">The internal service overrides</param>
        /// <returns></returns>
        IContainer Create(QueueContexts queueType, Action<IContainer> registerService, string queue, string connection, T register,
            ConnectionTypes connectionType, Action<IContainer> registerServiceInternal);

        /// <summary>
        /// Creates the IoC container
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="registerService">The user defined service overrides.</param>
        /// <param name="register">The transport registration module.</param>
        /// <param name="registerServiceInternal">The internal service overrides.</param>
        /// <returns></returns>
        IContainer Create(QueueContexts queueType, Action<IContainer> registerService, T register,
            Action<IContainer> registerServiceInternal);
    }
}