using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace TSSArt.Autofac.Decorator
{
	public class DecoratorComponentRegistration : IComponentRegistration
	{
		private readonly IComponentRegistration _inner;
		private readonly Parameter _parameter;
		private readonly Guid _id;
		private readonly IEnumerable<Service> _services;

		public DecoratorComponentRegistration(IComponentRegistration inner, IEnumerable<Service> services, Parameter parameter)
		{
			_inner = inner;
			_services = services;
			_parameter = parameter;
			_id = Guid.NewGuid();
		}

		Guid IComponentRegistration.Id => _id;

		IEnumerable<Service> IComponentRegistration.Services => _services;

		void IDisposable.Dispose() { }

		public override string ToString() => GetType().Name + ": " + _inner;

		void IComponentRegistration.RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
		{
			parameters = new List<Parameter>(parameters) { _parameter };
			_inner.RaisePreparing(context, ref parameters);
		}

		void IComponentRegistration.RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
		{
			_inner.RaiseActivating(context, parameters, ref instance);
		}

		void IComponentRegistration.RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
		{
			_inner.RaiseActivated(context, parameters, instance);
		}

		IInstanceActivator IComponentRegistration.Activator => _inner.Activator;

		IComponentLifetime IComponentRegistration.Lifetime => _inner.Lifetime;

		InstanceSharing IComponentRegistration.Sharing => _inner.Sharing;

		InstanceOwnership IComponentRegistration.Ownership => _inner.Ownership;

		IDictionary<string, object> IComponentRegistration.Metadata => _inner.Metadata;

		IComponentRegistration IComponentRegistration.Target => _inner.Target;

		event EventHandler<PreparingEventArgs> IComponentRegistration.Preparing { add => _inner.Preparing += value; remove => _inner.Preparing -= value; }
		event EventHandler<ActivatingEventArgs<object>> IComponentRegistration.Activating { add => _inner.Activating += value; remove => _inner.Activating -= value; }
		event EventHandler<ActivatedEventArgs<object>> IComponentRegistration.Activated { add => _inner.Activated += value; remove => _inner.Activated -= value; }
	}
}