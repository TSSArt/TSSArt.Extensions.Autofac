using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSSArt.Extensions.Autofac;

namespace TSSArt.HowToServer.Test
{
	[TestClass]
	public class AutofacDecoratorTest
    {
	    public interface IDecorated
	    {
		    string Process(string input);
	    }

	    public class SimpleDecoratedType : IDecorated
	    {
		    public string Process(string input) => "{" + input + "}";
	    }

	    public class LevelADecorator : IDecorated
	    {
		    private readonly IDecorated _nested;

		    public LevelADecorator(IDecorated nested) => _nested = nested;

		    public string Process(string input) => "A[" + _nested.Process(input) + "]A";
	    }

	    public class LevelCDecorator : IDecorated
	    {
		    private readonly IDecorated _nested;

		    public LevelCDecorator(IDecorated nested) => _nested = nested;

		    public string Process(string input) => "C[" + _nested.Process(input) + "]C";
	    }

	    public class LevelDDecorator : IDecorated
	    {
		    private readonly IDecorated _nested;

		    public LevelDDecorator(IDecorated nested) => _nested = nested;

		    public string Process(string input) => "D[" + _nested.Process(input) + "]D";
	    }

	    public class LevelBDecorator : IDecorated
	    {
		    private readonly IDecorated _nested;

		    public LevelBDecorator(IDecorated nested) => _nested = nested;

		    public string Process(string input) => "B[" + _nested.Process(input) + "]B";
	    }

	    [TestMethod]
	    public void AutofacDecorator_Decorators_should_be_called_in_proper_sequence()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>();
			builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();
		    var result = decorated.Process("0");

			Assert.AreEqual("B[A[{0}]A]B", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_Named_decorators_should_be_called_in_proper_sequence()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().NamedDecoration<IDecorated>("a");
			builder.RegisterType<LevelADecorator>().NamedDecoration<IDecorated>("a");
		    builder.RegisterType<SimpleDecoratedType>().NamedDecoration<IDecorated>("a");
		    var container = builder.Build();

		    var decorated = container.ResolveNamed<IDecorated>("a");
		    var result = decorated.Process("0");

			Assert.AreEqual("B[A[{0}]A]B", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_Keyed_decorators_should_be_called_in_proper_sequence()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().KeyedDecoration<IDecorated>(1);
		    builder.RegisterType<LevelADecorator>().KeyedDecoration<IDecorated>(1);
		    builder.RegisterType<SimpleDecoratedType>().KeyedDecoration<IDecorated>(1);
		    var container = builder.Build();

		    var decorated = container.ResolveKeyed<IDecorated>(1);
		    var result = decorated.Process("0");

			Assert.AreEqual("B[A[{0}]A]B", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_OnlyIf_false_should_exclude_first_decorator()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>().OnlyIf(cr => false);
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("A[{0}]A", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_OnlyIf_false_should_exclude_middle_decorator()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>().OnlyIf(cr => false);
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("B[{0}]B", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_OnlyIf_false_should_work_before_decoration_declariation()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().OnlyIf(cr => false).TypedDecoration<IDecorated>();
		    builder.RegisterType<LevelADecorator>().OnlyIf(cr => false).TypedDecoration<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("{0}", result);
	    }

	    [TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
	    public void AutofacDecorator_Not_allowed_same_type()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<LevelADecorator>();
		    builder.Build();
	    }

	    [TestMethod]
	    public void AutofacDecorator_Should_override_explicit_service_declaration()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>().As<IDecorated>();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>().As<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>().As<IDecorated>();
		    var container = builder.Build();

		    Assert.AreEqual(1, container.Resolve<IEnumerable<IDecorated>>().Count());
	    }

	    [TestMethod]
	    public void AutofacDecorator_Should_not_delete_other_service_declaration()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>().As<IDecorated>().AsSelf();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>().As<IDecorated>().AsSelf();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>().As<IDecorated>().AsSelf();
		    var container = builder.Build();

		    Assert.IsNotNull(container.Resolve<LevelADecorator>());
		    Assert.IsNotNull(container.Resolve<LevelBDecorator>());
		    Assert.IsNotNull(container.Resolve<SimpleDecoratedType>());
	    }

	    [TestMethod]
	    public void AutofacDecorator_Should_not_add_self_service_declaration()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    Assert.IsNull(container.ResolveOptional<LevelADecorator>());
		    Assert.IsNull(container.ResolveOptional<LevelBDecorator>());
		    Assert.IsNull(container.ResolveOptional<SimpleDecoratedType>());
	    }
		
	    [TestMethod] public void AutofacDecorator_should_created_4_components_registrations()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>().NamedDecoration<IDecorated>("name").KeyedDecoration<IDecorated>(100).KeyedDecoration<IDecorated>(5);
		    var container = builder.Build();

			Assert.AreEqual(5 + 1/*autofac-internal*/, container.ComponentRegistry.Registrations.Count());
		}

	    [TestMethod]
	    public void AutofacDecorator_Keyed_and_named_decorator_should_not_affect_and_break_registrations()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelCDecorator>().TypedDecoration<IDecorated>().KeyedDecoration<IDecorated>(100);
		    builder.RegisterType<LevelDDecorator>().TypedDecoration<IDecorated>();
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();

		    var result = decorated.Process("0");
			Assert.AreEqual("C[D[{0}]D]C", result);
	    }

	    [TestMethod]
	    public void AutofacDecorator_Keyed_and_named_decorator_should_process_keys_properly_case_1()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelADecorator>().NamedDecoration<IDecorated>("name").KeyedDecoration<IDecorated>(100);
		    builder.RegisterType<SimpleDecoratedType>().KeyedDecoration<IDecorated>(100);
		    var container = builder.Build();

		    var keyedDecorated = container.ResolveKeyed<IDecorated>(100);

		    var keyedResult = keyedDecorated.Process("2");

		    Assert.AreEqual("A[{2}]A", keyedResult);
	    }

	    [TestMethod]
	    public void AutofacDecorator_Keyed_and_named_decorator_should_process_keys_properly_case_2()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<LevelADecorator>().TypedDecoration<IDecorated>().KeyedDecoration<IDecorated>(100);
		    builder.RegisterType<LevelBDecorator>().TypedDecoration<IDecorated>().NamedDecoration<IDecorated>("name").KeyedDecoration<IDecorated>(100);
		    builder.RegisterType<LevelCDecorator>().TypedDecoration<IDecorated>().KeyedDecoration<IDecorated>(100);
		    builder.RegisterType<LevelDDecorator>().TypedDecoration<IDecorated>().NamedDecoration<IDecorated>("name");
		    builder.RegisterType<SimpleDecoratedType>().TypedDecoration<IDecorated>().NamedDecoration<IDecorated>("name").KeyedDecoration<IDecorated>(100);
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated>();
		    var namedDecorated = container.ResolveNamed<IDecorated>("name");
		    var keyedDecorated = container.ResolveKeyed<IDecorated>(100);

		    var result = decorated.Process("0");
		    var namedResult = namedDecorated.Process("1");
		    var keyedResult = keyedDecorated.Process("2");

		    Assert.AreEqual("A[B[C[D[{0}]D]C]B]A", result);
		    Assert.AreEqual("B[D[{1}]D]B", namedResult);
		    Assert.AreEqual("A[B[C[{2}]C]B]A", keyedResult);
	    }

	}
}
