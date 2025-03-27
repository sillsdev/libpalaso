// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	/// <summary>
	/// This class is an adapter for GeckoFx' GeckoWebBrowser class used by
	/// SIL.Windows.Forms.GeckoBasedControls.
	///
	/// Clients can use this class instead of accessing Gecko.GeckoWebBrowser
	/// directly.
	/// </summary>
	/// <remarks>
	/// All references to the GeckoWebBrowser are made by reflection so that we
	/// don't have a build dependency on GeckoFx (and possibly a particular version
	/// of GeckoFx).  Runtime dependency occurs only for the ArtOfReadingChooser at
	/// the moment, so programs that don't use the picture library don't need to
	/// include GeckoFx and xulrunner.  (Unless, of course they use some other
	/// library that uses GeckoFx and xulrunner.)
	/// </remarks>
	internal class ReflectiveGeckoBrowser
	{
		private static Assembly GeckoCoreAssembly;
		private static Assembly GeckoWinAssembly;
		private readonly Control _webBrowser; // Actually a Gecko.GeckoWebBrowser control.
		private const string GeckoBrowserType = "Gecko.GeckoWebBrowser";
		private const string XpcomType = "Gecko.Xpcom";

		internal ReflectiveGeckoBrowser()
		{
			LoadGeckoAssemblies();
			SetUpXulRunner();
			_webBrowser = InstantiateGeckoWebBrowser();
			Dock = DockStyle.Fill;
			AddGeckoDefinedEventHandler("DocumentCompleted", "OnDocumentCompleted");
			AddGeckoDefinedEventHandler("DomFocus", "OnDomFocus");
			AddGeckoDefinedEventHandler("DomBlur", "OnDomBlur");
			AddGeckoDefinedEventHandler("DomKeyDown", "OnDomKeyDown");
			AddGeckoDefinedEventHandler("DomKeyUp", "OnDomKeyUp");
			AddGeckoDefinedEventHandler("DomClick", "OnDomClick");
			AddGeckoDefinedEventHandler("DomMouseDown", "OnDomMouseDown");
		}

		/// <summary>
		/// Return the Control associated with this class (which is NOT a Control)
		/// </summary>
		/// <remarks>
		/// Sometimes an actual Control may be needed without needing to use it as anything
		/// more specific than just a Control.  This class is not a Control although it
		/// tries hard to mimic being a Gecko.GeckoWebBrowser in all other ways that are
		/// needed.
		/// </remarks>
		internal Control BrowserControl => _webBrowser;

		private static int XulRunnerVersion
		{
			get
			{
				var geckofx = GeckoCoreAssembly;
				if (geckofx == null)
					return 0;
				var versionAttribute = geckofx.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)
					.FirstOrDefault() as AssemblyFileVersionAttribute;
				return versionAttribute == null ? 0 : new Version(versionAttribute.Version).Major;
			}
		}

		private static void SetUpXulRunner()
		{
			if (IsXpcomInitialized())
				return;
			string xulRunnerPath = Environment.GetEnvironmentVariable("XULRUNNER");
			if (!Directory.Exists(xulRunnerPath))
			{
				xulRunnerPath = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution, "xulrunner");
				if (!Directory.Exists(xulRunnerPath))
				{
					// if this is a programmer, go look in the lib directory
					xulRunnerPath = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution,
						Path.Combine("lib", "xulrunner"));

					// on my build machine, I really like to have the dir labelled with the version.
					// but it's a hassle to update all the other parts (installer, build machine) with this number,
					// so we only use it if we don't find the unnumbered alternative.
					if (!Directory.Exists(xulRunnerPath))
					{
						xulRunnerPath = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution,
							Path.Combine("lib", "xulrunner" + XulRunnerVersion));
					}

					if (!Directory.Exists(xulRunnerPath))
					{
						throw new ConfigurationException(
							"Can't find the directory where xulrunner (version {0}) is installed",
							XulRunnerVersion);
					}
				}
			}
			InitializeXpcom(xulRunnerPath);
			Application.ApplicationExit += OnApplicationExit;
		}

		#region Reflective methods for handling Gecko in a version agnostic way

		/// <summary>
		/// Attempt to load GeckoAssemblies from the programs running environment. Try and load modern gecko dlls which have
		/// no version number in the filenames then fallback to trying to load geckofx 14.
		/// </summary>
		private static void LoadGeckoAssemblies()
		{
			if (GeckoCoreAssembly != null && GeckoWinAssembly != null)
				return;
			try
			{
				try
				{
					GeckoCoreAssembly = Assembly.Load("Geckofx-Core");
				}
				catch (FileNotFoundException)
				{
					//Fallback to geckofx version 14 name
					GeckoCoreAssembly = Assembly.LoadFrom("geckofx-core-14.dll");
				}
				try
				{
					GeckoWinAssembly = Assembly.Load("Geckofx-Winforms");
				}
				catch (FileNotFoundException)
				{
					//Fallback to geckofx version 14 name
					GeckoWinAssembly = Assembly.LoadFrom("Geckofx-Winforms-14.dll");
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Unable to load geckofx dependency. Files may not have been included in the build.",
					"Failed to load geckofx", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new ApplicationException("Unable to load geckofx dependency", e);
			}
		}

		static Type _browserType;
		private Type BrowserType
		{
			get
			{
				_browserType ??= GeckoWinAssembly.GetType(GeckoBrowserType);
				return _browserType;
			}
		}

		/// <summary>
		/// This method will reflectively add a locally defined handler to an event defined in the GeckoWebBrowser.
		/// This method will look up all the event types reflectively and can be used even when the EventArgs or
		/// EventHandler types are defined in the gecko assembly.
		/// </summary>
		/// <remarks>
		/// Calling code should just define the event handler to use EventArgs, then cast it to the proper
		/// proxy class defined in this file if it actually need to use the specific EventArgs subclass.
		/// </remarks>
		private void AddGeckoDefinedEventHandler(string eventName, string handlerName)
		{
			var browserEvent = BrowserType.GetEvent(eventName);
			var eventArgsType = browserEvent.EventHandlerType;
			var methodInfo = GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);
			var completedDelegate = Delegate.CreateDelegate(eventArgsType, this, methodInfo);
			var addEventMethod = browserEvent.GetAddMethod();
			addEventMethod.Invoke(_webBrowser, new object[] { completedDelegate });
		}

		/// <summary>
		/// Reflectively construct a GeckoWebBrowser.
		/// </summary>
		/// <returns>a reflectively created GeckoWebBrowser as a Control</returns>
		private Control InstantiateGeckoWebBrowser()
		{
			var constructor = BrowserType.GetConstructor(new Type[] { });
			var geckoWebBrowser = constructor.Invoke(new object[] { });
			return geckoWebBrowser as Control;
		}

		private static bool IsXpcomInitialized()
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var isInitializedProp = xpcomType.GetProperty("IsInitialized");
			var initialized = isInitializedProp.GetValue(null, BindingFlags.Static, null, null, null);
			return (bool)initialized;
		}

		private static void InitializeXpcom(string xulRunnerPath)
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var initMethod = xpcomType.GetMethod("Initialize", new[] { typeof(string) });
			initMethod.Invoke(null, new object[] { xulRunnerPath });
		}

		private static void ShutdownXpcom()
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var shutdownMethod = xpcomType.GetMethod("Shutdown");
			shutdownMethod.Invoke(null, null);
		}

		#endregion

		private static void OnApplicationExit(object sender, EventArgs e)
		{
			// We come here iff we initialized Xpcom. In that case we want to call shutdown,
			// otherwise the app might not exit properly.
			if (IsXpcomInitialized())
				ShutdownXpcom();
			Application.ApplicationExit -= OnApplicationExit;
		}

		#region Gecko.GeckoWebBrowser properties
		static PropertyInfo _dockProp;
		internal DockStyle Dock
		{
			get
			{
				_dockProp ??= BrowserType.GetProperty("Dock");
				return (DockStyle)_dockProp.GetValue(_webBrowser, null);
			}
			set
			{
				_dockProp ??= BrowserType.GetProperty("Dock");
				_dockProp.SetValue(_webBrowser, value, BindingFlags.Default, null, null, null);
			}
		}

		static PropertyInfo _parentProp;
		internal Control Parent
		{
			get
			{
				_parentProp ??= BrowserType.GetProperty("Parent");
				return _parentProp.GetValue(_webBrowser, null) as Control;
			}
			set
			{
				_parentProp ??= BrowserType.GetProperty("Parent");
				_parentProp.SetValue(_webBrowser, value, BindingFlags.Default, null, null, null);
			}
		}

		static PropertyInfo _noDefaultContextMenuProp;
		internal bool NoDefaultContextMenu
		{
			get
			{
				_noDefaultContextMenuProp ??= BrowserType.GetProperty("NoDefaultContextMenu");
				return (bool)_noDefaultContextMenuProp.GetValue(_webBrowser, null);
			}
			set
			{
				_noDefaultContextMenuProp ??= BrowserType.GetProperty("NoDefaultContextMenu");
				_noDefaultContextMenuProp.SetValue(_webBrowser, value, BindingFlags.Default, null, null, null);
			}
		}

		static PropertyInfo _documentProp;
		internal GeckoDocument Document
		{
			get
			{
				_documentProp ??= BrowserType.GetProperty("Document");
				return new GeckoDocument(_documentProp.GetValue(_webBrowser, null));
			}
		}

		static PropertyInfo _webBrowserFocusProp;
		internal nsWebBrowserFocus WebBrowserFocus
		{
			get
			{
				_webBrowserFocusProp ??= BrowserType.GetProperty("WebBrowserFocus");
				return new nsWebBrowserFocus(_webBrowserFocusProp.GetValue(_webBrowser, null));
			}
		}

		static PropertyInfo _containsFocusProp;
		internal bool ContainsFocus
		{
			get
			{
				_containsFocusProp ??= BrowserType.GetProperty("ContainsFocus");
				return (bool)_containsFocusProp.GetValue(_webBrowser, null);
			}
			set
			{
				_containsFocusProp ??= BrowserType.GetProperty("ContainsFocus");
				_containsFocusProp.SetValue(_webBrowser, value, BindingFlags.Default, null, null, null);
			}
		}

		static PropertyInfo _widthProp;
		internal int Width
		{
			get
			{
				_widthProp ??= BrowserType.GetProperty("Width");
				return (int)_widthProp.GetValue(_webBrowser, null);
			}
			set
			{
				_widthProp ??= BrowserType.GetProperty("Width");
				_widthProp.SetValue(_webBrowser, value, BindingFlags.Default, null, null, null);
			}
		}
		#endregion

		#region Gecko.GeckoWebBrowser methods
		static MethodInfo _disposeMethod;
		internal void Dispose()
		{
			_disposeMethod ??= BrowserType.GetMethod("Dispose");
			_disposeMethod.Invoke(_webBrowser, null);
		}

		static MethodInfo _focusMethod;
		internal bool Focus()
		{
			_focusMethod ??= BrowserType.GetMethod("Focus");
			return (bool)_focusMethod.Invoke(_webBrowser, null);
		}

		static MethodInfo _setInputFocusMethod;
		internal void SetInputFocus()
		{
			_setInputFocusMethod ??= BrowserType.GetMethod("SetInputFocus");
			_setInputFocusMethod.Invoke(_webBrowser, null);
		}

		static MethodInfo _removeInputFocusMethod;
		internal void RemoveInputFocus()
		{
			_removeInputFocusMethod ??= BrowserType.GetMethod("RemoveInputFocus");
			_removeInputFocusMethod.Invoke(_webBrowser, null);
		}

		static MethodInfo _hasInputFocusMethod;
		internal bool HasInputFocus()
		{
			_hasInputFocusMethod ??= BrowserType.GetMethod("HasInputFocus");
			return (bool)_hasInputFocusMethod.Invoke(_webBrowser, null);
		}

		static MethodInfo _loadHtmlMethod;
		internal void LoadHtml(string html)
		{
			_loadHtmlMethod ??= BrowserType.GetMethod("LoadHtml", new Type[] { typeof(string) });
			_loadHtmlMethod.Invoke(_webBrowser, new object[] { html });
		}
		#endregion

		#region Gecko.GeckoWebBrowser events
		// These must be registered via AddGeckoDefinedEventHandler() in the constructor.
		// If the actual EventHandler uses a Gecko subclass of EventArgs, we are responsible
		// for having a proxy class defined below, and the caller is responsible for creating
		// the proper proxy to encapsulate the EventArgs object.
		internal event EventHandler<EventArgs> DocumentCompleted;
		void OnDocumentCompleted(object sender, EventArgs e)
		{
			DocumentCompleted?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomFocus;
		void OnDomFocus(object sender, EventArgs e)
		{
			DomFocus?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomBlur;
		void OnDomBlur(object sender, EventArgs e)
		{
			DomBlur?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomKeyDown;
		void OnDomKeyDown(object sender, EventArgs e)
		{
			DomKeyDown?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomKeyUp;
		void OnDomKeyUp(object sender, EventArgs e)
		{
			DomKeyUp?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomClick;
		void OnDomClick(object sender, EventArgs e)
		{
			DomClick?.Invoke(this, e);
		}

		internal event EventHandler<EventArgs> DomMouseDown;
		void OnDomMouseDown(object sender, EventArgs e)
		{
			DomMouseDown?.Invoke(this, e);
		}
		#endregion
	}

	/// <summary>
	/// Proxy for a real Gecko.GeckoDocument object.
	/// </summary>
	internal class GeckoDocument
	{
		private object _document;
		private Type _type;
		private MethodInfo _getElementByIdMethod;

		internal GeckoDocument(object doc)
		{
			_document = doc;
			_type = doc.GetType();
		}

		internal GeckoElement GetElementById(string id)
		{
			_getElementByIdMethod ??=
				_type.GetMethod("GetElementById", new Type[] { typeof(string) });
			var element = _getElementByIdMethod.Invoke(_document, new object[] { id });
			return new GeckoBodyElement(element);
		}
	}

	/// <summary>
	/// Proxy for a real Gecko.GeckoElement object.
	/// </summary>
	internal class GeckoElement
	{
		protected object _element;
		protected Type _type;
		protected PropertyInfo _textContentProp;
		protected PropertyInfo _scrollHeightProp;

		internal GeckoElement(object element)
		{
			_element = element;
			_type = element.GetType();
		}

		internal string TextContent
		{
			get
			{
				_textContentProp ??= _type.GetProperty("TextContent");
				return _textContentProp.GetValue(_element, null) as string;
			}
			set
			{
				_textContentProp ??= _type.GetProperty("TextContent");
				_textContentProp.SetValue(_element, value, BindingFlags.Default, null, null, null);
			}
		}

		internal int ScrollHeight
		{
			get
			{
				_scrollHeightProp ??= _type.GetProperty("ScrollHeight");
				return (int)_scrollHeightProp.GetValue(_element, null);
			}
		}
	}

	/// <summary>
	/// Proxy for a real Gecko.GeckoHtmlElement.
	/// </summary>
	internal class GeckoHtmlElement : GeckoElement
	{
		private MethodInfo _focusMethod;
		private PropertyInfo _parentProp;

		internal GeckoHtmlElement(object element) : base(element)
		{
		}

		internal void Focus()
		{
			_focusMethod ??= _type.GetMethod("Focus");
			_focusMethod.Invoke(_element, null);
		}

		internal GeckoHtmlElement Parent
		{
			get
			{
				_parentProp ??= _type.GetProperty("Parent");
				return new GeckoHtmlElement(_parentProp.GetValue(_element, null));
			}
		}

	}

	/// <summary>
	/// Proxy for a real Gecko.DOM.GeckoBodyElement.
	/// </summary>
	/// <remarks>
	/// This is need to satisfy calling code.
	/// </remarks>
	internal class GeckoBodyElement : GeckoHtmlElement
	{
		internal GeckoBodyElement(object element) : base(element)
		{
		}
	}

	/// <summary>
	/// Proxy for a real Gecko.nsIWebBrowserFocus COM object.
	/// </summary>
	internal class nsWebBrowserFocus
	{
		private nsIWebBrowserFocus _webFocus;

		internal nsWebBrowserFocus(object webFocus)
		{
			_webFocus = webFocus as nsIWebBrowserFocus;
		}

		internal void Activate()
		{
			_webFocus.Activate();
		}

		internal void Deactivate()
		{
			_webFocus.Deactivate();
		}
	}

	/// <summary>
	/// Proxy for Gecko.Events.GeckoDocumentCompletedEventArgs.
	/// </summary>
	internal class GeckoDocumentCompletedEventArgs : EventArgs
	{
		internal Uri Uri { get; private set; }
		internal object Window { get; private set; }
		internal bool IsTopLevel { get; private set; }

		/// <summary>
		/// Constructor extracts Uri, Window, IsTopLevel properties from the EventArgs's type.
		/// </summary>
		internal GeckoDocumentCompletedEventArgs(EventArgs e)
		{
			Type eventArgsType = e.GetType();
			PropertyInfo uriProp = eventArgsType.GetProperty("Uri");
			Uri = (Uri)uriProp.GetValue(e, null);
			PropertyInfo windowProp = eventArgsType.GetProperty("Window");
			Window = windowProp.GetValue(e, null); // GeckoWindow
			PropertyInfo topLevelProp = eventArgsType.GetProperty("IsTopLevel");
			IsTopLevel = (bool)topLevelProp.GetValue(e, null);
		}
	}

	/// <summary>
	/// Proxy for Gecko.DomEventArgs.
	/// </summary>
	public class DomEventArgs : EventArgs
	{
		protected EventArgs _eventArgs;
		protected Type _eventArgsType;
		PropertyInfo _targetProp;
		PropertyInfo _handleProp;

		internal DomEventArgs(EventArgs e)
		{
			_eventArgs = e;
			_eventArgsType = e.GetType();
		}

		/// <summary>
		/// Gets the final destination of the event.
		/// </summary>
		internal DomEventTarget Target
		{
			get
			{
				_targetProp ??= _eventArgsType.GetProperty("Target");
				return new DomEventTarget(_targetProp.GetValue(_eventArgs, null));
			}
		}

		internal bool Handled
		{
			get
			{
				_handleProp ??= _eventArgsType.GetProperty("Handled");
				return (bool)_handleProp.GetValue(_eventArgs, null);
			}
			set
			{
				_handleProp ??= _eventArgsType.GetProperty("Handled");
				_handleProp.SetValue(_eventArgs, value, BindingFlags.Default, null, null, null);
			}
		}

	}

	/// <summary>
	/// Proxy for Gecko.DOM.DomEventTarget.
	/// </summary>
	internal class DomEventTarget
	{
		object _target;
		Type _targetType;
		MethodInfo _castToGeckoElementMethod;

		internal DomEventTarget(object target)
		{
			_target = target;
			_targetType = target.GetType();
		}

		internal GeckoElement CastToGeckoElement()
		{
			_castToGeckoElementMethod ??= _targetType.GetMethod("CastToGeckoElement");
			return new GeckoElement(_castToGeckoElementMethod.Invoke(_target, null));
		}
	}

	/// <summary>
	/// Proxy for Gecko.DomKeyEventArgs
	/// </summary>
	public class DomKeyEventArgs : DomUIEventArgs
	{
		PropertyInfo _keyCodeProp;
		PropertyInfo _altKeyProp;
		PropertyInfo _ctrlKeyProp;
		PropertyInfo _shiftKeyProp;

		internal DomKeyEventArgs(EventArgs e) : base(e)
		{
		}

		public uint KeyCode
		{
			get
			{
				_keyCodeProp ??= _eventArgsType.GetProperty("KeyCode");
				return (uint)_keyCodeProp.GetValue(_eventArgs, null);
			}
		}

		public bool AltKey
		{
			get
			{
				_altKeyProp ??= _eventArgsType.GetProperty("AltKey");
				return (bool)_altKeyProp.GetValue(_eventArgs, null);
			}
		}

		public bool CtrlKey
		{
			get
			{
				_ctrlKeyProp ??= _eventArgsType.GetProperty("CtrlKey");
				return (bool)_ctrlKeyProp.GetValue(_eventArgs, null);
			}
		}

		public bool ShiftKey
		{
			get
			{
				_shiftKeyProp ??= _eventArgsType.GetProperty("ShiftKey");
				return (bool)_shiftKeyProp.GetValue(_eventArgs, null);
			}
		}
	}

	/// <summary>
	/// Proxy for Gecko.DomUIEventArgs
	/// </summary>
	public class DomUIEventArgs : DomEventArgs
	{
		internal DomUIEventArgs(EventArgs e) : base(e)
		{
		}
	}
	/// <summary>
	/// Proxy for Gecko.DomMouseEventArgs
	/// </summary>
	/// <remarks>
	/// Not used in GeckoBox, but may be needed for other gecko-based controls
	/// if we get them implemented in Palaso as well as/instead of WeSay.
	/// </remarks>
	public class DomMouseEventArgs : DomUIEventArgs
	{
		public DomMouseEventArgs(EventArgs e) : base(e)
		{
		}
	}
}
