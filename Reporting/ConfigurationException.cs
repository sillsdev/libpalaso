using System;


namespace Palaso.Reporting
{
	public class ConfigurationException:ApplicationException
	{
		public ConfigurationException (string message, params object[] args)
			: base(string.Format(message, args))
		{
		}
	}
}