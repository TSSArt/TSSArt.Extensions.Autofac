using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace TSSArt.Autofac.Decorator
{
	public class DecoratorComponentRegistry : IComponentRegistry
	{
		private const string KeysHolder = "__TSS_DecoratorKeysDictionary";
		private static readonly Service KeyedMarkerService = new UniqueService();

		private readonly IComponentRegistry _inner;
		private readonly IServiceWithType _service;
		private readonly Dictionary<IServiceWithType, object> _keyBucket;

		public DecoratorComponentRegistry(IComponentRegistry inner, IServiceWithType service)
		{
			_inner = inner;
			_service = service;

			if (!inner.Properties.TryGetValue(KeysHolder, out var val) || !(val is Dictionary<IServiceWithType, object> keyBucket))
			{
				_keyBucket = new Dictionary<IServiceWithType, object>();
				inner.Properties[KeysHolder] = _keyBucket;
			}
			else
			{
				_keyBucket = keyBucket;
			}
		}

		private static List<Service> RegisterServices(IServiceWithType service, Dictionary<IServiceWithType, object> keyDic, DecoratorKey newKey)
		{
			var serviceType = service.ServiceType;
			var isOpenGeneric = serviceType.IsGenericTypeDefinition;
			var genericServiceType = serviceType.IsGenericType && !isOpenGeneric ? serviceType.GetGenericTypeDefinition() : null;
			var genericService = genericServiceType != null ? (IServiceWithType)service.ChangeType(genericServiceType) : null;
			var genericDic = genericService != null && keyDic.TryGetValue(genericService, out var dic) ? (Dictionary<IServiceWithType, object>)dic : null;
			
			IEnumerable<Service> GetServices()
			{
				if (genericDic != null && (genericDic.TryGetValue(service, out var genKey) || genericDic.TryGetValue(genericService, out genKey)))
				{
					yield return new KeyedService(genKey, serviceType);
					yield break;
				}

				if (!keyDic.TryGetValue(service, out var key))
				{
					yield return (Service)service;
					yield break;
				}

				if (!isOpenGeneric)
				{
					yield return new KeyedService(key, serviceType);
					yield break;
				}

				var tmpDic = (Dictionary<IServiceWithType, object>)key;
				foreach (var sk in tmpDic.Values)
				{
					yield return new KeyedService(sk, serviceType);
				}

				if (!tmpDic.ContainsKey(service))
				{
					yield return (Service)service;
				}
			}

			var services = GetServices().ToList();

			if (isOpenGeneric)
			{
				keyDic[service] = new Dictionary<IServiceWithType, object> { { service, newKey } };
				return services;
			}

			keyDic[service] = newKey;

			if (genericService == null)
			{
				return services;
			}

			if (genericDic != null)
			{
				genericDic[service] = newKey;
			}
			else
			{
				keyDic[genericService] = new Dictionary<IServiceWithType, object> { { service, newKey } };
			}

			return services;
		}

		private IEnumerable<IComponentRegistration> ComponentsToRegister(IComponentRegistration registration)
		{
			yield return registration;

			if (registration.Services.All(svc => svc != KeyedMarkerService))
			{
				var key = new DecoratorKey(_service);
				var parameter = CreateDecoratorParameter(_service, key);
				var services = RegisterServices(_service, _keyBucket, key);

				yield return new DecoratorComponentRegistration(registration, services.Append(KeyedMarkerService), parameter);
			}
		}

		private Parameter CreateDecoratorParameter(IServiceWithType service, DecoratorKey key)
		{
			if (service.ServiceType.IsGenericTypeDefinition)
			{
				return new ResolvedParameter(delegate (ParameterInfo info, IComponentContext ctx)
					{
						var type = info.ParameterType;
						var memb = info.Member;
						return type.IsGenericType && type.GetGenericTypeDefinition() == service.ServiceType
						                          && memb.Module.ResolveMethod(memb.MetadataToken).GetParameters()
							                          .First(pi => pi.MetadataToken == info.MetadataToken).ParameterType
							                          .GetGenericArguments().All(arg => arg.IsGenericParameter);
					},
					(info, ctx) => ctx.ResolveKeyed(key, info.ParameterType));
			}

			return new ResolvedParameter((info, ctx) => info.ParameterType == service.ServiceType, (info, ctx) => ctx.ResolveKeyed(key, info.ParameterType));
		}

		void IComponentRegistry.Register(IComponentRegistration registration)
		{
			foreach (var componentRegistration in ComponentsToRegister(registration))
			{
				_inner.Register(componentRegistration);
			}
		}

		void IComponentRegistry.Register(IComponentRegistration registration, bool preserveDefaults)
		{
			foreach (var componentRegistration in ComponentsToRegister(registration))
			{
				_inner.Register(componentRegistration, preserveDefaults);
			}
		}

		void IComponentRegistry.AddRegistrationSource(IRegistrationSource source)
		{
			var key = new DecoratorKey(_service);
			var parameter = CreateDecoratorParameter(_service, key);
			var services = RegisterServices(_service, _keyBucket, key);

			_inner.AddRegistrationSource(new DecoratorRegistrationSource(source, services, parameter));
		}

		void IDisposable.Dispose() => _inner.Dispose();
		bool IComponentRegistry.TryGetRegistration(Service service, out IComponentRegistration registration) => _inner.TryGetRegistration(service, out registration);
		bool IComponentRegistry.IsRegistered(Service service) => _inner.IsRegistered(service);
		IEnumerable<IComponentRegistration> IComponentRegistry.RegistrationsFor(Service service) => _inner.RegistrationsFor(service);
		public IEnumerable<IComponentRegistration> DecoratorsFor(IComponentRegistration registration) => _inner.DecoratorsFor(registration);

		IDictionary<string, object> IComponentRegistry.Properties => _inner.Properties;
		IEnumerable<IComponentRegistration> IComponentRegistry.Registrations => _inner.Registrations;
		IEnumerable<IRegistrationSource> IComponentRegistry.Sources => _inner.Sources;
		bool IComponentRegistry.HasLocalComponents => _inner.HasLocalComponents;

		event EventHandler<ComponentRegisteredEventArgs> IComponentRegistry.Registered
		{
			add => _inner.Registered += value;
			remove => _inner.Registered -= value;
		}

		event EventHandler<RegistrationSourceAddedEventArgs> IComponentRegistry.RegistrationSourceAdded
		{
			add => _inner.RegistrationSourceAdded += value;
			remove => _inner.RegistrationSourceAdded -= value;
		}
	}
}