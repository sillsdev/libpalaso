using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WeSay.UI
{
	public class CubicBezierCurve
	{
		private PointF _origin;

		// Polynomial Coefficients
		private PointF _A;
		private PointF _B;
		private PointF _C;

		public CubicBezierCurve(PointF origin, PointF control1, PointF control2, PointF end)
		{
			CalculatePolynomialCoefficients(origin, control1, control2, end);
			_origin = origin;
		}

		private void CalculatePolynomialCoefficients(PointF origin,
													 PointF control1,
													 PointF control2,
													 PointF end)
		{
			_C.X = 3.0f * (control1.X - origin.X);
			_B.X = 3.0f * (control2.X - control1.X) - _C.X;
			_A.X = end.X - origin.X - _C.X - _B.X;

			_C.Y = 3.0f * (control1.Y - origin.Y);
			_B.Y = 3.0f * (control2.Y - control1.Y) - _C.Y;
			_A.Y = end.Y - origin.Y - _C.Y - _B.Y;
		}

		/// <summary>
		/// Get Point on curve at parameter t
		/// </summary>
		/// <param name="t">the interval (varies between 0 and 1)</param>
		/// <returns></returns>
		private PointF GetPointFromInterval(float t)
		{
			float t_t = t * t; // interval squarred
			float t_t_t = t_t * t;

			PointF result = new PointF();
			result.X = ((_A.X * t_t_t) + (_B.X * t_t) + (_C.X * t) + _origin.X);
			result.Y = ((_A.Y * t_t_t) + (_B.Y * t_t) + (_C.Y * t) + _origin.Y);

			return result;
		}

		/// <summary>
		/// Given a DistanceRatio (ArcLength to CurveLength) gives us the
		/// X,Y coordinates of the arc with that length
		/// </summary>
		public PointF GetPointOnCurve(float distanceRatio)
		{
			if (distanceRatio < 0 || distanceRatio > 1)
			{
				throw new ArgumentOutOfRangeException("distanceRatio",
													  distanceRatio,
													  "distanceRatio is a ratio and so must be between 0 and 1");
			}
			return GetPointFromInterval(distanceRatio);
		}
	}

	public class Animator
	{
		public class AnimatorEventArgs: EventArgs
		{
			private readonly PointF _point;

			public AnimatorEventArgs(PointF point)
			{
				_point = point;
			}

			public PointF Point
			{
				get { return _point; }
			}
		}

		/// <summary>
		/// Converts a ratio of time to a ratio of distance
		/// </summary>
		/// <param name="time">the ratio of time that has past
		/// to the total time (varies uniformly between 0 and 1)</param>
		/// <returns>the ratio of the distance traveled to the total
		/// distance (varies between 0 and 1</returns>
		public delegate float Speed(float time);

		/// <summary>
		/// Gets the proportion into space at which a given proportion of
		/// the distance has been traveled along a path from the origin
		/// </summary>
		/// <remarks>The space is defined by a start point with
		/// coordinates 0,0 and an end point with coordinates 1,1</remarks>
		/// <param name="distance">the proportion of the distance traveled
		/// to the total distance (varies between 0 and 1</param>
		/// <returns>The proportional point into space at which
		/// the distance has been traveled</returns>
		public delegate PointF PointFromDistance(float distance);

		public delegate void AnimateEventDelegate(object sender, AnimatorEventArgs e);

		public event AnimateEventDelegate Animate = delegate { };
		public event EventHandler Finished = delegate { };

		private int _duration;
		private Speed _speedFunction;
		private PointFromDistance _pointFromDistanceFunction;
		private int _elapsedTime;
		private DateTime _lastMeasuredTime;
		private readonly Timer _timer;

		#region YAGNI

		private readonly float _repeatCount; // greater than 0 or infinite

		#endregion

		public Animator()
		{
			Duration = 500;
			_repeatCount = 1;
			_speedFunction = SpeedFunctions.LinearSpeed;
			_pointFromDistanceFunction = PointFromDistanceFunctions.LinearPointFromDistance;
			_elapsedTime = 0;
			_timer = new Timer();
			_timer.Tick += Tick;
		}

		private void Tick(object sender, EventArgs e)
		{
			DateTime now = DateTime.Now;
			_elapsedTime += now.Subtract(_lastMeasuredTime).Milliseconds;
			_lastMeasuredTime = now;

			if (_elapsedTime > Duration * _repeatCount)
			{
				Stop();
				Finished(this, new EventArgs());
			}
			OnAnimate();
		}

		private void OnAnimate()
		{
			//convert time to distance
			int elapsedTime;
			Math.DivRem(_elapsedTime, Duration, out elapsedTime);

			PointF pointF = PointFromDistanceFunction(SpeedFunction(elapsedTime / (float) Duration));
			Animate(this, new AnimatorEventArgs(pointF));
		}

		/// <summary>
		/// Starts the timer from the place it last was stopped
		/// </summary>
		public void Start()
		{
			_lastMeasuredTime = DateTime.Now;
			_timer.Start();
		}

		/// <summary>
		/// Stops the timer (effectively a pause until Reset is called)
		/// </summary>
		public void Stop()
		{
			_timer.Stop();
		}

		/// <summary>
		/// Resets the timer to time 0
		/// </summary>
		public void Reset()
		{
			_elapsedTime = 0;
		}

		/// <summary>
		/// Duration in Miliseconds
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When negative value.</exception>
		public int Duration
		{
			get { return _duration; }
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_duration = value;
			}
		}

		/// <summary>
		/// Determines the rate at which the annimation travels along the curve.
		/// </summary>
		public Speed SpeedFunction
		{
			get { return _speedFunction; }
			set { _speedFunction = value; }
		}

		public PointFromDistance PointFromDistanceFunction
		{
			get { return _pointFromDistanceFunction; }
			set { _pointFromDistanceFunction = value; }
		}

		/// <summary>
		/// The number of frames per second
		/// </summary>
		public float FrameRate
		{
			get { return 1000 / _timer.Interval; }
			set
			{
				if (value > 1000 || value <= 0)
				{
					throw new ArgumentOutOfRangeException("value",
														  value,
														  "FrameRate must be between 1 and 1000, inclusive");
				}
				_timer.Interval = (int) (1000f / value);
			}
		}

		/// <summary>
		/// The duration of a single frame in milliseconds
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">When negative.</exception>
		public int FrameDuration
		{
			get { return _timer.Interval; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_timer.Interval = value;
			}
		}

		public class SpeedFunctions
		{
			public static float LinearSpeed(float t)
			{
				return t;
			}

			/// <summary>
			/// if input t varies uniformly between 0 and 1,
			/// output t' starts at 0 slowly increasing, gaining speed until
			/// the middle values and then decelerating at it approaches 1.
			/// </summary>
			/// <param name="t">varies uniformly between 0 and 1</param>
			/// <returns>a number which varies between 0 and 1</returns>
			public static float SinSpeed(float t)
			{
				return (float) (Math.Sin(t * Math.PI - Math.PI / 2f) + 1f) / 2f;
			}
		}

		public class PointFromDistanceFunctions
		{
			public static PointF LinearPointFromDistance(float distance)
			{
				return new PointF(distance, distance);
			}
		}

		public static int GetValue(float proportion, int from, int to)
		{
			return (int) ((to - from) * proportion) + from;
		}

		public static T GetValue<T>(float proportion, IList<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (list.Count < 1)
			{
				throw new ArgumentOutOfRangeException("list", list, "list is empty");
			}
			int index = (int) (list.Count * proportion);
			return list[index];
		}
	}
}