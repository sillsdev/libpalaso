using System;
using Palaso.Lift;
using Palaso.Lift.Options;

using NUnit.Framework;

namespace Palaso.LexicalModel.Tests
{
	[TestFixture]
	public class OptionRefCollectionTests : IReceivePropertyChangeNotifications
	{
		[Test]
		public void CompareTo_Null_ReturnsGreater()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			Assert.AreEqual(1, reference.CompareTo(null));
		}

		[Test]
		public void CompareTo_OtherHasFewerEntries_ReturnsGreater()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			reference.Add("key1");
			OptionRefCollection other = new OptionRefCollection(this);
			Assert.AreEqual(1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasMoreEntries_ReturnsLess()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			OptionRefCollection other = new OptionRefCollection(this);
			other.Add("key2");
			Assert.AreEqual(-1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasEntrythatIsAlphabeticallyEarlier_ReturnsGreater()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			reference.Add("key2");
			OptionRefCollection other = new OptionRefCollection(this);
			other.Add("key1");
			Assert.AreEqual(1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasEntrythatIsAlphabeticallyLater_ReturnsLess()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			reference.Add("key1");
			OptionRefCollection other = new OptionRefCollection(this);
			other.Add("key2");
			Assert.AreEqual(-1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHassameListEntries_ReturnsEqual()
		{
			OptionRefCollection reference = new OptionRefCollection(this);
			reference.Add("key1");
			OptionRefCollection other = new OptionRefCollection(this);
			other.Add("key1");
			Assert.AreEqual(0, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherIsNotOptionRef_Throws()
		{
			OptionRef reference = new OptionRef();
			string other = "";
			Assert.Throws<ArgumentException>(() =>
reference.CompareTo(other));
		}

		public void NotifyPropertyChanged(string property)
		{
			//Do Nothing
		}
	}
}
