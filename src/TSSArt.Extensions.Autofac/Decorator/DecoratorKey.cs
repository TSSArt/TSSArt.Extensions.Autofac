using Autofac.Core;

namespace TSSArt.Autofac.Decorator
{
	public class DecoratorKey
	{
		public static readonly object Locator = new object();

		private readonly IServiceWithType _service;

		public DecoratorKey(IServiceWithType service) => _service = service;

		public override string ToString() => $"DecoratorKey_{_service.ServiceType.FullName}_{GetHashCode():X8}";
	}
}