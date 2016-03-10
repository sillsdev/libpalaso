namespace SIL.Linux.Logging
{
	/// <summary>
	/// Priority values defined for libc syslog() calls. Use these with SyslogLogger objects.
	/// </summary>
	public enum SyslogPriority
	{
		Emergency = 0,
		Alert = 1,
		Critical = 2,
		Error = 3,
		Warning = 4,
		Notice = 5,
		Info = 6,
		Debug = 7
	}
}