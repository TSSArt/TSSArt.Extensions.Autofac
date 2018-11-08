using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSSArt.Extensions.Autofac;

namespace TSSArt.HowToServer.Test
{
	[TestClass]
	public class AutofacStartableTest
	{
	    public class SimpleType : IStartWhenActivated
		{
		    public bool Started { get; private set; }

		    public void Start()
		    {
			    Started = true;
		    }
	    }

	    public class DisposableType : IStartWhenActivated, IDisposable
	    {
		    public bool Started { get; private set; }

		    public bool Disposed { get; private set; }

		    public void Start()
		    {
			    Started = true;
		    }

		    public void Dispose()
		    {
			    Disposed = true;
		    }
	    }

	    [TestMethod]
	    public void AutofacStartable_should_call_Started()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<SimpleType>().AsStartableWhenActivated();
		    var container = builder.Build();

		    var result = container.Resolve<SimpleType>();

			Assert.AreEqual(true, result.Started);
	    }

	    [TestMethod]
	    public void AutofacStartableDisposable_should_call_Started_and_Disposed()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<DisposableType>().AsStartableWhenActivated();
		    var container = builder.Build();

		    DisposableType result;

			using (var scope = container.BeginLifetimeScope())
		    {
			    result = scope.Resolve<DisposableType>();
			}

			Assert.AreEqual(true, result.Started);
			Assert.AreEqual(true, result.Disposed);
	    }
	}
}
