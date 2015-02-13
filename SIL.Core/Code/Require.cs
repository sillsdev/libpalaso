using System;

namespace SIL.Code
{
	public static class Require
	{
		public static void That(bool assertion, string message)
		{
			if (assertion)
				return;
			throw new InvalidOperationException(message);
		}

		public static void That(bool assertion)
		{
			if(!assertion)
				throw new InvalidOperationException();
		}
	}
}