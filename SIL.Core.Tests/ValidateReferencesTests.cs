using System.Reflection;
using NUnit.Framework;

namespace SIL.Tests
{
	[TestFixture]
    public class ValidateReferencesTests
    {
		[Test]
		public void EnsureNoUIReferences()
		{
			Assembly coreAssembly = Assembly.Load("SIL.Core");
			Assert.IsNotNull(coreAssembly, "Could not find SIL.Core.dll");

			foreach (AssemblyName referencedAssembly in coreAssembly.GetReferencedAssemblies())
			{
				// If this test fails, then make sure you didn't add a reference to one of the following DLLs.
				// These DLLs can not be referenced because SIL.Core is needed on mobile platforms which do
				// not have access to these libraries.
				// Any code that depends on these should be added to SIL.Core.Desktop.
				Assert.AreNotEqual("System.Drawing", referencedAssembly.Name);
				Assert.AreNotEqual("System.Management", referencedAssembly.Name);
				Assert.AreNotEqual("System.Configuration", referencedAssembly.Name);

				// Any code that depends on this should be added to SIL.Windows.Forms
				Assert.AreNotEqual("System.Windows.Forms", referencedAssembly.Name);
			}
		}
    }
}
