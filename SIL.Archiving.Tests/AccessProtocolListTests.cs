using NUnit.Framework;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	[Category("SkipOnTeamCity")]
	class AccessProtocolListTests
	{
		[Test]
		public void AccessProtocols_Load_LoadsProtocols()
		{
			var protocols = AccessProtocols.Load();

			Assert.NotNull(protocols);
			Assert.GreaterOrEqual(protocols.Count, 2);
		}
	}
}
