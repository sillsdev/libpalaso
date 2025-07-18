// Copyright (c) 2016-2025, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using SIL.Acknowledgements;
using SIL.Core;
using SIL.IO;
using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.Miscellaneous
{
	public partial class SILAboutBox : Form
	{
		private readonly Assembly _assembly;
		private readonly string _pathToAboutBoxHtml;
		private TempFile _tempAboutBoxHtmlFile; // after update by AcknowledgementsProvider
		private WebBrowserNavigatingEventHandler _navigatingHandlers;

		/// <summary>
		/// Event that occurs when the user clicks the "Check for Updates" button in the About
		/// dialog box. If the hosting application has subscribed to this event, then that
		/// button will be visible in the UI. Applications should respond by checking whether an
		/// update is available.
		/// </summary>
		public event EventHandler CheckForUpdatesClicked;

		/// <summary>
		/// Event that occurs when the user clicks the "Release Notes" button in the About
		/// dialog box. If the hosting application has subscribed to this event, then that
		/// button will be visible in the UI. Applications should respond by displaying the
		/// release notes.
		/// </summary>
		public event EventHandler ReleaseNotesClicked;

		/// <summary>
		/// Occurs before browser control navigation occurs within the HTML browser control in the
		/// About dialog box. It allows navigation to be canceled by setting
		/// <see cref="WebBrowserNavigatingEventArgs.Cancel"/> to false.
		/// </summary>
		public event WebBrowserNavigatingEventHandler Navigating
		{
			add
			{
				_navigatingHandlers += value;
				_browser.Navigating += value;
			}
			remove
			{
				_navigatingHandlers -= value;
				_browser.Navigating -= value;
			}
		}

		/// <summary>
		/// Occurs when the HTML browser control in the About dialog box has navigated to a new
		/// document and has begun loading it.
		/// </summary>
		public event WebBrowserNavigatedEventHandler Navigated
		{
			add => _browser.Navigated += value;
			remove => _browser.Navigated -= value;
		}

		/// <summary>
		/// Flag indicating whether external links in the HTML presented in the About dialog box
		/// should open inside the browser control or in the default web browser. Previously, they
		/// would always open inside the browser control unless the HTML explicitly set the target
		/// attribute, but this is generally not desirable, especially if any of the links are to
		/// content with Javascript, so the default behavior is now to try to open them in the
		/// default browser as configured on the user's system.
		/// </summary>
		public bool AllowExternalLinksToOpenInsideAboutBox { get; set; } = false;

		/// <summary>
		/// Creates a project AboutBox. There is now a DependencyMarker (see comments for usage) that can be added to
		/// a project's AboutBox html file that SILAboutBoxShown() will replace with a series of Acknowledgements gleaned
		/// from your project's dependencies. There is one caveat: using this marker will preclude the use of relative links
		/// for css styling or images in the html file. See some comments in that method for possible future ways around this
		/// limitation.
		/// </summary>
		/// <param name="pathToAboutBoxHtml">For example, use
		/// <see cref="FileLocationUtilities.GetFileDistributedWithApplication(string[])"/>(
		/// "DistFiles", "AboutBox.htm"). This also can accept a file URI.</param>
		/// <param name="useFullVersionNumber"><c>false</c> to display only the first three
		/// parts of the version number, i.e. "MajorVersion.MinorVersion.Build",
		/// <c>true</c> to display the full version number as found in Application.ProductVersion.
		/// Passing <c>true</c> is useful if you want to display, for example, the git revision of
		/// the build. Typically, this would be set in the AssemblyInformationalVersion.</param>
		/// <param name="logoVariant">If specified, allows caller to indicate particular SIL logo
		/// variant to display. By default, a variant is chosen at random.</param>
		public SILAboutBox(string pathToAboutBoxHtml, bool useFullVersionNumber = false,
			SilLogoVariant logoVariant = SilLogoVariant.Random)
		{
			_assembly = Assembly.GetEntryAssembly(); // assembly;
			_pathToAboutBoxHtml =
				Uri.TryCreate(pathToAboutBoxHtml, UriKind.Absolute, out var uri) && uri.IsFile
					? uri.LocalPath
					: pathToAboutBoxHtml;
			InitializeComponent();
			_versionNumber.Text = useFullVersionNumber ? Application.ProductVersion :
				GetShortVersionInfo();
			_buildDate.Text = GetBuiltOnDate();
			Text = string.Format(Text, GetApplicationTitle());
			logo.Image = SilResources.GetLogo(logoVariant);
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

		[PublicAPI]
		public void NotifyNoUpdatesAvailable()
		{
			_checkForUpdates.Text = LocalizationManager.GetString("AboutDialog.NoUpdates", "No Updates");
			_checkForUpdates.Enabled = false;
		}

		#region Assembly Attribute Accessors
		[PublicAPI]
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
				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
			}
		}

		[PublicAPI]
		public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

		[PublicAPI]
		public string AssemblyDescription
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		[PublicAPI]
		public string AssemblyProduct
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		[PublicAPI]
		public string AssemblyCopyright
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		[PublicAPI]
		public string AssemblyCompany
		{
			get
			{
				var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				return attributes.Length == 0 ? "" : ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}

		[PublicAPI]
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

		private string GetBuiltOnDate()
		{
			var file = PathHelper.StripFilePrefix(_assembly.Location);
			var fi = new FileInfo(file);

			// Use UTC for calculation of build-on-date so that we get the same date regardless
			// of timezone setting.
			return string.Format(LocalizationManager.GetString("AboutDialog.BuiltOnDate",
					"Built on {0}", "{0} is the date the application was built"),
				fi.LastWriteTimeUtc.ToString("dd-MMM-yyyy"));
		}

		private string GetShortVersionInfo()
		{
			var ver = _assembly.GetName().Version;
			return $"{ver.Major}.{ver.Minor}.{ver.Build}";
		}

		private string GetApplicationTitle()
		{
			foreach (var attribute in _assembly.GetCustomAttributes(false))
			{
				if (attribute is AssemblyTitleAttribute titleAttribute)
					return titleAttribute.Title;
			}
			return string.Empty;
		}

		/// <summary>
		/// Put this string in your project's AboutBox.html file. The SILAboutBox will replace it
		/// with all the dependencies it can collect from your project's dependencies'
		/// AssemblyInfo.cs files. Each dependency will be embedded in a &lt;li&gt;&lt;/li&gt;
		/// element, so normally you will want &lt;ul&gt;&lt;/ul&gt; around this DependencyMarker.
		/// The AcknowledgementsProvider doesn't put out the ul elements, since there may be
		/// other dependencies to be included besides those this tool can find.
		/// </summary>
		public const string DependencyMarker = "#DependencyAcknowledgements#";

		private void SILAboutBoxShown(object sender, EventArgs e)
		{
			// REVIEW: EB had changed from Navigate to this in ab66af23393f74b767ffd78c2182bd1fdc8eb963,
			// presumably to get around the AllowNavigation=false problem. It may work on Linux, but it
			// didn't on Windows, which would just show a blank browser.
			// _browser.Url = new Uri(_pathToAboutBoxHtml);
			// So I've instead modified the browser wrapper to always let the first navigation get
			// through, regardless.
			var filePath = AcknowledgementsProvider.GetFullNonUriFileName(_pathToAboutBoxHtml);
			var aboutBoxHtml = File.ReadAllText(filePath);
			// Without a charset='UTF-8' meta tag attribute, things like copyright symbols don't show up correctly.
			Debug.Assert(aboutBoxHtml.Contains(" charset"), "At a minimum, the About Box html should contain a meta charset='UTF-8' tag.");

			string newHtmlContents = null;
			
			if (aboutBoxHtml.Contains(DependencyMarker))
			{
				var insertableAcknowledgements = AcknowledgementsProvider.AssembleAcknowledgements();
				newHtmlContents = aboutBoxHtml.Replace(DependencyMarker, insertableAcknowledgements);
			}

			if (!AllowExternalLinksToOpenInsideAboutBox || _navigatingHandlers != null)
			{
				var fixedContents = HtmlUtils.HandleMissingLinkTargets(
					newHtmlContents ?? aboutBoxHtml,
					"About box", Environment.NewLine + "If you need to suppress this debug-only " +
					"warning and allow all external links to open inside the About box, use " +
					nameof(AllowExternalLinksToOpenInsideAboutBox) + ".  Alternatively, you " +
					$"could handle the {nameof(Navigating)} event to customize the navigation " +
					"behavior in your application.");
				if (fixedContents != null)
					newHtmlContents = fixedContents;
			}

			if (newHtmlContents == null) // No changes made to original HTML
				_browser.Navigate(_pathToAboutBoxHtml);
			else
			{
				_tempAboutBoxHtmlFile = HtmlUtils.CreatePatchedTempHtmlFile(newHtmlContents, _pathToAboutBoxHtml);
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
