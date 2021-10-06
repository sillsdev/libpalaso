// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2021, SIL International.   
// <copyright from='2021' to='2021 company='SIL International'>
//		Copyright (c) 2021, SIL International.   
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: ParentFormBase.cs
// ---------------------------------------------------------------------------------------------
using System;
using System.Media;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace SIL.Windows.Forms
{
	/// <summary>
	/// Base class for forms which need to show a child form that is modal with respect to
	/// the parent form but is not application modal.
	/// </summary>
	[PublicAPI]
	public class ParentFormBase : Form
	{
		private Form ModalChild { get; set; }

		[PublicAPI]
		protected bool IsShowingModalForm => ModalChild != null;

		protected delegate void ModalChildEventHandler();

		protected event ModalChildEventHandler OnModalFormShown;

		protected event ModalChildEventHandler OnModalFormClosed;

		protected override void OnActivated(EventArgs e)
		{
			if (ModalChild == null)
				base.OnActivated(e);
			else
			{
				SystemSounds.Beep.Play();
				ModalChild.Activate();
				ModalChild.BringToFront();
			}
		}

		protected void ShowModalChild<T>(T childForm, Action<T> onClosed = null) where T : Form
		{
			ModalChild = childForm;
			foreach (var ctrl in childForm.Controls)
			{
				// This is not 100% guaranteed to be right, since it is possible that a button could
				// have no DialogResult set at the outset, but during the course of the user's interaction with
				// the dialog box, a DialogResult could be assigned dynamically. But this is unlikely (does not
				// currently happen in Transcelerator) so by checking the DialogResult up-front, we avoid having
				// to hook up this click handler for other buttons.
				if (ctrl is Button btn && btn.DialogResult != DialogResult.None)
					btn.Click += (sender, args) =>
					{
						var clickedButton = (Button)sender;
						// There's always the slight possibility that the DialogResult was changed in the code to be None.
						if (clickedButton.DialogResult != DialogResult.None)
						{
							ModalChild.DialogResult = clickedButton.DialogResult;
							var dialog = ModalChild;
							ModalChild = null;
							dialog.Close();
						}
					};
			}
			childForm.Closed += (dialog, args) =>
			{
				ModalChild = null;
				T dlg = (T)dialog;
				onClosed?.Invoke(dlg);
				dlg.Dispose();
				OnModalFormClosed?.Invoke();
			};
			childForm.HandleCreated += (sender, args) =>
			{
				childForm.WindowState = FormWindowState.Normal;
				childForm.BringToFront();
				childForm.Activate();
			};
			childForm.Shown += (sender, args) =>
			{
				if (childForm.ActiveControl == null)
					childForm.SelectNextControl(childForm, true, true, true, false);
			};

			childForm.Show(this);
			OnModalFormShown?.Invoke();
		}
	}
}
