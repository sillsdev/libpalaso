using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	[ProvideProperty("Prompt", typeof (Control))]
	public class Prompt: Component, IExtenderProvider
	{
		private readonly Dictionary<Control, PromptPainter> _extendees;

		public Prompt()
		{
			_extendees = new Dictionary<Control, PromptPainter>();
		}

		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			VerifyNotDisposed();
			return extendee is TextBoxBase;
		}

		#endregion

		[DefaultValue("")]
		public string GetPrompt(Control c)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}

			PromptPainter value;
			if (_extendees.TryGetValue(c, out value))
			{
				return value.Prompt;
			}
			return string.Empty;
		}

		public void SetPrompt(Control c, string value)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			if (String.IsNullOrEmpty(value))
			{
				_extendees.Remove(c);
			}
			else
			{
				if (_extendees.ContainsKey(c))
				{
					_extendees[c].Prompt = value;
				}
				else
				{
					_extendees[c] = new PromptPainter(c, value);
				}
			}
		}

		public bool GetIsPromptVisible(Control c)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}

			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			PromptPainter value;
			if (_extendees.TryGetValue(c, out value))
			{
				return value.ShouldShowPrompt(c);
			}
			return false;
		}

		#region IComponent Members

		public override ISite Site
		{
			get
			{
				VerifyNotDisposed();
				return base.Site;
			}
			set
			{
				VerifyNotDisposed();
				base.Site = value;
			}
		}

		private bool _isDisposed = false;
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_extendees.Clear();
				_isDisposed = true;
			}

			base.Dispose(disposing);
		}

		private void VerifyNotDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
		#endregion

		#region Nested type: PromptPainter

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		private class PromptPainter: NativeWindow, IDisposable
		{
			private readonly Control _control;
			private bool _hasFocus;
			private string _prompt;

			public PromptPainter(Control c, string prompt)
			{
				if (c.IsHandleCreated)
				{
					AssignHandle(c.Handle);
				}
				c.HandleCreated += ControlHandleCreated;
				c.HandleDestroyed += ControlHandleDestroyed;
				_control = c;
				Prompt = prompt;
			}

			private void ControlHandleCreated(object sender, EventArgs e)
			{
				AssignHandle(((Control)sender).Handle);
			}

			private void ControlHandleDestroyed(object sender, EventArgs e)
			{
				ReleaseHandle();
			}

			public string Prompt
			{
				get { return _prompt; }
				set { _prompt = value; }
			}

			public bool Focused
			{
				get
				{
					return _hasFocus;
				}
			}

			protected override void WndProc(ref Message m)
			{
				const int WM_PAINT = 0xF;
				const int WM_SETFOCUS = 0x7;
				const int WM_KILLFOCUS = 0x8;

				base.WndProc(ref m);
				switch (m.Msg)
				{
					case WM_SETFOCUS:
						_hasFocus = true;
						_control.Invalidate();
						break;
					case WM_KILLFOCUS:
						_hasFocus = false;
						_control.Invalidate();
						break;
					case WM_PAINT:
						OnWmPaint();
						break;
				}
			}

			private void OnWmPaint()
			{
				if (ShouldShowPrompt(_control))
				{
					using (Graphics g = (_control is ComboBox)?_control.CreateGraphics():Graphics.FromHwnd(Handle))
					{
						TextFormatFlags flags = GetTextFormatFlags(_control);
						Rectangle bounds = _control.ClientRectangle;
						bounds.Inflate(-1, 0);
						TextRenderer.DrawText(g,
											  Prompt,
											  _control.Font,
											  bounds,
											  SystemColors.GrayText,
											  _control.BackColor,
											  flags);
					}
				}
			}

			private static TextFormatFlags GetTextFormatFlags(Control c)
			{
				TextFormatFlags flags = TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding |
										TextFormatFlags.WordBreak;
				HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;

				TextBox textbox = c as TextBox;
				if (textbox != null)
				{
					horizontalAlignment = textbox.TextAlign;
				}
				else
				{
					RichTextBox richtextbox = c as RichTextBox;
					if (richtextbox != null)
					{
						horizontalAlignment = richtextbox.SelectionAlignment;
					}
				}

				switch (horizontalAlignment)
				{
					case HorizontalAlignment.Center:
						flags |= TextFormatFlags.HorizontalCenter;
						break;
					case HorizontalAlignment.Left:
						flags |= TextFormatFlags.Left;
						break;
					case HorizontalAlignment.Right:
						flags |= TextFormatFlags.Right;
						break;
				}
				if (IsControlRightToLeft(c))
				{
					flags |= TextFormatFlags.RightToLeft;
				}
				return flags;
			}

			public bool ShouldShowPrompt(Control c)
			{
				return (!Focused && String.IsNullOrEmpty(c.Text));
			}

			private static bool IsControlRightToLeft(Control control)
			{
				while (control != null)
				{
					switch (control.RightToLeft)
					{
						case RightToLeft.Yes:
							return true;
						case RightToLeft.No:
							return false;
						case RightToLeft.Inherit:
							control = control.Parent;
							break;
					}
				}
				return false;
			}
			#region IDisposable Members
			private bool _disposed = false;

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!this._disposed)
				{
					if (disposing)
					{
						// dispose-only, i.e. non-finalizable logic
						_control.HandleCreated -= ControlHandleCreated;
						_control.HandleDestroyed -= ControlHandleDestroyed;
						if(Handle != IntPtr.Zero)
						{
							ReleaseHandle();
						}
					}

					// shared (dispose and finalizable) cleanup logic
					this._disposed = true;
				}
			}
			#endregion
		}

		#endregion
	}
}