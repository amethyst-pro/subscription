using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Configurations;
using Amethyst.Subscription.Observing;
using Amethyst.Subscription.Tests.Fakes;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Confluent.Kafka;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Amethyst.Subscription.Tests
{
    public sealed class BatchEventObserverTests
    {
        private readonly Fixture _fixture;
        private readonly IEventHandler _handler;
        private readonly BatchEventObserver _observer;
        private readonly IConsumer _consumer;

        public BatchEventObserverTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

            _handler = _fixture.Create<IEventHandler>();
            _consumer = _fixture.Create<IConsumer>();
            _observer = new BatchEventObserver(
                new BatchConfiguration(TimeSpan.FromDays(1), maxBatchCount: 10),
                _handler,
                _consumer);
        }

        [Fact]
        public async Task HandlingSameTypeEvents_AllHandledAsOneBatch()
        {
            var events = _fixture.CreateMany<RedEvent>(10).ToArray();
            var context = _fixture.Create<IEventContext>();
            context.Status.Returns(DeserializationStatus.Success);

            IReadOnlyCollection<RedEvent> receivedEvents = default;

            await _handler.Handle(
                Arg.Do<IReadOnlyCollection<RedEvent>>(e => receivedEvents = e),
                CancellationToken.None);

            foreach (var e in events)
            {
                context.GetEvent().Returns(e);

                await _observer.OnEventAppeared(context, CancellationToken.None);
            }

            await _observer.Complete();

            receivedEvents.Should().NotBeNull();
            receivedEvents.Should().Equal(events);
        }

        [Fact]
        public async Task HandlingDiffTypeEventsWithSameKey_AllHandledWithPreservedOrder()
        {
            var events = new object[]
            {
                _fixture.Create<RedEvent>(),
                _fixture.Create<BlueEvent>(),
                _fixture.Create<BlueEvent>(),
                _fixture.Create<RedEvent>(),
                _fixture.Create<RedEvent>(),
                _fixture.Create<BlueEvent>()
            };

            var context = _fixture.Create<IEventContext>();
            context.Status.Returns(DeserializationStatus.Success);
            context.GetKey().Returns(Guid.NewGuid());

            var receivedEvents = new List<IReadOnlyCollection<object>>();

            await _handler.Handle(
                Arg.Do<IReadOnlyCollection<RedEvent>>(e => receivedEvents.Add(e)),
                CancellationToken.None);

            await _handler.Handle(
                Arg.Do<IReadOnlyCollection<BlueEvent>>(e => receivedEvents.Add(e)),
                CancellationToken.None);

            foreach (var e in events)
            {
                context.GetEvent().Returns(e);

                await _observer.OnEventAppeared(context, CancellationToken.None);
            }

            await _observer.Complete();

            receivedEvents.Should().HaveSameCount(events);
            receivedEvents.SelectMany(l => l).Should().Equal(events);
        }

        [Fact]
        public async Task HandlingDiffTypeEventsWithDiffKey_AllHandledWithPreservedOrder()
        {
            var key1 = Guid.NewGuid();
            var key2 = Guid.NewGuid();
            var key3 = Guid.NewGuid();

            var events = new (Guid key, object @event, string label)[]
            {
                (key1, _fixture.Create<RedEvent>(), "1.1"),
                (key1, _fixture.Create<BlueEvent>(), "2.1"),
                (key2, _fixture.Create<BlueEvent>(), "1.2"),
                (key3, _fixture.Create<RedEvent>(), "1.1"),
                (key1, _fixture.Create<RedEvent>(), "3.1"),
                (key3, _fixture.Create<BlueEvent>(), "2.1")
            };

            var context = _fixture.Create<IEventContext>();
            context.Status.Returns(DeserializationStatus.Success);

            var receivedEvents = new List<IReadOnlyCollection<object>>();

            await _handler.Handle(
                Arg.Do<IReadOnlyCollection<RedEvent>>(e => receivedEvents.Add(e)),
                CancellationToken.None);

            await _handler.Handle(
                Arg.Do<IReadOnlyCollection<BlueEvent>>(e => receivedEvents.Add(e)),
                CancellationToken.None);

            foreach (var e in events)
            {
                context.GetEvent().Returns(e.@event);
                context.GetKey().Returns(e.key);

                await _observer.OnEventAppeared(context, CancellationToken.None);
            }

            await _observer.Complete();

            receivedEvents.Should().HaveSameCount(events.Select(e => e.label).Distinct());
            receivedEvents.SelectMany(l => l).Should().HaveCount(events.Length);
            receivedEvents.SelectMany(l => l).Should().BeEquivalentTo(events.Select(e => e.@event));

            receivedEvents[0].Should().Equal(events.Where(e => e.label == "1.1").Select(e => e.@event).ToArray());
            receivedEvents[1].Should().Equal(events.Where(e => e.label == "1.2").Select(e => e.@event).ToArray());
            receivedEvents[2].Should().Equal(events.Where(e => e.label == "2.1").Select(e => e.@event).ToArray());
            receivedEvents[3].Should().Equal(events.Where(e => e.label == "3.1").Select(e => e.@event).ToArray());
        }

        [Fact]
        public async Task HandlingEvents_LatestOffsetsCommitted()
        {
            var partitions = _fixture.CreateMany<Partition>(2).ToArray();
            var messages = _fixture.CreateMany<RedEvent>(3)
                .Select(e => ((object)e, partition: partitions[0], offset: _fixture.Create<Offset>()))
                .Concat(
                    _fixture.CreateMany<BlueEvent>(3)
                        .Select(e => ((object)e, partition: partitions[1], offset: _fixture.Create<Offset>())))
                .ToArray();

            var context = _fixture.Create<IEventContext>();
            context.Status.Returns(DeserializationStatus.Success);

            var topic = _fixture.Create<string>();

            TopicPartitionOffset[] committedOffsets = null;
            _consumer.Commit(Arg.Do<TopicPartitionOffset[]>(o => committedOffsets = o));

            foreach (var (@event, partition, offset) in messages)
            {
                context.GetEvent().Returns(@event);
                context.GetKey().Returns(Guid.NewGuid());

                context.Offset.Returns(new TopicPartitionOffset(topic, partition, offset));

                await _observer.OnEventAppeared(context, CancellationToken.None);
            }

            await _observer.Complete();

            committedOffsets.Should().HaveSameCount(partitions);

            var expectedOffsets = messages.GroupBy(m => m.partition)
                .Select(g => new TopicPartitionOffset(topic, g.Key, g.Select(i => i.offset.Value).Max()));

            committedOffsets.Should().BeEquivalentTo(expectedOffsets);
        }
    }
}
