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
using DotNetWorkQueue.Messages;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit;

namespace DotNetWorkQueue.Tests.Messages
{
    public class RegisterMessagesTests
    {
        [Fact]
        public void Register()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

            var messageHandlerRegistration = fixture.Create<IMessageHandlerRegistration>();
            fixture.Inject(messageHandlerRegistration);
            var test = fixture.Create<RegisterMessages>();

            void Action(IReceivedMessage<FakeMessage> message, IWorkerNotification worker)
            {
            }

            test.Register((Action<IReceivedMessage<FakeMessage>, IWorkerNotification>) Action);

            messageHandlerRegistration.Received(1).Set((Action<IReceivedMessage<FakeMessage>, IWorkerNotification>) Action);
        }
    }
}
