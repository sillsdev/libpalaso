using L10NSharp;
using SIL.Archiving;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI;
using SIL.IO;
using SIL.Windows.Forms.Archiving;
using SIL.Windows.Forms.Archiving.IMDI;
using static System.IO.Path;
using static System.String;

namespace ArchivingTestApp
{
	public partial class MainForm : Form
	{
		private const string kAppName = "Archiving Test App";

		private string GetTitle()
		{
			var title = m_txtTitle.Text;
			if (title.Length == 0)
				title = "Arbitrary title";
			return title;
		}

		public MainForm()
		{
			InitializeComponent();
		}

		private void m_btnRamp_Click(object sender, EventArgs e)
		{
			var title = GetTitle();
			var model = new RampArchivingDlgViewModel(kAppName, title,
				title.ToLatinOnly("~", "_", ""), SetFilesToArchive, GetFileDescription);
			using (var rampArchiveDlg = new ArchivingDlg(model, LocalizationManager.GetString(
				"ArchivingTestApp.MainForm.AdditionalArchiveProcessInfo", "This is just a test.")))
			{
				rampArchiveDlg.ShowDialog(this);
			}
		}

		private void m_btnIMDI_Click(object sender, EventArgs e)
		{
			var title = GetTitle();
			TempFile.NamePrefix = kAppName.Replace(" ", "_");
			var folder = new TempFile().Path;
			RobustFile.Delete(folder);
			Directory.CreateDirectory(folder);
			var model = new IMDIArchivingDlgViewModel(kAppName, title,
				title.ToLatinOnly("~", "_", ""), false, SetFilesToArchive, folder);

			model.ArchivingPackage.AccessCode = "internal";
			model.ArchivingPackage.Access.DateAvailable = "3 March 2029";
			model.ArchivingPackage.Access.Owner = "Fred";
			model.ArchivingPackage.Publisher = "SIL Elbonia";
			model.ArchivingPackage.Location = new ArchivingLocation
			{
				Address = "1234 Shoulder St.; Armstrong",
				Country = "Elbonia"
			};
			model.ArchivingPackage.Author = "Test Dude";
			model.ArchivingPackage.Owner = "Mike";

			foreach (ListViewGroup group in m_listFiles.Groups)
			{
				var session = model.AddSession(group.Header);
				foreach (var file in (from ListViewItem item in @group.Items select item.Text))
				{
					session.AddFile(new ArchivingFile(file));
					session.AddFileAccess(file, (ArchivingPackage)model.ArchivingPackage);
				}

				session.Genre = "Dance";
				session.SubGenre = "Entertainment";
				session.PlanningType = "Spontaneous";
			}


			using (var imdiArchiveDlg = new IMDIArchivingDlg(model, LocalizationManager.GetString(
				       "ArchivingTestApp.MainForm.AdditionalImdiArchiveProcessInfo", "This is a test of IMDI archival.")))
			{
				imdiArchiveDlg.ShowDialog(this);
			}
		}

		private void SetFilesToArchive(ArchivingDlgViewModel model, CancellationToken cancellationToken)
		{
			foreach (ListViewGroup group in m_listFiles.Groups)
			{
				var files = (from ListViewItem item in @group.Items select item.Text).ToList();
				model.AddFileGroup(group.Header, files, Format(LocalizationManager.GetString(
					"ArchivingTestApp.MainForm.AddingFileGroupProgressMsg",
					"Adding {0}", "Param is group name (directory)"), group.Header));
			}
		}

		private static string GetFileDescription(string key, string filename)
		{
			return filename.Replace("\\", "_");
		}

		private void HandleAddFilesClick(object sender, EventArgs e)
		{
			using (var dlg = new OpenFileDialog())
			{
				dlg.CheckFileExists = true;
				dlg.Multiselect = true;
				dlg.Title = Format(LocalizationManager.GetString(
					"ArchivingTestApp.MainForm.AddingFileGroupProgressMsg",
					"{0} - Select files to archive", "Param is app name"), kAppName);
				if (dlg.ShowDialog(this) == DialogResult.OK && dlg.FileNames.Any())
				{
					var group = new ListViewGroup(GetDirectoryName(dlg.FileNames[0]) ?? "root");

					foreach (var file in dlg.FileNames)
					{
						var item = new ListViewItem(file) { Group = group };
						m_listFiles.Items.Add(item);
					}

					m_listFiles.Groups.Add(group);
					m_listFiles.Refresh();

					m_btnRamp.Enabled = true;
					m_btnIMDI.Enabled = true;
				}
			}
		}
	}
}
