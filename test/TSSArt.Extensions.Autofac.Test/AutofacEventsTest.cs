using System;
using Autofac;
using Autofac.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSSArt.Extensions.Autofac;

namespace TSSArt.HowToServer.Test
{
	[TestClass]
	public class AutofacEventsTest
	{
		public interface ISimple<T>
		{
			bool Activating { get; set; }

			bool Activated { get; set; }

			void OnActivating();

			void OnActivated();
		}

		public class SimpleType<T> : ISimple<T>
		{
			public bool Activating { get; set; }

			public bool Activated { get; set; }

			public void OnActivating()
			{
				Activating = true;
			}
			public void OnActivated()
			{
				Activated = true;
			}
		}

		public class SimpleTypeDecorator<T> : ISimple<T>
		{
			private readonly ISimple<T> _inner;

			public SimpleTypeDecorator(ISimple<T> inner)
			{
				_inner = inner;
			}

			public bool Activating { get; set; }

			public bool Activated { get; set; }

			public void OnActivating()
			{
				Activating = true;
			}
			public void OnActivated()
			{
				Activated = true;
			}
		}

		public interface INSimple
		{
			bool Activating { get; set; }

			bool Activated { get; set; }

			void OnActivating();

			void OnActivated();
		}

		public class NSimpleType : INSimple
		{
			public bool Activating { get; set; }

			public bool Activated { get; set; }

			public void OnActivating()
			{
				Activating = true;
			}
			public void OnActivated()
			{
				Activated = true;
			}
		}

		public class NSimpleTypeDecorator : INSimple
		{
			private readonly INSimple _inner;

			public NSimpleTypeDecorator(INSimple inner)
			{
				_inner = inner;
			}

			public bool Activating { get; set; }

			public bool Activated { get; set; }

			public void OnActivating()
			{
				Activating = true;
			}
			public void OnActivated()
			{
				Activated = true;
			}
		}

	    [TestMethod]
	    public void AutofacActivating_should_call_OnActivating()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<SimpleType<int>>().OnActivating(e => e.Instance.OnActivating());
		    var container = builder.Build();

		    var result = container.Resolve<SimpleType<int>>();

			Assert.AreEqual(true, result.Activating);
			Assert.AreEqual(false, result.Activated);
		
		}

		[TestMethod]
		public void AutofacActivated_should_call_OnActivated()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<SimpleType<int>>().OnActivated(e => e.Instance.OnActivated());
			var container = builder.Build();

			var result = container.Resolve<SimpleType<int>>();

			Assert.AreEqual(false, result.Activating);
			Assert.AreEqual(true, result.Activated);
		}

		[TestMethod]
		public void AutofacActivating_with_Generic_TypedDecorator_should_call_OnActivating()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<SimpleType<int>>()
				.TypedDecoration<ISimple<int>>()
				.OnActivating(e => ((ISimple<int>) e.Instance).OnActivating());

			var container = builder.Build();

			var result = container.Resolve<ISimple<int>>();

			Assert.AreEqual(true, result.Activating);
		}

		[TestMethod]
		public void AutofacActivating_with_NonGeneric_TypedDecorator_should_call_OnActivating()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<NSimpleType>()
				.TypedDecoration<INSimple>()
				.OnActivating(e => ((INSimple) e.Instance).OnActivating());

			var container = builder.Build();

			var result = container.Resolve<INSimple>();

			Assert.AreEqual(true, result.Activating);
		}
	}
}