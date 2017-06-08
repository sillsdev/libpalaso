using System;
using NUnit.Framework;
using SIL.Reflection;

namespace SIL.Tests.Reflection
{
	[TestFixture]
	public class ReflectionHelperTests
	{
		[Test]
		public void CallMethodWithThrow_StaticMethodExists_ExpectToBeCalled()
		{
			Assert.AreEqual(49, ReflectionHelper.CallMethodWithThrow(typeof(MyStaticClass), "MyIntMethod", 7));
		}

		[Test]
		public void CallMethodWithThrow_StaticMethodMissing_ExpectException()
		{
			Assert.Throws<MissingMethodException>(() => ReflectionHelper.CallMethodWithThrow(typeof(MyStaticClass), "UnknownMethod", "blah"));
		}

		[Test]
		public void CallMethodWithThrow_InstanceMethodExistsOnDerivedType_ExpectToBeCalled()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			ReturnValueClass retVal = (ReturnValueClass)ReflectionHelper.CallMethodWithThrow(myClass, "MyDerivedObjectMethod", 3, 9);
			Assert.IsNotNull(retVal);
			Assert.AreEqual(3, retVal.X);
			Assert.AreEqual(9, retVal.Y);
		}

		[Test]
		public void CallMethodWithThrow_InstanceMethodExistsOnBaseType_ExpectToBeCalled()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			Assert.AreEqual("You got it!", ReflectionHelper.CallMethodWithThrow(myClass, "MyBaseStringMethod", "You "));
		}

		[Test]
		public void CallMethodWithThrow_InstanceMethodMissing_ExpectException()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			Assert.Throws<MissingMethodException>(() => ReflectionHelper.CallMethodWithThrow(myClass, "UnknownMethod", "blah"));
		}

		[Test]
		public void GetResult_StaticMethodExists_ExpectToBeCalled()
		{
			Assert.AreEqual(49, ReflectionHelper.GetResult(typeof(MyStaticClass), "MyIntMethod", 7));
		}

		[Test]
		public void GetResult_StaticMethodMissing_ExpectNull()
		{
			Assert.IsNull(ReflectionHelper.GetResult(typeof(MyStaticClass), "UnknownMethod", "blah"));
		}

		[Test]
		public void GetResult_InstanceMethodExistsOnDerivedType_ExpectToBeCalled()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			ReturnValueClass retVal = (ReturnValueClass)ReflectionHelper.GetResult(myClass, "MyDerivedObjectMethod", new object[] { 3, 9 });
			Assert.IsNotNull(retVal);
			Assert.AreEqual(3, retVal.X);
			Assert.AreEqual(9, retVal.Y);
		}

		[Test]
		public void GetResult_InstanceMethodExistsOnBaseType_ExpectToBeCalled()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			Assert.AreEqual("You got it!", ReflectionHelper.GetResult(myClass, "MyBaseStringMethod", "You "));
		}

		[Test]
		public void GetResult_InstanceMethodMissing_ExpectException()
		{
			MyDerivedClass myClass = new MyDerivedClass();
			Assert.IsNull(ReflectionHelper.GetResult(myClass, "UnknownMethod", "blah"));
		}

		#region Helper classes
		private static class MyStaticClass
		{
			private static int MyIntMethod(int value)
			{
				return value * value;
			}
		}

		private class MyBaseClass
		{
			private string MyBaseStringMethod(string val)
			{
				return val + "got it!";
			}
		}

		private class MyDerivedClass : MyBaseClass
		{
			private ReturnValueClass MyDerivedObjectMethod(int x, int y)
			{
				return new ReturnValueClass(x, y);
			}
		}

		private sealed class ReturnValueClass
		{
			public int X;
			public int Y;

			public ReturnValueClass(int x, int y)
			{
				X = x;
				Y = y;
			}
		}
		#endregion
	}
}
