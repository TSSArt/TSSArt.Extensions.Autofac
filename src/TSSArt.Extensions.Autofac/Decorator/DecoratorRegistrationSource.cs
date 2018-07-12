using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;

namespace TSSArt.Autofac.Decorator
{
	public class DecoratorRegistrationSource : IRegistrationSource
	{
		private readonly IRegistrationSource _inner;
		private readonly List<Service> _services;
		private readonly Parameter _parameter;

		public DecoratorRegistrationSource(IRegistrationSource inner, List<Service> services, Parameter parameter)
		{
			_inner = inner;
			_services = services;
			_parameter = parameter;
		}

		IEnumerable<IComponentRegistration> IRegistrationSource.RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			if (!(service is IServiceWithType serviceWithType))
			{
				return _inner.RegistrationsFor(service, registrationAccessor);
			}

			var serviceType = serviceWithType.ServiceType;
			if (!serviceType.IsGenericType)
			{
				return _inner.RegistrationsFor(service, registrationAccessor);
			}

			var genericService = serviceWithType.ChangeType(serviceType.GetGenericTypeDefinition());
			if (!_services.Contains(genericService))
			{
				return _inner.RegistrationsFor(service, registrationAccessor);
			}

			var specificServices = _services.Cast<IServiceWithType>().Select(svc => svc.ChangeType(serviceType));

			return _inner.RegistrationsFor(new KeyedService(DecoratorKey.Locator, serviceType), registrationAccessor)
				.Select(registration => new DecoratorComponentRegistration(registration, specificServices, _parameter));
		}

		public bool IsAdapterForIndividualComponents => false;
	}
}