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
using System.Reflection;
using EventFlow.EventStores;
using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.Extensions
{
    public static class EventFlowOptionsMetadataProvidersExtensions
    {
        public static IEventFlowOptions AddMetadataProvider<TMetadataProvider>(
            this IEventFlowOptions eventFlowOptions)
            where TMetadataProvider : class, IMetadataProvider
        {
            eventFlowOptions.ServiceCollection
                .AddTransient<IMetadataProvider, TMetadataProvider>();
            return eventFlowOptions;
        }

        public static IEventFlowOptions AddMetadataProviders(
            this IEventFlowOptions eventFlowOptions,
            params Type[] metadataProviderTypes)
        {
            return eventFlowOptions
                .AddMetadataProviders((IEnumerable<Type>) metadataProviderTypes);
        }

        public static IEventFlowOptions AddMetadataProviders(
            this IEventFlowOptions eventFlowOptions,
            Assembly fromAssembly,
            Predicate<Type> predicate = null)
        {
            predicate = predicate ?? (t => true);
            var metadataProviderTypes = fromAssembly
                .GetTypes()
                .Where(IsMetadataProvider)
                .Where(t => !t.HasConstructorParameterOfType(IsMetadataProvider))
                .Where(t => predicate(t));
            return eventFlowOptions.AddMetadataProviders(metadataProviderTypes);
        }

        public static IEventFlowOptions AddMetadataProviders(
            this IEventFlowOptions eventFlowOptions,
            IEnumerable<Type> metadataProviderTypes)
        {
            foreach (var t in metadataProviderTypes)
            {
                if (t.GetTypeInfo().IsAbstract) continue;
                if (!t.IsMetadataProvider())
                {
                    throw new ArgumentException($"Type '{t.PrettyPrint()}' is not an '{typeof(IMetadataProvider).PrettyPrint()}'");
                }

                eventFlowOptions.ServiceCollection.AddTransient(typeof(IMetadataProvider), t);
            }
            return eventFlowOptions;
        }

        private static bool IsMetadataProvider(this Type type)
        {
            return type.IsAssignableTo<IMetadataProvider>();
        }
    }
}