using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIL.Windows.Forms.CheckedComboBox;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ModalChildForm : Form
	{
		readonly Color[] _colors =
		{
			Color.Red, Color.Green, Color.Black,
			Color.White, Color.Orange, Color.Yellow,
			Color.Blue, Color.Maroon, Color.Pink, Color.Purple
		};

		private class ColorItem : CheckedComboBoxItem
		{
			public ColorItem(Color color) : base($"{color.Name} ({GetArgbString(color)})", color)
			{
			}

			internal static string GetArgbString(Color color) =>
				$"{color.ToArgb():X4}";

			public override string SummaryString =>
				GetArgbString((Color)Value);
		}

		private class ColorObject
		{
			public Color Color { get; }
			public string Name => Color.Name;
			public string ArgbValue => ColorItem.GetArgbString(Color);

			public ColorObject(Color color)
			{
				Color = color;
			}
		}

		public ModalChildForm()
		{
			InitializeComponent();

			// If more than 5 items, add a scroll bar to the dropdown.
			cboWhiteSpaceCharacters.MaxDropDownItems = 5;

			// Set a separator for how the checked items will appear in the Text portion.
			cboWhiteSpaceCharacters.ValueSeparator = ", "; // This is actually the default.

			PopulateCheckedComboBox();

			cboWhiteSpaceCharacters.DropDownClosed += CboWhiteSpaceCharacters_DropDownClosed;
		}

		private void CboWhiteSpaceCharacters_DropDownClosed(object sender, EventArgs e)
		{
			if (cboWhiteSpaceCharacters.ValueChanged)
			{
				var checkedItems = cboWhiteSpaceCharacters.CheckedItems;
				if (checkedItems.Count > 0)
					cboWhiteSpaceCharacters.ForeColor = chkUseCheckedComboBoxItems.Checked ?
						(Color)((ColorItem)checkedItems[0]).Value :
						((ColorObject)checkedItems[0]).Color;
			}
		}

		private void CboWhiteSpaceCharactersOnItemChecked(object sender, ItemCheckEventArgs e)
		{
			var checkedIndices = cboWhiteSpaceCharacters.CheckedIndices.Cast<int>().ToList();
			if (e.NewValue == CheckState.Unchecked)
				checkedIndices.Remove(e.Index);
			else if (e.NewValue == CheckState.Checked)
				checkedIndices.Add(e.Index);
			checkedIndices.Sort();
			lblCheckedIndicesData.Text = String.Join(",", checkedIndices);
		}

		private void chkUseCheckedComboBoxItems_CheckedChanged(object sender, EventArgs e)
		{
			PopulateCheckedComboBox();
		}

		private void PopulateCheckedComboBox()
		{
			cboWhiteSpaceCharacters.Items.Clear();

			foreach (var color in _colors)
			{
				// For the sake of illustrating the situation where some items should be initially
				// selected, we'll select the one corresponding to the current work day.
				cboWhiteSpaceCharacters.Items.Add(chkUseCheckedComboBoxItems.Checked ?
					(object)new ColorItem(color) : new ColorObject(color),
					cboWhiteSpaceCharacters.Items.Count == (int)DateTime.Now.DayOfWeek);
			}

			// Make the "Name" property the one to display, rather than the ToString() 
			// representation of the item.
			cboWhiteSpaceCharacters.DisplayMember = "Name";
		}
	}
}
