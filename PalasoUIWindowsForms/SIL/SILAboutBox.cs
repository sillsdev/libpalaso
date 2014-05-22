using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SIL
{
	public partial class SILAboutBox : Form
	{
		private readonly Assembly _assembly;
		private readonly string _pathToAboutBoxHtml;

		/// <summary>
		///
		/// </summary>
		/// <param name="pathToAboutBoxHtml">For example, use FileLocator.GetFileDistributedWithApplication("distfiles", "aboutBox.htm")</param>
		public SILAboutBox(string pathToAboutBoxHtml)
		{
			_assembly = Assembly.GetEntryAssembly(); // assembly;
			_pathToAboutBoxHtml = pathToAboutBoxHtml;
			InitializeComponent();
			_versionNumber.Text = GetShortVersionInfo();
			_buildDate.Text = GetBuiltOnDate();
			Text = "About " + GetTitle();
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
			var file = _assembly.CodeBase.Replace("file://", string.Empty);
			if (PlatformUtilities.Platform.IsWindows)
				file = file.TrimStart('/');
			var fi = new FileInfo(file);

			return string.Format("Built on {0}", fi.CreationTime.ToString("dd-MMM-yyyy"));
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
			_browser.Navigate(_pathToAboutBoxHtml);
			_browser.Navigated += _browser_Navigated;
		}


		private void _browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			_browser.Refresh();
			_browser.ScrollLastElementIntoView();
		}
	}
}
