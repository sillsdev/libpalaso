using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Palaso.Reporting;

using NDesk.DBus;
using org.freedesktop.DBus;
using org.freedesktop.IBus;

namespace Palaso.UI.WindowsForms.Keyboarding
{
	/// <summary>
	/// static class that allow creation of DBus connection to IBus's session DBus.
	/// </summary>
	static internal class IBusConnectionFactory
	{
		const string ENV_IBUS_ADDRESS = "IBUS_ADDRESS";
		const string IBUS_ADDRESS = "IBUS_ADDRESS";

		/// <summary>
		/// Attempts to return the file name of the ibus server config file that contains the socket name.
		/// </summary>
		static string IBusConfigFilename ()
		{
			// Implementation Plan:
			// Read file in $XDG_CONFIG_HOME/ibus/bus/* if ($XDG_CONFIG_HOME) not set then $HOME/.config/ibus/bus/*
			// Actual file is called 'localmachineid'-'hostname'-'displaynumber'
			// eg: 5a2f89ae5421972c24f8a4414b0495d7-unix-0
			// could check $DISPLAY to see if we are running not on display 0 or not.

			string directory = System.Environment.GetEnvironmentVariable ("XDG_CONFIG_HOME");
			if (String.IsNullOrEmpty (directory))
			{
				directory = System.Environment.GetEnvironmentVariable ("HOME");

				if (String.IsNullOrEmpty (directory))
					throw new ApplicationException ("$XDG_CONFIG_HOME or $HOME Environment not set");

				directory = Path.Combine (directory, ".config");
			}
			directory = Path.Combine (directory, "ibus");
			directory = Path.Combine (directory, "bus");

			DirectoryInfo di = new DirectoryInfo (directory);

			// default to 0 if we can't find from DISPLAY ENV var
			int displayNumber = 0;

			// DISPLAY is hostname:displaynumber.screennumber
			// or more nomally ':0.0"
			// so look for first number after :

			string display = System.Environment.GetEnvironmentVariable ("DISPLAY");
			if (display != String.Empty)
			{
				int start = display.IndexOf (':');
				int end = display.IndexOf ('.');
				if (start > 0 && end > 0)
					int.TryParse (display.Substring (start, end - start), out displayNumber);
			}

			string filter = String.Format ("*-{0}", displayNumber);
			FileInfo[] files = di.GetFiles (filter);

			if (files.Length != 1)
				throw new ApplicationException (String.Format ("Unable to locate IBus Config file in directory {0} with filter {1}. DISPLAY = {2}: {3}", directory, filter, display, files.Length < 1 ? "Unable to locate file" : "Too many files"));

			return files[0].FullName;

		}

		/// <summary>
		/// Read config file and return the socket name from it.
		/// </summary>
		static string GetSocket (string filename)
		{
			// Look for line
			// Set Enviroment 'DBUS_SESSION_BUS_ADDRESS' so DBus Library actually connects to IBus' DBus.
			// IBUS_ADDRESS=unix:abstract=/tmp/dbus-DVpIKyfU9k,guid=f44265fa3b2781284d54c56a4b0d83f3

			StreamReader s = new StreamReader (filename);
			string line = String.Empty;
			while (line != null)
			{
				line = s.ReadLine ();

				if (line.Contains (IBUS_ADDRESS))
				{
					string[] toks = line.Split ("=".ToCharArray (), 2);
					if (toks.Length != 2 || toks[1] == String.Empty)
						throw new ApplicationException (String.Format ("IBUS config file : {0} not as expected for line {1}. Expected IBUS_ADDRESS='some socket'", filename, line));

					return toks[1];
				}
			}

			throw new ApplicationException (String.Format ("IBUS config file : {0} doesn't contain {1} token", filename, IBUS_ADDRESS));
		}

		/// <summary>
		/// Create a DBus to connection to the IBus system in use.
		/// </summary>
		public static NDesk.DBus.Connection Create ()
		{
			// if Enviroment var IBUS_ADDRESS doesn't exist then attempt to read it from IBus server settings file.
			string socketName = System.Environment.GetEnvironmentVariable (ENV_IBUS_ADDRESS);
			if (String.IsNullOrEmpty (socketName))
				socketName = GetSocket (IBusConfigFilename ());

			// Equivelent to having $DBUS_SESSION_BUS_ADDRESS set
			Connection connnection = Bus.Open (socketName);

			return connnection;
		}
	}

	internal class IBus
	{
		org.freedesktop.IBus.IIBus _inputBus;

		public IBus (NDesk.DBus.Connection connection)
		{
			_inputBus = connection.GetObject<org.freedesktop.IBus.IIBus> ("org.freedesktop.IBus", new ObjectPath ("/org/freedesktop/IBus"));
		}

