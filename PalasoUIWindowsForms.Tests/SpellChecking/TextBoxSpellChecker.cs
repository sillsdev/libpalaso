using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace PalasoUIWindowsForms.Tests.SpellChecking
{
	[TestFixture]
	public class TextBoxSpellChecker
	{
		[Test, Ignore("By hand only")]
		public void TryInUI()
		{
			var form = new Form();
			var checker = new Palaso.UI.WindowsForms.Spelling.TextBoxSpellChecker();

			var box = new TextBox();
			box.Multiline = true;
			box.Dock = DockStyle.Fill;
			box.Text = "correct wrng correct";
			checker.SetLanguageForSpellChecking(box, "en");

			form.Controls.Add(box);

			form.ShowDialog();
		}

	}
}
