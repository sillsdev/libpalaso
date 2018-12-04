using System.Collections.Generic;
using NUnit.Framework;
using SIL.TestUtilities;
using Is = SIL.TestUtilities.NUnitExtensions.Is;

namespace SIL.WritingSystems.Tests
{
	public class CharacterSetDefinitionCloneableTests : CloneableTests<CharacterSetDefinition>
	{
		public override CharacterSetDefinition CreateNewCloneable()
		{
			return new CharacterSetDefinition("main");
		}

		protected override bool Equals(CharacterSetDefinition x, CharacterSetDefinition y)
		{
			if (x == null)
				return y == null;
			return x.ValueEquals(y);
		}

		public override string ExceptionList
		{
			get { return "|IsChanged|_characters|PropertyChanged|PropertyChanging|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
				{
					new ValuesToSet("to be", "!(to be)")
				};
			}
		}

		/// <summary>
		/// The generic test that clone copies everything can't handle lists.
		/// </summary>
		[Test]
		public void CloneCopiesCharacters()
		{
			var original = new CharacterSetDefinition("main");
			original.Characters.Add("a");
			original.Characters.Add("b");
			CharacterSetDefinition copy = original.Clone();
			Assert.That(copy.Characters, Is.EquivalentTo(new string[] {"a", "b"}));
		}

		[Test]
		public void ValueEqualsComparesCharacters()
		{
			var first = new CharacterSetDefinition("cs1");
			first.Characters.Add("a");
			first.Characters.Add("b");
			var second = new CharacterSetDefinition("cs1");

			Assert.That(first.ValueEquals(second), Is.False, "char set with empty characters should not equal one with some");
			second.Characters.Add("a");
			Assert.That(first.ValueEquals(second), Is.False, "char sets with different length character lists should not be equal");
			second.Characters.Add("b");
			Assert.That(first.ValueEquals(second), Is.True, "char sets with same character lists should be equal");

			second.Characters.Clear();
			second.Characters.Add("a");
			second.Characters.Add("c");
			Assert.That(first.ValueEquals(second), Is.False, "char sets with same-length lists of different characters should not be equal");
		}
	}
}
