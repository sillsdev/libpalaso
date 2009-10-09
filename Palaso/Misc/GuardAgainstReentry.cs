using System;

namespace Palaso.Misc
{
	public class GuardAgainstReentry : IDisposable
	{

		public int EntryCount { get; private set; }

		public GuardAgainstReentry()
		{
			EntryCount = 0;
			EnterExpected();
		}

		public void EnterNotExpected()
		{
			EntryCount++;
			if (EntryCount > 1)
			{
				throw new ApplicationException("Function reentry unexpected");
			}

		}

		public bool HasEntered
		{
			get { return EntryCount > 1; }
		}

		public void Dispose()
		{
			EntryCount--;
		}

		public void EnterExpected()
		{
			EntryCount++;
		}
	}
}