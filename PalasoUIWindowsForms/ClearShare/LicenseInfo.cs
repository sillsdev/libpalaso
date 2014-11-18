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

		/// <summary>
		/// A string that is a good short indication of the license type, and can be used in FromToken.
		/// </summary>
		public abstract string Token { get; }

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

		virtual public Image GetImage()
		{
			return null;
		}

		/// <summary>
		/// It doesn't make sense to let the user edit the description of a well-known license, even if the meta data is unlocked.
		/// </summary>
		public virtual bool EditingAllowed
		{
			get { return false; }//we don't konw
		}

		public abstract string Url { get; set; }

		public bool HasChanges { get; set; }

		/// <summary>
		/// custom or extra rights. Note that accoring to Creative Commons, "extra" rights are expressly dissallowed by section 7 a:
		///     http://creativecommons.org/licenses/by-nc/4.0/legalcode#s7a
		///     "The Licensor shall not be bound by any additional or different terms or conditions communicated by You unless expressly agreed."
		///
		/// However, consider the case of one application that uses this library, Bloom. A significant portion of the material it is trying to
		/// help people license is restricted to the country of origin. Niether CC nor anyone else is going to allow for that, so we're
		/// allowing people to express that restriction in this field, but the UI also makes it clear that these are
		/// not legally enforceable if they are choosing a CC license. While not legally enforcable, they are not worthless, as they define
		/// what is ethical. We expect that the vast majority of people are going to abide by them.
		/// </summary>
		public string RightsStatement {get; set; }
	}

	public class NullLicense : LicenseInfo
	{
		public override string GetDescription(string iso639_3LanguageCode)
		{
			return "For permission to reuse, contact the copyright holder.";
		}

		public override string Token
		{
			//do not think of changing this, there is data out there that could get messed up
			get { return "ask"; }
		}

		public override string ToString()
		{
			return "";
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
			return "";
		}

		public override string Token
		{
			//do not think of changing this, there is data out there that could get messed up
			get { return "custom"; }
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