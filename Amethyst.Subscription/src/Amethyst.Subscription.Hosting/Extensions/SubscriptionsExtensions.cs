using System.Reflection;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Handling;
using Microsoft.Extensions.DependencyInjection;

namespace Amethyst.Subscription.Hosting.Extensions
{
    public static class SubscriptionsExtensions
    {
        public static IServiceCollection AddSubscriptions(
            this IServiceCollection services,
            EndpointConfiguration configuration,
            params Assembly[] eventHandlersAssemblies)
        {
            services.Scan(t => t.FromAssemblies(eventHandlersAssemblies)
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.AddScoped<IEventHandlerFactory, EventHandlerFactory>();
            services.AddScoped<IEventHandlerScope, EventHandlerScope>();
            services.AddSingleton<IObserverFactoryResolver, ObserverFactoryResolver>();
            services.AddSingleton<SubscriptionEndpoint>();
            
            services.AddSingleton(configuration);
            
            return services;
        }
    }
}