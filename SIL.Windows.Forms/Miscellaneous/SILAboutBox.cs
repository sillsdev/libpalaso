// Copyright (c) 2016-2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
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
		/// Creates a project AboutBox. There is now a DependencyMarker (see comments for usage) that can be added to
		/// a project's AboutBox html file that SILAboutBoxShown() will replace with a series of Acknowledgements gleaned
		/// from your project's dependencies. There is one caveat: using this marker will preclude the use of relative links
		/// for css styling or images in the html file. See some comments in that method for possible future ways around this
		/// limitation.
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
			var file = PathHelper.StripFilePrefix(_assembly.CodeBase);
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
			if (!aboutBoxHtml.Contains(DependencyMarker))
			{
				_browser.Navigate(_pathToAboutBoxHtml);
			}
			else
			{
				// Without a charset='UTF-8' meta tag attribute, things like copyright symbols don't show up correctly.
				Debug.Assert(aboutBoxHtml.Contains(" charset"), "At a minimum, the About Box html should contain a meta charset='UTF-8' tag.");
				var insertableAcknowledgements = AcknowledgementsProvider.AssembleAcknowledgements();
				var newHtmlContents = aboutBoxHtml.Replace(DependencyMarker, insertableAcknowledgements);
				// Create a temporary file with the DependencyMarker replaced with our collected Acknowledgements.
				// This file will be deleted OnClosed.
				// This means that if your project uses the DependencyMarker in your html file, you will not be able to
				// link to a file on a relative path for css styles or images.
				// ----------
				// Comments on possible ways around this limitation from John Thomson:
				//		1.Document that an About Box HTML file which uses dependency injection must live in its own folder
				// with all dependent files, and copy the whole folder to a temp folder.
				// (could work but is a nuisance, especially for anyone who doesn't need any dependencies)
				//		2.Document that an About Box HTML file which uses dependency injection may only use a few common kinds
				// of relative links, search for matching links, and copy the appropriate files to a temp directory along
				// with the temp file.
				// (I rather like this idea. A fairly simple regular expression will search for src or rel followed by a value
				// with no path separators...something like(src | rel) = (['"])([^/\]*)\1 (or something similar...
				// handle white space...). That will catch all references to images, stylesheets, and scripts,
				// and if the bit of the RegEx that matches the filename corresponds to an existing file in the same folder
				// as the HTML we can just copy it. Unless they're doing relative paths to different folders that will do it,
				// and I think it's reasonable to have SOME restrictions in the interests of simplicity.
				// ----------
				_tempAboutBoxHtmlFile = new TempFile(newHtmlContents);
				_browser.Navigate(_tempAboutBoxHtmlFile.Path);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			// Clean up our temporary file
			try
			{
				_tempAboutBoxHtmlFile?.Dispose(); // Dispose handles the actual file deletion and exception catching
			}
			finally
			{
				base.OnClosed(e);
			}
		}
	}
}
