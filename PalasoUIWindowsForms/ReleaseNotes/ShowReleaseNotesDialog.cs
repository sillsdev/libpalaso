using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Palaso.IO;

namespace Palaso.UI.WindowsForms.ReleaseNotes
{
	/// <summary>
	/// Shows a dialog for release notes; accepts html and markdown
	/// </summary>
	public partial class ShowReleaseNotesDialog : Form
	{
		private readonly string _path;
		private TempFile _temp;

		public ShowReleaseNotesDialog(System.Drawing.Icon icon,string path)
		{
			_path = path;
			Icon = icon;
			InitializeComponent();
			_browser.Disposed += new EventHandler(_browser_Disposed);
		}

		void _browser_Disposed(object sender, EventArgs e)
		{
			try
			{
				_temp.Dispose();
			}
			catch (Exception error)
			{
				Debug.Fail(error.Message);
			}
		}

		private void ShowReleaseNotesDialog_Load(object sender, EventArgs e)
		{
			string contents = File.ReadAllText(_path);

			var md = new Markdown();
			_temp = TempFile.WithExtension("htm"); //enhance: will leek a file to temp
			File.WriteAllText(_temp.Path, md.Transform(contents));
			_browser.Navigate(_temp.Path);
		}
	}
}
