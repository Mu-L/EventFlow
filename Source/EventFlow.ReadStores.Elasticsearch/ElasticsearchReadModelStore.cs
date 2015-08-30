﻿// The MIT License (MIT)
//
// Copyright (c) 2015 Rasmus Mikkelsen
// https://github.com/rasmus/EventFlow
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
using Elasticsearch.Net;
using EventFlow.Aggregates;
using EventFlow.Logs;
using Nest;

namespace EventFlow.ReadStores.Elasticsearch
{
    public class ElasticsearchReadModelStore<TReadModel> : ReadModelStore<TReadModel>,
        IElasticsearchReadModelStore<TReadModel>
        where TReadModel : class, IElasticsearchReadModel, new()
    {
        private readonly IElasticClient _elasticClient;
        private readonly IReadModelDescriptionProvider _readModelDescriptionProvider;

        public ElasticsearchReadModelStore(
            ILog log,
            IElasticClient elasticClient,
            IReadModelDescriptionProvider readModelDescriptionProvider)
            : base(log)
        {
            _elasticClient = elasticClient;
            _readModelDescriptionProvider = readModelDescriptionProvider;
        }

        public override async Task<ReadModelEnvelope<TReadModel>> GetAsync(
            string id,
            CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            var readModelEnvelopes = await Task.WhenAll(readModelDescription.IndexNames.Select(i => GetFromIndexAsync(id, i))).ConfigureAwait(false);
            var readModelEnvelope = readModelEnvelopes.SingleOrDefault(e => !e.IsEmpty);
            return readModelEnvelope ?? ReadModelEnvelope<TReadModel>.Empty;
        }

        private async Task<ReadModelEnvelope<TReadModel>> GetFromIndexAsync(
            string id,
            IndexName indexName)
        {
            var getResponse = await _elasticClient.GetAsync<TReadModel>(
                id,
                indexName.Value)
                .ConfigureAwait(false);

            if (!getResponse.IsValid || !getResponse.Found)
            {
                return ReadModelEnvelope<TReadModel>.Empty;
            }

            var version = long.Parse(getResponse.Version);
            return ReadModelEnvelope<TReadModel>.With(getResponse.Source, version);
        }

        public override Task DeleteAllAsync(
            CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            return Task.WhenAll(readModelDescription.IndexNames.Select(i => _elasticClient.DeleteIndexAsync(i.Value, d => d)));
        }

        public override Task UpdateAsync(
            IReadOnlyCollection<ReadModelUpdate> readModelUpdates,
            IReadModelContext readModelContext,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken, Task<ReadModelEnvelope<TReadModel>>> updateReadModel,
            CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
            return Task.WhenAll(readModelDescription.IndexNames.Select(i => UpdateIndexAsync(readModelUpdates, readModelContext, updateReadModel, i, cancellationToken)));
        }

        private async Task UpdateIndexAsync(
            IEnumerable<ReadModelUpdate> readModelUpdates,
            IReadModelContext readModelContext,
            Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken, Task<ReadModelEnvelope<TReadModel>>> updateReadModel,
            IndexName indexName,
            CancellationToken cancellationToken)
        {
            foreach (var readModelUpdate in readModelUpdates)
            {
                var response = await _elasticClient.GetAsync<TReadModel>(
                    readModelUpdate.ReadModelId,
                    indexName.Value)
                    .ConfigureAwait(false);

                var readModelEnvelope = response.Found
                    ? ReadModelEnvelope<TReadModel>.With(response.Source, long.Parse(response.Version))
                    : ReadModelEnvelope<TReadModel>.Empty;

                readModelEnvelope = await updateReadModel(readModelContext, readModelUpdate.DomainEvents, readModelEnvelope, cancellationToken).ConfigureAwait(false);

                readModelEnvelope.ReadModel.Id = readModelUpdate.ReadModelId;

                await _elasticClient.IndexAsync(
                    readModelEnvelope.ReadModel,
                    d => d
                        .Id(readModelUpdate.ReadModelId)
                        .Index(indexName.Value)
                        .Version(readModelEnvelope.Version.GetValueOrDefault())
                        .VersionType(VersionType.ExternalGte))
                    .ConfigureAwait(false);
            }
        }
    }
}
