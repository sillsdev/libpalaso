using System;
using NUnit.Framework;
using Palaso.Data;

namespace Palaso.Tests.Data
{
	[TestFixture]
	public class PreciseDateTimeTests
	{
		[Test]
		public void Test()
		{
			DateTime previousDt = DateTime.MinValue;
			for (int i = 0;i != 1000000;++i)
			{
				DateTime dt = PreciseDateTime.UtcNow;
				Assert.Less(previousDt,
							dt,
							"New time should come after old time but were {0} followed by {1}, iteration {2}",
							previousDt.Ticks,
							dt.Ticks,
							i);
				previousDt = dt;
			}
			previousDt = PreciseDateTime.UtcNow;
			DateTime dt2 = DateTime.UtcNow;
			if (dt2 < previousDt)
			{
				Console.WriteLine("Time creep: {0} ticks (1 tick = 100ns)", previousDt.Ticks - dt2.Ticks);
			}
		}
	}
}