using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Annotations;
using SIL.Lift.Options;
using SIL.TestUtilities;

namespace SIL.Lift.Tests.Options
{
	[TestFixture]
	public class OptionRefCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new OptionRef();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//_suspendNotification: only turned on and off in one method, don't need to clone
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_parent|_suspendNotification|PropertyChanged|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet("to be", "!(to be)"),
								 new ValuesToSet(new List<string>{"to", "be"}, new List<string>{"!","to","be"}),
								 new ValuesToSet(new Annotation{IsOn = false}, new Annotation{IsOn = true})
							 };
			}
		}
	}

	[TestFixture]
	public class OptionRefTests
	{
		[Test]
		public void CompareTo_Null_ReturnsGreater()
		{
			OptionRef reference = new OptionRef();
			Assert.AreEqual(1, reference.CompareTo(null));
		}

		[Test]
		public void CompareTo_OtherHasGreaterKey_ReturnsLess()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key1";
			OptionRef other = new OptionRef();
			other.Key = "key2";
			Assert.AreEqual(-1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasLesserKey_ReturnsGreater()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key2";
			OptionRef other = new OptionRef();
			other.Key = "key1";
			Assert.AreEqual(1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasSameKey_ReturnsLesser()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key1";
			OptionRef other = new OptionRef();
			other.Key = "key1";
			Assert.AreEqual(0, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherIsNotOptionRef_Throws()
		{
			var reference = new OptionRef();
			const string other = "";
			Assert.Throws<ArgumentException>(
				() => reference.CompareTo(other)
			);
		}
	}
}
