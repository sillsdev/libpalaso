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
				_ = form.Handle; // force handle/layout so AutoSize takes effect
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
				_ = form.Handle; // force handle/layout so AutoSize takes effect
				var widthBeforeMax = button.Width;
				var heightBeforeMax = button.Height;

				button.MaximumSize = new Size(100, 0);

				Assert.That(button.Width, Is.LessThanOrEqualTo(100),
					"button should have wrapped to respect the newly-set MaximumSize");
				Assert.That(button.Width, Is.LessThan(widthBeforeMax),
					$"before={widthBeforeMax} after={button.Width}");
				Assert.That(button.Height, Is.GreaterThan(heightBeforeMax),
					"text should have wrapped onto more than one line");
			}
		}

		[Test]
		public void AutoSize_SetTrueAfterMaximumSize_StaysWithinMaximumAfterParentLayout()
		{
			using (var form = new Form())
			using (var button = new WrappingButton { MaximumSize = new Size(140, 0) })
			{
				form.Controls.Add(button);
				button.AutoSize = true;
				button.Text = "A substantially longer caption than the original";

				_ = form.Handle; // force a real parent layout pass
				form.PerformLayout();

				// Native AutoSize stays off while MaximumSize.Width requires wrapping - this
				// class takes over sizing instead - so the getter reports False here even though
				// AutoSize was set True; that's what keeps native single-line sizing from
				// re-asserting itself and overflowing MaximumSize on the next layout pass.
				Assert.That(button.AutoSize, Is.False);
				Assert.That(button.Width, Is.LessThanOrEqualTo(140),
					$"width={button.Width} should not exceed MaximumSize.Width=140 after layout");
			}
		}
	}
}
