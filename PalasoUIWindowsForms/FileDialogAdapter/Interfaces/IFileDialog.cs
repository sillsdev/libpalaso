// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// 	Copyright (c) 2011, SIL International. All Rights Reserved.
//
// 	Distributable under the terms of the MIT license.
// 	See http://opensource.org/licenses/MIT for details.
// </copyright>
// --------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.FileDialogAdapter
{
	/// <summary>
	/// Interface to the FileDialog
	/// </summary>
	public interface IFileDialog
	{
		bool AddExtension { get; set; }
		bool CheckFileExists { get; set; }
		bool CheckPathExists { get; set; }
		string DefaultExt { get; set; }
		string FileName { get; set; }
		string[] FileNames { get; }
		string Filter { get; set; }
		int FilterIndex { get; set; }
		string InitialDirectory { get; set; }
		bool RestoreDirectory { get; set; }
		bool ShowHelp { get; set; }
		bool SupportMultiDottedExtensions { get; set; }
		string Title { get; set; }
		bool ValidateNames { get; set; }

		void Reset();
		DialogResult ShowDialog();
		DialogResult ShowDialog(IWin32Window owner);

		event EventHandler Disposed;
		event CancelEventHandler FileOk;
		event EventHandler HelpRequest;
	}
}
