using System;
using System.Windows.Forms;
using NUnit.Framework;
using HotSpot=Palaso.UI.WindowsForms.HotSpot.HotSpot;

namespace PalasoUIWindowsForms.Tests.Hotspot
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
		[ExpectedException(typeof (ArgumentNullException))]
		public void Construct_ControlNull_Throws()
		{
			new HotSpot(null, 0, 1);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Construct_OffsetNegative_Throws()
		{
			new HotSpot(_textBox, -1, 1);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Construct_StartOfHotspotPastTextEnd_Throws()
		{
			new HotSpot(_textBox, _textBox.TextLength, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Construct_LengthNegative_Throws()
		{
			new HotSpot(_textBox, 0, -1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Construct_LengthZero_Throws()
		{
			new HotSpot(_textBox, 0, 0);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Construct_EndOfHotspotPastTextEnd_Throws()
		{
			new HotSpot(_textBox, 1, _textBox.TextLength);
		}

		[Test]
		public void Construct_SelectEntireText_DoesNotThrow()
		{
			Assert.IsNotNull(new HotSpot(_textBox, 0, _textBox.TextLength));
		}

		[Test]
		public void Text_GivenOffsetAndLength_SubstringOfTextFromControl()
		{
			_textBox.Text = "Hello my darling";
			HotSpot hotSpot = new HotSpot(_textBox, 6, 2);
			Assert.AreEqual("my", hotSpot.Text);
		}
	}
}