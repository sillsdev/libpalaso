using System;
using System.Collections.Generic;

namespace SIL.Code
{
	public class Detect
	{
		public static ReentryDetecter Reentry(object caller, string methodName)
		{
			return new ReentryDetecter(caller, methodName);
		}
	}

	public class ReentryDetecter : IDisposable
	{
		private static List<string> _currentlyInScope = new List<string>();
		private readonly string _name;
		private bool _didReenter;

		/// <example>
		///   using (var x = Detect.Reentry(this, "OnPropertyValueChanged"))
		///   {
		///         if(x.DidReenter)
		///             return;
		/// </example>
		public bool DidReenter
		{
			get { return _didReenter; }
		}

		/// <example>
		///   using (Detect.Reentry(this, "OnPropertyValueChanged").AndThrow())
		///   {
		/// </example>
		public IDisposable AndThrow()
		{
			if(_didReenter)
				throw new ApplicationException("Error in the flow of the application, which tried to reenter " + _name + ".");
			return this;
		}

		internal ReentryDetecter(object caller, string methodName)
		{
			_name = caller.GetType().ToString() + ":" + methodName;
			if (_currentlyInScope.Contains(_name))
			{
				_didReenter = true;
			}
			else
			{
				_currentlyInScope.Add(_name);
			}
		}

		public void Dispose()
		{
			if (!_didReenter)
			{
				_currentlyInScope.Remove(_name);
			}
		}


	}
}