		/// <summary>
		/// Allow Access to the underlying org.freedesktop.IBus.IIBus
		/// </summary>
		public org.freedesktop.IBus.IIBus InputBus
		{
			get { return _inputBus; }
		}
		/// <summary>
		/// Return the DBUS 'path' name for the currently focused InputContext
		/// Throws: System.Exception with message 'org.freedesktop.DBus.Error.Failed: No input context focused'
		/// if nothing is currently focused.
		/// </summary>
		public string GetFocusedInputContextPath ()
		{
			return _inputBus.CurrentInputContext ();
		}
	}

	internal class IBusInputContext
	{
		org.freedesktop.IBus.InputContext _inputContext;

		/// <summary>
		/// Wraps a connection to a specfic instance of an IBus InputContext
		/// inputContextName needs to be the name of specfic instance of the input context.
		/// For example "/org/freedesktop/IBus/InputContext_15"
		/// </summary>
		public IBusInputContext (NDesk.DBus.Connection connection, string inputContextName)
		{
			_inputContext = connection.GetObject<org.freedesktop.IBus.InputContext> ("org.freedesktop.DBus", new ObjectPath (inputContextName));
		}

		/// <summary>
		/// Allow Access to the underlying org.freedesktop.IBus.IIBus
		/// </summary>
		public org.freedesktop.IBus.InputContext InputContext
		{
			get { return _inputContext; }
		}
	}

	internal class IBusAdaptor
	{
		static NDesk.DBus.Connection _connection = null;

		/// <summary>
		/// Opens a _connection if one isn't already Opened.
		/// </summary>
		public static void EnsureConnection ()
		{
			if (_connection == null)
				_connection = IBusConnectionFactory.Create ();
		}

		/// <summary>
		/// Close the connection. set _connection to null.
		/// </summary>
		public static void CloseConnection ()
		{
			if (_connection != null)
			{
				_connection.Close ();
				_connection = null;
			}
		}

		public static void ActivateKeyboard (string name)
		{
			EnsureConnection ();

			IBus ibus = new IBus (_connection);
			string inputContextPath = ibus.GetFocusedInputContextPath ();

			IBusInputContext inputContextBus = new IBusInputContext (_connection, inputContextPath);

			if(!HasKeyboardNamed(name))
			{
				throw new ArgumentOutOfRangeException("IBus does not have a Keyboard with that name!" + name);
			}

			inputContextBus.InputContext.SetEngine (name);
		}

		/// <summary>
		/// Helper function the builds a list of Active Keyboards
		/// </summary>
		protected static IEnumerable<KeyboardController.KeyboardDescriptor> GetKeyboardDescriptors ()
		{
			try
			{
				EnsureConnection ();
			}
			catch
			{
				return false;
			}

			IBus ibus = new IBus (_connection);
			object[] engines = ibus.InputBus.ListActiveEngines ();
			for (int i = 0; i < (engines).Length; ++i)
			{
				IBusEngineDesc engineDesc = (IBusEngineDesc)Convert.ChangeType (engines[i], typeof(IBusEngineDesc));
				var v = new KeyboardController.KeyboardDescriptor ();
				v.Id = engineDesc.name;
				v.Name = engineDesc.longname;
				v.engine = KeyboardController.Engines.IBus;

				yield return v;
			}

		}

		public static List<KeyboardController.KeyboardDescriptor> KeyboardDescriptors
		{
			get { return new List<KeyboardController.KeyboardDescriptor>(GetKeyboardDescriptors () ); }
		}

		public static bool EngineAvailable
		{
			get
			{
				try
				{
					EnsureConnection ();
					return (_connection != null);
				} catch
				{
					return false;
				}
			}
		}

		public static void Deactivate ()
		{
			EnsureConnection ();

			IBus ibus = new IBus (_connection);
			string inputContextPath = ibus.GetFocusedInputContextPath ();

			IBusInputContext inputContextBus = new IBusInputContext (_connection, inputContextPath);
			inputContextBus.InputContext.Reset ();
		}

		public static bool HasKeyboardNamed (string name)
		{
			foreach (KeyboardController.KeyboardDescriptor d in GetKeyboardDescriptors ())
			{
				if (d.Name.Equals (name))
					return true;
			}

			return false;
		}

		public static string GetActiveKeyboard ()
		{
			try
			{
				EnsureConnection ();
			}
			catch
			{
				return String.Empty;
			}

			IBus ibus = new IBus (_connection);

			try
			{
				string inputContextPath = ibus.GetFocusedInputContextPath ();

				IBusInputContext inputContextBus = new IBusInputContext (_connection, inputContextPath);
				object engine = inputContextBus.InputContext.GetEngine ();
				if (engine == null)
					throw new ApplicationException ("Focused Input Context doesn't have an active Keyboard/Engine");

				IBusEngineDesc engineDesc = (IBusEngineDesc)Convert.ChangeType (engine, typeof(IBusEngineDesc));
				return engineDesc.longname;
			}
			catch (Exception e)
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(new ShowOncePerSessionBasedOnExactMessagePolicy(),
																 "Could not get ActiveKeyboard");
				return String.Empty;
			}
		}
	}
}
