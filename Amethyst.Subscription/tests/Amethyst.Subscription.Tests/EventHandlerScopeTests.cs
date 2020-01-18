using System;
using System.Threading.Tasks;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Hosting;
using Amethyst.Subscription.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Amethyst.Subscription.Tests
{
    public sealed class EventHandlerScopeTests
    {
        [Fact]
        public void Test()
        {
            var builder = new ServiceCollection();
            builder.AddScoped<IEventHandler<RedEvent>, RedEventHandler>();
            builder.AddScoped<IEventHandler<RedEvent>, AnotherRedEventHandler>();

            var provider = builder.BuildServiceProvider();

            var scope = new EventHandlerScopeFactory(provider.GetRequiredService<IServiceScopeFactory>())
                .BeginScope();

            var service = scope.Resolve<RedEvent>();

            service.Should().NotBeNull().And.HaveCount(2);
        }

        public class RedEventHandler : IEventHandler<RedEvent> {
            public Task HandleAsync(RedEvent @event)
            {
                throw new NotImplementedException();
            }
        }

        public class AnotherRedEventHandler : IEventHandler<RedEvent> {
            public Task HandleAsync(RedEvent @event)
            {
                throw new NotImplementedException();
            }
        }
    }
}
