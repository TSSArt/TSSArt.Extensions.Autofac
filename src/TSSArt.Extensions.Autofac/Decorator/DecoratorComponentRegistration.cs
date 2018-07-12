using System;
using System.Collections.Generic;
using System.Linq;
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
			var args = new PreparingEventArgs(context, this, parameters.Append(_parameter));
			Preparing?.Invoke(this, args);
			parameters = args.Parameters;
		}

		void IComponentRegistration.RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
		{
			var args = new ActivatingEventArgs<object>(context, this, parameters, instance);
			Activating?.Invoke(this, args);
			instance = args.Instance;
		}

		void IComponentRegistration.RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
		{
			var args = new ActivatedEventArgs<object>(context, this, parameters, instance);
			Activated?.Invoke(this, args);
		}

		IInstanceActivator IComponentRegistration.Activator => _inner.Activator;

		IComponentLifetime IComponentRegistration.Lifetime => _inner.Lifetime;

		InstanceSharing IComponentRegistration.Sharing => _inner.Sharing;

		InstanceOwnership IComponentRegistration.Ownership => _inner.Ownership;

		IDictionary<string, object> IComponentRegistration.Metadata => _inner.Metadata;

		IComponentRegistration IComponentRegistration.Target => _inner.Target;

		private event EventHandler<PreparingEventArgs> Preparing;
		private event EventHandler<ActivatingEventArgs<object>> Activating;
		private event EventHandler<ActivatedEventArgs<object>> Activated;

		event EventHandler<PreparingEventArgs> IComponentRegistration.Preparing { add => Preparing += value; remove => Preparing -= value; }
		event EventHandler<ActivatingEventArgs<object>> IComponentRegistration.Activating { add => Activating += value; remove => Activating -= value; }
		event EventHandler<ActivatedEventArgs<object>> IComponentRegistration.Activated { add => Activated += value; remove => Activated -= value; }
	}
}