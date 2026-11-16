using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.Tests.Widgets
{
	[TestFixture]
	public class WrappingButtonTests
	{
		[Test]
		public void Text_NoMaximumSize_AutoSizeStaysOnAndButtonGrowsForLongerText()
		{
			using (var form = new Form())
			using (var button = new WrappingButton { AutoSize = true, Text = "Short" })
			{
				form.Controls.Add(button);
				var handle = form.Handle; // force handle/layout so AutoSize takes effect
				var widthAfterShortText = button.Width;

				button.Text = "A substantially longer caption than the original";

				Assert.That(button.AutoSize, Is.True);
				Assert.That(button.Width, Is.GreaterThan(widthAfterShortText),
					$"before={widthAfterShortText} after={button.Width}");
			}
		}

		[Test]
		public void MaximumSize_SetAfterConstructionWithNoPriorMax_WrapsToFitWithinIt()
		{
			using (var form = new Form())
			using (var button = new WrappingButton
			{
				AutoSize = true,
				Text = "A substantially longer caption than the original"
			})
			{
				form.Controls.Add(button);
				var handle = form.Handle; // force handle/layout so AutoSize takes effect
				var widthBeforeMax = button.Width;

				button.MaximumSize = new Size(100, 0);

				Assert.That(button.Width, Is.LessThanOrEqualTo(100),
					"button should have wrapped to respect the newly-set MaximumSize");
				Assert.That(button.Width, Is.LessThan(widthBeforeMax),
					$"before={widthBeforeMax} after={button.Width}");
				Assert.That(button.Height, Is.GreaterThan(button.Font.Height),
					"text should have wrapped onto more than one line");
			}
		}
	}
}
