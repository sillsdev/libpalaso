using System;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	internal class KeyboardDefinitionV1
	{
		private readonly string _layout;
		private readonly string _locale;

		public KeyboardDefinitionV1(string layout, string locale)
		{
			_layout = layout;
			_locale = locale;
		}

		public string Layout
		{
			get { return _layout; }
		}

		public string Locale
		{
			get { return _locale; }
		}

		public PlatformID OperatingSystem { get; set; }
	}
}
