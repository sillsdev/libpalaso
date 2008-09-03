using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems.Collation;

namespace Palaso.Tests.WritingSystems.Collation
{
	[TestFixture]
	public class SystemCollatorTests
	{
		private SystemCollator _collator;

		[Test]
		public void NullId_ProducesCollator()
		{
			_collator = new SystemCollator(null);
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void EmptyId_ProducesCollator()
		{
			_collator = new SystemCollator(String.Empty);
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void InvalidId_ProducesCollator()
		{
			_collator = new SystemCollator("This shouldn't be a valid culture ID.");
			Assert.IsNotNull(_collator);
		}

		[Test]
		public void English_ProducesCollator()
		{
			_collator = new SystemCollator("en");
			Assert.IsNotNull(_collator);
		}
	}
}
