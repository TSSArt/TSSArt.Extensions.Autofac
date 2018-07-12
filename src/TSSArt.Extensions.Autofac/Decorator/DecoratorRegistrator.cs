using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;

namespace TSSArt.Autofac.Decorator
{
	public static class DecoratorRegistrator
	{
		public static void Register(IRegistrationBuilder<object, ReflectionActivatorData, object> builder, IServiceWithType service)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			if (service == null) throw new ArgumentNullException(nameof(service));

			var regData = builder.RegistrationData;
			var deferredCallback = regData.DeferredCallback;
			if (deferredCallback == null)
			{
				throw new NotSupportedException("DeferredCallback missing");
			}

			if (service.ServiceType == builder.ActivatorData.ImplementationType)
			{
				throw new InvalidOperationException($"Service type [{service.ServiceType}] can not be same type as Implementor");
			}

			var previousAction = deferredCallback.Callback;
			deferredCallback.Callback = Callback;

			void Callback(IComponentRegistry registry)
			{
				regData.AddService(new KeyedService(DecoratorKey.Locator, service.ServiceType));
				var services = regData.Services.Where(s => s != (Service)service).ToList();
				regData.ClearServices();
				regData.AddServices(services);

				previousAction(new DecoratorComponentRegistry(registry, service));
			}
		}
	}
}