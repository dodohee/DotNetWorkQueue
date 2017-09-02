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
using System.Threading;

namespace DotNetWorkQueue.Transport.Memory.Basic
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="DotNetWorkQueue.IClearExpiredMessages" />
    public class ClearExpiredMessages : IClearExpiredMessages
    {
        /// <summary>
        /// Clears the expired messages from the queue
        /// </summary>
        /// <param name="cancelToken">The cancel token. When signaled, processing should stop as soon as possible</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">This should never execute, since message expiration is not supported</exception>
        public long ClearMessages(CancellationToken cancelToken)
        {
            throw new NotImplementedException("This should never execute, since message expiration is not supported");
        }
    }
}
