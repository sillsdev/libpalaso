using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.Windows.Forms.HotSpot;

namespace SIL.Windows.Forms.Tests.Hotspot
{
	[TestFixture]
	[Category("MouseSensitive")] // do not move your mouse with these tests
	[Category("SkipOnTeamCity")]
	public class HotSpotProviderTestForms
	{
		private class ClickableTextBox : TextBox
		{
			public new void OnMouseClick(MouseEventArgs e)
			{
				base.OnMouseClick(e);
			}

			public new void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
			}

			public new void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);
			}
		}

		private HotSpotProvider _hotSpotProvider;
		private ClickableTextBox _textBox;
		private HotSpot.HotSpot _spot1;
		private HotSpot.HotSpot _spot2;
		private Point _originalCursorPosition;
		private Form _form;

		[SetUp]
		public void Setup()
		{
			_hotSpotProvider = new HotSpotProvider();
			_textBox = new ClickableTextBox();
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
			bool mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			_hotSpotProvider.SetEnableHotSpots(_textBox, false);

			Application.DoEvents();
			Assert.IsFalse(mouseEnterFired);

			MoveMouseSoOverHotSpot(_spot1);
			Assert.IsFalse(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseEnter_MouseOverHotSpot_FiresMouseEnter()
		{
			bool mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			Application.DoEvents();
			Assert.IsFalse(mouseEnterFired);

			MoveMouseSoOverHotSpot(_spot1);
			Assert.IsTrue(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseEnter_MouseNotOverHotSpot_DoesNotFireMouseEnter()
		{
			bool mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			Application.DoEvents();

			Assert.IsFalse(mouseEnterFired);
			MoveMouseSoNotOverHotSpot();
			Assert.IsFalse(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseEnter_MouseOverHotSpotThenOffControlThenBackOverSameHotSpot_FiresMouseEnter()
		{
			bool mouseEnterFired = false;
			_spot1.MouseEnter += delegate { mouseEnterFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);
			mouseEnterFired = false;

			MoveMouseOffControl();

			Assert.IsFalse(mouseEnterFired);

			MoveMouseSoOverHotSpot(_spot1);

			Assert.IsTrue(mouseEnterFired);
		}

		[Test]
		public void HotSpotMouseLeave_MouseOverHotSpotThenAway_FiresMouseLeave()
		{
			bool mouseLeaveFired = false;
			_spot1.MouseLeave += delegate { mouseLeaveFired = true; };

			Application.DoEvents();
			Assert.IsFalse(mouseLeaveFired);

			MoveMouseSoOverHotSpot(_spot1);
			Assert.IsFalse(mouseLeaveFired);

			// mouse off hot spot still over control
			MoveMouseSoNotOverHotSpot();

			Assert.IsTrue(mouseLeaveFired);
		}

		[Test]
		public void HotSpotMouseLeave_MouseOverHotSpotThenOffControl_FiresMouseLeave()
		{
			bool mouseLeaveFired = false;
			_spot1.MouseLeave += delegate { mouseLeaveFired = true; };

			Application.DoEvents();
			Assert.IsFalse(mouseLeaveFired);

			MoveMouseSoOverHotSpot(_spot1);
			Assert.IsFalse(mouseLeaveFired);

			MoveMouseOffControl();

			Assert.IsTrue(mouseLeaveFired);
		}

		[Test]
		public void HotSpotMouseLeave_MouseNotOverHotSpotThenOffControl_DoesNotFireMouseLeave()
		{
			bool mouseLeaveFired = false;
			_spot1.MouseLeave += delegate { mouseLeaveFired = true; };

			Application.DoEvents();

			Assert.IsFalse(mouseLeaveFired);

			MoveMouseSoNotOverHotSpot();
			Assert.IsFalse(mouseLeaveFired);

			MoveMouseOffControl();

			Assert.IsFalse(mouseLeaveFired);
		}

		[Test]
		public void HotSpotMouseMove_MouseOverHotSpot_FiresMouseMove()
		{
			bool mouseMoveFired = false;
			_spot1.MouseMove += delegate { mouseMoveFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);

			Assert.IsTrue(mouseMoveFired);
		}

		[Test]
		public void HotSpotMouseMove_MouseNotOverHotSpot_DoesNotFireMouseMove()
		{
			bool mouseMoveFired = false;
			_spot1.MouseMove += delegate { mouseMoveFired = true; };

			Application.DoEvents();

			MoveMouseSoNotOverHotSpot();

			Assert.IsFalse(mouseMoveFired);
		}

		[Test]
		public void HotSpotMouseMove_MouseOverHotSpotThenMoveOverHotSpot_FiresMouseMove()
		{
			bool mouseMoveFired = false;
			_spot1.MouseMove += delegate { mouseMoveFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);
			mouseMoveFired = false;

			// move over hot spot
			Point position = Cursor.Position;
			position.Offset(1, 1);
			Cursor.Position = position;
			Application.DoEvents();

			Assert.IsTrue(mouseMoveFired);
		}

		[Test]
		public void HotSpotMouseHover_MouseHoverOverHotSpot_FiresMouseHover()
		{
			bool mouseHoverFired = false;
			_spot1.MouseHover += delegate { mouseHoverFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);
			Hover();
			Assert.IsTrue(mouseHoverFired);
		}

		[Test]
		public void HotSpotMouseHover_MouseHoverNotOverHotSpot_DoesNotFireMouseHover()
		{
			bool mouseHoverFired = false;
			_spot1.MouseHover += delegate { mouseHoverFired = true; };

			Application.DoEvents();

			MoveMouseSoNotOverHotSpot();
			Hover();
			Assert.IsFalse(mouseHoverFired);
		}

		[Test]
		public void HotSpotMouseDown_MouseDownOverHotSpot_FiresMouseDown()
		{
			bool mouseDownFired = false;
			_spot1.MouseDown += delegate { mouseDownFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);

			MouseDown();
			Assert.IsTrue(mouseDownFired);
		}

		[Test]
		public void HotSpotMouseDown_MouseDownNotOverHotSpot_DoesNotFireMouseDown()
		{
			bool mouseDownFired = false;
			_spot1.MouseDown += delegate { mouseDownFired = true; };

			Application.DoEvents();

			MoveMouseSoNotOverHotSpot();
			MouseDown();
			Assert.IsFalse(mouseDownFired);
		}

		[Test]
		public void HotSpotMouseClick_MouseClickOverHotSpot_FiresMouseClick()
		{
			bool mouseClickFired = false;
			_spot1.MouseClick += delegate { mouseClickFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);
			MouseClick();
			Assert.IsTrue(mouseClickFired);
		}

		[Test]
		public void HotSpotMouseClick_MouseClickNotOverHotSpot_DoesNotFireMouseClick()
		{
			bool mouseClickFired = false;
			_spot1.MouseClick += delegate { mouseClickFired = true; };

			Application.DoEvents();

			MoveMouseSoNotOverHotSpot();
			MouseClick();
			Assert.IsFalse(mouseClickFired);
		}

		[Test]
		public void HotSpotMouseUp_MouseUpOverHotSpot_FiresMouseUp()
		{
			bool mouseUpFired = false;
			_spot1.MouseUp += delegate { mouseUpFired = true; };

			Application.DoEvents();

			MoveMouseSoOverHotSpot(_spot1);
			MouseUp();
			Assert.IsTrue(mouseUpFired);
		}

		[Test]
		public void HotSpotMouseUp_MouseUpNotOverHotSpot_DoesNotFireMouseUp()
		{
			bool mouseUpFired = false;
			_spot1.MouseUp += delegate { mouseUpFired = true; };

			Application.DoEvents();

			MoveMouseSoNotOverHotSpot();
			MouseUp();
			Assert.IsFalse(mouseUpFired);
		}

		private void MouseDown()
		{
			Point position = _textBox.PointToClient(Cursor.Position);
			_textBox.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1,
													position.X,
													position.Y,
													0));
			Application.DoEvents();
		}

		private void MouseUp()
		{
			Point position = _textBox.PointToClient(Cursor.Position);
			_textBox.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1,
												  position.X,
												  position.Y,
												  0));
			Application.DoEvents();
		}


		private void MouseClick()
		{
			Point position = _textBox.PointToClient(Cursor.Position);
			_textBox.OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1,
													 position.X,
													 position.Y,
													 0));
			Application.DoEvents();
		}


		private static void Hover()
		{
			Thread.Sleep(SystemInformation.MouseHoverTime);
			Thread.Sleep(100);
			Application.DoEvents();
		}


		private void MoveMouseSoNotOverHotSpot()
		{
			MoveMouseToPositionAtCharIndex(0);
			Application.DoEvents();
		}

		private void MoveMouseSoOverHotSpot(HotSpot.HotSpot hotSpot)
		{
			MoveMouseToPositionAtCharIndex(hotSpot.Offset);
			Application.DoEvents();
		}

		private void MoveMouseOffControl()
		{
			// mouse off control
			// (to get the _textBox's OnMouseLeave to fire, we have to put the mouse in the non-client area of the control)
			Cursor.Position = _form.PointToScreen(_textBox.Location);
			Application.DoEvents();
		}

		private void MoveMouseToPositionAtCharIndex(int offset)
		{
			Point position = GetPositionFromCharIndex(_textBox, offset);
			Cursor.Position = _textBox.PointToScreen(position);
		}

		private static Point GetPositionFromCharIndex(TextBoxBase textBox, int offset)
		{
			Point position = textBox.GetPositionFromCharIndex(offset);
			// the following is necessary to work around a .Net bug
			int x = (Int16) position.X;
			int y = (Int16) position.Y;
			return new Point(x, y);
		}
	}
}