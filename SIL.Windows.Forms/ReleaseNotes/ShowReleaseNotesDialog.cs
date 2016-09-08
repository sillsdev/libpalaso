using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MarkdownDeep;
using SIL.IO;

namespace SIL.Windows.Forms.ReleaseNotes
{
	/// <summary>
	/// Shows a dialog for release notes; accepts html and markdown
	/// </summary>
	public partial class ShowReleaseNotesDialog : Form
	{
		private readonly string _path;
		private TempFile _temp;
		private readonly Icon _icon;

		public ShowReleaseNotesDialog(Icon icon, string path)
		{
			_path = path;
			_icon = icon;
			InitializeComponent();
		}

		private void ShowReleaseNotesDialog_Load(object sender, EventArgs e)
		{
			string contents = File.ReadAllText(_path);

			var md = new Markdown();
			// Disposed of during dialog Dispose()
			_temp = TempFile.WithExtension("htm");
			File.WriteAllText(_temp.Path, GetBasicHtmlFromMarkdown(md.Transform(contents)));
			_browser.Url = new Uri(_temp.Path);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			// a bug in Mono requires us to wait to set Icon until handle created.
			Icon = _icon;
		}

		private string GetBasicHtmlFromMarkdown(string markdownHtml)
		{
			return string.Format("<html><head><meta charset=\"utf-8\"/></head><body>{0}</body></html>", markdownHtml);
		}
	}
}
