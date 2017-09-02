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

using DotNetWorkQueue.Transport.RelationalDatabase.Basic.Command;
using DotNetWorkQueue.Validation;

namespace DotNetWorkQueue.Transport.RelationalDatabase.Basic.CommandHandler
{
    /// <inheritdoc />
    /// <summary>
    /// Sets the status on the status table
    /// </summary>
    public class SetStatusTableStatusTransactionCommandHandler: ICommandHandler<SetStatusTableStatusTransactionCommand>
    {
        private readonly IPrepareCommandHandler<SetStatusTableStatusTransactionCommand> _prepareCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetStatusTableStatusTransactionCommandHandler"/> class.
        /// </summary>
        /// <param name="prepareCommand">The prepare command.</param>
        public SetStatusTableStatusTransactionCommandHandler(
            IPrepareCommandHandler<SetStatusTableStatusTransactionCommand> prepareCommand)
        {
            Guard.NotNull(() => prepareCommand, prepareCommand);
            _prepareCommand = prepareCommand;
        }
        /// <inheritdoc />
        public void Handle(SetStatusTableStatusTransactionCommand command)
        {
            using (
                var commandSqlUpdateStatusRecord = command.Connection.CreateCommand())
            {
                commandSqlUpdateStatusRecord.Transaction = command.Transaction;
                _prepareCommand.Handle(command, commandSqlUpdateStatusRecord, CommandStringTypes.UpdateStatusRecord);
                commandSqlUpdateStatusRecord.ExecuteNonQuery();
            }
        }
    }
}
