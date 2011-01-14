using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class WritingSystemDefinitionValidatorTests
	{
		[Test]
		public void IsValidWritingSystem_IsoIsNotEmpty_ReturnsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			Assert.IsTrue(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_IsoIsEmpty_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = String.Empty;
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioAndScriptIsNotSetCorrectly_ReturnsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "Zxxx";
			ws.Variant = WellKnownSubTags.Audio.VariantMarker;
			Assert.IsTrue(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_VariantContainsUnderscoreInsteadOfDash_Throws()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "bogus";
			Assert.Throws<ArgumentException>(() => ws.Variant = "x_audio");
		}

		[Test]
		public void IsValidWritingSystem_VariantContainsCapitalXDashAUDIOAndScriptIsZxxx_WsIsTreatedAsAudioWsAndReturnsFalseDueToBadScriptSubTag()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "bogus";
			ws.Variant = "X-AUDIO";
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioAndScriptIsCapitalZXXX_ReturnsTrue()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "ZXXX";
			ws.Variant = WellKnownSubTags.Audio.VariantMarker;
			Assert.IsTrue(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioAndScriptIsNotZxxx_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "ltn";
			ws.Variant = WellKnownSubTags.Audio.VariantMarker;
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_VariantIndicatesThatWsIsAudioButContainsotherThanJustTheNecassaryXDashAudioTagAndScriptIsNotZxxx_WsIsTreatedAsAudioWsAndReturnsFalseDueToBadScriptSubTag()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de";
			ws.Script = "ltn";
			ws.Variant = "x-private-x-audio";
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_LanguageSubtagContainsXDashAudio_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(()=>ws.ISO = "de-x-audio");
		}

		[Test]
		public void IsValidWritingSystem_LanguageContainsZxxx_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de-Zxxx";
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_LanguageSubtagContainsCapitalXDashAudio_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			Assert.Throws<ArgumentException>(()=>ws.ISO = "de-X-AuDiO");
		}

		[Test]
		public void IsValidWritingSystem_LanguageSubtagContainsCapitalZxxx_ReturnsFalse()
		{
			WritingSystemDefinition ws = new WritingSystemDefinition();
			ws.ISO = "de-ZXXX";
			Assert.IsFalse(WritingSystemDefinitionValidator.IsValidWritingSystem(ws));
		}

		[Test]
		public void IsValidWritingSystem_ScriptSubtagContainsOtherThanLegalScripts_ReturnsFalse()
		{
			throw new NotImplementedException();
		}
	}
}
