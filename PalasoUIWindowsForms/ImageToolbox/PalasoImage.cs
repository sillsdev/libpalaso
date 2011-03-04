using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Palaso.Code;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	public class PalasoImage
	{
		public PalasoImage()
		{
			Licenses = new List<LicenseInfo>();
		}

		public PalasoImage(Image image)
		{
			Image = image;
			FileName = null;
			Licenses = new List<LicenseInfo>();
		}

		public Image Image { get; private set; }

		/// <summary>
		/// Really, just the name, not the path.  Use if you want to save the image somewhere.
		/// Will be null if the PalasoImage was created via an Image instead of a file.
		/// </summary>
		public string FileName { get; private set; }

		///<summary>
		/// 0 or more licenses offered by the copyright holder
		///</summary>
		public List<LicenseInfo> Licenses { get; set; }

		public string CopyrightNotice { get; set; }

		public string IllustratorPhotographer { get; set;}

		/// <summary>
		/// Should the user be able to make changes?
		/// </summary>
		public bool Locked
		{
			get; set;
		}

		/// <summary>
		/// Save an image to the given path, with the metadata embeded
		/// </summary>
		/// <param name="path"></param>
		public void Save(string path)
		{
		}

		public static PalasoImage FromFile(string path)
		{
			return new PalasoImage()
					   {
						   Image = LoadImageWithoutLocking(path),
						   FileName = Path.GetFileName(path)
			};
		}


		private static Image LoadImageWithoutLocking(string path)
		{
			//locks until the image is dispose of some day, which is counter-intuitive to me
			//  return Image.FromFile(path);

			//following work-around from http://support.microsoft.com/kb/309482
			using (var fs = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return Image.FromStream(fs);
			}
		}
		public static PalasoImage FromImage(Image image)
		{
			Guard.AgainstNull(image, "image");
			return new PalasoImage()
					   {
						   Image = image
					   };
		}
	}

	public abstract class LicenseInfo
	{
		public string GetDescription(string iso639_3LanguageCode)
		{
			//if we don't have it, just return English ("en")
			return "to do";
		}

		public void SetDescription(string iso639_3LanguageCode, string description)
		{
		}

		public abstract Image GetImage();

		/// <summary>
		/// It doesn't make sense to let the user edit the description of a well-known license, even if the meta data is unlocked.
		/// </summary>
		public abstract bool EditableWhenNotLocked{ get;}
	}

	public class CreativeCommonsLicense : LicenseInfo
	{
		//we'll need to give out an image, description, url.
		//what you *store* in the image metadata is a different question.
		public override Image GetImage()
		{
			throw new NotImplementedException();
		}

		public override bool EditableWhenNotLocked
		{
			get { return false; }
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

		public override bool EditableWhenNotLocked
		{
			get { return true; }
		}
	}
}