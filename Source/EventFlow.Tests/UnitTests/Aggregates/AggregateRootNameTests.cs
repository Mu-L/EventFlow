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
using EventFlow.Aggregates;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Aggregates;
using FluentAssertions;
using NUnit.Framework;

namespace EventFlow.Tests.UnitTests.Aggregates
{
    [Category(Categories.Unit)]
    public class AggregateRootNameTests
    {
        public class MyFancyAggregate : AggregateRoot<MyFancyAggregate, ThingyId>
        {
            public MyFancyAggregate(ThingyId id) : base(id) { }
        }

        [AggregateName("BetterNamedAggregate")]
        public class MyOtherFancyAggregate : AggregateRoot<MyOtherFancyAggregate, ThingyId>
        {
            public MyOtherFancyAggregate(ThingyId id) : base(id) { }
        }

        public class FancyDomainStuff : AggregateRoot<FancyDomainStuff, ThingyId>
        {
            public FancyDomainStuff(ThingyId id) : base(id) { }
        }

        [TestCase(typeof(MyFancyAggregate), "MyFancyAggregate")]
        [TestCase(typeof(MyOtherFancyAggregate), "BetterNamedAggregate")]
        [TestCase(typeof(FancyDomainStuff), "FancyDomainStuff")]
        public void AggregateName(Type aggregateType, string expectedName)
        {
            // Arrange
            var aggregate = (IAggregateRoot) Activator.CreateInstance(aggregateType, ThingyId.New);

            // Assert
            aggregate.Name.Value.Should().Be(expectedName);
        }
    }
}