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
			else if ( properties.ContainsKey("rights (en)"))
			{
				return CustomLicense.FromMetadata(properties);
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

		public bool HasChanges { get; set; }

		/// <summary>
		/// custom or extra rights
		/// </summary>
		public string RightsStatement {get; set; }
	}

	public class NullLicense : LicenseInfo
	{
		public override string GetDescription(string iso639_3LanguageCode)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "No License";
		}

		public override string Url
		{
			get { return ""; }
			set { }
		}
	}


	public class CustomLicense : LicenseInfo
	{
//        public void SetDescription(string iso639_3LanguageCode, string description)
//        {
//			RightsStatement = description;
//        }

		public override string ToString()
		{
			return "Custom License";
		}

		public override string GetDescription(string iso639_3LanguageCode)
		{
			if (string.IsNullOrEmpty(RightsStatement))
				return "For permission to reuse, contact the copyright holder.";
			return RightsStatement;
		}

		public override Image GetImage()
		{
			return null;
		}

		public override bool EditingAllowed
		{
			get { return false; }//it may be ok, but we can't read the description.
		}

		public override string Url { get; set; }

		public static LicenseInfo FromMetadata(Dictionary<string, string> properties)
		{
			 if(!properties.ContainsKey("rights (en)"))
				throw new ApplicationException("A license property is required in order to make a  Custom License from metadata.");

			var license = new CustomLicense();
			license.RightsStatement = properties["rights (en)"];
			return license;
		}
	}


}