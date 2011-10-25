using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Palaso.Code;
using Palaso.CommandLineProcessing;
using Palaso.IO;
using Palaso.Progress.LogBox;
using Palaso.Extensions;

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

		//public bool HaveLoadedMetadata { get; set; }
		private string _copyrightNotice;
		public string CopyrightNotice
		{
			get { return _copyrightNotice; }
			set
			{
				Guard.Against(Locked, "You must first explicitly unlock the metadata of this image before setting an metatdata values.");
				_copyrightNotice = value;
			}
		}

		private string _illustratorPhotographer;
		public string IllustratorPhotographer
		{
			get { return _illustratorPhotographer; }
			set
			{
				Guard.Against(Locked, "You must first explicitly unlock the metadata of this image before setting an metatdata values.");
				_illustratorPhotographer = value;
			}
		}

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
		   Image.Save(path);

			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			//-E   -overwrite_original_in_place -d %Y
			StringBuilder arguments = new StringBuilder();
			if(!string.IsNullOrEmpty(CopyrightNotice))
			{
				arguments.AppendFormat(" -copyright=\"{0}\"", CopyrightNotice);
				arguments.AppendFormat(" -XMP-xmpRights:Marked=\"True\" --XMP-dc:Rights=\"{0}\"", CopyrightNotice);
			}
			if (!string.IsNullOrEmpty(IllustratorPhotographer))
				arguments.AppendFormat(" -author=\"{0}\"", IllustratorPhotographer);

			if(Licenses.Count>0 && !string.IsNullOrEmpty(Licenses[0].Url))
			{
				arguments.AppendFormat(" -XMP-cc:License=\"{0}\"", Licenses[0].Url);
				arguments.AppendFormat(" -XMP-xmpRights:Marked=\"True\"");
			}
			arguments.AppendFormat(" \"{0}\"", path);
			CommandLineRunner.Run(exifPath,arguments.ToString(), Path.GetDirectoryName(path), 5, new NullProgress());
																												 // -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");
		}

		public static PalasoImage FromFile(string path)
		{
			var i = new PalasoImage()
					   {
						   Image = LoadImageWithoutLocking(path),
						   FileName = Path.GetFileName(path)
			};
			string s = null;
			var properties = GetImageProperites(path);
			if (properties.TryGetValue("copyright", out s))
			{
				i.CopyrightNotice = s;
			}
			if(properties.TryGetValue("author", out s))
			{
				i.IllustratorPhotographer = s;
			}

			s = null;
			if(properties.TryGetValue("license", out s))
			{
				i.Licenses.Add(CreativeCommonsLicense.FromUrl(s));
			}


			//if this came in with meta data, we don't want the UI to encourage editing it.
			i.Locked = properties.Count > 0;
			return i;
		}

		private static Dictionary<string,string> GetImageProperites(string path)
		{
			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			var result =CommandLineRunner.Run(exifPath, string.Format("-XMP-cc:License -author -copyright \"{0}\"", path), Path.GetDirectoryName(path), 5, new NullProgress());
			var lines= result.StandardOutput.SplitTrimmed('\r');
			var values = new Dictionary<string, string>();
			foreach (var line in lines)
			{
				var parts = line.SplitTrimmed(':');
				if (parts.Count < 2)
					continue;

				//recombine any parts of the value which had a colon (like a url does)
				string value = parts[1];
				for (int i = 2; i < parts.Count; ++i)
					value = value + ":" + parts[i];

				values.Add(parts[0].ToLower(), value);
			}
			return values;
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


}