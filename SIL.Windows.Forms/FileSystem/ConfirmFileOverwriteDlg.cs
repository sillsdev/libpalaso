// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International.   
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
			get { return (AcceptButton == btnNo) ? btnNo.DialogResult : btnYes.DialogResult; }
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
			get { return Text; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("title");

				Text = value;
			}
		}

		public bool ApplyToAll
		{
			get { return chkApplyToAll.Checked; }
			set { chkApplyToAll.Checked = value; }
		}
	}
}