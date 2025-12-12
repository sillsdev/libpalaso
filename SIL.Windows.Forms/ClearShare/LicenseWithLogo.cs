using System.Drawing;
using SIL.Core.ClearShare;
using System.Collections.Generic;
using L10NSharp;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// Describes a single license, under which many works can be licensed for use
	/// </summary>
	public class LicenseWithLogo : License
	{
		public Image Logo { get; private set; }

		//Review (JH asks in Oct 2016): Why does this exist? The only uses in libpalaso are in tests and examples. Bloom does not use it.
		// Review Ariel June 2025: CreativeCommonsLicenseWithImage.FromToken is used in FieldWorks and in Bloom, but LicenseWithLogo.FromToken has no uses in Bloom or anywhere in sillsdev except in libpalaso tests & examples.
		public static LicenseInfo FromToken(string abbr)
		{
			switch (abbr)
			{
				case "ask": return new NullLicense();
				case "custom": return new CustomLicense();
				default:
					return CreativeCommonsLicense.FromToken(abbr);
			}
		}

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			unchecked
			{
				int result = base.GetHashCode();
				result = (result * 397) ^ (Logo != null ? Logo.GetHashCode() : 0);
				return result;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override bool Equals(object other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (GetType() != other.GetType())
				return false;

			return AreContentsEqual(other as License);
		}
	}
}
