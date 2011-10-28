using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Palaso.CommandLineProcessing;
using Palaso.Extensions;
using Palaso.IO;
using Palaso.Progress.LogBox;

namespace Palaso.UI.WindowsForms.ClearShare
{
	/// <summary>
	/// Provides reading and writing of metdata, currently for any file which exiftool can read AND write (images, pdf).
	/// ExifTool can read many more formats that it can write (video, html, docx), but I have not tested those yet  (should be easy).
	/// ExifTool can also read/write sidecar files, but that is not yet implemented here, either (should be easy).
	/// Where multiple metadata formats are in a file (XMP, EXIF, IPTC-IIM), exif provides conformance to the MedatData
	/// Working Group guidelines: http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf, which we use by telling exiftool to
	/// to "-use MWG": http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html.  E.g., this puts the "Copyright" into both the exif "copyright", and "xmp:Rights".
	///
	/// Microsoft Pro Photo Tools: http://www.microsoft.com/download/en/details.aspx?id=13518
	/// </summary>
	public class MetaDataAccess
	{
		public MetaDataAccess()
		{
			AllowEditingMetadata = true;
		}

		/// <summary>
		/// Create a MetaDataAccess by reading an existing media file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static MetaDataAccess FromFile(string path)
		{
			var m = new MetaDataAccess();
			m._path = path;

			var properties = MetaDataAccess.GetImageProperites(path);

			foreach (var assignment in MetaDataAccess.MetaDataAssignments)
			{
				string propertyValue;
				if (properties.TryGetValue(assignment.ResultLabel.ToLower(), out propertyValue))
				{
					assignment.AssignmentAction.Invoke(m, propertyValue);
					m.AllowEditingMetadata = false;

				}
			}
			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			m.HasChanges = false;
			return m;
		}

		private LicenseInfo _license;

		///<summary>
		/// 0 or more licenses offered by the copyright holder
		///</summary>
		public LicenseInfo License
		{
			get { return _license; }
			set
			{
				if (value != _license)
					HasChanges = true;
				_license = value;
			}
		}

		private string _copyrightNotice;
		public string CopyrightNotice
		{
			get { return _copyrightNotice; }
			set
			{
				if (value.Trim().Length == 0)
					value = null;
				if (value != _copyrightNotice)
					HasChanges = true;
				_copyrightNotice = value;
			}
		}

		private string _creator;

		/// <summary>
		/// Use this for artist, photographer, company, whatever.  It is mapped to EXIF:Author and XMP:AttributionName
		/// </summary>
		public string Creator
		{
			get { return _creator; }
			set
			{
				if (value.Trim().Length == 0)
					value = null;
				if (value != _creator)
					HasChanges = true;
				_creator = value;
			}
		}

		private string _attributionUrl;

		/// <summary>
		/// Use this for the site to link to in attribution.  It is mapped to XMP-Creative Commons--AttributionUrl, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionUrl
		{
			get { return _attributionUrl; }
			set
			{
				if (value.Trim().Length == 0)
					value = null;
				if (value != _attributionUrl)
					HasChanges = true;
				_attributionUrl = value;
			}
		}


		private static Dictionary<string, string> GetImageProperites(string path)
		{
			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			var args = new StringBuilder();
			foreach (var assignment in MetaDataAssignments)
			{
				args.Append(" " + assignment.Switch + " ");
			}
			var result = CommandLineRunner.Run(exifPath, String.Format("{0} \"{1}\"", args.ToString(), path), Path.GetDirectoryName(path), 5, new NullProgress());
#if DEBUG
			Debug.WriteLine("reading");
			Debug.WriteLine(args.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
			var lines = result.StandardOutput.SplitTrimmed('\n');
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


		private class MetaDataAssignement
		{
			public Func<MetaDataAccess, string> GetStringFunction { get; set; }
			public Func<MetaDataAccess, bool> ShouldSetValue { get; set; }
			public string Switch;
			public string ResultLabel;
			public Action<MetaDataAccess, string> AssignmentAction;

			public MetaDataAssignement(string Switch, string resultLabel, Action<MetaDataAccess, string> assignmentAction, Func<MetaDataAccess, string> stringProvider)
				: this(Switch, resultLabel, assignmentAction, stringProvider, p => !String.IsNullOrEmpty(stringProvider(p)))
			{
			}

			public MetaDataAssignement(string @switch, string resultLabel, Action<MetaDataAccess, string> assignmentAction, Func<MetaDataAccess, string> stringProvider, Func<MetaDataAccess, bool> shouldSetValueFunction)
			{
				GetStringFunction = stringProvider;
				ShouldSetValue = shouldSetValueFunction;
				Switch = @switch;
				ResultLabel = resultLabel;
				AssignmentAction = assignmentAction;
			}
		}

		private static List<MetaDataAssignement> MetaDataAssignments
		{
			get
			{
				var assignments = new List<MetaDataAssignement>();
				assignments.Add(new MetaDataAssignement("-copyright", "copyright", (p, value) => p.CopyrightNotice = value, p => p.CopyrightNotice));

				assignments.Add(new MetaDataAssignement("-Author", "Author", (p, value) => p.Creator = value, p => p.Creator));
				assignments.Add(new MetaDataAssignement("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));

				assignments.Add(new MetaDataAssignement("-XMP-cc:License", "license",
													   (p, value) =>
													   p.License=CreativeCommonsLicense.FromUrl(value),
													   p => p.License.Url, p => p.License !=null));
				return assignments;
			}
		}

		public bool AllowEditingMetadata { get; private set; }

		public bool HasChanges { get; private set; }

		private string _path;

		public void Write()
		{

			var exifToolPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			//-E   -overwrite_original_in_place -d %Y
			StringBuilder arguments = new StringBuilder();

			foreach (var assignment in MetaDataAccess.MetaDataAssignments)
			{
				if (assignment.ShouldSetValue(this))
				{
					arguments.AppendFormat(" " + assignment.Switch + "=\"" + assignment.GetStringFunction(this) + "\" ");
				}
			}

			if (arguments.ToString().Length == 0)
			{
				//no metadata
				return;
			}

			//NB: when it comes time to having multiple contibutors, see Hatton's question on http://u88.n24.queensu.ca/exiftool/forum/index.php/topic,3680.0.html.  We need -sep ";" or whatever to ensure we get a list.

			arguments.AppendFormat(" -use MWG ");  //see http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html  and http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf
			arguments.AppendFormat(" \"{0}\"", _path);
			var result = CommandLineRunner.Run(exifToolPath, arguments.ToString(), Path.GetDirectoryName(_path), 5, new NullProgress());
			// -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");
#if DEBUG
			Debug.WriteLine("writing");
			Debug.WriteLine(arguments.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
			//as of right now, we are clean with respect to what is on disk, no need to save.
			HasChanges = false;
		}
	}
}
