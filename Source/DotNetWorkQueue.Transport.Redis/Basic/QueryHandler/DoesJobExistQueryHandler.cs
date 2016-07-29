﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2016 Brian Lehnen
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWorkQueue.Transport.Redis.Basic.Lua;
using DotNetWorkQueue.Transport.Redis.Basic.Query;

namespace DotNetWorkQueue.Transport.Redis.Basic.QueryHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class DoesJobExistQueryHandler : IQueryHandler<DoesJobExistQuery, QueueStatus>
    {
        private readonly DoesJobExistLua _lua;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetErrorCountQueryHandler" /> class.
        /// </summary>
        /// <param name="lua">The lua.</param>
        public DoesJobExistQueryHandler(DoesJobExistLua lua)
        {
            Guard.NotNull(() => lua, lua);
            _lua = lua;
        }

        /// <summary>
        /// Handles the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public QueueStatus Handle(DoesJobExistQuery query)
        {
            return _lua.Execute(query.JobName, query.ScheduledTime);
        }
    }
}
