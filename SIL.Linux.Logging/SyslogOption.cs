namespace SIL.Linux.Logging
{
	/// <summary>
	/// Option flags defined for libc openlog() calls. Use these with SyslogLogger objects.
	/// </summary>
	[System.Flags]
	public enum SyslogOption
	{
		LogPid = 1,
		LogToConsoleIfErrorSendingToSyslog = 2,
		DelayOpenUntilFirstSyslogCall = 4,
		DontDelayOpen = 8,
		DEPRECATED_DontWaitForConsoleForks = 16,
		AlsoLogToStderr = 32
	}
}
