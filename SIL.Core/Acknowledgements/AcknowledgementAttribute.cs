using System;

namespace SIL.Acknowledgements
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AcknowledgementAttribute : Attribute
	{
		private string _html;

		/// <summary>
		/// Add an Acknowledgement attribute (aa) to your AssemblyInfo.cs file for each dependency.
		/// Then the AcknowledgementsProvider will be able to collect your project's dependencies and display them
		/// automatically in your SIL AboutBox.
		/// Example:
		/// [assembly: Acknowledgement("Ionic.Zip.dll", "Ionic.Zip", aaCopyright: "Dino Chiesa",
		/// aaLicense: "https://opensource.org/licenses/MS-PL", aaLocation: "./Ionic.Zip.dll",
		/// aaHtml: "<li><a href=\"http://www.codeplex.com/DotNetZip\">Ionic.Zip</a> (MS-PL) by Dino Chiesa - a library for handling zip archives (Flavor=Retail)</li>")]
		/// 
		/// You will also need to add a "using SIL.Acknowledgements;" to your AssemblyInfo file.
		/// </summary>
		/// <param name="aaKey"></param>
		/// <param name="aaName"></param>
		/// <param name="aaLicense"></param>
		/// <param name="aaCopyright"></param>
		/// <param name="aaLocation"></param>
		/// <param name="aaHtml"></param>
		public AcknowledgementAttribute(string aaKey, string aaName,
			string aaLicense = "", string aaCopyright = "", string aaLocation = "", string aaHtml = "")
		{
			Key = aaKey;
			Name = aaName;
			License = aaLicense;
			Copyright = aaCopyright;
			Location = aaLocation;
			Html = aaHtml;
		}

		/// <summary>
		/// Acknowledgements will be sorted by Name. This string will only be displayed if
		/// no Html is provided (i.e. it is used in the default Html).
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Key should be something that will be unique and stable (as much as possible),
		/// but not version-based, so we can eliminate duplicates.
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// A Url that identifies the License under which we are using the dependency.
		/// </summary>
		public string License { get; private set; }
		/// <summary>
		/// The name of the person or organization that holds the copyright to the code.
		/// </summary>
		public string Copyright { get; private set; }
		/// <summary>
		/// The location gives the expected place in a client’s source tree where the dependency files would be found.
		/// This is useful for making Linux copyright files. It can be a list (comma-separated).
		/// </summary>
		public string Location { get; private set; }

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
					_html += " (" + License + ")";
				}
				_html += "</li>";
				return _html ?? string.Empty;
			}
			private set { _html = value; }
		}
	}
}
