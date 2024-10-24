// Copyright (c) 2014-2024, SIL Global

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Control for entering passwords. Contains a password reveal eye that can be clicked and held to reveal the entered password.
	/// </summary>
	public partial class PasswordBox : TextBox
	{
		private readonly PictureBox _eye;

		/// <summary/>
		public PasswordBox()
		{
			InitializeComponent();
			_eye = new PictureBox();

			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PasswordBox));
			_eye = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(_eye)).BeginInit();
			this.SuspendLayout();

			_eye.Image = global::SIL.Windows.Forms.Properties.Resources.passwordRevealEye16x16;
			_eye.Name = "eyePicture";
			_eye.Size = new System.Drawing.Size(16, 16);
			_eye.TabIndex = 0;
			_eye.TabStop = false;
			_eye.Cursor = Cursors.Arrow;

			this.UseSystemPasswordChar = true;

			_eye.MouseDown += RevealPassword;
			_eye.MouseUp += HidePassword;

			this.Controls.Add(_eye);
		}

		private void RevealPassword(object sender, MouseEventArgs mouseEventArgs)
		{
			this.UseSystemPasswordChar = false;
		}

		private void HidePassword(object sender, MouseEventArgs e)
		{
			this.UseSystemPasswordChar = true;
		}

		/// <summary/>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			var textBoxWidth = this.Size.Width;
			const int margin = 8;
			_eye.Location = new Point(textBoxWidth - _eye.Width - margin, 0);
		}
	}
}
