using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class StampAssemblyTests
	{

		[Test]
		public void GetExistingAssemblyVersion_Normal_GetsAllFourParts()
		{
			var stamper = new StampAssemblies.StampAssemblies();
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
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.7.*.*"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.123.456", null, null);
			Assert.That(s, Is.StringContaining("0.7.123.456"));
		}

		[Test]
		public void GetModifiedContents_LastPartIsHash_HashReplacedWithZero()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.7.*.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.123.9e1b12ec3712", null, null);
			Assert.That(s, Is.StringContaining("0.7.123.0"));
		}

		/// <summary>
		/// Test that our regex doesn't choke on "1.0.*"
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingHasShortForm_TreatsMissingAsStar()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""1.2.*"")]
[assembly: AssemblyFileVersion(""1.2.*"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.345.6", null, null);
			Assert.That(s, Is.StringContaining("1.2.345.6"));
		}

		/// <summary>
		/// Test the situation we've seen with team city where the existing is *.*.*.*"
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingAllStars_UseZeroAsNeeded()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""*.*.*.*"")]
[assembly: AssemblyFileVersion(""*.*.*.*"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.345.6", null, null);
			Assert.That(s, Is.StringContaining("0.0.345.6"));
		}

		[Test]
		public void GetModifiedContents_Existing1000_UseZeroAsNeeded()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.121.93bc7076063f", null, null);
			Assert.That(s, Is.StringContaining("1.0.121.0"));
		}


		/// <summary>
		/// This is actually what our build scripts do as of Sept 2010... they don't care what is in the assembly.cs.
		/// The buid.proj file specifies something like 0.3.$(BuildCounter)
		/// </summary>
		[Test]
		public void GetModifiedContents_ExistingHasNumbersButCallSpecifiesWholeVersion_UsesTheIncomingVersion()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content =
				@"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""0.0.9.789"")]
[assembly: AssemblyFileVersion(""0.0.9.789"")]";

			var s = stamper.GetModifiedContents(content, true, "0.3.14", null, null);
			Assert.That(s, Is.StringContaining("0.3.14"), s);
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
			var stamper = new StampAssemblies.StampAssemblies();
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

			var s = stamper.GetModifiedContents(content, true, "*.*.121.93bc7076063f", null, null);
			Assert.That(s, Is.StringContaining("0.1.121.0"));
		}

		[Test]
		public void GetModifiedContents_FileVersionFollowsParam()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""9.9.9.99"")]
[assembly: AssemblyFileVersion(""8.8.8.88"")]";

			var v = stamper.GetModifiedContents(content, true, "5.4.3.2", "1.2.3.4", null);
			Assert.That(v, Is.StringContaining("AssemblyVersion(\"5.4.3.2\")"));
			Assert.That(v, Is.StringContaining("AssemblyFileVersion(\"1.2.3.4\")"));
			Assert.That(v, Is.Not.StringContaining("9.9.9.99"));
			Assert.That(v, Is.Not.StringContaining("8.8.8.88"));
		}

		[Test]
		public void GetModifiedContents_FileVersionMatchesVersionWhenMissing()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""9.9.9.99"")]
[assembly: AssemblyFileVersion(""8.8.8.88"")]";

			var v = stamper.GetModifiedContents(content, true, "5.4.3.2", null, null);
			Assert.That(v, Is.StringContaining("AssemblyVersion(\"5.4.3.2"));
			Assert.That(v, Is.StringContaining("AssemblyFileVersion(\"5.4.3.2"));
			Assert.That(v, Is.Not.StringContaining("9.9.9.99"));
			Assert.That(v, Is.Not.StringContaining("8.8.8.88"));
		}
		/// <summary>
		/// This test simulates the actual configuration of the icu.net wrapper as of January 2013.
		/// In order to have easier compatability between Palaso and FLEx which use different versions
		/// we did not want the AssemblyVersion to change after each build.
		/// </summary>
		[Test]
		public void GetModifiedContents_ActualICUConfigTest()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion(""4.2.1.0"")]
[assembly: AssemblyFileVersion(""4.2.1.0"")]";

			var v = stamper.GetModifiedContents(content, true, "*.*.*", "*.*.*.346", null);
			Assert.That(v, Is.StringContaining("AssemblyVersion(\"4.2.1.0"));
			Assert.That(v, Is.StringContaining("AssemblyFileVersion(\"4.2.1.346"));
			Assert.That(v, Is.Not.StringContaining("AssemblyFileVersion(\"4.2.1.0"));
		}

		[Test]
		public void GetModifiedContents_InformationalVersionKeepsHashFromIncomingVersion_NoFileVersion()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"
[assembly: AssemblyVersion(""0.7.*.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]
[assembly: AssemblyInformationalVersion(""2.3.4.5"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.123.9e1b12ec3712", null, null);
			Assert.That(s, Is.StringContaining("AssemblyVersion(\"0.7.123.0\")"));
			Assert.That(s, Is.StringContaining("AssemblyFileVersion(\"1.0.123.0\")"));
			Assert.That(s, Is.StringContaining("AssemblyInformationalVersion(\"2.3.123.9e1b12ec3712\")"));
		}

		[Test]
		public void GetModifiedContents_InformationalVersionKeepsHashFromIncomingVersion_BothParams()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"
[assembly: AssemblyVersion(""0.7.*.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]
[assembly: AssemblyInformationalVersion(""2.3.4.5"")]";

			var s = stamper.GetModifiedContents(content, true, "*.*.123.9e1b12ec3712", "*.*.456.f7a874", null);
			Assert.That(s, Is.StringContaining("AssemblyVersion(\"0.7.123.0\")"));
			Assert.That(s, Is.StringContaining("AssemblyFileVersion(\"1.0.456.0\")"));
			Assert.That(s, Is.StringContaining("AssemblyInformationalVersion(\"2.3.123.9e1b12ec3712\")"));
		}

		[Test]
		public void GetModifiedContents_MSBuildProps()
		{
			var stamper = new StampAssemblies.StampAssemblies();
			var content = @"
<Version>1.2.*</Version>
<AssemblyVersion>0.7.*.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
<InformationalVersion>2.3.4.5</InformationalVersion>";

			var s = stamper.GetModifiedContents(content, false, "*.*.123.9e1b12ec3712", "*.*.456.f7a874", "*.*.3-4");
			Assert.That(s, Is.StringContaining("<Version>1.2.3-4</Version>"));
			Assert.That(s, Is.StringContaining("<AssemblyVersion>0.7.123.0</AssemblyVersion>"));
			Assert.That(s, Is.StringContaining("<FileVersion>1.0.456.0</FileVersion"));
			Assert.That(s, Is.StringContaining("<InformationalVersion>2.3.123.9e1b12ec3712</InformationalVersion>"));
		}
	}
}
