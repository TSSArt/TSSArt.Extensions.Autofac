using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSSArt.Extensions.Autofac;

namespace TSSArt.HowToServer.Test
{
	[TestClass]
	public class AutofacGenericDecoratorTest
	{
		// ReSharper disable once UnusedTypeParameter
		public interface IDecorated<T>
	    {
		    string Process(string input);
	    }

	    public class GenericDecoratedType<T> : IDecorated<T>
		{
			public string Process(string input) => "{" + typeof(T).Name + ":" + input + "}";
		}

	    public class GenericLevelADecorator<T> : IDecorated<T>
	    {
		    private readonly IDecorated<T> _nested;

		    public GenericLevelADecorator(IDecorated<T> nested) => _nested = nested;

		    public string Process(string input) => "A[" + _nested.Process(input) + "]A";
	    }

	    public class GenericLevelBDecorator<T> : IDecorated<T>
	    {
		    private readonly IDecorated<T> _nested;

		    public GenericLevelBDecorator(IDecorated<T> nested) => _nested = nested;

		    public string Process(string input) => "B[" + _nested.Process(input) + "]B";
	    }

		public class GenericLevelCDecorator<T> : IDecorated<T>
		{
			private readonly IDecorated<T> _nested;

			public GenericLevelCDecorator(IDecorated<T> nested) => _nested = nested;

			public string Process(string input) => "C[" + _nested.Process(input) + "]C";
		}

		public class GenericLevelDDecorator<T> : IDecorated<T>
		{
			private readonly IDecorated<T> _nested;

			public GenericLevelDDecorator(IDecorated<T> nested) => _nested = nested;

			public string Process(string input) => "D[" + _nested.Process(input) + "]D";
		}

	    public class GenericImproperDecoratorK1<T> : IDecorated<T>
	    {
		    private readonly IDecorated<int> _nested;

		    public GenericImproperDecoratorK1(IDecorated<int> nested) => _nested = nested;

			public string Process(string input) => "K[" + _nested.Process(input) + "]K";
		}

		[TestMethod]
	    public void AutofacDecorator_Typed_generic_should_work_as_simple()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<GenericLevelBDecorator<int>>().TypedDecoration<IDecorated<int>>();
		    builder.RegisterType<GenericLevelADecorator<int>>().TypedDecoration<IDecorated<int>>();
		    builder.RegisterType<GenericDecoratedType<int>>().TypedDecoration<IDecorated<int>>();
		    var container = builder.Build();

		    var decorated = container.Resolve<IDecorated<int>>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("B[A[{Int32:0}]A]B", result);
		}

	    [TestMethod]
	    [ExpectedException(typeof(ComponentNotRegisteredException))]
		public void AutofacDecorator_Typed_generic_should_not_resolve_other_type()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterType<GenericLevelADecorator<int>>().TypedDecoration<IDecorated<int>>();
		    builder.RegisterType<GenericDecoratedType<int>>().TypedDecoration<IDecorated<int>>();
		    var container = builder.Build();

