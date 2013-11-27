using System;

namespace Palaso.Data
{
	/// <summary>
	/// Pseudo precise in that the ticks value returned does not actually
	/// conform to the definition of Ticks but will be strictly increasing
	/// each time it is called.
	/// </summary>
	public static class PreciseDateTime
	{
		private static DateTime _lastDateTime;
		private static long _pseudoTicks;

		public static DateTime UtcNow
		{
			get
			{
				// The resolution of the UtcNow property depends on the system timer.
				// Windows NT ~ 10ms (actual tests show more like ~150-200ms)

				DateTime dt = DateTime.UtcNow;
				if (dt.Ticks == _lastDateTime.Ticks)
				{
					_pseudoTicks += 1;
					dt = dt.AddTicks(_pseudoTicks);
				}
				else if (dt.Ticks <= _lastDateTime.Ticks + _pseudoTicks)
				{
					_pseudoTicks -= dt.Ticks - _lastDateTime.Ticks - 1;
					_lastDateTime = dt;
					dt = dt.AddTicks(_pseudoTicks);
				}
				else
				{
					_lastDateTime = dt;
					_pseudoTicks = 0;
				}
				return dt;
			}
		}
	}
}