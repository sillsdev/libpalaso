using System;
using System.Drawing;

namespace SIL.Windows.Forms.ImageToolbox.Cropping
{
	internal class Grip
	{
		internal Rectangle Rectangle;

		internal enum Sides  {Left, Right, Top, Bottom};

		internal Grip(int value, int width, int height, Sides side, System.Func<int> getCenterValue, System.Func<int> getMinValue , System.Func<int> getMaxValue)
		{
			Side = side;
			_getCenterValue = getCenterValue;
			_getMinValue = getMinValue;
			_getMaxValue = getMaxValue;
			/*not VALUE property!*/_value = value;
			Rectangle = new Rectangle(value, value, width, height);
		}

		internal Sides Side;

		//these are functions becuase they are dependent on the location of other grips
		private readonly System.Func<int> _getCenterValue;
		private readonly System.Func<int> _getMinValue;
		private readonly System.Func<int> _getMaxValue;
		private int _value;

		internal int Width
		{
			get { return Rectangle.Width; }
		}
		internal int Height
		{
			get { return Rectangle.Height; }
		}

		internal Point Location
		{
			get { return Rectangle.Location; }
			set { Rectangle.Location = value; }
		}

		internal int Top
		{
			get { return Rectangle.Top; }
		}

		internal int Bottom
		{
			get { return Rectangle.Bottom; }
		}

		internal int Right
		{
			get { return Rectangle.Right; }

		}
		internal int Left
		{
			get { return Rectangle.Left; }

		}

		internal bool MovesVertically { get { return Rectangle.Width > Rectangle.Height; } }

		internal int Value
		{
			get
			{
				return _value;
			}

			set
			{
				_value = Math.Max(_getMinValue(),value);
				_value = Math.Min(_value, _getMaxValue());
				UpdateRectangle();
			}
		}

		internal int InnerEdge
		{
			get
			{
				switch (Side)
				{
					case Sides.Left:
						return Value + Width;
					case Sides.Right:
						return Value+Width;
					case Sides.Top:
						return Value+Height;
					case Sides.Bottom:
						return Value+Height;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		internal void UpdateRectangle()
		{
			switch (Side)
			{
				case Sides.Left:
					var top = _getCenterValue()  - (Height / 2);
					Location = new Point(_value, top);//at value 0, our left is at 0 (and it's the right edge of the affordance which counts)
					break;
				case Sides.Right:
					var left = _getCenterValue()  - (Height / 2);
					Location = new Point(_value + Width, left);
					break;
				case Sides.Top:
										var top2 = _getCenterValue()  - (Width / 2);
										Location = new Point(top2, _value);//at value 0, out bottom is at 0+height, but our top is at 0
					break;
				case Sides.Bottom:
					var left2 = _getCenterValue() - (Width / 2);
					Location = new Point(left2, _value + Height);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}



		internal bool Contains(Point location)
		{
			return Rectangle.Contains(location);
		}

		const int GripDotSpacing = 3;
		internal void Paint(Graphics g)
		{
//            int rows = Rectangle.Height / GripDotSpacing;
//            for (int row = 0; row < rows; row += 2)
//            {
//                DrawOneRowGrip(g, Rectangle, row, 0);
//                DrawOneRowGrip(g, Rectangle, row + 1, 2);
//            }

			g.FillRectangle(Brushes.Gray, Rectangle);
		}

		private void DrawOneRowGrip(Graphics graphics, Rectangle r, int row, int offset)
		{
			using var controlLightPen = new Pen(SystemColors.ControlLightLight);
			using var controlDarkPen = new Pen(SystemColors.ControlDark);
			for (int column = 0; column < r.Width; column += 4)
			{
				// light dot
				graphics.DrawRectangle(controlLightPen,
									   r.X + offset + column,
									   r.Y + 1 + (row * GripDotSpacing),
									   1,
									   1);
				// dark dot
				graphics.DrawRectangle(controlDarkPen,
									   r.X + offset + column - 1,
									   r.Y + (row * GripDotSpacing),
									   1,
									   1);
			}
		}


	}
}