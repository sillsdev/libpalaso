using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using SIL.IO;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.HtmlBrowser;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Any links web links will open in the default bowser.
	/// For file:/// links: The path will be extracted and handed to the OS to open,
	/// unless there is a class "showFileLocation", in which case we attempt to open a file
	/// explorer and select the file.
	/// </summary>
	/// <remarks>Links currently only work when Geckofx is used in the application.</remarks>
	public partial class HtmlLabel : UserControl
	{
		private readonly XWebBrowser _browser;
		private string _html;

		public HtmlLabel()
		{
			InitializeComponent();

			if (this.DesignModeAtAll())
			{
				return;
			}

			LoadGeckoAssembly();
			_browser = new XWebBrowser();

			_browser.Parent = this;
			_browser.Dock = DockStyle.Fill;
			Controls.Add(_browser);
			_browser.IsWebBrowserContextMenuEnabled = false;
			_browser.Margin = new Padding(0);
		}

		private static Assembly GeckoCoreAssembly { get; set; }

		/// <summary>
		/// Just a simple html string, no html, head, body tags.
		/// </summary>
		[Browsable(true), CategoryAttribute("Text")]
		public string HTML
		{
			get { return _html; }
			set
			{
				_html = value;
				if (this.DesignModeAtAll())
					return;

				if (_browser != null)
				{
					_browser.Visible = !string.IsNullOrEmpty(_html);
					var htmlColor = ColorTranslator.ToHtml(ForeColor);
					var backgroundColor = ColorTranslator.ToHtml(BackColor);
					if (_browser.Visible)
					{
						var s = "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"></head><body style=\"background-color: " +
								backgroundColor +
								"\"><span style=\"color:" + htmlColor + "; font-family:Segoe UI, Arial; font-size:" + Font.Size +
								"pt\">" + _html + "</span></body></html>";
						_browser.DocumentText = s;
					}
				}
			}
		}

		private void HtmlLabel_Load(object sender, EventArgs e)
		{
			if (this.DesignModeAtAll())
				return;

			HTML = _html;//in the likely case that there's html waiting to be shown
			_browser.DomClick += OnBrowser_DomClick;
		}

		/// <summary>
		/// Attempt to load GeckoAssemblies from the programs running environment. Try and load modern gecko dlls which have
		/// no version number in the filenames then fallback to trying to load geckofx 14.
		/// </summary>
		private static bool LoadGeckoAssembly()
		{
			if (GeckoCoreAssembly != null)
				return true;
			try
			{
				try
				{
					GeckoCoreAssembly = Assembly.Load("Geckofx-Core");
				}
				catch(FileNotFoundException)
				{
					//Fallback to geckofx version 14 name
					GeckoCoreAssembly = Assembly.LoadFrom("geckofx-core-14.dll");
				}
			}
			catch(Exception)
			{
				MessageBox.Show("Unable to load geckofx dependancy. Files may not have been included in the build.",
					"Failed to load geckofx", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			return true;
		}

		private void OnBrowser_DomClick(object sender, EventArgs ge)
		{
			if (this.DesignModeAtAll() || GeckoCoreAssembly == null)
				return;

			var domEventArgsType = GeckoCoreAssembly.GetType("Gecko.DomEventArgs");
			var targetProp = domEventArgsType.GetProperty("Target");
			var targetRaw = targetProp.GetValue(ge, new object[0]);
			var handledProp = domEventArgsType.GetProperty("Handled", typeof(bool));

			var domEventTargetType = GeckoCoreAssembly.GetType("Gecko.DOM.DomEventTarget");
			var castToElementMethod = domEventTargetType.GetMethod("CastToGeckoElement");
			var target = castToElementMethod.Invoke(targetRaw, new object[0]);

			var elementType = GeckoCoreAssembly.GetType("Gecko.GeckoHtmlElement");
			var getAttributeMethod = elementType.GetMethod("GetAttribute", new[] { typeof(string) });
			var tagNameProp = elementType.GetProperty("TagName");

			if (target == null)
				return;

			var tagName = tagNameProp.GetValue(target, null) as string;
			if (tagName == "A")
			{
				var url = getAttributeMethod.Invoke(target, new[] { "href" }) as string;
				if (url.StartsWith("file://", StringComparison.Ordinal))
				{
					var path = url.Replace("file://", "");

					var classAttr = getAttributeMethod.Invoke(target, new[] {"class"}) as string;
					if (classAttr != null && classAttr.Contains("showFileLocation"))
					{
						PathUtilities.SelectFileInExplorer(path);
					}
					else
					{
						SIL.Program.Process.SafeStart(path);
					}
				}
				else
				{
					SIL.Program.Process.SafeStart(url);
				}
				handledProp.SetValue(ge, true, null); //don't let the browser navigate itself
			}
		}
	}
}