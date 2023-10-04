using NUnit.Framework;
using SIL.ObjectModel;

namespace SIL.Tests.ObjectModel
{
	[TestFixture]
	public class BulkObservableListTests
	{
		[Test]
		public void MoveRange_MoveSingleForward()
		{
			var list = new BulkObservableList<string> { "item1", "item2", "item3" };
			list.MoveRange(0, 1, 1);
			Assert.That(list, Is.EqualTo(new[] { "item2", "item1", "item3" }));
		}

		[Test]
		public void MoveRange_MoveSingleBackward()
		{
			var list = new BulkObservableList<string> { "item1", "item2", "item3" };
			list.MoveRange(1, 1, 0);
			Assert.That(list, Is.EqualTo(new[] { "item2", "item1", "item3" }));
		}

		[Test]
		public void MoveRange_MoveMultipleForward()
		{
			var list = new BulkObservableList<string> { "item1", "item2", "item3", "item4" };
			list.MoveRange(0, 2, 3);
			Assert.That(list, Is.EqualTo(new[] { "item3", "item1", "item2", "item4" }));
		}

		[Test]
		public void MoveRange_MoveMultipleBackward()
		{
			var list = new BulkObservableList<string> { "item1", "item2", "item3", "item4" };
			list.MoveRange(1, 2, 0);
			Assert.That(list, Is.EqualTo(new[] { "item2", "item3", "item1", "item4" }));
		}
	}
}
