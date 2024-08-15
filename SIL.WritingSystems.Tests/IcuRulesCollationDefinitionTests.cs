using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class IcuRulesCollationDefinitionTests
	{
		[Test]
		public void Validate_SelfReferencingImport_Valid()
		{
			var ws = new WritingSystemDefinition("en-US");
			ws.Collations.Add(new IcuRulesCollationDefinition("private") {IcuRules = "&B<t<<<T<s<<<S<e<<<E\r\n"});
			var collation = new IcuRulesCollationDefinition("standard")
			{
				OwningWritingSystemDefinition = ws,
				Imports = {new IcuCollationImport("en-US", "private")},
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			string message;
			Assert.That(collation.Validate(out message), Is.True);
			Assert.That(collation.CollationRules, Is.EqualTo("&B<t<<<T<s<<<S<e<<<E\r\n&C<k<<<K<x<<<X<i<<<I\r\n"));
		}

		[Test]
		public void Validate_NonSelfReferencingImport_Valid()
		{
			var ws = new WritingSystemDefinition("en-US");
			ws.Collations.Add(new IcuRulesCollationDefinition("other") {IcuRules = "&B<t<<<T<s<<<S<e<<<E\r\n"});
			var wsFactory = new TestWritingSystemFactory();
			wsFactory.WritingSystems.Add(ws);
			var collation = new IcuRulesCollationDefinition("standard")
			{
				WritingSystemFactory = wsFactory,
				Imports = {new IcuCollationImport("en-US", "other")},
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			string message;
			Assert.That(collation.Validate(out message), Is.True);
			Assert.That(collation.CollationRules, Is.EqualTo("&B<t<<<T<s<<<S<e<<<E\r\n&C<k<<<K<x<<<X<i<<<I\r\n"));
		}

		[Test]
		public void Validate_SelfReferencingImport_OwningWritingSystemDefinitionNotSet_NotValid()
		{
			var collation = new IcuRulesCollationDefinition("standard")
			{
				Imports = {new IcuCollationImport("en-US", "private")},
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			string message;
			Assert.That(collation.Validate(out message), Is.False);
			Assert.That(message, Is.EqualTo("Unable to import the private collation rules from en-US."));
		}

		[Test]
		public void Validate_NonSelfReferencingImport_WritingSystemFactoryNotSet_NotValid()
		{
			var collation = new IcuRulesCollationDefinition("standard")
			{
				Imports = {new IcuCollationImport("en-US", "other")},
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			string message;
			Assert.That(collation.Validate(out message), Is.False);
			Assert.That(message, Is.EqualTo("Unable to import the other collation rules from en-US."));
		}

		[Test]
		public void Validate_ImportInvalidRules_NotValid()
		{
			var ws = new WritingSystemDefinition("en-US");
			ws.Collations.Add(new IcuRulesCollationDefinition("private") {IcuRules = "&&&B<t<<<T<s<<<S<e<<<E\r\n"});
			var collation = new IcuRulesCollationDefinition("standard")
			{
				OwningWritingSystemDefinition = ws,
				Imports = {new IcuCollationImport("en-US", "private")},
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			string message;
			Assert.That(collation.Validate(out message), Is.False);
			Assert.That(message, Is.EqualTo("Unable to import the private collation rules from en-US."));
		}

		[Test]
		public void Collation_GetSortKeyWorks()
		{
			var ws = new WritingSystemDefinition("en-US");
			ws.Collations.Add(new IcuRulesCollationDefinition("private")
				{ IcuRules = "&B<t<<<T<s<<<S<e<<<E\r\n" });
			var collation = new IcuRulesCollationDefinition("standard")
			{
				OwningWritingSystemDefinition = ws,
				Imports = { new IcuCollationImport("en-US", "private") },
				IcuRules = "&C<k<<<K<x<<<X<i<<<I\r\n"
			};
			var sortKey = collation.Collator.GetSortKey("test");
			Assert.That(sortKey, Is.Not.Null);
		}
	}
}
