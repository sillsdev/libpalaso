#if MONO
using System;
using System.IO;
using NDesk.DBus;

namespace IBusDotNet
{
	/// <summary>
	/// static class that allow creation of DBus connection to IBus's session DBus.
	/// </summary>
	internal class IBusConnectionFactory
	{
		const string ENV_IBUS_ADDRESS = "IBUS_ADDRESS";
		const string IBUS_ADDRESS = "IBUS_ADDRESS";
		const string IBUS_DAEMON_PID = "IBUS_DAEMON_PID";

		/// <summary>
		/// Attempts to return the file name of the ibus server config file that contains the socket name.
		/// </summary>
		static string IBusConfigFilename()
		{
			// Implementation Plan:
			// Read file in $XDG_CONFIG_HOME/ibus/bus/* if ($XDG_CONFIG_HOME) not set then $HOME/.config/ibus/bus/*
			// Actual file is called 'localmachineid'-'hostname'-'displaynumber'
			// eg: 5a2f89ae5421972c24f8a4414b0495d7-unix-0
			// could check $DISPLAY to see if we are running not on display 0 or not.

			string directory = System.Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
			if (String.IsNullOrEmpty(directory)) {
				directory = System.Environment.GetEnvironmentVariable("HOME");

				if (String.IsNullOrEmpty(directory))
					throw new ApplicationException("$XDG_CONFIG_HOME or $HOME Environment not set");

				directory = Path.Combine(directory, ".config");
			}
			directory = Path.Combine(directory, "ibus");
			directory = Path.Combine(directory, "bus");

			DirectoryInfo di = new DirectoryInfo(directory);

			// default to 0 if we can't find from DISPLAY ENV var
			int displayNumber = 0;

			// DISPLAY is hostname:displaynumber.screennumber
			// or more nomally ':0.0"
			// so look for first number after :

			string display = System.Environment.GetEnvironmentVariable("DISPLAY");
			if (display != String.Empty) {
				int start = display.IndexOf(':');
				int end = display.IndexOf('.');
				if (start > 0 && end > 0)
					int.TryParse(display.Substring(start, end - start), out displayNumber);
			}

			string filter = String.Format("*-{0}", displayNumber);
			FileInfo[] files = di.GetFiles(filter);

			if (files.Length != 1)
				throw new ApplicationException(String.Format("Unable to locate IBus Config file in directory {0} with filter {1}. DISPLAY = {2}: {3}", directory, filter, display, files.Length < 1 ? "Unable to locate file" : "Too many files"));

			return files[0].FullName;

		}

		/// <summary>
		/// Read config file and return the socket name from it.
		/// </summary>
		static string GetSocket(string filename)
		{
			// Look for line
			// Set Enviroment 'DBUS_SESSION_BUS_ADDRESS' so DBus Library actually connects to IBus' DBus.
			// IBUS_ADDRESS=unix:abstract=/tmp/dbus-DVpIKyfU9k,guid=f44265fa3b2781284d54c56a4b0d83f3

			StreamReader s = new StreamReader(filename);
			string line = String.Empty;
			while (line != null) {
				line = s.ReadLine();

				if (line.Contains(IBUS_ADDRESS)) {
					string[] toks = line.Split("=".ToCharArray(), 2);
					if (toks.Length != 2 || toks[1] == String.Empty)
						throw new ApplicationException(String.Format("IBUS config file : {0} not as expected for line {1}. Expected IBUS_ADDRESS='some socket'", filename, line));

					return toks[1];
				}
			}

			throw new ApplicationException(String.Format("IBUS config file : {0} doesn't contain {1} token", filename, IBUS_ADDRESS));
		}

		static string GetPID(string filename)
		{
			// Look for line
			// IBUS_DAEMON_PID=8314

			StreamReader s = new StreamReader(filename);
			string line = String.Empty;
			while (line != null) {
				line = s.ReadLine();

				if (line.Contains(IBUS_DAEMON_PID)) {
					string[] toks = line.Split("=".ToCharArray(), 2);
					if (toks.Length != 2 || toks[1] == String.Empty)
						throw new ApplicationException(String.Format("IBUS config file : {0} not as expected for line {1}. Expected IBUS_DAEMON_PID='some process id'", filename, line));

					return toks[1];
				}
			}

			throw new ApplicationException(String.Format("IBUS config file : {0} doesn't contain {1} token", filename, IBUS_ADDRESS));
		}

		static IBusConnection singleConnection = null;
		/// <summary>
		/// Create a DBus to connection to the IBus system in use.
		/// Returns null if it can't conenct to ibus.
		/// </summary>
		public static IBusConnection Create()
		{
			if (singleConnection != null)
			{
				singleConnection.AddRef();
				return singleConnection;
			}

			try
			{
				// if Enviroment var IBUS_ADDRESS doesn't exist then attempt to read it from IBus server settings file.
				string socketName = System.Environment.GetEnvironmentVariable(ENV_IBUS_ADDRESS);

				string pid = GetPID(IBusConfigFilename());
				System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(int.Parse(pid));
				// throws System.Exception if process not found

				if (String.IsNullOrEmpty(socketName))
					socketName = GetSocket(IBusConfigFilename());

				// Equivalent to having $DBUS_SESSION_BUS_ADDRESS set
				singleConnection = new IBusConnection(Bus.Open(socketName));
				singleConnection.Disposed += HandleSingleConnectionDisposed;
			}
			catch(System.Exception) { } // ignore - ibus may not be running.

			return singleConnection;
		}

		private static void HandleSingleConnectionDisposed (object sender, EventArgs e)
		{
			singleConnection = null;
		}

		public static void DestroyConnection()
		{
			if (singleConnection != null)
				singleConnection.Close();
		}
	}
}
#endif