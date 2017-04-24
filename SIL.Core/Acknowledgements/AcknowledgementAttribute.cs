// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace SIL.Acknowledgements
{
	/// <summary>
	/// If you add a "using SIL.Acknowledgements;" to your project's AssemblyInfo.cs file and
	/// add an AcknowledgementAttribute for each dependency, then the AcknowledgementsProvider
	/// will be able to collect your project's dependencies and display them automatically in your project's SILAboutBox.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AcknowledgementAttribute : Attribute
	{
		private string _html;

		/// <summary>
		/// Example Usage:
		/// [assembly: Acknowledgement("Ionic.Zip.dll", "Ionic.Zip", copyright: "Dino Chiesa",
		/// license: "https://opensource.org/licenses/MS-PL", location: "./Ionic.Zip.dll",
		/// html: "<li><a href=\"http://www.codeplex.com/DotNetZip\">Ionic.Zip</a> (MS-PL) by Dino Chiesa - a library for handling zip archives (Flavor=Retail)</li>")]
		/// The above example provides a custom html. Leaving it out would generate the default html value.
		/// See the Html property comment for the default value for this example.
		/// </summary>
		/// <param name="key">required</param>
		/// <param name="name">required</param>
		/// <param name="license">optional</param>
		/// <param name="copyright">optional</param>
		/// <param name="location">optional</param>
		/// <param name="html">optional</param>
		public AcknowledgementAttribute(string key, string name,
			string license = "", string copyright = "", string location = "", string html = "")
		{
			Key = key;
			Name = name;
			License = license;
			Copyright = copyright;
			Location = location;
			Html = html;
		}

		/// <summary>
		/// Acknowledgements will be sorted by Name. This string will show up in the default Html,
		/// if no custom Html is set. This is a required field.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Key should be something that will be unique and stable (as much as possible),
		/// but not version-based, so we can eliminate duplicates. This is a required field.
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// An optional (but highly recommended!) Url that identifies the License under which we are using the dependency.
		/// </summary>
		public string License { get; }

		/// <summary>
		/// The name of the person or organization that holds the copyright to the code.
		/// This is an optional field.
		/// </summary>
		public string Copyright { get; }

		/// <summary>
		/// The location gives the expected place in a client’s source tree where the dependency files would be found.
		/// This is useful for making Linux copyright files. It can be a list (comma-separated).
		/// (e.g. ./IrrKlang.dll, ./ikpFlac.dll, ./ikpMP3.dll)
		/// This is an optional field.
		/// </summary>
		public string Location { get; }

		/// <summary>
		/// If no custom Html is provided, this will generate a default <li></li> entry filled in with the information
		/// provided in the acknowledgement.
		/// 
		/// Example default Html based on the example acknowledgement in the ctor comment is:
		/// "<li>Ionic.Zip: Dino Chiesa <a href='https://opensource.org/licenses/MS-PL'></a></li>"
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
					_html += Name;
				}
				if (!string.IsNullOrEmpty(Copyright))
				{
					_html += ": " + Copyright;
				}
				if (!string.IsNullOrEmpty(License))
				{
					_html += " <a href='" + License + "'>" + License + "</a>";
				}
				_html += "</li>";
				return _html ?? string.Empty;
			}
			set { _html = value; }
		}
	}
}
