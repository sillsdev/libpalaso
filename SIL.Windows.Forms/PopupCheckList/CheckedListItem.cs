namespace SIL.Windows.Forms.PopupCheckList
{
	public class CheckedListItem
	{
		private bool _checked;

		public delegate void ItemCheckStateChangedHandler(
			CheckedListItem sender, bool checkState);
		public event ItemCheckStateChangedHandler ItemCheckStateChanged;

		public bool Checked
		{
			get => _checked;
			set
			{
				if (_checked != value)
				{
					_checked = value;
					ItemCheckStateChanged?.Invoke(this, value);
				}
			}
		}

		public virtual string CheckedListDisplay { get; set; }
		public virtual string EditSummaryDisplay { get; set; }
	}
}
