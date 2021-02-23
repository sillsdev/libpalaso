using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Markdig;
using SIL.IO;

namespace SIL.Windows.Forms.ReleaseNotes
{
	/// <summary>
	/// Shows a dialog for release notes; accepts html and markdown
	/// </summary>
	/// <remarks>
	/// Despite the name, this dialog is generally useful for displaying
	/// formatted information, not just for release notes.
	/// </remarks>
	public partial class ShowReleaseNotesDialog : Form
	{
		private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

		private readonly string _path;
		private TempFile _temp;
		private readonly Icon _icon;

		public bool ApplyMarkdown { get; set;}

		public ShowReleaseNotesDialog(Icon icon, string path, bool applyMarkdown = true)
		{
			_path = path;
			_icon = icon;
			ApplyMarkdown = applyMarkdown;
			InitializeComponent();
		}

		private void ShowReleaseNotesDialog_Load(object sender, EventArgs e)
		{
			string contents = File.ReadAllText(_path);
			// Disposed of during dialog Dispose()
			_temp = TempFile.WithExtension("htm");
			if (ApplyMarkdown)
			{
				File.WriteAllText(_temp.Path, GetBasicHtmlFromMarkdown(Markdown.ToHtml(contents, pipeline)));
			}
			else if (contents.Contains("<html>") && contents.Contains("<body"))
			{
				// apparently full fledged HTML already, so just copy the input file
				File.WriteAllText(_temp.Path, contents);
			}
			else
			{
				// likely markdown output from another process, so add outer html elements
				File.WriteAllText(_temp.Path, GetBasicHtmlFromMarkdown(contents));
			}
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
