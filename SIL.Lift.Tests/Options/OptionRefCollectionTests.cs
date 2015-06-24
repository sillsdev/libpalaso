using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using SIL.Lift.Options;
using SIL.TestUtilities;

namespace SIL.Lift.Tests.Options
{
	[TestFixture]
	public class OptionRefCollectionCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new OptionRefCollection();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_parent|PropertyChanged|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet(new BindingList<OptionRef> {new OptionRef("option")}, new BindingList<OptionRef> {new OptionRef("not an option")})
							 };
			}
		}
	}

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


		[Test]
		public void Merge_BothEmpty_Empty()
		{
			OptionRefCollection a = new OptionRefCollection(this);
			OptionRefCollection b = new OptionRefCollection(this);
			a.MergeByKey(b);
			Assert.AreEqual(0, a.Count);
		}
		[Test]
		public void Merge_OtherEmpty_Unchanged()
		{
			OptionRefCollection a = new OptionRefCollection(this);
			a.Add("1");
			OptionRefCollection b = new OptionRefCollection(this);
			a.MergeByKey(b);
			Assert.AreEqual(1, a.Count);
		}
		[Test]
		public void Merge_TargetEmptyOtherNot_GetsOther()
		{
			OptionRefCollection a = new OptionRefCollection(this);
			OptionRefCollection b = new OptionRefCollection(this);
			b.Add("1");
			b.Add("2");
			a.MergeByKey(b);
			Assert.AreEqual(2, a.Count);
			Assert.AreEqual(2, b.Count);
		}

		[Test]
		public void Merge_OneOverlap_NoDuplicates()
		{
			OptionRefCollection a = new OptionRefCollection(this);
			a.Add("1");
			a.Add("2");
			OptionRefCollection b = new OptionRefCollection(this);
			b.Add("1");
			b.Add("3");
			a.MergeByKey(b);
			Assert.AreEqual(3, a.Count);
			Assert.Contains("1", a.Keys.ToArray());
			Assert.Contains("2", a.Keys.ToArray());
			Assert.Contains("3", a.Keys.ToArray());
		}

		[Test]
		public void Merge_SourceHasEmbeddedXml_Copied()
		{
			OptionRefCollection a = new OptionRefCollection(this);
			a.Add("1");
			OptionRefCollection b = new OptionRefCollection(this);
			b.Add("1");
			((OptionRef)(b.Members[0])).EmbeddedXmlElements.Add("line1");
			((OptionRef)(b.Members[0])).EmbeddedXmlElements.Add("line2");
			a.MergeByKey(b);
			Assert.AreEqual(1, a.Count);
			Assert.AreEqual(2, ((OptionRef)(a.Members[0])).EmbeddedXmlElements.Count);
		}

		[Test]
		public void Merge_SourceAndTargetBothEmbeddedXml_ReturnsFalseWithTargetUnchanged()
		{
				OptionRefCollection a = new OptionRefCollection(this);

				a.Add("1");
				((OptionRef) (a.Members[0])).EmbeddedXmlElements.Add("line1");
				OptionRefCollection b = new OptionRefCollection(this);
				b.Add("1");
				b.Add("2");
				((OptionRef) (b.Members[0])).EmbeddedXmlElements.Add("line1");
				var result =  a.MergeByKey(b);
				Assert.IsFalse(result);
				Assert.AreEqual(1, a.Members.Count);
		}

		public void NotifyPropertyChanged(string property)
		{
			//Do Nothing
		}
	}
}
