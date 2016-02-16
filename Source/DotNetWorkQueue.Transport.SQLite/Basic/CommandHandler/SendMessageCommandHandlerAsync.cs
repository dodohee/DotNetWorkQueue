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
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Exceptions;
using DotNetWorkQueue.Serialization;
using DotNetWorkQueue.Transport.SQLite.Basic.Command;

namespace DotNetWorkQueue.Transport.SQLite.Basic.CommandHandler
{
    /// <summary>
    /// Sends a message to the queue
    /// </summary>
    internal class SendMessageCommandHandlerAsync : ICommandHandlerWithOutputAsync<SendMessageCommand, long>
    {
        private readonly TableNameHelper _tableNameHelper;
        private readonly ICompositeSerialization _serializer;
        private bool? _messageExpirationEnabled;
        private readonly IHeaders _headers;
        private readonly Lazy<SqLiteMessageQueueTransportOptions> _options;
        private readonly SqLiteCommandStringCache _commandCache;
        private readonly TransportConfigurationSend _configurationSend;
        private readonly IGetTime _getTime;
        private readonly ISqLiteTransactionFactory _transactionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessageCommandHandler" /> class.
        /// </summary>
        /// <param name="tableNameHelper">The table name helper.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="optionsFactory">The options factory.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="commandCache">The command cache.</param>
        /// <param name="configurationSend">The configuration send.</param>
        /// <param name="getTimeFactory">The get time factory.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        public SendMessageCommandHandlerAsync(TableNameHelper tableNameHelper,
            ICompositeSerialization serializer,
            ISqLiteMessageQueueTransportOptionsFactory optionsFactory,
            IHeaders headers,
            SqLiteCommandStringCache commandCache, 
            TransportConfigurationSend configurationSend,
            IGetTimeFactory getTimeFactory,
            ISqLiteTransactionFactory transactionFactory)
        {
            Guard.NotNull(() => tableNameHelper, tableNameHelper);
            Guard.NotNull(() => serializer, serializer);
            Guard.NotNull(() => optionsFactory, optionsFactory);
            Guard.NotNull(() => headers, headers);
            Guard.NotNull(() => commandCache, commandCache);
            Guard.NotNull(() => configurationSend, configurationSend);
            Guard.NotNull(() => getTimeFactory, getTimeFactory);

            _tableNameHelper = tableNameHelper;
            _serializer = serializer;
            _options = new Lazy<SqLiteMessageQueueTransportOptions>(optionsFactory.Create);
            _headers = headers;
            _commandCache = commandCache;
            _configurationSend = configurationSend;
            _getTime = getTimeFactory.Create();
            _transactionFactory = transactionFactory;
        }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="commandSend">The command.</param>
        /// <returns></returns>
        /// <exception cref="DotNetWorkQueueException">Failed to insert record - the ID of the new record returned by SQLite was 0</exception>
        public async Task<long> Handle(SendMessageCommand commandSend)
        {
            if (!DatabaseExists.Exists(_configurationSend.ConnectionInfo.ConnectionString))
            {
                return 0;
            }

            if (!_messageExpirationEnabled.HasValue)
            {
                _messageExpirationEnabled = _options.Value.EnableMessageExpiration ||
                                            _options.Value.QueueType == QueueTypes.RpcReceive ||
                                            _options.Value.QueueType == QueueTypes.RpcSend;
            }

            using (var connection = new SQLiteConnection(_configurationSend.ConnectionInfo.ConnectionString))
            {
                connection.Open();

                var expiration = TimeSpan.Zero;
                if (_messageExpirationEnabled.Value)
                {
                    expiration = MessageExpiration.GetExpiration(commandSend, _headers);
                }

                SQLiteCommand commandStatus = null;
                using (var command = GetMainCommand(commandSend, connection))
                {
                    long id;
                    using (var commandMeta = CreateMetaDataRecord(commandSend.MessageData.GetDelay(), expiration,
                        connection,
                        commandSend.MessageToSend, commandSend.MessageData))
                    {
                        if (_options.Value.EnableStatusTable)
                        {
                            commandStatus = CreateStatusRecord(connection, commandSend.MessageToSend,
                                commandSend.MessageData);
                        }

                        using (var trans = _transactionFactory.Create(connection).BeginTransaction())
                        {
                            try
                            {
                                command.Transaction = trans;
                                id = Convert.ToInt64(await command.ExecuteScalarAsync().ConfigureAwait(false));
                                if (id > 0)
                                {
                                    commandMeta.Transaction = trans;
                                    commandMeta.Parameters.Add("@QueueID", DbType.Int64, 8).Value = id;
                                    await commandMeta.ExecuteNonQueryAsync().ConfigureAwait(false);
                                    if (commandStatus != null)
                                    {
                                        commandStatus.Transaction = trans;
                                        commandStatus.Parameters.Add("@QueueID", DbType.Int64, 8).Value = id;
                                        await commandStatus.ExecuteNonQueryAsync().ConfigureAwait(false);
                                    }
                                    trans.Commit();
                                }
                                else
                                {
                                    throw new DotNetWorkQueueException(
                                        "Failed to insert record - the ID of the new record returned by SQLite was 0");
                                }
                            }
                            finally
                            {
                                commandStatus?.Dispose();
                            }
                        }
                    }
                    return id;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Query checked")]
        private SQLiteCommand GetMainCommand(SendMessageCommand commandSend, SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = _commandCache.GetCommand(SqLiteCommandStringTypes.InsertMessageBody);
            var serialization =
                _serializer.Serializer.MessageToBytes(new MessageBody { Body = commandSend.MessageToSend.Body });

            command.Parameters.Add("@body", DbType.Binary, -1);
            command.Parameters["@body"].Value = serialization.Output;

            commandSend.MessageToSend.SetHeader(_headers.StandardHeaders.MessageInterceptorGraph,
                serialization.Graph);

            command.Parameters.Add("@headers", DbType.Binary, -1);
            command.Parameters["@headers"].Value =
                _serializer.InternalSerializer.ConvertToBytes(commandSend.MessageToSend.Headers);
            return command;
        }

        /// <summary>
        /// Creates the status record.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private SQLiteCommand CreateStatusRecord(SQLiteConnection connection, IMessage message,
            IAdditionalMessageData data)
        {
            var command = connection.CreateCommand();
            SendMessage.BuildStatusCommand(command, _tableNameHelper, _headers, data, message, 0, _options.Value, _getTime.GetCurrentUtcDate());
            return command;
        }

        #region Insert Meta data record

        /// <summary>
        /// Creates the meta data record.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="expiration">The expiration.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="message">The message.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private SQLiteCommand CreateMetaDataRecord(TimeSpan? delay, TimeSpan expiration, SQLiteConnection connection,
            IMessage message, IAdditionalMessageData data)
        {
            var command = new SQLiteCommand(connection);
            SendMessage.BuildMetaCommand(command, _tableNameHelper, _headers,
                data, message, 0, _options.Value, delay, expiration, _getTime.GetCurrentUtcDate());
            return command;
        }
        #endregion
    }
}