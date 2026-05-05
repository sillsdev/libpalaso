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
			if (_tempDir != null && Directory.Exists(_tempDir))
				Directory.Delete(_tempDir, recursive: true);
		}

		[Test]
		public void GetString_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetString("NoSuch.Key", "Fallback A");
			Assert.That(result, Is.EqualTo("Fallback A"));
		}

		[Test]
		public void GetString_WithComment_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetString("NoSuch.Key2", "Fallback B", "Comment text");
			Assert.That(result, Is.EqualTo("Fallback B"));
		}

		[Test]
		public void UILanguageId_ReturnsActiveLocale()
		{
			Assert.That(_localizer.UILanguageId, Is.EqualTo(kLang));
		}

		[Test]
		public void GetIsStringAvailableForLangId_UnknownKey_ReturnsFalse()
		{
			var result = _localizer.GetIsStringAvailableForLangId("NoSuch.Key3", kLang);
			Assert.That(result, Is.False);
		}

		[Test]
		public void GetDynamicString_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetDynamicString(kAppId, "NoSuch.Dynamic", "Fallback C");
			Assert.That(result, Is.EqualTo("Fallback C"));
		}

		[Test]
		public void GetDynamicStringOrEnglish_UnknownKey_ReturnsFallbackEnglish()
		{
			var result = _localizer.GetDynamicStringOrEnglish(kAppId, "NoSuch.Dynamic2",
				"Fallback D", null, kLang);
			Assert.That(result, Is.EqualTo("Fallback D"));
		}
	}
}
