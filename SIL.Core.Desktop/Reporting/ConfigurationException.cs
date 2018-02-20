using System;

namespace SIL.Reporting
{
	public class ConfigurationException:ApplicationException
	{
		public ConfigurationException (string message, params object[] args)
			: base(string.Format(message, args))
		{
		}
	}
}