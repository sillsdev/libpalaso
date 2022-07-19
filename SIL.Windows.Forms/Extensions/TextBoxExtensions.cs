using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	internal static class TextBoxExtensions
	{
		public static void SetMultiLineText(this TextBox textBox, string text)
		{
			textBox.Multiline = true; // Normally this will already be set, but just to be sure.
			textBox.Text = Regex.Replace(text, @"(?<!\r)\n", Environment.NewLine);
		}
	}
}
