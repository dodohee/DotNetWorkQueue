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
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.ConsumerMethod;
using DotNetWorkQueue.IntegrationTests.Shared.ProducerMethod;
using DotNetWorkQueue.Transport.Redis.Basic;
using DotNetWorkQueue.Transport.Redis.IntegrationTests;
using Xunit;

namespace DotNetWorkQueue.Transport.Redis.Linq.Integration.Tests.ConsumerMethod
{
    [Collection("Redis")]
    public class SimpleMethodConsumer
    {
        [Theory]
        [InlineData(1000, 0, 240, 5, ConnectionInfoTypes.Linux, LinqMethodTypes.Dynamic),
        InlineData(50, 5, 200, 10, ConnectionInfoTypes.Linux, LinqMethodTypes.Dynamic),
        InlineData(10, 5, 180, 7, ConnectionInfoTypes.Linux, LinqMethodTypes.Dynamic),
        InlineData(1000, 0, 240, 25, ConnectionInfoTypes.Linux, LinqMethodTypes.Dynamic),
        InlineData(1000, 0, 240, 5, ConnectionInfoTypes.Windows, LinqMethodTypes.Dynamic),
        InlineData(50, 5, 200, 10, ConnectionInfoTypes.Windows, LinqMethodTypes.Dynamic),
        InlineData(10, 5, 180, 7, ConnectionInfoTypes.Windows, LinqMethodTypes.Dynamic),
        InlineData(1000, 0, 240, 25, ConnectionInfoTypes.Windows, LinqMethodTypes.Dynamic),
        InlineData(1000, 0, 240, 5, ConnectionInfoTypes.Linux, LinqMethodTypes.Compiled),
        InlineData(50, 5, 200, 10, ConnectionInfoTypes.Linux, LinqMethodTypes.Compiled),
        InlineData(10, 5, 180, 7, ConnectionInfoTypes.Linux, LinqMethodTypes.Compiled),
        InlineData(1000, 0, 240, 25, ConnectionInfoTypes.Linux, LinqMethodTypes.Compiled),
        InlineData(1000, 0, 240, 5, ConnectionInfoTypes.Windows, LinqMethodTypes.Compiled),
        InlineData(50, 5, 200, 10, ConnectionInfoTypes.Windows, LinqMethodTypes.Compiled),
        InlineData(10, 5, 180, 7, ConnectionInfoTypes.Windows, LinqMethodTypes.Compiled),
        InlineData(1000, 0, 240, 25, ConnectionInfoTypes.Windows, LinqMethodTypes.Compiled)]
        public void Run(int messageCount, int runtime, int timeOut, int workerCount, ConnectionInfoTypes type, LinqMethodTypes linqMethodTypes)
        {
            var queueName = GenerateQueueName.Create();
            var logProvider = LoggerShared.Create(queueName, GetType().Name);
            var connectionString = new ConnectionInfo(type).ConnectionString;
            using (
                var queueCreator =
                    new QueueCreationContainer<RedisQueueInit>(
                        serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
            {
                try
                {
                    var id = Guid.NewGuid();
                    var producer = new ProducerMethodShared();
                    if (linqMethodTypes == LinqMethodTypes.Compiled)
                    {
                        producer.RunTestCompiled<RedisQueueInit>(queueName,
                            connectionString, false, messageCount, logProvider, Helpers.GenerateData,
                            Helpers.Verify, false, id, GenerateMethod.CreateCompiled, runtime);
                    }
                    else
                    {
                        producer.RunTestDynamic<RedisQueueInit>(queueName,
                           connectionString, false, messageCount, logProvider, Helpers.GenerateData,
                           Helpers.Verify, false, id, GenerateMethod.CreateDynamic, runtime);
                    }

                    var consumer = new ConsumerMethodShared();
                    consumer.RunConsumer<RedisQueueInit>(queueName, connectionString, false, logProvider,
                        runtime, messageCount,
                        workerCount, timeOut, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(12), id);

                    new VerifyQueueRecordCount(queueName, connectionString).Verify(0, false);
                }
                finally
                {
                    using (
                        var oCreation =
                            queueCreator.GetQueueCreation<RedisQueueCreation>(queueName,
                                connectionString)
                        )
                    {
                        oCreation.RemoveQueue();
                    }
                }
            }
        }
    }
}
