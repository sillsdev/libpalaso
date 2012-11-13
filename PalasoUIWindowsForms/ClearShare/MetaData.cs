using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
	public class Metadata
	{
		public Metadata()
		{
			IsEmpty = true;
		}

		/// <summary>
		/// Create a MetadataAccess by reading an existing media file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Metadata FromFile(string path)
		{
			var m = new Metadata();
			m._path = path;

			LoadProperties(path, m);
			return m;
		}

		/// <summary>
		/// NB: this is used in 2 places; one is loading from the image we are linked to, the other from a sample image we are copying metadata from
		/// </summary>
		/// <param name="path"></param>
		/// <param name="m"></param>
		private static void LoadProperties(string path, Metadata m)
		{
			var properties = GetImageProperites(path);

			foreach (var assignment in MetadataAssignments)
			{
				string propertyValue;
				if (properties.TryGetValue(assignment.ResultLabel.ToLower(), out propertyValue))
				{
					assignment.AssignmentAction.Invoke(m, propertyValue);
					m.IsEmpty = false;
				}
			}
			m.License = LicenseInfo.FromXmp(properties);

			//NB: we're loosing non-ascii somewhere... the copyright symbol is just the most obvious
			if (!string.IsNullOrEmpty(m.CopyrightNotice))
			{
				m.CopyrightNotice = m.CopyrightNotice.Replace("Copyright ?", "Copyright ©");
			}

			//clear out the change-setting we just caused, because as of right now, we are clean with respect to what is on disk, no need to save.
			m.HasChanges = false;
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
				if (!(value is NullLicense))
					IsEmpty = false;
			}
		}

		private string _copyrightNotice;
		public string CopyrightNotice
		{
			get { return _copyrightNotice; }
			set
			{
				if (value!=null && value.Trim().Length == 0)
					value = null;
				if (value != _copyrightNotice)
					HasChanges = true;
				_copyrightNotice = FixArtOfReadingCopyrightProblem(value);
				if (!String.IsNullOrEmpty(_copyrightNotice))
					IsEmpty = false;

			}
		}

		/// <summary>
		/// Somehow we shipped art of reading with a copyright which read:
		/// "Copyright, SIL International 2009. This work is licensed under a Creative Commons Attribution-NoDeriv" and so forth.
		/// This trims that off.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FixArtOfReadingCopyrightProblem(string value)
		{
			if(string.IsNullOrEmpty(value))
				return string.Empty;
			var startOfProblem = value.IndexOf("This work");
			if (startOfProblem == -1)
				return value;
			return value.Substring(0, startOfProblem);
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
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _creator)
					HasChanges = true;
				_creator = value;
				if (!String.IsNullOrEmpty(_creator))
					IsEmpty = false;
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
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _attributionUrl)
					HasChanges = true;
				_attributionUrl = value;
				if (!String.IsNullOrEmpty(_attributionUrl))
					IsEmpty = false;
			}
		}


		private string _collectionName;

		/// <summary>
		/// E.g. "Art of Reading"
		/// </summary>
		public string CollectionName
		{
			get { return _collectionName; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionName)
					HasChanges = true;
				_collectionName = value;
				if (!String.IsNullOrEmpty(_collectionName))
					IsEmpty = false;

			}
		}

		private string _collectionUri;
		public string CollectionUri
		{
			get { return _collectionUri; }
			set
			{
				if (value != null && value.Trim().Length == 0)
					value = null;
				if (value != _collectionUri)
					HasChanges = true;
				_collectionUri = value;
				if (!String.IsNullOrEmpty(_collectionUri))
					IsEmpty = false;

			}
		}

		/// <summary>
		/// Used during UI editing to know if we should enable the OK button
		/// </summary>
		public bool IsMinimallyComplete
		{
			get
			{
				//I'm thinking, license is secondary. Primary is who did it or what the copyright is... if you have a license without any of that... it's kind of bogus.
				return !String.IsNullOrEmpty(CopyrightNotice) || !String.IsNullOrEmpty(Creator) ||
					   !String.IsNullOrEmpty(CollectionUri);
			}
		}


		private static Dictionary<string, string> GetImageProperites(string path)
		{
			path = CommandLineRunner.MakePathToFileSafeFromEncodingProblems(path);

			var values = new Dictionary<string, string>();
			try
			{
				var exifPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
				var args = new StringBuilder();
				args.Append("-charset cp65001 "); //utf-8
				foreach (var assignment in MetadataAssignments)
				{
					args.Append(" " + assignment.Switch + " ");
				}
				var directory = Path.GetDirectoryName(path);//NB: this doesn't go out to commandline, so there are no unicode worries
				var result = CommandLineRunner.Run(exifPath, String.Format("{0} \"{1}\"", args.ToString(), path),
												   _commandLineEncoding, directory, 20 /*had a possiblefailure at 5: BL-242*/,
												   new NullProgress());
				if(result.DidTimeOut)
				{
					Palaso.Reporting.ErrorReport.ReportNonFatalExceptionWithMessage(new Exception(), "The program which reads metadata (e.g. copyright) from the image: " +
																	 path+" ran out of time.");
					return values;
				}
				result.RaiseExceptionIfFailed("GetImageProperites({0})", path);

#if DEBUG
				Debug.WriteLine("reading");
				Debug.WriteLine(args.ToString());
				Debug.WriteLine(result.StandardError);
				Debug.WriteLine(result.StandardOutput);
#endif
				var lines = result.StandardOutput.SplitTrimmed('\n');

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
			}
			catch (Exception error)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(error,
																 "The program had trouble checking the metadata in the image: " +
																 path);
			}
			return values;
		}

		private class MetadataAssignement
		{
			public Func<Metadata, string> GetStringFunction { get; set; }
			public Func<Metadata, bool> ShouldSetValue { get; set; }
			public string Switch;
			public string ResultLabel;
			public Action<Metadata, string> AssignmentAction;

			public MetadataAssignement(string Switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider)
				: this(Switch, resultLabel, assignmentAction, stringProvider, p => !String.IsNullOrEmpty(stringProvider(p)))
			{
			}

			public MetadataAssignement(string @switch, string resultLabel, Action<Metadata, string> assignmentAction, Func<Metadata, string> stringProvider, Func<Metadata, bool> shouldSetValueFunction)
			{
				GetStringFunction = stringProvider;
				ShouldSetValue = shouldSetValueFunction;
				Switch = @switch;
				ResultLabel = resultLabel;
				AssignmentAction = assignmentAction;
			}
		}

		private static List<MetadataAssignement> MetadataAssignments
		{
			get
			{
				var assignments = new List<MetadataAssignement>();
				assignments.Add(new MetadataAssignement("-copyright", "copyright", (p, value) => p.CopyrightNotice = value, p => p.CopyrightNotice));

				assignments.Add(new MetadataAssignement("-Author", "Author", (p, value) => p.Creator = value, p => p.Creator));
				assignments.Add(new MetadataAssignement("-XMP:CollectionURI", "Collection URI", (p, value) => p.CollectionUri = value, p => p.CollectionUri));
				assignments.Add(new MetadataAssignement("-XMP:CollectionName", "Collection Name", (p, value) => p.CollectionName = value, p => p.CollectionName));
				assignments.Add(new MetadataAssignement("-XMP-cc:AttributionURL", "Attribution URL", (p, value) => p.AttributionUrl = value, p => p.AttributionUrl));
				assignments.Add(new MetadataAssignement("-XMP-cc:License", "license",
													   (p, value) => { },//p.License=LicenseInfo.FromUrl(value), //we need to use for all the properties to set up the license
													   p => p.License.Url, p => p.License !=null));
				//NB: CC also has a custom one, for adding rights beyond the normal. THat's not what this is (at least right now). This is for custom licenses.
				assignments.Add(new MetadataAssignement("-XMP-dc:Rights-en", "Rights (en)",
													   (p, value) => { },//p.License=LicenseInfo.FromUrl(value), //we need to use for all the properties to set up the license
													   p => p.License.RightsStatement, p => p.License != null));

				return assignments;
			}
		}

		public bool IsEmpty { get; private set; }

		private bool _hasChanges;
		public bool HasChanges
		{
			get
			{
				return _hasChanges || (License!=null && License.HasChanges);
			}
			set
			{
				_hasChanges = value;
				if(!value && License!=null)
					License.HasChanges = false;
			}
		}

		/// <summary>
		/// Attempt to cut off notices like "Copyright Blah 2009. Blah blah blah"
		/// </summary>
		public string ShortCopyrightNotice
		{
			get {
				var i = CopyrightNotice.IndexOf('.');
				if(i<0)
					return CopyrightNotice;
				return CopyrightNotice.Substring(0, i);
			}
		}

		private string _path;
		private static Encoding _commandLineEncoding = Encoding.UTF8;

		public void Write()
		{
			Write(_path);
		}

		public void Write(string path)
		{
			var exifToolPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			//-E   -overwrite_original_in_place -d %Y
			StringBuilder arguments = new StringBuilder();

			//No arguments.Append("-P "); //don't change the modified date  (this isn't totally obvious... it's good unless it interferes with backup)

			AddAssignmentArguments(arguments);

			if (arguments.ToString().Length == 0)
			{
				//no metadata
				return;
			}

			//NB: when it comes time to having multiple contibutors, see Hatton's question on http://u88.n24.queensu.ca/exiftool/forum/index.php/topic,3680.0.html.  We need -sep ";" or whatever to ensure we get a list.

			//doesn't work: arguments.Append("-charset cp65001 ");//utf-8
			arguments.Append(" -E ");//in AddAssignmentArguments we make the values use html encoded values
			arguments.Append(" -overwrite_original_in_place "); //we do this becuase we're giving it an 8.3 version of the name (useful when there's non-ascii in the filepath), and without this, exiftool gets messed up when trying to finish up via renaming. This makes it do a copy instead, which works even with the 8.3 names.
			arguments.AppendFormat("-use MWG ");  //see http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html  and http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf
			arguments.AppendFormat(" \"{0}\"", CommandLineRunner.MakePathToFileSafeFromEncodingProblems(path));
			var directory = Path.GetDirectoryName(path);//NB: this doesn't go out to commandline, so there are no unicode worries
			var result = CommandLineRunner.Run(exifToolPath, arguments.ToString(), _commandLineEncoding, directory, 5, new NullProgress());
			// -XMP-dc:Rights="Copyright SIL International" -XMP-xmpRights:Marked="True" -XMP-cc:License="http://creativecommons.org/licenses/by-sa/2.0/" *.png");

			result.RaiseExceptionIfFailed("Write({0}",path);

			//-overwrite_original didn't work for this
			var extra = path + "_original";
			try
			{
				if (File.Exists(extra))
					File.Delete(extra);
			}
			catch (Exception)
			{
				//not worth reporting
			}

#if DEBUG
			Debug.WriteLine("writing");
			Debug.WriteLine(arguments.ToString());
			Debug.WriteLine(result.StandardError);
			Debug.WriteLine(result.StandardOutput);
#endif
			//as of right now, we are clean with respect to what is on disk, no need to save.
			HasChanges = false;
		}

		private void AddAssignmentArguments(StringBuilder arguments)
		{
			foreach (var assignment in MetadataAssignments)
			{
				if (assignment.ShouldSetValue(this))
				{
					arguments.AppendFormat(" " + assignment.Switch + "=\"" + CommandLineRunner.HtmlEncodeNonAsciiCharacters(assignment.GetStringFunction(this)) + "\" ");
				}
			}
		}

		public void SetupReasonableLicenseDefaultBeforeEditing()
		{
			if(License == null || License is NullLicense)
			{
				License= new CreativeCommonsLicense(true,true,CreativeCommonsLicense.DerivativeRules.Derivatives);
			}
		}

		public Metadata DeepCopy()
		{
			return (Metadata) CloneObject(this);
		}

		/// <summary>
		/// This is actually a generic cloning method, copied into this class just because UI needs to clone this class.
		/// From http://stackoverflow.com/questions/78536/cloning-objects-in-c-sharp/78551#78551
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static object CloneObject(object source)
		{
			//grab the type and create a new instance of that type
			Type sourceType = source.GetType();
			//  var target =  Activator.CreateInstance(source.GetType(), null);
			object target = Activator.CreateInstance(sourceType, true);

			//grab the properties
			PropertyInfo[] PropertyInfo =
				sourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			//iterate over the properties and if it has a 'set' method assign it from the source TO the target
			foreach (PropertyInfo item in PropertyInfo)
			{
				if (item.CanWrite)
				{
					//value types can simply be 'set'
					if (item.PropertyType.IsValueType || item.PropertyType.IsEnum ||
						item.PropertyType.Equals(typeof(System.String)))
					{
						item.SetValue(target, item.GetValue(source, null), null);
					}
						//object/complex types need to recursively call this method until the end of the tree is reached
					else
					{
						object propertyValue = item.GetValue(source, null);
						if (propertyValue == null)
						{
							item.SetValue(target, null, null);
						}
						else
						{
							Type z = item.PropertyType;
							item.SetValue(target, CloneObject(propertyValue), null);
							//item.SetValue(target, CloneObject<LicenseInfo>(propertyValue as LicenseInfo), null);
						}
					}
				}
			}
			//return the new item
			return target;
		}

		/// <summary>
		/// Saves all the metadata that fits in XMP to a file.
		/// </summary>
		/// <example>SaveXmplFile("c:\dir\metadata.xmp")</example>
		public void SaveXmpFile(string path)
		{
			Debug.Assert(path.EndsWith(".xmp"), "No really, the file must end in .xmp or exiftool won't work.");
			if(File.Exists(path))
				File.Delete(path);

			//NO! this will actually create a file with the 8.3 name, which isn't what we want: path = CommandLineRunner.MakePathToFileSafeFromEncodingProblems(path);
			//In fact I have so far been unable to allow non-ascii characters in the filename.
			//We can, do however, get non-ascii in the path (e.g. the user's account name). The difference is that
			//the director already exists, wheras the file doesn't. I've tried pre-creating it, but exiftool then can't
			//handle overwriting it (no, the various -overwrite_ flags don't work for writing xmp files).

			StringBuilder arguments = new StringBuilder();
			//doesn't work: arguments.Append("-charset cp65001 ");//utf-8
			arguments.Append(" -E ");//in AddAssignmentArguments we make the values use html encoded values


			//we send it in wihtout the whole directory, because that might have non-ascii characters
			arguments.AppendFormat("-o \"{0}\"", Path.GetFileName(path));
			AddAssignmentArguments(arguments);

			//arguments.AppendFormat(" -use MWG ");  //see http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/MWG.html  and http://www.metadataworkinggroup.org/pdf/mwg_guidance.pdf

			var exifToolPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");
			var directory = Path.GetDirectoryName(path);//NB: this doesn't go out to commandline, so there are no unicode worries
			var result = CommandLineRunner.Run(exifToolPath, arguments.ToString(), _commandLineEncoding, directory, 5, new NullProgress());

			result.RaiseExceptionIfFailed("SaveXmpFile({0})", path);
		}


		/// <summary>
		/// Loads all metadata found in the XMP file.
		/// </summary>
		/// <example>LoadXmplFile("c:\dir\metadata.xmp")</example>
		public void LoadXmpFile(string path)
		{
			if(!File.Exists(path))
				throw new FileNotFoundException(path);

			path = CommandLineRunner.MakePathToFileSafeFromEncodingProblems(path);

			var exifToolPath = FileLocator.GetFileDistributedWithApplication("exiftool.exe");


			//OK, so exiftool doesn't actually let us just read an xmp file. It needs an image to push the values into.
			//So we oblige by creating a temp image, pushing the values in, then reading out the values. Wheeww.
			using(var temp = TempFile.WithExtension("png"))
			{
				File.Delete(temp.Path);
				using (var tempImage = new Bitmap(1, 1))
				{
					tempImage.Save(temp.Path);
				}
				StringBuilder arguments = new StringBuilder();
				arguments.Append("-charset cp65001 ");//utf-8
				arguments.Append(" -overwrite_original_in_place "); //we do this becuase we're giving it an 8.3 version of the name (useful when there's non-ascii in the filepath), and without this, exiftool gets messed up when trying to finish up via renaming. This makes it do a copy instead, which works even with the 8.3 names.
				arguments.AppendFormat(" -all -tagsfromfile \"{0}\" -all:all \"{1}\"", CommandLineRunner.MakePathToFileSafeFromEncodingProblems(path), CommandLineRunner.MakePathToFileSafeFromEncodingProblems(temp.Path));
				var directory =Path.GetDirectoryName(path);//NB: this doesn't go out to commandline, so there are no unicode worries
				var result = CommandLineRunner.Run(exifToolPath, arguments.ToString(), _commandLineEncoding, directory, 5, new NullProgress());
				result.RaiseExceptionIfFailed("LoadXmpFile({0})",path);
				LoadProperties(temp.Path, this);
			}

		}

		/// <summary>
		/// Save the current metadata in the user settings, so that in the future, a call to LoadFromStoredExamplar() will retrieve them.
		/// This is used to quickly populate metadata with the values used in the past (e.g. many images will have the same illustruator, license, etc.)
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		public void StoreAsExemplar(FileCategory category)
		{
			SaveXmpFile(GetExemplarPath(category));
		}

		/// <summary>
		/// Get previously saved values from a file in the user setting.
		/// This is used to quickly populate metadata with the values used in the past (e.g. many images will have the same illustruator, license, etc.)
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		public void LoadFromStoredExemplar(FileCategory category)
		{
			LoadXmpFile(GetExemplarPath(category));
			HasChanges = true;
		}

		/// <summary>
		/// Tell if there is previously saved values from a file in the user setting.
		/// </summary>
		/// <param name="category">e.g. "image", "document"</param>
		static public bool HaveStoredExemplar(FileCategory category)
		{
			return File.Exists(GetExemplarPath(category));
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="ideal_iso639LanguageCode">e.g. "en" or "fr"</param>
		/// <returns></returns>
		public string GetSummaryParagraph(string ideal_iso639LanguageCode)
		{
			var b = new StringBuilder();
			b.AppendLine("Creator: " + Creator);
			b.AppendLine(CopyrightNotice);
			if(!string.IsNullOrEmpty(CollectionName))
				b.AppendLine(CollectionName);
			if (!string.IsNullOrEmpty(CollectionUri))
				b.AppendLine(CollectionUri);
			if (!string.IsNullOrEmpty(License.Url))
				b.AppendLine(License.Url);
			b.AppendLine(License.GetDescription(ideal_iso639LanguageCode));
			return b.ToString();
		}

		/// <summary>
		/// For use on a hyperlink/button
		/// </summary>
		/// <returns></returns>
		static public string GetStoredExemplarSummaryString(FileCategory category)
		{
			try
			{
				var m = new Metadata();
				m.LoadFromStoredExemplar(category);
				return string.Format("{0}/{1}/{2}", m.Creator, m.CopyrightNotice, m.License.ToString());
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private static string GetExemplarPath(FileCategory category)
		{
			String appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var path = appData.CombineForPath("palaso");
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = path.CombineForPath("rememberedMetadata-" + Enum.GetName(typeof(FileCategory), category) + ".xmp");
			return path;
		}

		/// <summary>
		/// used when storing/retrieving exemplar metadata
		/// </summary>
		public enum FileCategory
		{
			Audio,
			Image,
			Document
		};

		public void SetCopyrightNotice(string year, string by)
		{
			if(!string.IsNullOrEmpty(year))
				CopyrightNotice = "Copyright © " + year + ", " + by;
			else
				CopyrightNotice = "Copyright © " + by;
		}

		const string kCopyrightPattern = @"\D*(?<year>\d\d\d\d)?(,\s)?(?<by>.+)?";
		const string kNoYearPattern = @"([cC]opyright\s+)?(COPYRIGHT\s+)?\©?\s*(?<by>.+)";


		public string GetCopyrightYear()
		{
			if (string.IsNullOrEmpty(CopyrightNotice))
				return string.Empty;
			Match m = Regex.Match(CopyrightNotice, kCopyrightPattern);
			return m.Groups["year"].Value;
		}

		public string GetCopyrightBy()
		{
			if(string.IsNullOrEmpty(CopyrightNotice))
				return string.Empty;
			Match m = Regex.Match(CopyrightNotice, kCopyrightPattern);

			//I was never able to get the cases without years to work with the standard pattern (just not good enough with regex's).
			if (!m.Groups["year"].Success && !m.Groups["by"].Success)
			{
				m = Regex.Match(CopyrightNotice, kNoYearPattern);
			}

			return m.Groups["by"].Value.Trim();
		}
	}
}
