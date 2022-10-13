﻿using Microsoft.Extensions.DependencyInjection;

namespace FAIL.Helpers;
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddActivatorServices<TInterface, TActivator>(this IServiceCollection collection)
    {
        // Get all types in the executing assembly. There are many ways to do this, but this is fastest.
        foreach (var type in typeof(ServiceCollectionExtensions).Assembly.GetTypes())
        {
            if (typeof(TInterface).IsAssignableFrom(type) && !type.IsAbstract)
                _ = collection.AddSingleton(typeof(TInterface), type);
        }

        // Register the activator so you can activate the instances.
        _ = collection.AddSingleton(typeof(TActivator));

        return collection;
    }
}
