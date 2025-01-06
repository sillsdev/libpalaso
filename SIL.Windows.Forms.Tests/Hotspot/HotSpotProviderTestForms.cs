using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.HotSpot;

namespace SIL.Windows.Forms.Tests.Hotspot
{
	[TestFixture]
	public class HotSpotProviderTestForms
	{
		private HotSpotProvider _hotSpotProvider;
		private TextBox _textBox;
		private HotSpot.HotSpot _spot1;
		private HotSpot.HotSpot _spot2;
		private Point _originalCursorPosition;
		private Form _form;

		[SetUp]
		public void Setup()
		{
			_hotSpotProvider = new HotSpotProvider();
			_textBox = new TextBox();
			_textBox.Width = 350;
			_textBox.Text = "Now is the time for ...";
			_spot1 = new HotSpot.HotSpot(_textBox, 7, 3);
			_spot2 = new HotSpot.HotSpot(_textBox, 16, 3);

			_hotSpotProvider.SetEnableHotSpots(_textBox, true);
			_hotSpotProvider.RetrieveHotSpots +=
				delegate(object sender, RetrieveHotSpotsEventArgs e)
				{
					e.AddHotSpot(_spot1);
					e.AddHotSpot(_spot2);
					e.Color = Color.Yellow;
				};
			_originalCursorPosition = Cursor.Position;

			_form = new Form();
			_form.Controls.Add(_textBox);
			_form.Show();
		}

		[TearDown]
		public void TearDown()
		{
			_hotSpotProvider.Dispose();
			_form.Dispose();

			Cursor.Position = _originalCursorPosition;
		}

		[Test]
		public void DisableHotSpotProvider_MouseEventsNoLongerFired()
		{
			var mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			_hotSpotProvider.SetEnableHotSpots(_textBox, false);

			Assert.IsFalse(mouseEnterFired);

			_spot1.SimulateMouseMove();
			Assert.IsFalse(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseEnter_MouseOverHotSpot_FiresMouseEnter()
		{
			var mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			Assert.IsFalse(mouseEnterFired);

			_spot1.SimulateMouseMove();
			Assert.IsTrue(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseEnter_MouseNotOverHotSpot_DoesNotFireMouseEnter()
		{
			var mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			Assert.IsFalse(mouseEnterFired);

			_textBox.SimulateMouseMove(0, 0);
			Assert.IsFalse(mouseEnterFired);
		}

		[Test]
		public void
			HotSpotMouseEnter_MouseOverHotSpotThenOffControlThenBackOverSameHotSpot_FiresMouseEnter()
		{
			var mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			_spot1.SimulateMouseMove();
			mouseEnterFired = false;

			_textBox.SimulateMouseMove(-10, -10); // Move off control
			Assert.IsFalse(mouseEnterFired);

			_spot1.SimulateMouseMove();
			Assert.IsTrue(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseLeave_MouseOverHotSpotThenAway_FiresMouseLeave()
		{
			var mouseLeaveFired = false;
			_spot1.MouseLeave += delegate { mouseLeaveFired = true; };

			Assert.IsFalse(mouseLeaveFired);

			_spot1.SimulateMouseMove();
			Assert.IsFalse(mouseLeaveFired);

			_textBox.SimulateMouseMove(0, 0); // Move off hot spot
			Assert.IsTrue(mouseLeaveFired);
		}

		[Test]
		public void HotSpotMouseMove_MouseOverHotSpot_FiresMouseMove()
		{
			var mouseMoveFired = false;
			_spot1.MouseMove += delegate { mouseMoveFired = true; };

			_spot1.SimulateMouseMove();

			Assert.IsTrue(mouseMoveFired);
		}

		[Test]
		public void HotSpotMouseMove_MouseNotOverHotSpot_DoesNotFireMouseMove()
		{
			var mouseMoveFired = false;
			_spot1.MouseMove += delegate { mouseMoveFired = true; };

			_textBox.SimulateMouseMove(new Point(0, 0));

			Assert.IsFalse(mouseMoveFired);
		}

		[Test]
		public void HotSpotMouseHover_MouseHoverOverHotSpot_FiresMouseHover()
		{
			var mouseHoverFired = false;
			_spot1.MouseHover += delegate { mouseHoverFired = true; };

			_spot1.SimulateMouseMove();

			_textBox.SimulateMouseHover();
			Assert.IsTrue(mouseHoverFired);
		}

		[Test]
		public void HotSpotMouseHover_MouseHoverNotOverHotSpot_DoesNotFireMouseHover()
		{
			var mouseHoverFired = false;
			_spot1.MouseHover += delegate { mouseHoverFired = true; };

			_textBox.SimulateMouseMove(new Point(0, 0));

			_textBox.SimulateMouseHover();
			Assert.IsFalse(mouseHoverFired);
		}

		[Test]
		public void HotSpotMouseDown_MouseDownOverHotSpot_FiresMouseDown()
		{
			var mouseDownFired = false;
			_spot1.MouseDown += delegate { mouseDownFired = true; };

			_spot1.SimulateMouseMove();

			_spot1.SimulateMouseDown();
			Assert.IsTrue(mouseDownFired);
		}

		[Test]
		public void HotSpotMouseClick_MouseClickOverHotSpot_FiresMouseClick()
		{
			var mouseClickFired = false;
			_spot1.MouseClick += delegate { mouseClickFired = true; };

			_spot1.SimulateMouseMove();

			_spot1.SimulateMouseClick();
			Assert.IsTrue(mouseClickFired);
		}

		[Test]
		public void HotSpotMouseUp_MouseUpOverHotSpot_FiresMouseUp()
		{
			var mouseUpFired = false;
			_spot1.MouseUp += delegate { mouseUpFired = true; };

			// Get the position of the hotspot
			var hotSpotPosition = _textBox.GetPositionFromCharIndex(_spot1.Offset);

			// Simulate moving the mouse over the hot spot
			_textBox.SimulateMouseMove(hotSpotPosition.X, hotSpotPosition.Y);

			// Simulate mouse up event
			_textBox.SimulateMouseUp(hotSpotPosition.X, hotSpotPosition.Y);

			// Verify that the MouseUp event was fired
			Assert.IsTrue(mouseUpFired);
		}

		[Test]
		public void HotSpotMouseUp_MouseUpNotOverHotSpot_DoesNotFireMouseUp()
		{
			bool mouseUpFired = false;
			_spot1.MouseUp += delegate { mouseUpFired = true; };

			_textBox.SimulateMouseMove(0, 0);
			_textBox.SimulateMouseUp(0, 0);
			Assert.IsFalse(mouseUpFired);
		}
	}
}