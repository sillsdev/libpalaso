// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using IBusDotNet;
using SIL.Keyboarding;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	/// <summary>
	/// Class for handling ibus keyboards on Linux. Currently just a wrapper for KeyboardSwitcher.
	/// </summary>
	public class IbusKeyboardRetrievingAdaptor : IKeyboardRetrievingAdaptor
	{
		private readonly IIbusCommunicator _ibusComm;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="SIL.Windows.Forms.Keyboarding.Linux.IbusKeyboardRetrievingAdaptor"/> class.
		/// </summary>
		public IbusKeyboardRetrievingAdaptor(): this(new IbusCommunicator())
		{
		}

		/// <summary>
		/// Used in unit tests
		/// </summary>
		public IbusKeyboardRetrievingAdaptor(IIbusCommunicator ibusCommunicator)
		{
			_ibusComm = ibusCommunicator;
		}

		protected virtual void InitKeyboards()
		{
			Dictionary<string, IbusKeyboardDescription> curKeyboards = KeyboardController.Instance.Keyboards.OfType<IbusKeyboardDescription>().ToDictionary(kd => kd.Id);
			foreach (IBusEngineDesc ibusKeyboard in GetIBusKeyboards())
			{
				string id = string.Format("{0}_{1}", ibusKeyboard.Language, ibusKeyboard.LongName);
				IbusKeyboardDescription existingKeyboard;
				if (curKeyboards.TryGetValue(id, out existingKeyboard))
				{
					if (!existingKeyboard.IsAvailable)
					{
						existingKeyboard.SetIsAvailable(true);
						existingKeyboard.IBusKeyboardEngine = ibusKeyboard;
					}
					curKeyboards.Remove(id);
				}
				else
				{
					var keyboard = new IbusKeyboardDescription(id, ibusKeyboard, SwitchingAdaptor);
					KeyboardController.Instance.Keyboards.Add(keyboard);
				}
			}

			foreach (IbusKeyboardDescription existingKeyboard in curKeyboards.Values)
				existingKeyboard.SetIsAvailable(false);
		}

		protected virtual IBusEngineDesc[] GetIBusKeyboards()
		{
			if (!_ibusComm.Connected)
				return new IBusEngineDesc[0];

			var ibusWrapper = new InputBus(_ibusComm.Connection);
			return ibusWrapper.ListActiveEngines();
		}

		internal IBusEngineDesc[] GetAllIBusKeyboards()
		{
			if (!_ibusComm.Connected)
				return new IBusEngineDesc[0];

			var ibusWrapper = new InputBus(_ibusComm.Connection);
			return ibusWrapper.ListEngines();
		}

		protected IIbusCommunicator IbusCommunicator
		{
			get { return _ibusComm; }
		}

		#region IKeyboardRetrievingAdaptor implementation

		/// <summary>
		/// The type of keyboards this adaptor handles: system or other (like Keyman, ibus...)
		/// </summary>
		public virtual KeyboardAdaptorType Type
		{
			get { return KeyboardAdaptorType.OtherIm; }
		}

		/// <summary>
		/// Checks whether this keyboard retriever can get keyboards. Different desktop
		/// environments use differing APIs to get the available keyboards. If this class is
		/// able to find the available keyboards this property will return <c>true</c>,
		/// otherwise <c>false</c>.
		/// </summary>
		public virtual bool IsApplicable
		{
			get
			{
				return _ibusComm.Connected && GetIBusKeyboards().Length > 0;
			}
		}

		/// <summary>
		/// Gets the keyboard adaptor that deals with keyboards that this class retrieves.
		/// </summary>
		public IKeyboardSwitchingAdaptor SwitchingAdaptor { get; protected set; }

		/// <summary>
		/// Initialize the installed keyboards
		/// </summary>
		public virtual void Initialize()
		{
			SwitchingAdaptor = new IbusKeyboardSwitchingAdaptor(_ibusComm);
			InitKeyboards();
		}

		public void UpdateAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary>
		/// Only the primary (Type=System) adapter is required to implement this method. This one makes keyboards
		/// during Initialize, but is not used to make an unavailable keyboard to match an LDML file.
		/// </summary>
		public virtual KeyboardDescription CreateKeyboardDefinition(string id)
		{
			throw new NotImplementedException();
		}

		public bool CanHandleFormat(KeyboardFormat format)
		{
			return false;
		}

		public Action GetKeyboardSetupAction()
		{
			string args;
			var setupApp = GetKeyboardSetupApplication(out args);
			if (setupApp == null)
			{
				return null;
			}
			return () =>
			{
				using (Process.Start(setupApp, args)) { }
			};
		}

		protected virtual string GetKeyboardSetupApplication(out string arguments)
		{
			arguments = null;
			return File.Exists("/usr/bin/ibus-setup") ? "/usr/bin/ibus-setup" : null;
		}

		public bool IsSecondaryKeyboardSetupApplication
		{
			get { return false; }
		}

		#endregion

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~IbusKeyboardRetrievingAdaptor()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				_ibusComm.Dispose();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.

			IsDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}
