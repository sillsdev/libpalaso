using System.Windows.Forms;
using NUnit.Framework;

namespace SIL.Windows.Forms.Tests.SpellChecking
{
	[TestFixture]
	public class TextBoxSpellChecker
	{
		[Test]
		[Explicit("By hand only")]
		public void TryInUI()
		{
			var form = new Form();
			var checker = new global::SIL.Windows.Forms.Spelling.TextBoxSpellChecker();

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
