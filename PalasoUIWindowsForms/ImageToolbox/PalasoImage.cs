using System;
using System.Collections.Generic;
using System.Diagnostics;
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
				Guard.Against(MetaDataLocked, "You must first explicitly unlock the metadata of this image before setting an metatdata values.");
				_copyrightNotice = value;
			}
		}

		/// <summary>
		/// Use this for artist, photographer, company, whatever.  It is mapped to XMP-Creative Commons--AttributionName, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionName { get; set; }

		/// <summary>
		/// Use this for the site to link to in attribution.  It is mapped to XMP-Creative Commons--AttributionUrl, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionUrl { get; set; }

		/// <summary>
		/// Should the user be able to make changes to MetaData?
		/// </summary>
		public bool MetaDataLocked
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

			foreach (var assignment in MetaDataAssignments)
			{
				if(assignment.ShouldSetValue(this))
				{
					arguments.AppendFormat(" " + assignment.Switch + "=\"" + assignment.GetStringFunction(this) + "\" ");
				}
			}

			if (arguments.ToString().Length == 0)
			{
				//no metadata
				return;
			}

			arguments.AppendFormat(" \"{0}\"", path);
			var result = CommandLineRunner.Run(exifPath,arguments.ToString(), Path.GetDirectoryName(path), 5, new NullProgress());
																												 // -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");
#if DEBUG
			Debug.WriteLine("writing");
			Debug.WriteLine(arguments.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
		}



		public static PalasoImage FromFile(string path)
		{
			var i = new PalasoImage()
					   {
						   Image = LoadImageWithoutLocking(path),
						   FileName = Path.GetFileName(path)
			};
			var properties = GetImageProperites(path);


			foreach (var assignment in MetaDataAssignments)
			{
				string propertyValue;
				if (properties.TryGetValue(assignment.ResultLabel.ToLower(), out propertyValue))
				{
					assignment.AssignmentAction.Invoke(i,propertyValue);
					i.MetaDataLocked = true;
				}
			}
			return i;
		}


		private static Dictionary<string,string> GetImageProperites(string path)
		{
			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			var args = new StringBuilder();
			foreach (var assignment in MetaDataAssignments)
			{
				args.Append(" "+assignment.Switch+" ");
			}
			var result =CommandLineRunner.Run(exifPath, string.Format("{0} \"{1}\"", args.ToString(), path), Path.GetDirectoryName(path), 5, new NullProgress());
#if DEBUG
			Debug.WriteLine("reading");
			Debug.WriteLine(args.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
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

		private class MetaDataAssignemtn
		{
			public Func<PalasoImage, string> GetStringFunction { get; set; }
			public Func<PalasoImage, bool> ShouldSetValue { get; set; }
			public string Switch;
			public string ResultLabel;
			public Action<PalasoImage, string> AssignmentAction;

			public MetaDataAssignemtn(string Switch, string resultLabel, Action<PalasoImage, string> assignmentAction, Func<PalasoImage, string> stringProvider)
				: this(Switch, resultLabel, assignmentAction,stringProvider, p => !string.IsNullOrEmpty(stringProvider(p)))
			{
			}

			public MetaDataAssignemtn(string @switch, string resultLabel, Action<PalasoImage, string> assignmentAction, Func<PalasoImage, string> stringProvider, Func<PalasoImage, bool> shouldSetValueFunction)
			{
				GetStringFunction = stringProvider;
				ShouldSetValue = shouldSetValueFunction;
				Switch = @switch;
				ResultLabel = resultLabel;
				AssignmentAction = assignmentAction;
			}
		}

		private static List<MetaDataAssignemtn> MetaDataAssignments
		{
			get
			{
				var assignments = new List<MetaDataAssignemtn>();
				assignments.Add(new MetaDataAssignemtn("-copyright", "copyright", (p, value) => p.CopyrightNotice = value, p => p.CopyrightNotice));

				assignments.Add(new MetaDataAssignemtn("-author", "author", (p, value) => p.AttributionName = value, p => p.AttributionName));
				assignments.Add(new MetaDataAssignemtn("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));

				assignments.Add(new MetaDataAssignemtn("-XMP-cc:License", "license",
													   (p, value) =>
													   p.Licenses.Add(
														   CreativeCommonsLicense.FromUrl(
															   value)),
												   p=>p.Licenses[0].Url, p => p.Licenses.Count > 0));
				return assignments;
			}
		}

	}


}