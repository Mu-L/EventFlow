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

namespace EventFlow.Configuration
{
    public class LoadedVersionedTypes : ILoadedVersionedTypes
    {
        public LoadedVersionedTypes(
            IEnumerable<Type> jobTypes,
            IEnumerable<Type> commandTypes,
            IEnumerable<Type> eventTypes,
            IEnumerable<Type> sagaTypes,
            IEnumerable<Type> snapshotTypes)
        {
            Jobs = jobTypes.ToList();
            Commands = commandTypes.ToList();
            Events = eventTypes.ToList();
            Sagas = sagaTypes.ToList();
            SnapshotTypes = snapshotTypes.ToList();
        }

        public IReadOnlyCollection<Type> Jobs { get; }
        public IReadOnlyCollection<Type> Commands { get; }
        public IReadOnlyCollection<Type> Events { get; }
        public IReadOnlyCollection<Type> Sagas { get; }
        public IReadOnlyCollection<Type> SnapshotTypes { get; }
    }
}