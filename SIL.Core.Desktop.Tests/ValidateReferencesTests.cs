using System.Reflection;
using NUnit.Framework;

namespace SIL.Tests
{
	[TestFixture]
    public class ValidateReferencesTests
    {
		[Test]
		public void EnsureNoReferenceToWindowsForms()
		{
			Assembly desktopAssembly = Assembly.Load("SIL.Core.Desktop");
			Assert.IsNotNull(desktopAssembly, "Could not find SIL.Core.Desktop.dll");

			foreach (AssemblyName referencedAssembly in desktopAssembly.GetReferencedAssemblies())
			{
				// If this test fails, then make sure you didn't add a reference to the following DLL
				// Any code that depends on this should be added to SIL.Windows.Forms
				Assert.AreNotEqual("System.Windows.Forms", referencedAssembly.Name);
			}
		}
    }
}
