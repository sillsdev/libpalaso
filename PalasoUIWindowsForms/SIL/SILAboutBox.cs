using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using L10NSharp;
using Palaso.IO;

namespace Palaso.UI.WindowsForms.SIL
{
	public partial class SILAboutBox : Form
	{
		private readonly Assembly _assembly;
		private readonly string _pathToAboutBoxHtml;

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
			Text = "About " + GetTitle();
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
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
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
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion


		private string GetBuiltOnDate()
		{
			var file = FileUtils.StripFilePrefix(_assembly.CodeBase);
			var fi = new FileInfo(file);

			// Use UTC for calculation of build-on-date so that we get the same date regardless
			// of timezone setting.
			return string.Format("Built on {0}", fi.CreationTimeUtc.ToString("dd-MMM-yyyy"));
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

		private string GetTitle()
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

		private void SILAboutBoxShown(object sender, EventArgs e)
		{
			//review: EB had changed from Navigate to this in ab66af23393f74b767ffd78c2182bd1fdc8eb963, presumably to 
			// get around the AllowNavigation=false problem. It may work on Linux, but it didn't on Windows, which would just show a blank browser.
			//_browser.Url = new Uri(_pathToAboutBoxHtml);
			// So I've instead modified the browser wrapper to always let the first navigation get through, regardless
			_browser.Navigate(_pathToAboutBoxHtml);
		}
	}
}
