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

using DotNetWorkQueue.Transport.RelationalDatabase;
using DotNetWorkQueue.Transport.SqlServer.Basic;
using DotNetWorkQueue.Validation;
using Polly;

namespace DotNetWorkQueue.Transport.SqlServer.Decorator
{
    /// <inheritdoc />
    internal class RetryCommandHandlerOutputDecorator<TCommand, TOutput> : ICommandHandlerWithOutput<TCommand, TOutput>
    {
        private readonly ICommandHandlerWithOutput<TCommand, TOutput> _decorated;
        private readonly IPolicies _policies;
        private Policy _policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryCommandHandlerOutputDecorator{TCommand,TOutput}" /> class.
        /// </summary>
        /// <param name="decorated">The decorated.</param>
        /// <param name="policies">The policies.</param>
        public RetryCommandHandlerOutputDecorator(ICommandHandlerWithOutput<TCommand, TOutput> decorated,
            IPolicies policies)
        {
            Guard.NotNull(() => decorated, decorated);
            Guard.NotNull(() => policies, policies);

            _decorated = decorated;
            _policies = policies;
        }

        /// <inheritdoc />
        public TOutput Handle(TCommand command)
        {
            Guard.NotNull(() => command, command);

            if (_policy == null)
            {
                _policies.Registry.TryGet(TransportPolicyDefinitions.RetryCommandHandler, out _policy);
            }
            if (_policy == null) return _decorated.Handle(command);
            var result = _policy.ExecuteAndCapture(() => _decorated.Handle(command));
            if (result.FinalException != null)
                throw result.FinalException;
            return result.Result;
        }
    }
}
