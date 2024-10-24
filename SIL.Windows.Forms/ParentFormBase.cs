// ---------------------------------------------------------------------------------------------
#region // Copyright 2024 SIL Global
// <copyright from='2021' to='2024' company='SIL Global'>
//		Copyright (c) 2024 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: ParentFormBase.cs
// ---------------------------------------------------------------------------------------------
using System;
using System.Drawing;
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
		protected bool IsShowingModalForm => ModalChild != null && ModalChild.Visible;

		protected delegate void ModalChildEventHandler();

		/// <summary>
		/// Occurs after any modal child form is shown in response to a call to
		/// <see cref="ShowModalChild{T}"/>.
		/// </summary>
		/// <remarks>Handle this event to disable any controls or timer-based events on the
		/// parent form that might still be active even though the parent form cannot be
		/// made the active form.</remarks>
		protected event ModalChildEventHandler OnModalFormShown;

		/// <summary>
		/// Occurs when a modal child form is closed (before the onClosed handler passed into
		/// <see cref="ShowModalChild{T}"/> is called).
		/// </summary>
		/// <remarks>Handle this event if needed to re-enable any controls or timer-based events on
		/// the parent form that were disabled in response to <see cref="OnModalFormShown"/> that
		/// should be enabled before the onClosed handler is called.</remarks>
		protected event ModalChildEventHandler OnModalFormClosed;

		/// <summary>
		/// Occurs when a modal child form is disposed.
		/// </summary>
		/// <remarks>Handle this event if needed to re-enable any controls or timer-based events on
		/// the parent form that were disabled in response to <see cref="OnModalFormShown"/> that
		/// should be enabled after the onClosed handler is called.</remarks>
		protected event ModalChildEventHandler OnModalFormDisposed;

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

		/// <summary>
		/// Shows the given form as a dialog box which is modal with respect to
		/// the this form (not application modal).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="childForm">The dialog box to show.  </param>
		/// <param name="onClosed">Occurs when the modal form is closed. Note that this event will
		/// be called after the <see cref="OnModalFormClosed"/> event is fired and before the
		/// <see cref="OnModalFormDisposed"/> event is fired.</param>
		/// <remarks>Any buttons that have their <see cref="Button.DialogResult"/> property set to
		/// any value other than <see cref="DialogResult.None"/> at the time this method is called
		/// will cause the dialog box to close and have its <see cref="Form.DialogResult"/> set to
		/// the then-current value of the button's <see cref="Button.DialogResult"/> property. Note
		/// that this means that if a button's <see cref="Button.DialogResult"/> property is
		/// initially <see cref="DialogResult.None"/> and is set to some other value after the
		/// dialog box is opened, clicking it will not automatically cause the the dialog box to
		/// close. This is subtly different from the behavior when a form is shown using
		/// <see cref="Form.ShowDialog()"/>.
		/// </remarks>
		protected void ShowModalChild<T>(T childForm, Action<T> onClosed = null) where T : Form
		{
			ModalChild = childForm;
			foreach (var ctrl in childForm.Controls)
			{
				// As described in the remarks above, this is not 100% guaranteed to reproduce the
				// same behavior as ShowDialog. But this is unlikely (and documented). By checking
				// the DialogResult up-front, we avoid having to hook up this click handler for
				// other buttons.
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
				OnModalFormClosed?.Invoke();
				ModalChild = null;
				T dlg = (T)dialog;
				onClosed?.Invoke(dlg);
				dlg.Dispose();
				OnModalFormDisposed?.Invoke();
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

			if (childForm.StartPosition == FormStartPosition.CenterParent)
			{
				childForm.StartPosition = FormStartPosition.Manual;
				childForm.Location = new Point(Math.Max(Location.X, Location.X + (Width - childForm.Width) / 2),
					Math.Max(Location.Y, Location.Y + (Height - childForm.Height) / 2));
			}
			childForm.Show(this);
			OnModalFormShown?.Invoke();
		}
	}
}
