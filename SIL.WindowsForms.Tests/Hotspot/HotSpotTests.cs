using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace SIL.WindowsForms.Tests.Hotspot
{
	[TestFixture]
	public class HotSpotTests
	{
		private TextBox _textBox;

		[SetUp]
		public void SetUp()
		{
			_textBox = new TextBox();
			_textBox.Text = "Hello world!";
		}

		[TearDown]
		public void TearDown()
		{
			_textBox.Dispose();
		}

		[Test]
		public void Construct_ControlNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
 new HotSpot.HotSpot(null, 0, 1));
		}

		[Test]
		public void Construct_OffsetNegative_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
new HotSpot.HotSpot(_textBox, -1, 1));
		}

		[Test]
		public void Construct_StartOfHotspotPastTextEnd_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
new HotSpot.HotSpot(_textBox, _textBox.TextLength, 1));
		}

		[Test]
		public void Construct_LengthNegative_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				new HotSpot.HotSpot(_textBox, 0, -1));
		}

		[Test]
		public void Construct_LengthZero_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				new HotSpot.HotSpot(_textBox, 0, 0));
		}

		[Test]
		public void Construct_EndOfHotspotPastTextEnd_Throws()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			new HotSpot.HotSpot(_textBox, 1, _textBox.TextLength));
		}

		[Test]
		public void Construct_SelectEntireText_DoesNotThrow()
		{
			Assert.IsNotNull(new HotSpot.HotSpot(_textBox, 0, _textBox.TextLength));
		}

		[Test]
		public void Text_GivenOffsetAndLength_SubstringOfTextFromControl()
		{
			_textBox.Text = "Hello my darling";
			HotSpot.HotSpot hotSpot = new HotSpot.HotSpot(_textBox, 6, 2);
			Assert.AreEqual("my", hotSpot.Text);
		}
	}
}