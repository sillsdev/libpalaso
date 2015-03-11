// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of the MIT license.
// 	See http://opensource.org/licenses/MIT for details.
// </copyright>
// --------------------------------------------------------------------------------------------
#if __MonoCS__
using System;
using System.IO;
using Gtk;
using Palaso.UI.WindowsForms.FileDialogAdapter;

namespace Palaso.UI.WindowsForms.FileDialogAdapter.Linux
{
	internal class SaveFileDialogLinux: FileDialogLinux, ISaveFileDialog
	{
		public SaveFileDialogLinux()
		{
			Action = FileChooserAction.Save;
			LocalReset();
		}

		#region ISaveFileDialog implementation
		public Stream OpenFile()
		{
			return new FileStream(FileName, FileMode.Create);
		}

		public bool CreatePrompt { get; set; }
		public bool OverwritePrompt { get; set; }
		#endregion

		private void LocalReset()
		{
			CreatePrompt = false;
			OverwritePrompt = true;
			Title = FileDialogStrings.TitleSave;
		}

		public override void Reset()
		{
			base.Reset();
			LocalReset();
		}

		protected override void ReportFileNotFound(string fileName)
		{
			ShowMessageBox(FileDialogStrings.FileNotFoundSave, ButtonsType.Ok, MessageType.Warning,
				fileName);
		}

		private bool OkToCreateFile()
		{
			return ShowMessageBox(FileDialogStrings.CreateFile,
				ButtonsType.YesNo, MessageType.Question, InternalFileName) == ResponseType.Yes;
		}

		protected override FileChooserDialog CreateFileChooserDialog()
		{
			var dlg = base.CreateFileChooserDialog();
			dlg.DoOverwriteConfirmation = OverwritePrompt;
			return dlg;
		}

		protected override void OnFileOk(System.ComponentModel.CancelEventArgs e)
		{
			if (CreatePrompt && !File.Exists(InternalFileName))
			{
				if (!OkToCreateFile())
				{
					e.Cancel = true;
					return;
				}
			}
			base.OnFileOk(e);
		}
	}
}
#endif
