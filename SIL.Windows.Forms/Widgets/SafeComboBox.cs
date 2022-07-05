using System;
using System.ComponentModel;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Replaces a Windows combo box control (System.Windows.Forms.ComboBox)
	/// Returns an empty string when get_Text generates an IndexOutOfRangeException (FB 6643, 6750)
	/// </summary>
	public class SafeComboBox : ComboBox
	{
		/// <summary>
		/// Occurs before mouse down.
		/// </summary>
		public event EventHandler<MouseEventArgs> BeforeMouseDown;

		protected override void OnMouseDown(MouseEventArgs e)
		{
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
}
