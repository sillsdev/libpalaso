// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.IO;


#if __MonoCS__
using System;
using System.Collections.Generic;
using IBusDotNet;
using Palaso.UI.WindowsForms.Keyboarding.Interfaces;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Linux
{
	/// <summary>
	/// This class gets the available ibus keyboards
	/// </summary>
	[CLSCompliant(false)]
	public class IbusKeyboardRetrievingAdaptor: IKeyboardRetrievingAdaptor
	{
		protected IIbusCommunicator _IBusCommunicator;
		protected IbusKeyboardSwitchingAdaptor _adaptor;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="Palaso.UI.WindowsForms.Keyboarding.Linux.IbusKeyboardRetriever"/> class.
		/// </summary>
		public IbusKeyboardRetrievingAdaptor(): this(new IbusCommunicator())
		{
		}

		/// <summary>
		/// Used in unit tests
		/// </summary>
		public IbusKeyboardRetrievingAdaptor(IIbusCommunicator ibusCommunicator)
		{
			_IBusCommunicator = ibusCommunicator;
		}

		protected virtual void InitKeyboards()
		{
			foreach (var ibusKeyboard in GetIBusKeyboards())
			{
				var keyboard = new IbusKeyboardDescription(_adaptor, ibusKeyboard, UInt32.MaxValue);
				KeyboardController.Manager.RegisterKeyboard(keyboard);
			}
		}

		protected virtual IBusEngineDesc[] GetIBusKeyboards()
		{
			if (!_IBusCommunicator.Connected)
				return new IBusEngineDesc[0];

			var ibusWrapper = new InputBus(_IBusCommunicator.Connection);
			return ibusWrapper.ListActiveEngines();
		}

		internal IBusEngineDesc[] GetAllIBusKeyboards()
		{
			if (!_IBusCommunicator.Connected)
				return new IBusEngineDesc[0];

			var ibusWrapper = new InputBus(_IBusCommunicator.Connection);
			return ibusWrapper.ListEngines();
		}

		#region IKeyboardRetriever implementation
		/// <summary>
		/// The type of keyboards this retriever handles: system or other (like Keyman, ibus...)
		/// </summary>
		public virtual KeyboardType Type
		{
			get { return KeyboardType.OtherIm; }
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
				return _IBusCommunicator.Connected && GetIBusKeyboards().Length > 0;
			}
		}

		/// <summary>
		/// Gets the keyboard adaptor that deals with keyboards that this class retrieves.
		/// </summary>
		public IKeyboardSwitchingAdaptor Adaptor { get { return _adaptor; } }

		/// <summary>
		/// Initialize this keyboard retriever
		/// </summary>
		public virtual void Initialize()
		{
			_adaptor = new IbusKeyboardSwitchingAdaptor(_IBusCommunicator);
		}

		/// <summary>
		/// Retrieve and register the available keyboards
		/// </summary>
		public virtual void RegisterAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary>
		/// Update the available keyboards
		/// </summary>
		public virtual void UpdateAvailableKeyboards()
		{
			InitKeyboards();
		}

		/// <summary>
		/// List of keyboard layouts that either gave an exception or other error trying to
		/// get more information. We don't have enough information for these keyboard layouts
		/// to include them in the list of installed keyboards.
		/// </summary>
		public List<IKeyboardErrorDescription> ErrorKeyboards
		{
			get
			{
				return new List<IKeyboardErrorDescription>();
			}
		}

		// Currently we expect this to only be useful on Windows.
		public IKeyboardDefinition GetKeyboardForInputLanguage(IInputLanguage inputLanguage)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Only the primary (Type=System) adapter is required to implement this method. This one makes keyboards
		/// during Initialize, but is not used to make an unavailable keyboard to match an LDML file.
		/// </summary>
		public virtual IKeyboardDefinition CreateKeyboardDefinition(string layout, string locale)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Shutdown this instance and prevent futher use
		/// </summary>
		public virtual void Close()
		{
			if (!_IBusCommunicator.IsDisposed)
			{
				_IBusCommunicator.Dispose();
			}

			_adaptor = null;
		}

		public virtual string GetKeyboardSetupApplication(out string arguments)
		{
			arguments = null;
			return File.Exists("/usr/bin/ibus-setup") ? "/usr/bin/ibus-setup" : null;
		}

		public bool IsSecondaryKeyboardSetupApplication
		{
			get { return false; }
		}

		#endregion
	}
}
#endif
