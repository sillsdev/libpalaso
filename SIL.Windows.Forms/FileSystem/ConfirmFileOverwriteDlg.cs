// ---------------------------------------------------------------------------------------------
#region // Copyright 2024 SIL Global
// <copyright from='2013' to='2024' company='SIL Global'>
//		Copyright (c) 2024 SIL Global
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: ConfirmFileOverwriteDlg.cs
// ---------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.FileSystem
{
	public partial class ConfirmFileOverwriteDlg : Form
	{
		public ConfirmFileOverwriteDlg(string filename)
		{
			InitializeComponent();

			lblFilename.Text = filename;
		}

		public ConfirmFileOverwriteDlg(string filename, bool yesIsDefault)
			: this(filename)
		{
			if (yesIsDefault)
				DefaultResult = DialogResult.Yes;
		}

		public ConfirmFileOverwriteDlg(string filename, string title)
			: this(filename)
		{
			Title = title;
		}

		public ConfirmFileOverwriteDlg(string filename, string title, bool yesIsDefault) :
			this(filename, yesIsDefault)
		{
			Title = title;
		}

		public DialogResult DefaultResult
		{
			get => (AcceptButton == btnNo) ? btnNo.DialogResult : btnYes.DialogResult;
			set
			{
				switch (value)
				{
					case DialogResult.No:
						AcceptButton = btnNo;
						break;
					case DialogResult.Yes:
						AcceptButton = btnYes;
						break;
					default:
						throw new ArgumentException("Yes and No are the only possible results.");
				}
			}
		}

		public string Title
		{
			get => Text;
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("title");

				Text = value;
			}
		}

		public bool ApplyToAll
		{
			get => chkApplyToAll.Checked;
			set => chkApplyToAll.Checked = value;
		}
	}
}