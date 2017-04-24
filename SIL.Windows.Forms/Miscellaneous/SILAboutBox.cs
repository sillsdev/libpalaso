// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using L10NSharp;
using SIL.Acknowledgements;
using SIL.IO;

namespace SIL.Windows.Forms.Miscellaneous
{
	public partial class SILAboutBox : Form
	{
		private readonly Assembly _assembly;
		private readonly string _pathToAboutBoxHtml;
		private TempFile _tempAboutBoxHtmlFile; // after update by AcknowledgementsProvider

		public event EventHandler CheckForUpdatesClicked;
		public event EventHandler ReleaseNotesClicked;

		/// <summary>
		///
		/// </summary>
		/// <param name="pathToAboutBoxHtml">For example, use FileLocator.GetFileDistributedWithApplication("distfiles", "aboutBox.htm")</param>
		/// <param name="useFullVersionNumber"><c>false</c> to only display the first three
		/// parts of the version number, i.e. "MajorVersion.MinorVersion.Build",
		/// <c>true</c> to display the full version number as found in Application.ProductVersion.
		/// Passing <c>true</c> is useful if you want to display e.g. the git or hg revision of
		/// the build. Typically this would be set in the AssemblyInformationalVersion.</param>
		public SILAboutBox(string pathToAboutBoxHtml, bool useFullVersionNumber = false)
		{
			_assembly = Assembly.GetEntryAssembly(); // assembly;
			_pathToAboutBoxHtml = pathToAboutBoxHtml;
			InitializeComponent();
			_versionNumber.Text = useFullVersionNumber ? Application.ProductVersion :
				GetShortVersionInfo();
			_buildDate.Text = GetBuiltOnDate();
			Text = GetWindowTitle();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (CheckForUpdatesClicked == null)
				_checkForUpdates.Visible = false;
			else
				_checkForUpdates.Click += (sender, args) => CheckForUpdatesClicked(this, args);

			if (ReleaseNotesClicked == null)
				_releaseNotesLabel.Visible = false;
			else
				_releaseNotesLabel.Click += (sender, args) => ReleaseNotesClicked(this, args);
		}

		public void NotifyNoUpdatesAvailable()
		{
			_checkForUpdates.Text = LocalizationManager.GetString("AboutDialog.NoUpdates", "No Updates");
			_checkForUpdates.Enabled = false;
		}

		#region Assembly Attribute Accessors

		public string AssemblyTitle
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					var titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}

