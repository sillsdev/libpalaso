///////////////////////////////////////////////////////////////////////////////
//  From http://www.codeproject.com/KB/audio-video/PeakMeterCS.aspx
//  Copyright (c) 2008 Ernest Laurentin (elaurentin@netzero.net)
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source distribution.
///////////////////////////////////////////////////////////////////////////////
//
//
// Subsequent additions/modifications:
// hatton: Added PeakLevel property, to simplify wiring to audio recorders
// So, to use, simply drag this on the form, then call PeakLevel occasionally.
// bogle: Changed to use Windows.Forms.Timer to avoid deadlock.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Media.Naudio.UI
{
	public enum PeakMeterStyle
	{
		PMS_Horizontal = 0,
		PMS_Vertical   = 1
	}

	internal struct PeakMeterData
	{
		public int Value;
		public int Falloff;
		public int Speed;
	}

	[ToolboxBitmap(typeof(MyResourceNamespace), "PeakMeterCtrl.pmicon.bmp")]
	public partial class PeakMeterCtrl : Control
	{
		private const byte DarkenByDefault = 40;
		private const byte LightenByDefault = 150;
		private const int MinRangeDefault = 60;
		private const int MedRangeDefault = 80;
		private const int MaxRangeDefault = 100;
		private const int FalloffFast = 1;
		private const int FalloffNormal = 10;
		private const int FalloffSlow = 100;
		private const int FalloffDefault = 10;
		private const int DecrementPercent = 10;
		private const int BandsMin = 1;
		private const int BandsMax = 1000;
		private const int BandsDefault = 8;
		private const int LEDMin  = 1;
		private const int LEDMax  = 1000;
		private const int LEDDefault = 8;
		private const int cxyMargin = 1;

		private int _MinRangeValue;
		private int _MedRangeValue;
		private int _MaxRangeValue;
		private PeakMeterData[] _meterData;
		private System.Windows.Forms.Timer _animationTimer;
		public PeakMeterCtrl()
		{
			InitializeComponent();
			InitDefault();
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			//this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}

		/// <summary>
		/// the client needs to set this occasionally, for this control to be of any use. Value is between 0 and 1.0
		/// </summary>
		public float PeakLevel
		{
			set { SetData(new int[] { (int)(value * 100.0) }, 0, 1); }
		}


		private void InitDefault()
		{
			_MinRangeValue = MinRangeDefault; // [0,60[
			_MedRangeValue = MedRangeDefault; // [60,80[
			_MaxRangeValue = MaxRangeDefault; // [80,100[
			_meterData = null;
			_animationTimer = null;
			_ShowGrid = true;
			_ColoredGrid = false;
			_GridColor = Color.Gainsboro;
			_ColorNormal = Color.Green;
			_ColorMedium = Color.Yellow;
			_ColorHigh = Color.Red;
			_ColorNormalBack = LightenColor(_ColorNormal, LightenByDefault);
			_ColorMediumBack = LightenColor(_ColorMedium, LightenByDefault);
			_ColorHighBack = LightenColor(_ColorHigh, LightenByDefault);
			_BandsCount = BandsDefault;
			_LEDCount = LEDDefault;
			_FalloffSpeed = FalloffNormal;
			_FalloffEffect = true;
			_FalloffColor = DarkenColor(_GridColor, DarkenByDefault);
			ResetControl();
		}

		#region Control properties
		private PeakMeterStyle _PmsMeterStyle;
		[Category("Appearance"), DefaultValue(PeakMeterStyle.PMS_Horizontal)]
		public PeakMeterStyle MeterStyle
		{
			get { return _PmsMeterStyle; }
			set { _PmsMeterStyle = value; Refresh(); }
		}

		private bool _ShowGrid;
		[Category("Appearance"), DefaultValue(true)]
		public bool ShowGrid
		{
			get { return _ShowGrid; }
			set { _ShowGrid = value; Refresh(); }
		}

		private bool _ColoredGrid;
		[Category("Appearance"), DefaultValue(false)]
		public bool ColoredGrid
		{
			get { return _ColoredGrid; }
			set { _ColoredGrid = value; Refresh(); }
		}

		private Color _GridColor;
		[Category("Appearance")]
		public Color GridColor
		{
			get { return _GridColor; }
			set { _GridColor = value; Refresh(); }
		}

		private Color _ColorNormal;
		[Category("Appearance")]
		public Color ColorNormal
		{
			get { return _ColorNormal; }
			set { _ColorNormal = value; Refresh(); }
		}

		private Color _ColorMedium;
		[Category("Appearance")]
		public Color ColorMedium
		{
			get { return _ColorMedium; }
			set { _ColorMedium = value; Refresh(); }
		}

		private Color _ColorHigh;
		[Category("Appearance")]
		public Color ColorHigh
		{
			get { return _ColorHigh; }
			set { _ColorHigh = value;  Refresh(); }
		}

		private Color _ColorNormalBack;
		[Category("Appearance")]
		public Color ColorNormalBack
		{
			get { return _ColorNormalBack; }
			set { _ColorNormalBack = value; Refresh(); }
		}

		private Color _ColorMediumBack;
		[Category("Appearance")]
		public Color ColorMediumBack
		{
			get { return _ColorMediumBack; }
			set { _ColorMediumBack = value; Refresh(); }
		}

		private Color _ColorHighBack;
		[Category("Appearance")]
		public Color ColorHighBack
		{
			get { return _ColorHighBack; }
			set { _ColorHighBack = value; Refresh(); }
		}

		private int _BandsCount;
		[Category("Appearance"), DefaultValue(BandsDefault)]
		public int BandsCount
		{
			get { return _BandsCount; }
			set {
				if (value >= BandsMin && value <= BandsMax)
				{
					_BandsCount = value;
					ResetControl();
					Refresh();
				}
			}
		}

		private int _LEDCount;
		[Category("Appearance"), DefaultValue(LEDDefault)]
		public int LEDCount
		{
			get { return _LEDCount; }
			set {
				if (value >= LEDMin && value <= LEDMax)
				{
					_LEDCount = value;
					Refresh();
				}
			}
		}

		private int _FalloffSpeed;
		[Category("Falloff Effect"), DefaultValue(FalloffDefault)]
		public int FalloffSpeed
		{
			get { return _FalloffSpeed; }
			set { _FalloffSpeed = value; }
		}

		private bool _FalloffEffect;
		[Category("Falloff Effect"), DefaultValue(true)]
		public bool FalloffEffect
		{
			get { return _FalloffEffect; }
			set { _FalloffEffect = value; }
		}

		private Color _FalloffColor;
		[Category("Falloff Effect")]
		public Color FalloffColor
		{
			get { return _FalloffColor; }
			set { _FalloffColor = value; }
		}

		#endregion

		[Browsable(false)]
		public bool IsActive
		{
			get { return (_animationTimer != null && _animationTimer.Enabled); }
		}

		#region Control Methods

		/// <summary>
		/// Start animation
		/// </summary>
		/// <param name="delay"></param>
		/// <returns></returns>
		public void Start(int delay)
		{
			if (InvokeRequired)
			{
				Invoke((Action<int>)Start, delay);
				return;
			}
			StartAnimation(delay);
		}

		/// <summary>
		/// Stop Animation
		/// </summary>
		/// <returns></returns>
		public void Stop()
		{
			if (InvokeRequired)
			{
				Invoke((Action)Stop);
				return;
			}
			StopAnimation();
		}

		/// <summary>
		/// Set number of LED bands
		/// </summary>
		/// <param name="BandsCount">Number of bands</param>
		/// <param name="LEDCount">Number of LED per bands</param>
		public void SetMeterBands(int BandsCount, int LEDCount)
		{
			if (BandsCount < BandsMin || BandsCount > BandsMax)
				throw new ArgumentOutOfRangeException("BandsCount");
			if (LEDCount < LEDMin || LEDCount > LEDMax)
				throw new ArgumentOutOfRangeException("LEDCount");
			_BandsCount = BandsCount;
			_LEDCount = LEDCount;
			ResetControl();
			Refresh();
		}

		/// <summary>
		/// Set range info
		/// </summary>
		/// <param name="minRangeVal">Min Range</param>
		/// <param name="medRangeVal">Medium Range</param>
		/// <param name="maxRangeVal">High Range</param>
		public void SetRange(int minRangeVal, int medRangeVal, int maxRangeVal)
		{
			if (maxRangeVal <= medRangeVal || medRangeVal < minRangeVal )
				throw new ArgumentOutOfRangeException("minRangeVal");
			_MinRangeValue = minRangeVal;
			_MedRangeValue = medRangeVal;
			_MaxRangeValue = maxRangeVal;
			ResetControl();
			Refresh();
		}

		/// <summary>
		/// Set meter band value
		/// </summary>
		/// <param name="arrayValue">Array value for the bands</param>
		/// <param name="offset">Starting offset position</param>
		/// <param name="size">Number of values to set</param>
		/// <returns></returns>
		public void SetData(int[] arrayValue, int offset, int size)
		{
			if (arrayValue == null)
				throw new ArgumentNullException("arrayValue");
			if (arrayValue.Length < (offset + size))
				throw new ArgumentOutOfRangeException("arrayValue");
			if (_animationTimer == null)
				throw new InvalidOperationException("Peak meter must have been started before setting data.");

			if (InvokeRequired)
			{
				BeginInvoke((Action<int[], int, int>)SetData, arrayValue, offset, size);
				return;
			}

			Monitor.Enter(this._meterData);

			int maxIndex = offset + size;
			for (int i = offset; i < maxIndex; i++)
			{
				if (i < this._meterData.Length)
				{
					PeakMeterData pm = this._meterData[i];
					pm.Value = Math.Min(arrayValue[i], this._MaxRangeValue);
					pm.Value = Math.Max(pm.Value, 0);
					if (pm.Falloff < pm.Value)
					{
						pm.Falloff = pm.Value;
						pm.Speed = this._FalloffSpeed;
					}
					_meterData[i] = pm;
				}
			}
			Monitor.Exit(_meterData);

			// check that timer should be restarted
			if (!_animationTimer.Enabled)
			{
				_animationTimer.Start();
			}
			else
			{
				Refresh();
			}
		}
		#endregion

		/// <summary>
		/// Make a color darker
		/// </summary>
		/// <param name="color">Color to darken</param>
		/// <param name="darkenBy">Value to decrease by</param>
		protected virtual Color DarkenColor(Color color, byte darkenBy)
		{
			byte red = (byte)(color.R > darkenBy ? (color.R - darkenBy) : 0);
			byte green = (byte)(color.G > darkenBy ? (color.G - darkenBy) : 0);
			byte blue = (byte)(color.B > darkenBy ? (color.B - darkenBy) : 0);
			return Color.FromArgb(red, green, blue);
		}
		/// <summary>
		/// Make a color lighter
		/// </summary>
		/// <param name="color"></param>
		/// <param name="lightenBy"></param>
		/// <returns></returns>
		protected virtual Color LightenColor(Color color, byte lightenBy)
		{
			byte red = (byte)((color.R + lightenBy) <= 255 ? (color.R + lightenBy) : 255);
			byte green = (byte)((color.G + lightenBy) <= 255 ? (color.G + lightenBy) : 255);
			byte blue = (byte)((color.B + lightenBy) <= 255 ? (color.B + lightenBy) : 255);
			return Color.FromArgb(red, green, blue);
		}

		protected static bool InRange(int value, int rangeMin, int rangeMax)
		{
			return (value >= rangeMin && value <= rangeMax);
		}

		protected void ResetControl()
		{
			_meterData = new PeakMeterData[_BandsCount];
			PeakMeterData pm;
			pm.Value = _MaxRangeValue;
			pm.Falloff = _MaxRangeValue;
			pm.Speed = _FalloffSpeed;
			for (int i = 0; i < _meterData.Length; i++)
			{
				_meterData[i] = pm;
			}
		}
		protected void StartAnimation(int period)
		{
			if (!IsActive)
			{
				_animationTimer = new System.Windows.Forms.Timer();
				_animationTimer.Tick += delegate { TimerCallback(); };
			}
			_animationTimer.Interval = period;
			_animationTimer.Start();
		}

		protected void StopAnimation()
		{
			if (IsActive)
				_animationTimer.Stop();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			Stop();
			base.OnHandleDestroyed(e);
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			Refresh();
		}

		protected void TimerCallback()
		{
			try
			{
				if (IsHandleCreated)
					BeginInvoke(new MethodInvoker(Refresh));
				else
					return;
			}
			catch { }

			int nDecValue = _MaxRangeValue / _LEDCount;
			bool noUpdate = true;

			Monitor.Enter(_meterData);
			for (int i = 0; i < _meterData.Length; i++)
			{
				PeakMeterData pm = _meterData[i];

				if (pm.Value > 0)
				{
					pm.Value -= (_LEDCount > 1 ? nDecValue : (_MaxRangeValue * DecrementPercent) / 100);
					if (pm.Value < 0)
						pm.Value = 0;
					noUpdate = false;
				}

				if (pm.Speed > 0)
				{
					pm.Speed -= 1;
					noUpdate = false;
				}

				if (pm.Speed == 0 && pm.Falloff > 0)
				{
					pm.Falloff -= (_LEDCount > 1 ? nDecValue >> 1 : 5);
					if (pm.Falloff < 0)
						pm.Falloff = 0;
					noUpdate = false;
				}

				// re-assign PeakMeterData
				_meterData[i] = pm;
			}

			Monitor.Exit(_meterData);

			if (noUpdate) // Stop timer if no more data but do not reset ID
				StopAnimation();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			// Calling the base class OnPaint
			base.OnPaint(e);

			var rc = ClientRectangle;

			using (var br = new SolidBrush(BackColor))
			{
				e.Graphics.FillRectangle(br, rc);

				if (MeterStyle == PeakMeterStyle.PMS_Horizontal)
					DrawHorzBand(e.Graphics);
				else
					DrawVertBand(e.Graphics);
			}
		}

		protected void DrawHorzBand(Graphics g)
		{
			int nMaxRange = (_MedRangeValue == 0) ? Math.Abs(_MaxRangeValue - _MinRangeValue) : _MaxRangeValue;
			int nVertBands = (_LEDCount > 1 ? _LEDCount : (nMaxRange * DecrementPercent) / 100);
			int nMinVertLimit = _MinRangeValue * nVertBands / nMaxRange;
			int nMedVertLimit = _MedRangeValue * nVertBands / nMaxRange;
			int nMaxVertLimit = nVertBands;

			if (_MedRangeValue == 0)
			{
				nMedVertLimit = Math.Abs(_MinRangeValue) * nVertBands / nMaxRange;
				nMinVertLimit = 0;
			}

			var rc = ClientRectangle;
			var bandSize = new Size(rc.Width / _BandsCount, rc.Height / nVertBands);
			var bandRect = new Rectangle(rc.Location, bandSize);

			// Draw band from bottom
			bandRect.Offset(0, rc.Height - bandSize.Height);
			int xDecal = (_BandsCount > 1 ? cxyMargin : 0);
			//int yDecal = 0;

			var gridPen = new Pen(ShowGrid ? GridColor : BackColor);
			var fallPen = new Pen(FalloffColor);

			for (int band = 0; band < _BandsCount; band++)
			{
				int nValue = _meterData[band].Value;
				int nVertValue = nValue * nVertBands / nMaxRange;
				var prevBandRect = bandRect;

				for (int nVert = 0; nVert < nVertBands; nVert++)
				{
					// Find color based on range value
					var bandColor = gridPen.Color;

					// Draw grid line (level) bar
					if (ShowGrid && (nVert == nMinVertLimit || nVert == nMedVertLimit || nVert == (nVertBands-1)))
					{
						Point []points = new Point[2];
						points[0].X = bandRect.Left;
						points[0].Y = bandRect.Top + (bandRect.Height>>1);
						points[1].X = bandRect.Right;
						points[1].Y = points[0].Y;
						g.DrawPolygon(gridPen, points);
					}

					if (_MedRangeValue == 0)
					{
						int nVertStart = nMedVertLimit+nVertValue;
						if ( InRange(nVert, nVertStart, nMedVertLimit-1) )
							bandColor = ColorNormal;
						else if (nVert >= nMedVertLimit && InRange(nVert, nMedVertLimit, nVertStart))
							bandColor = ColorHigh;
						else
							bandColor = (nVert < nMedVertLimit) ? ColorNormalBack : ColorHighBack;
					}
					else if (nVertValue < nVert)
					{
						if (ShowGrid && ColoredGrid)
						{
							if (InRange(nVert, 0, nMinVertLimit))
								bandColor = ColorNormalBack;
							else if (InRange(nVert, nMinVertLimit+1, nMedVertLimit))
								bandColor = ColorMediumBack;
							else if (InRange(nVert, nMedVertLimit+1, nMaxVertLimit))
								bandColor = ColorHighBack;
						}
					}
					else
					{
						if (nValue == 0)
						{
							if (ShowGrid && ColoredGrid)
								bandColor = ColorNormalBack;
						}
						else if (InRange(nVert, 0, nMinVertLimit))
							bandColor = ColorNormal;
						else if (InRange(nVert, nMinVertLimit+1, nMedVertLimit))
							bandColor = ColorMedium;
						else if (InRange(nVert, nMedVertLimit+1, nMaxVertLimit))
							bandColor = ColorHigh;
					}

					if (bandColor != BackColor)
					{
						using (var fillBrush = new SolidBrush(bandColor))
						{
							if (_LEDCount > 1)
								bandRect.Inflate(-cxyMargin, -cxyMargin);

							g.FillRectangle(fillBrush, bandRect.Left, bandRect.Top, bandRect.Width + 1, bandRect.Height);

							if (_LEDCount > 1)
								bandRect.Inflate(cxyMargin, cxyMargin);
						}
					}

					bandRect.Offset(0, -bandSize.Height);
				}

				// Draw falloff effect
				if (FalloffEffect && IsActive)
				{
					int nMaxHeight = bandSize.Height*nVertBands;
					var points = new Point[2];
					points[0].X = prevBandRect.Left + xDecal;
					points[0].Y = prevBandRect.Bottom - (_meterData[band].Falloff * nMaxHeight) / _MaxRangeValue;
					points[1].X = prevBandRect.Right - xDecal;
					points[1].Y = points[0].Y;
					g.DrawPolygon(fallPen, points);
				}

				// Move to Next Horizontal band
				bandRect.Offset(bandSize.Width, bandSize.Height * nVertBands);
			}

			gridPen.Dispose();
			fallPen.Dispose();
		}

		protected void DrawVertBand(Graphics g)
		{
			int nMaxRange = (_MedRangeValue == 0) ? Math.Abs(_MaxRangeValue - _MinRangeValue) : _MaxRangeValue;
			int numberOfLeds = (_LEDCount > 1 ? _LEDCount : (nMaxRange * DecrementPercent) / 100);
			int nMinHorzLimit = _MinRangeValue * numberOfLeds/nMaxRange;
			int nMedHorzLimit = _MedRangeValue * numberOfLeds/nMaxRange;
			int nMaxHorzLimit = numberOfLeds;

			if ( _MedRangeValue == 0 )
			{
				nMedHorzLimit = Math.Abs(_MinRangeValue)*numberOfLeds/nMaxRange;
				nMinHorzLimit = 0;
			}

			var rc = ClientRectangle;
			var bandSize = new Size(rc.Width / numberOfLeds, rc.Height / _BandsCount);
			var bandRect = new Rectangle(rc.Location, bandSize);

			// Draw band from top
			bandRect.Offset(0, rc.Height - bandSize.Height * _BandsCount);
			//int xDecal = 0;
			int yDecal = (_BandsCount>1 ? cxyMargin : 0);

			var gridPen = new Pen(ShowGrid ? GridColor : BackColor);
			var fallPen = new Pen(FalloffColor );

			for (int band = 0; band < _BandsCount; band++)
			{
				int dataValue = _meterData[band].Value;
				int nHorzValue = dataValue * numberOfLeds / nMaxRange;
				var rcPrev = bandRect;

				for (int bands = 0; bands < numberOfLeds; bands++)
				{
					// Find color based on range value
					var colorBand = gridPen.Color;

					if (ShowGrid && (bands == nMinHorzLimit || bands == nMedHorzLimit || bands == (numberOfLeds-1)))
					{
						var points = new Point[2];
						points[0].X = bandRect.Left + (bandRect.Width >> 1);
						points[0].Y = bandRect.Top;
						points[1].X = points[0].X;
						points[1].Y = bandRect.Bottom;
						g.DrawPolygon(gridPen, points);
					}

					if (_MedRangeValue == 0)
					{
						int nHorzStart = nMedHorzLimit+nHorzValue;

						if (InRange(bands, nHorzStart, nMedHorzLimit - 1))
							colorBand = ColorNormal;
						else if ( bands >= nMedHorzLimit && InRange(bands, nMedHorzLimit, nHorzStart))
							colorBand = ColorHigh;
						else
							colorBand = (bands < nMedHorzLimit) ? ColorNormalBack : ColorHighBack;
					}
					else if (nHorzValue < bands)
					{
						if (ShowGrid && ColoredGrid)
						{
							if  (InRange(bands, 0, nMinHorzLimit))
								colorBand = ColorNormalBack;
							else if (InRange(bands, nMinHorzLimit + 1, nMedHorzLimit))
								colorBand = ColorMediumBack;
							else if (InRange(bands, nMedHorzLimit + 1, nMaxHorzLimit))
								colorBand = ColorHighBack;
						}
					}
					else
					{
						if (dataValue == 0)
						{
							if (ShowGrid && ColoredGrid)
								colorBand = ColorNormalBack;
						}
						else if (InRange(bands, 0, nMinHorzLimit))
							colorBand = ColorNormal;
						else if ( InRange(bands, nMinHorzLimit + 1, nMedHorzLimit) )
							colorBand = ColorMedium;
						else if ( InRange(bands, nMedHorzLimit + 1, nMaxHorzLimit) )
							colorBand = ColorHigh;
					}

					if (colorBand != BackColor)
					{
						using (var fillBrush = new SolidBrush(colorBand))
						{
							if (_LEDCount > 1)
								bandRect.Inflate(-cxyMargin, -cxyMargin);

							g.FillRectangle(fillBrush, bandRect.Left, bandRect.Top, bandRect.Width, bandRect.Height + 1);

							if (_LEDCount > 1)
								bandRect.Inflate(cxyMargin, cxyMargin);
						}
					}

					bandRect.Offset(bandSize.Width, 0);
				}

				// Draw falloff effect
				if (FalloffEffect && IsActive)
				{
					int nMaxWidth = bandSize.Width * numberOfLeds;
					var points = new Point[2];
					points[0].X = rcPrev.Left + (_meterData[band].Falloff * nMaxWidth) / _MaxRangeValue;
					points[0].Y = rcPrev.Top + yDecal;
					points[1].X = points[0].X;
					points[1].Y = rcPrev.Bottom - yDecal;
					g.DrawPolygon(fallPen, points);
				}

				// Move to Next Vertical band
				bandRect.Offset(-bandSize.Width * numberOfLeds, bandSize.Height);
			}

			gridPen.Dispose();
			fallPen.Dispose();
		}
	}

// use this to find resource namespace
	internal class MyResourceNamespace
	{
	}
}