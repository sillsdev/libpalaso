﻿using NUnit.Framework;
using Palaso.BuildTasks.StampAssemblies;

namespace Palaso.BuildTask.Tests
{
	[TestFixture]
	public class StampAssemblyTests
	{

		[Test]
		public void GetExistingAssemblyVersion_Normal_GetsAllFourParts()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""1.*.3.44"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var v = stamper.GetExistingAssemblyVersion(content);
			Assert.AreEqual("1", v.parts[0]);
			Assert.AreEqual("*", v.parts[1]);
			Assert.AreEqual("3", v.parts[2]);
			Assert.AreEqual("44", v.parts[3]);
		}

		[Test]
		public void GetModifiedContents_LastTwoLeftToBuildScript_CorrectMerge()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.7.*.*"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, "*.*.123.456", null);
			Assert.IsTrue(s.Contains("0.7.123.456"));
		}

		[Test]
		public void GetModifiedContents_LastPartIsHash_HashReplacedWithZero()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.7.*.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, "*.*.123.9e1b12ec3712", null);
			Assert.IsTrue(s.Contains("0.7.123.0"));
		}

		/// <summary>
		/// Test that our regex doesn't choke on "1.0.*"
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingHasShortForm_TreatsMissingAsStar()
		{
			var stamper = new StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""1.2.*"")]
[assembly: AssemblyFileVersion(""1.2.*"")]";

			var s = stamper.GetModifiedContents(content, "*.*.345.6", null);
			Assert.IsTrue(s.Contains("1.2.345.6"));
		}

		/// <summary>
		/// Test the situation we've seen with team city where the existing is *.*.*.*"
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingAllStars_UseZeroAsNeeded()
		{
			var stamper = new StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""*.*.*.*"")]
[assembly: AssemblyFileVersion(""*.*.*.*"")]";

			var s = stamper.GetModifiedContents(content, "*.*.345.6", null);
			Assert.IsTrue(s.Contains("0.0.345.6"));
		}

		[Test]
		public void GetModifiedContents_Existing1000_UseZeroAsNeeded()
		{
			var stamper = new StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, "*.*.121.93bc7076063f", null);
			Assert.IsTrue(s.Contains("1.0.121.0"));
		}


		/// <summary>
		/// This is actually what our build scripts do as of Sept 2010... they don't care what is in the assembly.cs.
		/// The buid.proj file specifies something like 0.3.$(BuildCounter)
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingHasNumbersButCallSpecifiesWholeVersion_UsesTheIncomingVersion()
		{
			var stamper = new StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.0.9.789"")]
[assembly: AssemblyFileVersion(""0.0.9.789"")]";

			var s = stamper.GetModifiedContents(content, "0.3.14", null);
			Assert.IsTrue(s.Contains("0.3.14"), s);
		}

		/// <summary>
		/// this is a mystery because the same data fails on Team city. There, it logs
		/// StampAssemblies: Merging existing 0.1.9999.9999 with incoming *.*.121.93bc7076063f to produce 0.0.121.0.
		/// here, it logs
		/// StampAssemblies: Merging existing 0.1.9999.9999 with incoming *.*.121.93bc7076063f to produce 0.1.121.0.
		/// </summary>
		[Test]
		public void GetModifiedContents_Puzzle()
		{
			var stamper = new StampAssemblies();
			var content =
				@"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle('SayMore')]
[assembly: AssemblyDescription('')]
[assembly: AssemblyConfiguration('')]
[assembly: AssemblyCompany('SIL International')]
[assembly: AssemblyProduct('SayMore')]
[assembly: AssemblyCopyright('')]
[assembly: AssemblyTrademark('')]
[assembly: AssemblyCulture('')]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid('f83ce30d-c534-4127-8ba5-7cfd5f4998bb')]

//thsese are marked with 9's to flag that if a version gets out with these,
// then it wasn't built by our build system so we don't really know what version it is
[assembly: AssemblyVersion('0.1.9999.9999')]
[assembly: AssemblyFileVersion('0.1.9999.9999')]

".Replace('\'', '"');

			var s = stamper.GetModifiedContents(content, "*.*.121.93bc7076063f", null);
			Assert.IsTrue(s.Contains("0.1.121.0"));
		}

		[Test]
		public void GetModifiedContents_FileVersionFollowsParam()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""9.9.9.99"")]
[assembly: AssemblyFileVersion(""8.8.8.88"")]";

			var v = stamper.GetModifiedContents(content, "5.4.3.2", "1.2.3.4");
			Assert.IsTrue(v.Contains("AssemblyVersion(\"5.4.3.2"));
			Assert.IsTrue(v.Contains("AssemblyFileVersion(\"1.2.3.4"));
			Assert.IsFalse(v.Contains("9.9.9.99"));
			Assert.IsFalse(v.Contains("8.8.8.88"));
		}

		[Test]
		public void GetModifiedContents_FileVersionMatchesVersionWhenMissing()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""9.9.9.99"")]
[assembly: AssemblyFileVersion(""8.8.8.88"")]";

			var v = stamper.GetModifiedContents(content, "5.4.3.2", null);
			Assert.IsTrue(v.Contains("AssemblyVersion(\"5.4.3.2"));
			Assert.IsTrue(v.Contains("AssemblyFileVersion(\"5.4.3.2"));
			Assert.IsFalse(v.Contains("9.9.9.99"));
			Assert.IsFalse(v.Contains("8.8.8.88"));
		}
		/// <summary>
		/// This test simulates the actual configuration of the icu.net wrapper as of January 2013.
		/// In order to have easier compatability between Palaso and FLEx which use different versions
		/// we did not want the AssemblyVersion to change after each build.
		/// </summary>
		[Test]
		public void GetModifiedContents_ActualICUConfigTest()
		{
			var stamper = new StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""4.2.1.0"")]
[assembly: AssemblyFileVersion(""4.2.1.0"")]";

			var v = stamper.GetModifiedContents(content, "*.*.*", "*.*.*.346");
			Assert.IsTrue(v.Contains("AssemblyVersion(\"4.2.1.0"));
			Assert.IsTrue(v.Contains("AssemblyFileVersion(\"4.2.1.346"));
			Assert.IsFalse(v.Contains("AssemblyFileVersion(\"4.2.1.0"));
		}
	}
}