		    container.Resolve<IDecorated<long>>();
		}

	    [TestMethod]
		[ExpectedException(typeof(DependencyResolutionException))]
	    public void AutofacDecorator_Invalid_generic_decorator_parameter_should_not_be_resolved_through_decorator()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
		    builder.RegisterGeneric(typeof(GenericImproperDecoratorK1<>)).TypedDecoration(typeof(IDecorated<>));
		    builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));

			var container = builder.Build();

		    container.Resolve<IDecorated<int>>();
	    }

	    [TestMethod]
	    public void AutofacDecorator_Invalid_generic_decorator_parameter_should_not_be_resolved_through_decorator_but_allow_individual_resolve()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
		    builder.RegisterGeneric(typeof(GenericImproperDecoratorK1<>)).TypedDecoration(typeof(IDecorated<>))
				.WithParameter(TypedParameter.From<IDecorated<int>>(new GenericDecoratedType<int>()));

			var container = builder.Build();

		    var decorated = container.Resolve<IDecorated<int>>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("A[K[{Int32:0}]K]A", result);
	    }

		[TestMethod]
	    public void AutofacDecorator_Open_generic_should_resolve_typed_generics()
	    {
		    var builder = new ContainerBuilder();
		    builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>));
		    builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

		    var decorated = container.Resolve<IDecorated<int>>();
		    var result = decorated.Process("0");

		    Assert.AreEqual("B[A[{Int32:0}]A]B", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_1()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int32:0}]A", result);
		}

		[TestMethod]
		[ExpectedException(typeof(DependencyResolutionException))]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_1_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			var container = builder.Build();

			container.Resolve<IDecorated<long>>();
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_2()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int32:0}]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_2_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("{Int64:0}", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_3()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[B[{Int32:0}]B]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_3_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int64:0}]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_4()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[B[{Int32:0}]B]A", result);
		}

		[TestMethod]
		[ExpectedException(typeof(DependencyResolutionException))]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_4_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			var container = builder.Build();

			container.Resolve<IDecorated<long>>();
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_5()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[B[C[{Int32:0}]C]B]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_5_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int64:0}]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_6()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericLevelCDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelDDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[B[C[D[{Int32:0}]D]C]B]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_6_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericLevelCDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelDDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("B[C[{Int64:0}]C]B", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_7()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[B[{Int32:0}]B]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_7_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[C[{Int64:0}]C]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_8()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("B[{Int32:0}]B", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_8_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelBDecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelCDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("C[{Int64:0}]C", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_9()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericDecoratedType<long>)).TypedDecoration(typeof(IDecorated<long>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int32:0}]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_9_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericDecoratedType<long>)).TypedDecoration(typeof(IDecorated<long>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[{Int64:0}]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_10()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelBDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericLevelCDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericDecoratedType<long>)).TypedDecoration(typeof(IDecorated<long>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var result = decorated.Process("0");

			Assert.AreEqual("A[C[{Int32:0}]C]A", result);
		}

		[TestMethod]
		public void AutofacDecorator_Open_generic_should_work_properly_with_closed_generics_case_10_alternate()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType(typeof(GenericLevelADecorator<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericLevelBDecorator<long>)).TypedDecoration(typeof(IDecorated<long>));
			builder.RegisterGeneric(typeof(GenericLevelCDecorator<>)).TypedDecoration(typeof(IDecorated<>));
			builder.RegisterType(typeof(GenericDecoratedType<int>)).TypedDecoration(typeof(IDecorated<int>));
			builder.RegisterType(typeof(GenericDecoratedType<long>)).TypedDecoration(typeof(IDecorated<long>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<long>>();
			var result = decorated.Process("0");

			Assert.AreEqual("B[C[{Int64:0}]C]B", result);
		}
		
		[TestMethod]
		public void AutofacDecorator_Generic_keyed_and_named_decorator_should_process_keys_properly_case()
		{
			var builder = new ContainerBuilder();
			builder.RegisterGeneric(typeof(GenericLevelADecorator<>)).TypedDecoration(typeof(IDecorated<>)).KeyedDecoration(100, typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericLevelBDecorator<>)).TypedDecoration(typeof(IDecorated<>)).NamedDecoration("name", typeof(IDecorated<>)).KeyedDecoration(100, typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericLevelCDecorator<>)).TypedDecoration(typeof(IDecorated<>)).KeyedDecoration(100, typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericLevelDDecorator<>)).TypedDecoration(typeof(IDecorated<>)).NamedDecoration("name", typeof(IDecorated<>));
			builder.RegisterGeneric(typeof(GenericDecoratedType<>)).TypedDecoration(typeof(IDecorated<>)).NamedDecoration("name", typeof(IDecorated<>)).KeyedDecoration(100, typeof(IDecorated<>));
			var container = builder.Build();

			var decorated = container.Resolve<IDecorated<int>>();
			var namedDecorated = container.ResolveNamed<IDecorated<int>>("name");
			var keyedDecorated = container.ResolveKeyed<IDecorated<int>>(100);
			var result = decorated.Process("0");
			var namedResult = namedDecorated.Process("1");
			var keyedResult = keyedDecorated.Process("2");

			Assert.AreEqual("B[D[{Int32:1}]D]B", namedResult);
			Assert.AreEqual("A[B[C[{Int32:2}]C]B]A", keyedResult);
			Assert.AreEqual("A[B[C[D[{Int32:0}]D]C]B]A", result);
		}
	}
}
