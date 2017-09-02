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

namespace DotNetWorkQueue.Transport.Redis.Basic
{
    /// <summary>
    /// Holds information indicating if an existing message should have it's queue delay time increased
    /// </summary>
    internal class RedisQueueDelay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedisQueueDelay"/> class.
        /// </summary>
        /// <param name="increaseDelay">The increase delay.</param>
        public RedisQueueDelay(TimeSpan increaseDelay)
        {
            IncreaseDelay = increaseDelay;
        }
        /// <summary>
        /// Gets the increase delay.
        /// </summary>
        /// <value>
        /// The increase delay.
        /// </value>
        public TimeSpan IncreaseDelay { get; }
    }
}
