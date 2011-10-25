using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets
{
	/// <summary>
	/// This version does two things:
	/// 1) you don't have to first get focus before it will pay attention to you trying to check a box (e.g. it doesn't swallow the first attempt)
	/// 2) only clicks on the actual box will check/uncheck the box.
	///
	/// Got help from: http://stackoverflow.com/questions/2093961/checkedlistbox-control-only-checking-the-checkbox-when-the-actual-checkbox-is
	/// </summary>
	public partial class BetterCheckedListBox : CheckedListBox
	{
		bool AuthorizeCheck { get; set; }

		public BetterCheckedListBox()
		{
			InitializeComponent();
			CheckOnClick = false;
		}

		public BetterCheckedListBox(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}



		private void OnItemCheck(object sender, ItemCheckEventArgs e)
		{
			if (!AuthorizeCheck)
				e.NewValue = e.CurrentValue; //check state change was not through authorized actions
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			Point loc = PointToClient(Cursor.Position);
			for (int i = 0; i < Items.Count; i++)
			{
				Rectangle rec = GetItemRectangle(i);
				rec.Width = 16; //checkbox itself has a default width of about 16 pixels

				if (rec.Contains(loc))
				{
					AuthorizeCheck = true;
					bool newValue = !GetItemChecked(i);
					SetItemChecked(i, newValue);//check
					AuthorizeCheck = false;

					return;
				}
			}
		}

	}
}
