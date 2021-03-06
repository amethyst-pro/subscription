using System.Reflection;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Handling;
using Microsoft.Extensions.DependencyInjection;

namespace Amethyst.Subscription.Hosting
{
    public static class SubscriptionsExtensions
    {
        public static IServiceCollection AddSubscriptions(
            this IServiceCollection services,
            EndpointConfiguration configuration,
            ServiceLifetime handlersLifetime = ServiceLifetime.Scoped,
            params Assembly[] handlersAssemblies)
        {
            if (handlersAssemblies?.Length > 0)
                services.Scan(t => t.FromAssemblies(handlersAssemblies)
                    .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                    .AsImplementedInterfaces()
                    .WithLifetime(handlersLifetime));

            services.AddSingleton(configuration);
            services.AddHostedService<SubscriptionEndpoint>();
            services.AddSingleton<IObserverFactoryResolver, ObserverFactoryResolver>();
            services.AddSingleton<IEventHandlerFactory, EventHandlerFactory>();
            services.AddSingleton<IEventHandlerScopeFactory, EventHandlerScopeFactory>();
            
            return services;
        }
    }
}