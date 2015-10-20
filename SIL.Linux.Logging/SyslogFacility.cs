namespace SIL.Linux.Logging
{
	/// <summary>
	/// "Facility" value for syslog() calls. Use these with SyslogLogger objects.
	/// Comments below are derived from the libc source and RFC 3164.
	/// All values are bitshifted left by 3 to match the values defined in libc's sys/syslog.h.
	/// </summary>
	public enum SyslogFacility
	{
		/// <summary>
		/// Kernel messages; user code cannot use this value, and will be assigned the User facility if it attempts to do so.
		/// </summary>
		Kernel = 0 << 3,

		/// <summary>
		/// User-level messages. This is the value most programs should be using.
		/// </summary>
		User = 1 << 3,

		/// <summary>
		/// Mail system
		/// </summary>
		Mail = 2 << 3,
		
		/// <summary>
		/// System daemons
		/// </summary>
		Daemon = 3 << 3,

		/// <summary>
		/// Security and/or authorization messages
		/// </summary>
		SecurityOrAuth = 4 << 3,

		/// <summary>
		/// Messages generated interally by syslogd
		/// </summary>
		InternalUseOnly = 5 << 3,

		/// <summary>
		/// Line printer subsystem
		/// </summary>
		LPRSubsystem = 6 << 3,

		/// <summary>
		/// Network news subsystem
		/// </summary>
		NNTPSubsystem = 7 << 3,

		/// <summary>
		/// UUCP subsystem
		/// </summary>
		UUCPSubsystem = 8 << 3,

		/// <summary>
		/// Clock daemon (cron, at, etc). Per RFC 3164, there are two facility values in use for clock daemons.
		/// </summary>
		ClockDaemon1 = 9 << 3,

		/// <summary>
		/// Security and/or authorization messages (private)
		/// </summary>
		SecurityOrAuthPrivate = 10 << 3,

		/// <summary>
		/// FTP subsystem
		/// </summary>
		FTPSubsystem = 11 << 3,

		/// <summary>
		/// NTP subsystem
		/// </summary>
		NTPSubsystem = 12 << 3,

		/// <summary>
		/// Log audit messages
		/// </summary>
		LogAudit = 13 << 3,

		/// <summary>
		/// Log alert messages
		/// </summary>
		LogAlert = 14 << 3,

		/// <summary>
		/// Clock daemon (cron, at, etc). Per RFC 3164, there are two facility values in use for clock daemons.
		/// </summary>
		ClockDaemon2 = 15 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse0 = 16 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse1 = 17 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse2 = 18 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse3 = 19 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse4 = 20 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse5 = 21 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse6 = 22 << 3,

		/// <summary>
		/// Reserved for local use
		/// </summary>
		LocalUse7 = 23 << 3
	}
}