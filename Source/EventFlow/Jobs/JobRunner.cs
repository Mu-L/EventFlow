// The MIT License (MIT)
// 
// Copyright (c) 2015-2025 Rasmus Mikkelsen
// https://github.com/eventflow/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Core;
using EventFlow.Exceptions;

namespace EventFlow.Jobs
{
    public class JobRunner : IJobRunner
    {
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IServiceProvider _serviceProvider;

        public JobRunner(
            IServiceProvider serviceProvider,
            IJobDefinitionService jobDefinitionService,
            IJsonSerializer jsonSerializer)
        {
            _serviceProvider = serviceProvider;
            _jobDefinitionService = jobDefinitionService;
            _jsonSerializer = jsonSerializer;
        }

        public Task ExecuteAsync(string jobName, int version, string json, CancellationToken cancellationToken)
        {
            if (!_jobDefinitionService.TryGetDefinition(jobName, version, out var jobDefinition))
            {
                throw UnknownJobException.With(jobName, version);
            }

            var executeCommandJob = (IJob) _jsonSerializer.Deserialize(json, jobDefinition.Type);
            return executeCommandJob.ExecuteAsync(_serviceProvider, cancellationToken);
        }
    }
}
