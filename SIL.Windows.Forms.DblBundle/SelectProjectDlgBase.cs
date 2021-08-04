using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using SIL.DblBundle;

namespace SIL.Windows.Forms.DblBundle
{
	public abstract class SelectProjectDlgBase : IDisposable
	{
		private OpenFileDialog m_fileDialog;
		private string m_defaultDir;

		protected abstract string DefaultBundleDirectory { get; set; }
		protected abstract string ProjectFileExtension { get; }
		protected abstract string Title { get; }
		protected abstract string ProductName { get; }

		protected SelectProjectDlgBase(bool allowProjectFiles = true, string defaultFile = null)
		{
			FileName = File.Exists(defaultFile) ? Path.GetFileName(defaultFile) : null;
			m_defaultDir = (defaultFile != null ? Path.GetDirectoryName(defaultFile) : DefaultBundleDirectory);

		}

		protected virtual OpenFileDialog CreateFileDialog()
		{
			if (string.IsNullOrEmpty(m_defaultDir) || !Directory.Exists(m_defaultDir))
			{
				m_defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			string projectFiles = "";
			if (allowProjectFiles)
				projectFiles = string.Format("{0} ({1})|{1}|",
					string.Format(LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ProjectFilesLabel", "{0} Project Files", "{0} is the product name"), ProductName),
					"*" + ProjectFileExtension);
			m_fileDialog = new OpenFileDialog
			{
				Title = Title,
				InitialDirectory = m_defaultDir,
				FileName = FileName,
				Filter = string.Format("{0} ({1})|{1}|{2}{3} ({4})|{4}",
					LocalizationManager.GetString("DialogBoxes.SelectProjectDlg.ResourceBundleFileTypeLabel", "Text Resource Bundle files"),
					"*" + DblBundleFileUtils.kDblBundleExtension,
					projectFiles,
					LocalizationManager.GetString("DialogBoxes.FileDlg.AllFilesLabel", "All Files"),
					"*.*"),
				DefaultExt = DblBundleFileUtils.kDblBundleExtension
			};

			return m_fileDialog;
		}

		public DialogResult ShowDialog()
		{
			m_fileDialog = CreateFileDialog();
			var result = m_fileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileName = m_fileDialog.FileName;
				var dir = Path.GetDirectoryName(FileName);
				if (!string.IsNullOrEmpty(dir))
					DefaultBundleDirectory = dir;
			}
			return result;
		}

		public string FileName { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_fileDialog.Dispose();
				m_fileDialog = null; 
			}
		}
	}
}
