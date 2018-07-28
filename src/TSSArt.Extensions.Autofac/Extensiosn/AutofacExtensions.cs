using System;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using TSSArt.Autofac.Decorator;

namespace TSSArt.Extensions.Autofac
{
	using ITypedBuilder = IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>;
	using IGenericBuilder = IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>;

	public static class AutofacExtensions
	{
		public static ITypedBuilder TypedDecoration<TService>(this ITypedBuilder builder) => Register(builder, new TypedService(typeof(TService)));

		public static ITypedBuilder NamedDecoration<TService>(this ITypedBuilder builder, string serviceName) => Register(builder, new KeyedService(serviceName, typeof(TService)));

		public static ITypedBuilder KeyedDecoration<TService>(this ITypedBuilder builder, object serviceKey) => Register(builder, new KeyedService(serviceKey, typeof(TService)));

		public static ITypedBuilder TypedDecoration(this ITypedBuilder builder, Type serviceType) => Register(builder, new TypedService(serviceType));

		public static ITypedBuilder NamedDecoration(this ITypedBuilder builder, string serviceName, Type serviceType) => Register(builder, new KeyedService(serviceName, serviceType));

		public static ITypedBuilder KeyedDecoration(this ITypedBuilder builder, object serviceKey, Type serviceType) => Register(builder, new KeyedService(serviceKey, serviceType));

		public static IGenericBuilder TypedDecoration(this IGenericBuilder builder, Type serviceType) => Register(builder, new TypedService(serviceType));

		public static IGenericBuilder NamedDecoration(this IGenericBuilder builder, string serviceName, Type serviceType) => Register(builder, new KeyedService(serviceName, serviceType));

		public static IGenericBuilder KeyedDecoration(this IGenericBuilder builder, object serviceKey, Type serviceType) => Register(builder, new KeyedService(serviceKey, serviceType));


		private static ITypedBuilder Register(ITypedBuilder builder, IServiceWithType service)
		{
			DecoratorRegistrator.Register(builder, service);
			return builder;
		}

		private static IGenericBuilder Register(IGenericBuilder builder, IServiceWithType service)
		{
			DecoratorRegistrator.Register(builder, service);
			return builder;
		}
	}
}