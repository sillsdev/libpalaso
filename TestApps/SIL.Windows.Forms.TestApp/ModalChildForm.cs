using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.CheckedComboBox;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ModalChildForm : Form
	{
		private class ColorItem : CheckedComboBoxItem
		{
			public ColorItem(Color color) : base($"{color.Name} ({GetARGBString(color)})", color)
			{
			}

			private static string GetARGBString(Color color) =>
				$"{color.ToArgb():X4}";

			public override string SummaryString =>
				GetARGBString((Color)Value);
		}

		public ModalChildForm()
		{
			InitializeComponent();

			// If more than 5 items, add a scroll bar to the dropdown.
			cboWhiteSpaceCharacters.MaxDropDownItems = 5;

			// Make the "Name" property the one to display, rather than the ToString() 
			// representation of the item.
			cboWhiteSpaceCharacters.DisplayMember = "Name";

			// Set a separator for how the checked items will appear in the Text portion.
			cboWhiteSpaceCharacters.ValueSeparator = ", ";

			Color[] colors =
			{
				Color.Red, Color.Green, Color.Black,
				Color.White, Color.Orange, Color.Yellow,
				Color.Blue, Color.Maroon, Color.Pink, Color.Purple
			};

			for (int i = 0; i < colors.Length; i++)
				cboWhiteSpaceCharacters.Items.Add(new ColorItem(colors[i]));

			cboWhiteSpaceCharacters.DropDownClosed += CboWhiteSpaceCharacters_DropDownClosed;
		}

		private void CboWhiteSpaceCharacters_DropDownClosed(object sender, System.EventArgs e)
		{
			if (cboWhiteSpaceCharacters.ValueChanged)
			{
				var checkedItems = cboWhiteSpaceCharacters.CheckedItems;
				if (checkedItems.Count > 0)
					cboWhiteSpaceCharacters.ForeColor = (Color)((ColorItem)checkedItems[0]).Value;
			}
		}
	}
}
