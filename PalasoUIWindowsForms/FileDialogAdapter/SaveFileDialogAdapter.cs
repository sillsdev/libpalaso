// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of the MIT license.
// 	See http://opensource.org/licenses/MIT for details.
// </copyright>
// --------------------------------------------------------------------------------------------
using System.IO;

namespace Palaso.UI.WindowsForms.FileDialogAdapter
{
	public class SaveFileDialogAdapter: FileDialogAdapter, ISaveFileDialog
	{
		public SaveFileDialogAdapter()
		{
			m_dlg = Manager.CreateSaveFileDialog();
		}

		public bool CreatePrompt
		{
			get { return ((ISaveFileDialog)m_dlg).CreatePrompt; }
			set { ((ISaveFileDialog)m_dlg).CreatePrompt = value; }
		}

		public bool OverwritePrompt
		{
			get { return ((ISaveFileDialog)m_dlg).OverwritePrompt; }
			set { ((ISaveFileDialog)m_dlg).OverwritePrompt = value; }
		}

		public Stream OpenFile()
		{
			return ((ISaveFileDialog)m_dlg).OpenFile();
		}
	}
}