		public AcknowledgementAttribute[] AssemblyAcknowledgements
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AcknowledgementAttribute), false);
				if (attributes.Length == 0)
				{
					return new AcknowledgementAttribute[0];
				}
				return attributes.Cast<AcknowledgementAttribute>().ToArray();

			}
		}
		#endregion

		private string GetWindowTitle()
		{
			// The window title was originally "About " with the application title appended.
			// There was also the L10N id "AboutDialog.AboutDialogWindowTitle" which wasn't actually being displayed but
			// which was based on the English "About" (and probably was translated in many apps).
			// I considered trying to repurpose this id, but realized its presence in existing files would
			// prevent translators from seeing that it needed to have the {0} added.
			// Therefore, I've simply used a new id which corrects the string.
			return string.Format(LocalizationManager.GetString("AboutDialog.WindowTitle", "About {0}", "{0} is the application name"), GetApplicationTitle());
		}

		private string GetBuiltOnDate()
		{
			var file = FileUtils.StripFilePrefix(_assembly.CodeBase);
			var fi = new FileInfo(file);

			// Use UTC for calculation of build-on-date so that we get the same date regardless
			// of timezone setting.
			return string.Format(LocalizationManager.GetString("AboutDialog.BuiltOnDate", "Built on {0}", "{0} is the date the application was built"), fi.CreationTimeUtc.ToString("dd-MMM-yyyy"));
		}

		private string GetShortVersionInfo()
		{
			var ver = _assembly.GetName().Version;
			return string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
		}

		private string GetCopyright()
		{
			foreach (object attribute in _assembly.GetCustomAttributes(false))
			{
				if (attribute.GetType() == typeof(System.Reflection.AssemblyCopyrightAttribute))
				{
					return ((AssemblyCopyrightAttribute) attribute).Copyright;
				}
			}
			return string.Empty;
		}

		private string GetApplicationTitle()
		{
			foreach (object attribute in _assembly.GetCustomAttributes(false))
			{
				if (attribute.GetType() == typeof(System.Reflection.AssemblyTitleAttribute))
				{
					return ((AssemblyTitleAttribute) attribute).Title;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Put this string in your project's AboutBox.html file. The SILAboutBox will replace it with all the
		/// dependencies it can collect from your project's dependencies' AssemblyInfo.cs files.
		/// Each dependency will be embedded in a <li></li> element, so normally you will want <ul></ul> around
		/// this DependencyMarker. The AcknowledgementsProvider doesn't put out the ul elements, since there may be
		/// other dependencies to be included besides those this tool can find.
		/// </summary>
		public const string DependencyMarker = "#DependencyAcknowledgements#";

		private void SILAboutBoxShown(object sender, EventArgs e)
		{
			//review: EB had changed from Navigate to this in ab66af23393f74b767ffd78c2182bd1fdc8eb963, presumably to
			// get around the AllowNavigation=false problem. It may work on Linux, but it didn't on Windows, which would just show a blank browser.
			//_browser.Url = new Uri(_pathToAboutBoxHtml);
			// So I've instead modified the browser wrapper to always let the first navigation get through, regardless
			var filePath = AcknowledgementsProvider.GetFullNonUriFileName(_pathToAboutBoxHtml);
			var aboutBoxHtml = File.ReadAllText(filePath);
			aboutBoxHtml = ValidateHtml(aboutBoxHtml);
			if (!aboutBoxHtml.Contains(DependencyMarker))
			{
				_browser.Navigate(_pathToAboutBoxHtml);
			}
			else
			{
				var insertableAcknowledgements = AcknowledgementsProvider.AssembleAcknowledgements();
				var newHtmlContents = aboutBoxHtml.Replace(DependencyMarker, insertableAcknowledgements);
				// Create a temporary file with the DependencyMarker replaced with our collected Acknowledgements.
				// This file will be deleted OnClosed.
				_tempAboutBoxHtmlFile = new TempFile(newHtmlContents);
				_browser.Navigate(_tempAboutBoxHtmlFile.Path);
			}
		}

		internal const string MinimalHtmlContents = "<html><head><meta charset='UTF-8' /></head><body></body><ul>" +
								DependencyMarker + "</ul></html>";

		/// <summary>
		/// Make sure the About Box html is valid by loading it into an XmlDocument.
		/// Also ensure that it uses Unicode, since even common things like copyright symbols
		/// won't be rendered correctly otherwise.
		/// Internal for testing
		/// </summary>
		internal static string ValidateHtml(string htmlString)
		{
			XmlDocument dom;
			XmlNode headNode;
			try
			{
				dom = new XmlDocument {InnerXml = htmlString};
				var charsetNode = dom.SelectSingleNode("//head/meta[@charset]");
				if (charsetNode != null)
				{
					return htmlString; // don't overwrite any existing charset attribute
				}
				headNode = dom.SelectSingleNode("//head");
				if (headNode == null)
				{
					var bodyNode = dom.SelectSingleNode("//body");
					if (bodyNode == null)
					{
						// What!? Someone created an AboutBox html file with no body element?!
						// Create a minimal file
						return MinimalHtmlContents;
					}
					headNode = dom.CreateElement("head");
					bodyNode.ParentNode.InsertBefore(headNode, bodyNode);
				}
				var metaNode = dom.CreateElement("meta");
				var charsetAttr = dom.CreateAttribute("charset");
				charsetAttr.Value = "UTF-8";
				metaNode.SetAttributeNode(charsetAttr);
				headNode.AppendChild(metaNode);
				return dom.InnerXml;
			}
			catch (XmlException ex)
			{
				Debug.WriteLine("Loading the AboutBox html threw an exception: " + ex.Message);
				return MinimalHtmlContents;
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			// Clean up our temporary file
			try
			{
				if (_tempAboutBoxHtmlFile != null) // shouldn't happen, but might as well be careful.
				{
					File.Delete(_tempAboutBoxHtmlFile.Path);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Temporary file deletion failed. " + _tempAboutBoxHtmlFile.Path + ex.Message);
			}
			finally
			{
				base.OnClosed(e);
			}
		}
	}
}
