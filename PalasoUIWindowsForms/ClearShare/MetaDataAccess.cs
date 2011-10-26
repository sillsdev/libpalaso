using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.Code;
using Palaso.CommandLineProcessing;
using Palaso.Extensions;
using Palaso.IO;
using Palaso.Progress.LogBox;
using Palaso.UI.WindowsForms.ImageToolbox;

namespace Palaso.UI.WindowsForms.ClearShare
{
	public class MetaDataAccess
	{
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
				}
			}
			return m;
		}

		///<summary>
		/// 0 or more licenses offered by the copyright holder
		///</summary>
		public LicenseInfo License { get; set; }

		public string CopyrightNotice  { get; set; }

		/// <summary>
		/// Use this for artist, photographer, company, whatever.  It is mapped to XMP-Creative Commons--AttributionName, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionName { get; set; }

		/// <summary>
		/// Use this for the site to link to in attribution.  It is mapped to XMP-Creative Commons--AttributionUrl, but may be used even if you don't have a creative commons license
		/// </summary>
		public string AttributionUrl { get; set; }


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
			var lines = result.StandardOutput.SplitTrimmed('\r');
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

				assignments.Add(new MetaDataAssignement("-author", "author", (p, value) => p.AttributionName = value, p => p.AttributionName));
				assignments.Add(new MetaDataAssignement("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));

				assignments.Add(new MetaDataAssignement("-XMP-cc:License", "license",
													   (p, value) =>
													   p.License=CreativeCommonsLicense.FromUrl(value),
													   p => p.License.Url, p => p.License !=null));
				return assignments;
			}
		}

		private string _path;

		public void Write()
		{

			var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
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

			arguments.AppendFormat(" \"{0}\"", _path);
			var result = CommandLineRunner.Run(exifPath, arguments.ToString(), Path.GetDirectoryName(_path), 5, new NullProgress());
			// -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");
#if DEBUG
			Debug.WriteLine("writing");
			Debug.WriteLine(arguments.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif

		}
	}
}
