using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	public class HorizontalSpinner : Control
	{
		#region Public Constructor, Enums
		public HorizontalSpinner()
		{
			Height = 22;
			Width = 25;
		}

		private enum ArrowDirection { Left, Right };

		private enum SpinnerState { Normal, Pushed, Hot };
		#endregion

		#region Private Fields
		private readonly Color arrowColor = SystemColors.ControlText;
		private readonly Color hoverColor = SystemColors.ControlText;

		// internal state
		SpinnerState leftState = SpinnerState.Normal;
		SpinnerState rightState = SpinnerState.Normal;
		// Drawing Coordinates and Areas
		Rectangle leftBounds;
		Rectangle rightBounds;
		// Event delegates
		public event EventHandler Decremented;
		public event EventHandler Incremented;

		#endregion

		#region Properties
		private SpinnerState LeftState
		{
			set
			{
				if (value == leftState)
					return;
				leftState = value;
				//Invalidate(); //Trying Refresh to address PT-31. Refresh() is stronger.
				Refresh();
			}
		}

		private SpinnerState RightState
		{
			set
			{
				if (value == rightState)
					return;
				rightState = value;
				Refresh();
			}
		}

		#endregion

		#region Control Events
		private void OnDecrement()
		{
			if (Decremented != null)
				Decremented(this, EventArgs.Empty);
		}

		private void OnIncrement()
		{
			if (Incremented != null)
				Incremented(this, EventArgs.Empty);
		}
		#endregion

		#region Owner Draw

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (e.ClipRectangle.Width == 0)
				return; //avoids certain design time errors

			CalculateCoordinates();
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			DrawButton(e, ArrowDirection.Right);
			DrawButton(e, ArrowDirection.Left);
		}

		private void CalculateCoordinates()
		{
			leftBounds = rightBounds = ClientRectangle;
			leftBounds.Width /= 2;
			rightBounds.Width -= leftBounds.Width;
			rightBounds.X = leftBounds.Width;
		}

		private void DrawButton(PaintEventArgs e, ArrowDirection direction)
		{
			Rectangle rect = (direction == ArrowDirection.Right) ? rightBounds : leftBounds;
			SpinnerState state = (direction == ArrowDirection.Right) ? rightState : leftState;
			// Draw Button
			ButtonState[] bs = { ButtonState.Normal, ButtonState.Pushed, ButtonState.Normal }; //style translation
			ControlPaint.DrawButton(e.Graphics, rect, bs[(int)state]); //translate style and draw
			// Draw Arrow and Possible Highlight
			DrawArrow(e, rect, direction, state);
		}

		private void DrawArrow(PaintEventArgs pe, Rectangle bounds, ArrowDirection direction, SpinnerState state)
		{
			const float scalingFactor = 0.3F;

			Color theColor = (state == SpinnerState.Hot) ? hoverColor : arrowColor;
			int facing = direction == ArrowDirection.Left ? 1 : -1;
			PointF center = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			// nudge center to account for button's 3-D optical illusion.
			center.Y -= 1; center.X -= facing;
			float arrowHeight = Math.Min(Width / 2.0f, Height) * scalingFactor;
			float halfHeight = arrowHeight / 2 * facing;
			// map out points for a simple right isosceles triangle
			PointF apex = center; apex.X -= halfHeight;
			PointF base1 = center; base1.X += halfHeight;
			PointF base2 = base1; base1.Y -= arrowHeight; base2.Y += arrowHeight;

			// draw the triangle
			using (Brush b = new SolidBrush(theColor))
			{
				pe.Graphics.FillPolygon(b, new[] { apex, base1, base2 });
			}
		}
		#endregion

		#region Wrap UI Events
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (leftBounds.Contains(e.Location))
				LeftState = SpinnerState.Pushed;
			else if (rightBounds.Contains(e.Location))
				RightState = SpinnerState.Pushed;

			if (leftBounds.Contains(e.Location))
			{
				if (RightToLeft == RightToLeft.No)
					OnDecrement();
				else
					OnIncrement();
			}
			else if (rightBounds.Contains(e.Location))
			{
				if (RightToLeft == RightToLeft.No)
					OnIncrement();
				else
					OnDecrement();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			LeftState = RightState = SpinnerState.Normal;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button != MouseButtons.None)
				return;

			LeftState = (leftBounds.Contains(e.Location)) ? SpinnerState.Hot : SpinnerState.Normal;
			RightState = (rightBounds.Contains(e.Location)) ? SpinnerState.Hot : SpinnerState.Normal;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			LeftState = RightState = SpinnerState.Normal;
		}
		#endregion
	}
}
