using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Widgets
{
	public delegate void CopyPasteEmbeddedTextBox();

	/// <summary>
	/// Replaces a Windows combo box control (System.Windows.Forms.ComboBox)
	/// Returns an empty string when get_Text generates an IndexOutOfRangeException (FB 6643, 6750)
	/// </summary>
	public class SafeComboBox : ComboBox
	{
		public event CopyPasteEmbeddedTextBox CopyEvent;
		public event CopyPasteEmbeddedTextBox PasteEvent;

		[DllImport("user32.dll", EntryPoint = "FindWindowExA", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
		private TextBoxHandle textBoxHandle;

		/// <summary>
		/// Get handle of TextBox that is embedded in the ComboBox,
		/// and is needed for copying and pasting VerseRefs using context menu
		/// </summary>
		private void GetTextBoxHandle()
		{
			if (textBoxHandle == null)
			{
				IntPtr embeddedTextBox = FindWindowEx(this.Handle, IntPtr.Zero, "EDIT", null);

				textBoxHandle = new TextBoxHandle();
				textBoxHandle.AssignHandle(embeddedTextBox);

				textBoxHandle.CopyEvent += OnCopyEvent;
				textBoxHandle.PasteEvent += OnPasteEvent;
			}
		}

		protected virtual void OnCopyEvent()
		{
			CopyEvent?.Invoke();
		}

		protected virtual void OnPasteEvent()
		{
			PasteEvent?.Invoke();
		}

		/// <summary>
		/// Occurs before mouse down.
		/// </summary>
		public event EventHandler<MouseEventArgs> BeforeMouseDown;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			GetTextBoxHandle();

			if (BeforeMouseDown != null)
				BeforeMouseDown(this, e);

			base.OnMouseDown(e);
		}

		// Trap IndexOutOfRangeExceptions and recover nicely
		[Localizable(true)]
		[Bindable(true)]
		public override string Text
		{
			get
			{
				try
				{
					return base.Text;
				}
				catch (IndexOutOfRangeException)
				{
					return "";
				}
				catch (ArgumentOutOfRangeException)
				{
					return "";
				}
			}
			set
			{
				// TODO: fix mono bug - Mono attempts to be smart and doesn't change the text if value is the selected list item.
				// If the current Text doesn't equal anything in the list, changing the text to the same as the selected list item
				// becomes a NULL OP.
				if (Platform.IsMono && SelectedItem != null && GetItemText(SelectedItem) == value)
					SelectedItem = null;

				try
				{
					base.Text = value;
				}
				catch (IndexOutOfRangeException)
				{
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
		}
	}

	/// <summary>
	/// Handle of the TextBox that is embedded in the ComboBox
	/// </summary>
	public class TextBoxHandle : NativeWindow
	{
		public event CopyPasteEmbeddedTextBox CopyEvent;
		public event CopyPasteEmbeddedTextBox PasteEvent;

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case (0x301): //WM_COPY
					OnCopyEvent();
					break;
				case (0x302): //WM_PASTE
					OnPasteEvent();
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}

		protected virtual void OnCopyEvent()
		{
			CopyEvent?.Invoke();
		}

		protected virtual void OnPasteEvent()
		{
			PasteEvent?.Invoke();
		}
	}
}
