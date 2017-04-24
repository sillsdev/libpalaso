// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;

namespace SIL.Acknowledgements
{
	/// <summary>
	/// If you add a "using SIL.Acknowledgements;" to your project's AssemblyInfo.cs file and
	/// add an AcknowledgementAttribute for each dependency, then the AcknowledgementsProvider
	/// will be able to collect your project's dependencies and display them in your project's SILAboutBox.
	/// You just need to add the string #DependencyAcknowledgements# (probably surrounded by a <ul></ul> element)
	/// to your project's about box html file and the AcknowledgementsProvider will replace it
	/// with the collected Acknowledgements each set in a <li></li> element.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AcknowledgementAttribute : Attribute
	{
		private string _html;
		private string _copyright;
		private string _name;

		/// <summary>
		/// Example Usage:
		/// [assembly: Acknowledgement("Ionic.Zip.dll", copyright: "Dino Chiesa", url: "http://www.codeplex.com/DotNetZip",
		/// licenseUrl: "https://opensource.org/licenses/MS-PL", location: "./Ionic.Zip.dll",
		/// html: "<li><a href='http://www.codeplex.com/DotNetZip'>Ionic.Zip</a> (MS-PL) by Dino Chiesa - a library for handling zip archives (Flavor=Retail)</li>")]
		/// The above example provides a custom html. Leaving it out would generate the default html value.
		/// See the Html property comment for the default value for this example.
		/// </summary>
		/// <param name="key">required</param>
		/// <param name="name">optional</param>
		/// <param name="url">optional</param>
		/// <param name="licenseUrl">optional</param>
		/// <param name="copyright">optional</param>
		/// <param name="location">optional</param>
		/// <param name="html">optional</param>
		public AcknowledgementAttribute(string key, string name = "", string url = "",
			string licenseUrl = "", string copyright = "", string location = "", string html = "")
		{
			Key = key;
			Name = name;
			Url = url;
			LicenseUrl = licenseUrl;
			Copyright = copyright;
			Location = location;
			Html = html;
		}

		/// <summary>
		/// Acknowledgements will be sorted by Name. This string will show up in the default Html,
		/// if no custom Html is set. This is an optional field.
		/// If this property is set, it will override the automatic value produced by examining the dll itself.
		/// </summary>
		public string Name {
			get
			{
				if (!string.IsNullOrEmpty(_name) || string.IsNullOrEmpty(Location))
					return _name;
				// Try to get it from the dll/exe file
				var versionInfo = ExtractExecutableVersionInfo();
				return versionInfo == null ? _name : versionInfo.ProductName;
			}
			private set { _name = value; }
		}

		/// <summary>
		/// Key should be something that will be unique and stable (as much as possible),
		/// but not version-based, so we can eliminate duplicates. This is a required field.
		///
		/// For now we are just using the Name of the Reference as listed in Visual Studio. In the .csproj file,
		/// this can be found in the Include attribute of the Reference element up until the first comma.
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// An optional (but highly recommended!) Url that does one of the following:
		/// - points to the source repo,
		/// - points to a nuget package,
		/// - points to the main project website.
		/// If Url is provided and Html is not set, the default Html will create a link to this url
		/// with the Name property as its label.
		/// Thus, the primary goal is to be a URL that is a good starting point for someone who is
		/// interested in learning more about this dependency
		/// </summary>
		public string Url { get; private set; }

		/// <summary>
		/// An optional (but highly recommended!) Url that identifies the License under which we are using
		/// the dependency (e.g. "https://opensource.org/licenses/MIT").
		/// </summary>
		public string LicenseUrl { get; private set; }

		/// <summary>
		/// The name of the person or organization that holds the copyright to the code.
		/// This is an optional field.
		/// If this property is set, it will override the automatic value produced by examining the dll itself.
		/// </summary>
		public string Copyright
		{
			get
			{
				if (!string.IsNullOrEmpty(_copyright) || string.IsNullOrEmpty(Location))
					return _copyright;
				// Try to get it from the dll/exe file
				var versionInfo = ExtractExecutableVersionInfo();
				return versionInfo == null ? _copyright : versionInfo.LegalCopyright;
			}
			private set { _copyright = value; }
		}

		/// <summary>
		/// If we can't find the file using the Location, return null.
		/// Otherwise returns the located executable file's FileVersionInfo.
		/// </summary>
		private FileVersionInfo ExtractExecutableVersionInfo()
		{
			try
			{
				return FileVersionInfo.GetVersionInfo(Location.Split(',')[0]);
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		/// The location gives the expected place in a client’s source tree where the dependency files would be found.
		/// This is useful for making Linux copyright files. It can be a list (comma-separated).
		/// Items may be relative to the client solution (e.g., something dependent on GeckoFx would specify
		/// "packages/{filenames}", since that’s where Nuget will always put those files)
		/// or relative to the DLL that has the acknowledgement attributes (e.g., SIL.Media.dll would specify
		/// "./IrrKlang.dll, ./ikpFlac.dll, ./ikpMP3.dll", since we expect these DLLs to be downloaded somehow
		/// to the same place in the project source tree as SIL.Media.dll itself).
		/// If Name and/or Copyright are not specified, the code will use this Location field to find suitable substitutes
		/// in the FileVersionInfo.
		/// This is an optional field.
		/// </summary>
		public string Location { get; private set; }

		/// <summary>
		/// If no custom Html is provided, this will generate a default <li></li> entry filled in with the information
		/// provided in the acknowledgement.
		///
		/// Example default Html based on the example acknowledgement in the ctor comment is:
		/// "<li><a href='http://www.codeplex.com/DotNetZip'>DotNetZip Library</a>: Dino Chiesa <a href='https://opensource.org/licenses/MS-PL'></a></li>"
		///
		/// Provide your own Html if you desire, for example, additional comments on a dependency to show up in a
		/// consumer's SILAboutBox. Since each Acknowledgement's Html Property creates a <li></li> element, usually
		/// your project's AboutBox html file would contain the SILAboutBox.DependencyMarker surrounded
		/// by a <ul></ul> element.
		/// </summary>
		public string Html
		{
			get
			{
				if (!string.IsNullOrEmpty(_html))
					return _html;

				// Create a default html
				_html = "<li>";
				if (!string.IsNullOrEmpty(Name))
				{
					// If we have a Url, create a link, otherwise just use Name
					_html += !string.IsNullOrEmpty(Url)
						? "<a href='" + Url + "'>" + Name + "</a>"
						: Name;
				}
				if (!string.IsNullOrEmpty(Copyright))
				{
					_html += ": " + Copyright;
				}
				if (!string.IsNullOrEmpty(LicenseUrl))
				{
					_html += " <a href='" + LicenseUrl + "'>" + LicenseUrl + "</a>";
				}
				_html += "</li>";
				return _html ?? string.Empty;
			}
			private set { _html = value; }
		}
	}
}
