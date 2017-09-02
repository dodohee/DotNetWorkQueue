﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2017 Brian Lehnen
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

namespace DotNetWorkQueue.JobScheduler
{
    /// <summary>
    /// Holds registration information for the job scheduler
    /// </summary>
    public class JobQueueContainerRegistrations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobQueueContainerRegistrations"/> class.
        /// </summary>
        /// <param name="queueCreationRegistrations">The queue creation registrations.</param>
        /// <param name="queueRegistrations">The queue registrations.</param>
        public JobQueueContainerRegistrations(Action<IContainer> queueCreationRegistrations,
            Action<IContainer> queueRegistrations)
        {
            if (queueCreationRegistrations != null)
            {
                QueueCreationRegistrations = queueCreationRegistrations;
            }
            else
            {
                QueueCreationRegistrations = x => { };
            }

            if (queueRegistrations != null)
            {
                QueueRegistrations = queueRegistrations;
            }
            else
            {
                QueueRegistrations = x => { };
            }
        }
        /// <summary>
        /// Gets the queue creation registrations.
        /// </summary>
        /// <value>
        /// The queue creation registrations.
        /// </value>
        public Action<IContainer> QueueCreationRegistrations { get; }
        /// <summary>
        /// Gets the queue registrations.
        /// </summary>
        /// <value>
        /// The queue registrations.
        /// </value>
        public Action<IContainer> QueueRegistrations { get; }
    }
}
