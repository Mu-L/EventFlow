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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Extensions;
using Microsoft.Extensions.Logging;

namespace EventFlow.ReadStores
{
    public class MultipleAggregateReadStoreManager<TReadStore, TReadModel, TReadModelLocator> :
        ReadStoreManager<TReadStore, TReadModel>
        where TReadStore : IReadModelStore<TReadModel>
        where TReadModel : class, IReadModel
        where TReadModelLocator : IReadModelLocator
    {
        private readonly TReadModelLocator _readModelLocator;

        public MultipleAggregateReadStoreManager(
            ILogger<MultipleAggregateReadStoreManager<TReadStore, TReadModel, TReadModelLocator>> logger,
            IServiceProvider serviceProvider,
            TReadStore readModelStore,
            IReadModelDomainEventApplier readModelDomainEventApplier,
            TReadModelLocator readModelLocator,
            IReadModelFactory<TReadModel> readModelFactory)
            : base(logger, serviceProvider, readModelStore, readModelDomainEventApplier, readModelFactory)
        {
            _readModelLocator = readModelLocator;
        }

        protected override IReadOnlyCollection<ReadModelUpdate> BuildReadModelUpdates(
            IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            var readModelUpdates = (
                from de in domainEvents
                let readModelIds = _readModelLocator.GetReadModelIds(de)
                from rid in readModelIds
                group de by rid into g
                select new ReadModelUpdate(g.Key, g.OrderBy(d => d.Timestamp).ThenBy(d => d.AggregateSequenceNumber).ToList())
                ).ToList();
            return readModelUpdates;
        }

        protected override async Task<ReadModelUpdateResult<TReadModel>> UpdateAsync(
            IReadModelContext readModelContext,
            IReadOnlyCollection<IDomainEvent> domainEvents,
            ReadModelEnvelope<TReadModel> readModelEnvelope,
            CancellationToken cancellationToken)
        {
            var readModel = readModelEnvelope.ReadModel;
            if (readModel == null)
            {
                readModel = await ReadModelFactory.CreateAsync(
                    readModelEnvelope.ReadModelId,
                    cancellationToken)
                    .ConfigureAwait(false);
            }

            await ReadModelDomainEventApplier.UpdateReadModelAsync(
                readModel,
                domainEvents,
                readModelContext,
                cancellationToken)
                .ConfigureAwait(false);

            return readModelEnvelope.AsModifedResult(
                readModel,
                readModelEnvelope.Version.GetValueOrDefault() + 1 // the best we can do
                );
        }
    }
}
