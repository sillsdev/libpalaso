using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Migration;

namespace Palaso.Tests.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	[TestFixture]
	public class Version0Migrator
	{
		[Test]
		public void Migrate_IsMigratedToV1()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_VariantIsAudioConformScriptIsNot_ScriptIsFixed()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_LanguageContainsValidVariant_ValidVariantIsMovedToVariant()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_LanguageContainsValidScript_ValidScriptIsMovedToScript()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_LanguageContainsInvalidLanguageTag_EntireTagIsMarkedAsPrivateUse()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_ScriptTagContainsInvalidScript_ScriptIsSetToUnknown()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_RegionTagContainsInvalidRegion_RegionIsSetToUnknown()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void Migrate_VariantTagContainsInvalidVariant_VariantIsMarkedAsPrivateUse()
		{
			throw new NotImplementedException();
		}
	}
}
