using System;

namespace Palaso.Misc
{
	public class GuardAgainstReentry : IDisposable
	{
		private int _entryCount;

		public GuardAgainstReentry()
		{
			_entryCount = 0;
			EnterExpected();
		}

		public void EnterNotExpected()
		{
			_entryCount++;
			if (_entryCount > 1)
			{
				throw new ApplicationException("Function reentry unexpected");
			}

		}

		public bool HasEntered
		{
			get { return _entryCount > 1; }
		}

		public void Dispose()
		{
			_entryCount--;
		}

		public void EnterExpected()
		{
			_entryCount++;
		}
	}
}