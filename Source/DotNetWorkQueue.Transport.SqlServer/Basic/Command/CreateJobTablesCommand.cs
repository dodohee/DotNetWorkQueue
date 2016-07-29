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
using DotNetWorkQueue.Transport.SqlServer.Schema;

namespace DotNetWorkQueue.Transport.SqlServer.Basic.Command
{
    /// <summary>
    /// Creates tables for storing job info
    /// </summary>
    public class CreateJobTablesCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateJobTablesCommand"/> class.
        /// </summary>
        /// <param name="tables">The tables.</param>
        public CreateJobTablesCommand(List<Table> tables)
        {
            Guard.NotNull(() => tables, tables);
            Tables = tables;
        }
        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        public List<Table> Tables { get; private set; }
    }
}
