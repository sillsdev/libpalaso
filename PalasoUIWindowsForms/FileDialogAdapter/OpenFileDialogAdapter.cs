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
	/// <summary>Cross-platform OpenFile dialog. On Windows it displays .NET's WinForms
	/// OpenFileDialog, on Linux the GTK FileChooserDialog.</summary>
	public class OpenFileDialogAdapter: FileDialogAdapter, IOpenFileDialog
	{
		public OpenFileDialogAdapter()
		{
			m_dlg = Manager.CreateOpenFileDialog();
		}

		#region IOpenFileDialog implementation
		public Stream OpenFile()
		{
			return ((IOpenFileDialog)m_dlg).OpenFile();
		}

		public bool Multiselect
		{
			get { return ((IOpenFileDialog)m_dlg).Multiselect; }
			set { ((IOpenFileDialog)m_dlg).Multiselect = value; }
		}
		#endregion
	}
}
