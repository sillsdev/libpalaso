// From https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown
// License: https://www.codeproject.com/info/cpol10.aspx

namespace SIL.Windows.Forms.CheckedComboBox
{
    public class CheckedComboBoxItem
    {
	    public object Value { get; set; }
	    public string Name { get; set; }

        public CheckedComboBoxItem()
        {
        }

        public CheckedComboBoxItem(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() => $"name: '{Name}', value: {Value}";

        public virtual string SummaryString => null;
    }
}
