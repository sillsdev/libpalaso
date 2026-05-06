// Copyright (c) 2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using L10NSharp;
using NUnit.Framework;
using SIL.Core.Desktop.i18n;

namespace SIL.Tests.i18n
{
	[TestFixture]
	public class L10NSharpLocalizerTests
	{
		private string _tempDir;
		private ILocalizationManager _manager;
		private L10NSharpLocalizer _localizer;

		private const string kLang = "en";
		private const string kAppId = "L10NSharpLocalizerTests";

		[SetUp]
		public void SetUp()
		{
			_tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_tempDir);
			_manager = LocalizationManager.Create(kLang, kAppId, "L10NSharpLocalizer Tests",
				"1.0", _tempDir, null, new[] { "SIL." }, null);
			_localizer = new L10NSharpLocalizer();
		}

		[TearDown]
		public void TearDown()
		{
			_manager?.Dispose();
			LocalizationManager.ForgetDisposedManagers();
			if (_tempDir != null && Directory.Exists(_tempDir))
				Directory.Delete(_tempDir, recursive: true);
		}

		[Test]
		public void GetString_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetString("NoSuch.Key.Static", "Static fallback");
			Assert.That(result, Is.EqualTo("Static fallback"));
		}

		[Test]
		public void UILanguageId_ReturnsActiveLocale()
		{
			Assert.That(_localizer.UILanguageId, Is.EqualTo(kLang));
		}

		[Test]
		public void GetIsStringAvailableForLangId_UnknownKey_ReturnsFalse()
		{
			var result = _localizer.GetIsStringAvailableForLangId("NoSuch.Key", kLang);
			Assert.That(result, Is.False);
		}

		[Test]
		public void GetDynamicStringOrEnglish_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetDynamicStringOrEnglish(kAppId, "NoSuch.Key.Dynamic",
				"Dynamic fallback", null, kLang);
			Assert.That(result, Is.EqualTo("Dynamic fallback"));
		}

		[Test]
		public void GetString_WithTranslation_ReturnsTranslatedString()
		{
			const string kFrAppId = kAppId + "Fr";
			const string kKey = "Test.Greeting";
			const string kEnglish = "Hello";
			const string kFrench = "Bonjour";

			var xliffPath = Path.Combine(_tempDir, kFrAppId + ".fr.xlf");
			File.WriteAllText(xliffPath, $@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff xmlns=""urn:oasis:names:tc:xliff:document:1.2"" version=""1.2"">
	<file source-language=""en"" original=""{kFrAppId}.dll"" target-language=""fr"">
		<body>
			<trans-unit id=""{kKey}"">
				<source xml:lang=""en"">{kEnglish}</source>
				<target xml:lang=""fr"" state=""final"">{kFrench}</target>
			</trans-unit>
		</body>
	</file>
</xliff>");

			using var frManager = LocalizationManager.Create("fr", kFrAppId,
				"L10NSharpLocalizer Tests", "1.0", _tempDir, null, new[] { "SIL." }, null);

			var result = _localizer.GetDynamicStringOrEnglish(
				kFrAppId, kKey, kEnglish, null, "fr");
			Assert.That(result, Is.EqualTo(kFrench));
		}
	}
}
