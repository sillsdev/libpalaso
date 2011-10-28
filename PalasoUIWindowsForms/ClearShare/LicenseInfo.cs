using System;
using System.Drawing;

namespace Palaso.UI.WindowsForms.ClearShare
{
	/// <summary>
	/// !!!!!!!!!!!!!! To keep from losing edits (having the owning metaccess not know that changes should be saved) these need to be immutable
	/// </summary>
	public abstract class LicenseInfo
	{
		public string GetDescription(string iso639_3LanguageCode)
		{
			//if we don't have it, just return English ("en")
			return "to do";
		}

//        public void SetDescription(string iso639_3LanguageCode, string description)
//        {
//        }

		public abstract Image GetImage();

		/// <summary>
		/// It doesn't make sense to let the user edit the description of a well-known license, even if the meta data is unlocked.
		/// </summary>
		public abstract bool EditingAllowed{ get;}

		public string Url { get; set; }
	}

	public class CreativeCommonsLicense : LicenseInfo
	{
		public CreativeCommonsLicense()
		{
			Url = "http://creativecommons.org/licenses/by-sa/2.0/";
		}
		//we'll need to give out an image, description, url.
		//what you *store* in the image metadata is a different question.
		public override Image GetImage()
		{
			return LicenseLogos.cc_BY_88x31;
		}

		public override bool EditingAllowed
		{
			get { return false; }
		}

		public static LicenseInfo FromUrl(string url)
		{
			return new CreativeCommonsLicense() {Url = url};
		}
	}

	public class CustomLicense : LicenseInfo
	{
		public void SetDescription(string iso639_3LanguageCode, string description)
		{
		}

		public override Image GetImage()
		{
			throw new NotImplementedException();
		}

		public override bool EditingAllowed
		{
			get { return true; }
		}
	}
}