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
	/// <summary>
	/// Interface to the OpenFileDialog
	/// </summary>
	public interface IOpenFileDialog: IFileDialog
	{
		bool Multiselect { get; set; }

		Stream OpenFile();
	}
}
