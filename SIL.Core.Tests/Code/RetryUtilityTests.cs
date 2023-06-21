using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SIL.Code;

namespace SIL.Tests.Code
{
	/// <summary>
	/// This is a work in progress. At this point it only has a test for one recently-added bit of functionality.
	/// </summary>
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
	}
}
