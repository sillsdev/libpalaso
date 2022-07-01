using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	public delegate void CopyPasteEnterTextBox();

	/// <summary>
	/// Subclass of a TextBox which captures the enter key.
	/// Normal text boxes cannot process enter keys, and the 
	/// default button on the form is selected instead.
	/// Also copy and paste event are overridden,
	/// so complete VerseRefs are copied/pasted
	/// </summary>
	public class EnterTextBox : TextBox
	{
		public event CopyPasteEnterTextBox CopyEvent;
		public event CopyPasteEnterTextBox PasteEvent;

		protected override bool IsInputKey(Keys key)
		{
			if (key == Keys.Enter)
				return true;
			return base.IsInputKey(key);
		}

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
