// Copyright (c) 2026 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using L10NSharp;
using NUnit.Framework;
using SIL.Core.Desktop.i18n;
using SIL.TestUtilities;

namespace SIL.Tests.i18n
{
	/// <summary>
	/// Tests basic functionality of <see cref="SIL.Core.Desktop.i18n.L10NSharpLocalizer"/>, which
	/// wraps L10NSharp's <see cref="LocalizationManager"/>.
	/// </summary>
	[TestFixture]
	public class L10NSharpLocalizerTests
	{
		private TemporaryFolder _tempFolder;
		private ILocalizationManager _manager;
		private L10NSharpLocalizer _localizer;

		private const string kLang = "en";
		private const string kAppId = "L10NSharpLocalizerTest";
		private const string kAppName = "L10NSharpLocalizer Test";

		[SetUp]
		public void SetUp()
		{
			_tempFolder = new TemporaryFolder(kAppId);
			_manager = LocalizationManager.Create(
				kLang, kAppId, kAppName, "1.0", _tempFolder.Path, null, new[] { "SIL." }, null);
			_localizer = new L10NSharpLocalizer();
		}

		[TearDown]
		public void TearDown()
		{
			_manager?.Dispose();
			LocalizationManager.ForgetDisposedManagers();
			_tempFolder?.Dispose();
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
			const string kKey = "Static.No.Adds.Key";
			_localizer.GetString(kKey, "Not added to cache");

			var resultB = _localizer.GetIsStringAvailableForLangId(kKey, kLang);
			Assert.That(resultB, Is.False);
		}

		[Test]
		public void GetIsStringAvailableForLangId_KeyRegisteredWithGetDynamicString_ReturnsTrue()
		{
			const string kKey = "Dynamic.Adds.Key";
			_localizer.GetDynamicString(kAppId, kKey, "Added to cache");

			var resultB = _localizer.GetIsStringAvailableForLangId(kKey, kLang);
			Assert.That(resultB, Is.True);
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
			const string kFrAppName = kAppName + " French";
			const string kKey = "Test.Greeting";
			const string kEnglish = "Hello";
			const string kFrench = "Bonjour";

			File.WriteAllText(Path.Combine(_tempFolder.Path, $"{kFrAppId}.fr.xlf"),
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"" xmlns:sil=""http://sil.org/software/XLiff"">
	<file original=""{kFrAppId}.dll"" source-language=""en"" target-language=""fr"">
		<body>
			<trans-unit id=""{kKey}"" sil:dynamic=""true"">
				<source xml:lang=""en"">{kEnglish}</source>
				<target xml:lang=""fr"" state=""final"">{kFrench}</target>
			</trans-unit>
		</body>
	</file>
</xliff>");

			// For test isolation, create a new localization manager for French.
			// In actual usage, a single localization manager would be used for all locales.
			using var frManager = LocalizationManager.Create("fr", kFrAppId, kFrAppName, "1.0",
				_tempFolder.Path, null, new[] { "SIL." }, null);

			var result = _localizer.GetDynamicStringOrEnglish(
				kFrAppId, kKey, kEnglish, null, "fr");
			Assert.That(result, Is.EqualTo(kFrench));
		}
	}
}
