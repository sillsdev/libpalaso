using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SIL.Code;

namespace SIL.Tests.Code
{
	public class RetryUtilityTests
	{
		[Test]
		public void TypesIncludes_MainClassAndSubclasses_ButNotSuperclass()
		{
			var types = new HashSet<Type> ( new[] {typeof(IOException), typeof(NullReferenceException)} );
			Assert.That(RetryUtility.TypesIncludes(types, typeof(NullReferenceException)), Is.True);
			Assert.That(RetryUtility.TypesIncludes(types, typeof(IOException)), Is.True);
			Assert.That(RetryUtility.TypesIncludes(types, typeof(FileNotFoundException)), Is.True);
			Assert.That(RetryUtility.TypesIncludes(types, typeof(SystemException)), Is.False);
		}

		[Test]
		public void Retry_WorksOnFirstAttempt()
		{
			var n = 0;
			RetryUtility.Retry(() => { n++; }, 3, 1);
			Assert.That(n, Is.EqualTo(1));
		}

		[Test]
		public void Retry_WorksOnSecondAttempt()
		{
			var n = 0;
			RetryUtility.Retry(() => {
				n++;
				if (n == 1)
					throw new ApplicationException();
			}, 3, 1, new HashSet<Type> (new[] { typeof(Exception) }));
			Assert.That(n, Is.EqualTo(2));
		}

		[Test]
		public void Retry_StopsAfter3Attempts()
		{
			var n = 0;
			Assert.That(() => RetryUtility.Retry(() => {
				n++;
				throw new ApplicationException();
			}, 3, 1, new HashSet<Type> (new[] { typeof(Exception) })), Throws.Exception.TypeOf<ApplicationException>());
			Assert.That(n, Is.EqualTo(3));
		}

		[Test]
		public void Retry_NoWorkFor0Attempts()
		{
			var n = 0;
			RetryUtility.Retry(() => { n++; }, 0, 1);
			Assert.That(n, Is.EqualTo(0));
		}

		[Test]
		public void Retry_NoRetriesWithoutExceptionType()
		{
			var n = 0;
			Assert.That(() => RetryUtility.Retry(() => {
				n++;
				throw new ApplicationException();
			}, 3, 1, null), Throws.Exception.TypeOf<ApplicationException>());
			Assert.That(n, Is.EqualTo(1));
		}

		[Test]
		public void Retry_DefaultsToIOException()
		{
			var n = 0;
			Assert.That(() => RetryUtility.Retry(() => {
				n++;
				throw new IOException();
			}, 3, 1, null), Throws.Exception.TypeOf<IOException>());
			Assert.That(n, Is.EqualTo(3));
		}

		[Test]
		public void Retry_CatchesSpecifiedExceptionsOnly()
		{
			var n = 0;
			Assert.That(() => RetryUtility.Retry(() => {
				n++;
				throw new Exception();
			}, 3, 1, new HashSet<Type> (new[] { typeof(ApplicationException) })),
				Throws.Exception.TypeOf<Exception>());
			Assert.That(n, Is.EqualTo(1));
		}
	}
}
