using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	public static class ComboBoxExtensions
	{
		public static bool Contains(this ComboBox cb, string value, StringComparison stringComparison)
		{
			var sequenceEnum = cb.Items.GetEnumerator();
			while (sequenceEnum.MoveNext())
			{
				string element = sequenceEnum.Current as string;
				if (element != null && element.Equals(value, stringComparison))
					return true;
			}
			return false;
		}
	}
}
