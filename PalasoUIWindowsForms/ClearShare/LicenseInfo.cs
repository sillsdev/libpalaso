using System;
using System.Collections.Generic;
using System.Drawing;

namespace Palaso.UI.WindowsForms.ClearShare
{
	/// <summary>
	/// !!!!!!!!!!!!!! To keep from losing edits (having the owning metaccess not know that changes should be saved) these need to be immutable
	/// </summary>
	public abstract class LicenseInfo
	{
		public static LicenseInfo FromXmp(Dictionary<string, string> properties)
		{
			if(properties.ContainsKey("license") && properties["license"].Contains("creativecommons"))
			{
				return CreativeCommonsLicense.FromMetadata(properties);
			}
			return new NullLicense();
		}

		public abstract string GetDescription(string iso639_3LanguageCode);

		virtual public Image GetImage()
		{
			return null;
		}

		/// <summary>
		/// It doesn't make sense to let the user edit the description of a well-known license, even if the meta data is unlocked.
		/// </summary>
		public virtual bool EditingAllowed
		{
			get { throw new NotImplementedException(); }
		}

		public abstract string Url { get; set; }
	}

	public class NullLicense : LicenseInfo
	{
		public override string GetDescription(string iso639_3LanguageCode)
		{
			throw new NotImplementedException();
		}

		public override string Url
		{
			get { return ""; }
			set { throw new NotImplementedException(); }
		}
	}

	public class CustomLicense : LicenseInfo
	{
		public void SetDescription(string iso639_3LanguageCode, string description)
		{
		}

		public override string GetDescription(string iso639_3LanguageCode)
		{
			return "";
		}

		public override Image GetImage()
		{
			return null;
		}

		public override bool EditingAllowed
		{
			get { return true; }
		}

		public override string Url { get; set; }
	}
}