using System;
using System.Collections.Generic;

namespace ProjectArchitecture.Composition
{
    public sealed class ServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void Register<TService>(TService service) where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), $"Cannot register null service for '{serviceType.Name}'.");
            }

            if (services.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.Name}' is already registered. Remove it before registering another implementation.");
            }

            services.Add(serviceType, service);
        }

        public bool TryRegister<TService>(TService service) where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();

            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), $"Cannot register null service for '{serviceType.Name}'.");
            }

            if (services.ContainsKey(serviceType))
            {
                return false;
            }

            services.Add(serviceType, service);
            return true;
        }

        public TService Get<TService>() where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();

            if (!services.TryGetValue(serviceType, out object service))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.Name}' is not registered. Check scene bootstrap order and GameEntryPoint setup.");
            }

            return (TService)service;
        }

        public bool TryGet<TService>(out TService service) where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();

            if (services.TryGetValue(serviceType, out object registeredService))
            {
                service = (TService)registeredService;
                return true;
            }

            service = null;
            return false;
        }

        public bool Contains<TService>() where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();
            return services.ContainsKey(serviceType);
        }

        public bool Remove<TService>() where TService : class
        {
            Type serviceType = ValidateServiceType<TService>();
            return services.Remove(serviceType);
        }

        public void Clear()
        {
            services.Clear();
        }

        private static Type ValidateServiceType<TService>() where TService : class
        {
            Type serviceType = typeof(TService);
            if (!serviceType.IsInterface)
            {
                throw new InvalidOperationException(
                    $"ServiceLocator registrations must use interfaces. '{serviceType.Name}' is not an interface.");
            }

            return serviceType;
        }
    }
}
