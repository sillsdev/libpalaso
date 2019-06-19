// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SIL.IO;
using SIL.Reporting;
using SIL.Windows.Forms.HtmlBrowser;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	/// <summary>
	/// This class is an adapter for GeckoFx' GeckoWebBrowser class. It is used by
	/// Palaso.UI.WindowsForms.HtmlBrowser.XWebBrowser.
	///
	/// Clients should NOT use this class directly. Instead they should use the XWebBrowser
	/// class (or Gecko.GeckoWebBrowser if they need GeckoFx functionality).
	/// </summary>
	/// <remarks> The assembly containing this class gets loaded dynamically so
	/// that we don't have a dependency on GeckoFx unless we want to use it.</remarks>
	class GeckoFxWebBrowserAdapter: IWebBrowser
	{
		private const string GeckoBrowserType = "Gecko.GeckoWebBrowser";
		private const string XpcomType = "Gecko.Xpcom";
		private readonly Control _webBrowser;
		internal static Assembly GeckoCoreAssembly;
		internal static Assembly GeckoWinAssembly;

		public GeckoFxWebBrowserAdapter(Control parent)
		{
			LoadGeckoAssemblies();
			SetUpXulRunner();
			_webBrowser = InstantiateGeckoWebBrowser();
			parent.Controls.Add(_webBrowser);

			var callbacks = parent as IWebBrowserCallbacks;
			AddEventHandler(_webBrowser, "CanGoBackChanged", (sender, e) => callbacks.OnCanGoBackChanged(e));
			AddEventHandler(_webBrowser, "CanGoForwardChanged", (sender, e) => callbacks.OnCanGoForwardChanged(e));
			AddEventHandler(_webBrowser, "DocumentTitleChanged", (sender, e) => callbacks.OnDocumentTitleChanged(e));
			AddEventHandler(_webBrowser, "StatusTextChanged", (sender, e) => callbacks.OnStatusTextChanged(e));
			AddGeckoDefinedEventHandler(_webBrowser, "DocumentCompleted", "DocumentCompletedHandler");
			AddGeckoDefinedEventHandler(_webBrowser, "Navigated", "NavigatedHandler");
			AddGeckoDefinedEventHandler(_webBrowser, "Navigating", "NavigatingHandler");
			AddGeckoDefinedEventHandler(_webBrowser, "CreateWindow2", "CreateWindow2Handler");
			AddGeckoDefinedEventHandler(_webBrowser, "ProgressChanged", "WebProgressHandler");
			AddGeckoDefinedEventHandler(_webBrowser, "DomClick", "DomClickHandler");
		}

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
					// Gecko 45
					xulRunnerPath = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution,
						Path.Combine("Firefox"));
					if (!Directory.Exists(xulRunnerPath))
					{
						//if this is a programmer, go look in the lib directory
						xulRunnerPath = Path.Combine(FileLocationUtilities.DirectoryOfApplicationOrSolution,
							Path.Combine("lib", "xulrunner"));
					}

					//on my build machine, I really like to have the dir labelled with the version.
					//but it's a hassle to update all the other parts (installer, build machine) with this number,
					//so we only use it if we don't find the unnumbered alternative.
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

		private Uri GetGeckoNavigatedEventArgsUri(object eventArg)
		{
			var eventType = GeckoWinAssembly.GetType("Gecko.GeckoNavigatedEventArgs");
			return GetUriValue(eventArg, eventType);
		}

		private Uri GetGeckoNavigatingEventArgsUri(object eventArg)
		{
			var eventType = GeckoCoreAssembly.GetType("Gecko.Events.GeckoNavigatingEventArgs") ??
								 GeckoWinAssembly.GetType("Gecko.GeckoNavigatingEventArgs"); //Try new ns then old ns
			return GetUriValue(eventArg, eventType);
		}

		private static Uri GetUriValue(object eventArg, Type eventType)
		{
			var uriField = eventType.GetField("Uri");
			return uriField.GetValue(eventArg) as Uri;
		}

		private void SetCancelEventArgsCancel(EventArgs eventArg, bool cancelValue)
		{
			var cancelArgs = eventArg as CancelEventArgs;
			if(cancelArgs != null)
			{
				cancelArgs.Cancel = cancelValue;
			}
		}

		private Uri GetBrowserUrl(object webBrowser)
		{
			return GetBrowserProperty<Uri>(webBrowser, "Url");
		}

		/// <summary>
		/// This method will reflectively add a locally defined handler to an event of the
		/// GeckoWebBrowser. If the type of the EventHandler or EventArg is defined in gecko
		/// then the <code>AddGeckoDefinedEventhandler</code> must be used.
		/// </summary>
		private void AddEventHandler(Control webBrowser, string eventName, EventHandler action)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var browserEvent = webBrowserType.GetEvent(eventName);
			browserEvent.AddEventHandler(webBrowser, action);
		}

		/// <summary>
		/// This method will reflectively add a locally defined handler to an event defined in the GeckoWebBrowser.
		/// This method will look up all the event types reflectively and can be used even when the EventArgs or
		/// EventHandler types are defined in the gecko assembly.
		/// </summary>
		private void AddGeckoDefinedEventHandler(Control webBrowser, string eventName, string handlerName)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var browserEvent = webBrowserType.GetEvent(eventName);
			if (browserEvent == null)
			{
				switch (eventName)
				{
					// CreateWindow2 is marked obsolete as far back as Geckofx29: [Obsolete("Merged to CreateWindow event, just use it")]
					// It no longer exists at all in Geckofx60.
					// But it is needed/used in Geckofx14 which we supposedly still support in this flexible code...
					case "CreateWindow2":
						browserEvent = webBrowserType.GetEvent("CreateWindow");
						break;
					default:
						// let events take their course, so to speak... (crash upcoming)
						break;
				}
			}
			var eventArgsType = browserEvent.EventHandlerType;
			var methodInfo = GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);
			var docCompletedDelegate = Delegate.CreateDelegate(eventArgsType, this, methodInfo);
			var addEventMethod = browserEvent.GetAddMethod();
			addEventMethod.Invoke(webBrowser, new object[] { docCompletedDelegate });
		}

// ReSharper disable UnusedMember.Local
// these Handlers are all used by reflection
		private void WebProgressHandler(object sender, EventArgs e)
		{
			var geckoProgressArgsType = GeckoWinAssembly.GetType("Gecko.GeckoProgressEventArgs");
			var currentProgressProp = geckoProgressArgsType.GetField("CurrentProgress");
			var currentProgressVal = currentProgressProp.GetValue(e);
			var maxProgressProp = geckoProgressArgsType.GetField("MaximumProgress");
			var maxProgressVal = maxProgressProp.GetValue(e);
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			callbacks.OnProgressChanged(new WebBrowserProgressChangedEventArgs((long)currentProgressVal, (long)maxProgressVal));
		}

		private void NavigatedHandler(object sender, EventArgs args)
		{
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			callbacks.OnNavigated(new WebBrowserNavigatedEventArgs(GetGeckoNavigatedEventArgsUri(args)));
		}

		private void DocumentCompletedHandler(object sender, EventArgs args)
		{
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			callbacks.OnDocumentCompleted(new WebBrowserDocumentCompletedEventArgs(GetBrowserUrl(_webBrowser)));
		}

		private void NavigatingHandler(object sender, EventArgs args)
		{
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			var ev = new WebBrowserNavigatingEventArgs(GetGeckoNavigatingEventArgsUri(args), string.Empty);
			callbacks.OnNavigating(ev);
			SetCancelEventArgsCancel(args, ev.Cancel);
		}

		private void CreateWindow2Handler(object sender, EventArgs args)
		{
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			var ev = new CancelEventArgs();
			callbacks.OnNewWindow(ev);
			SetCancelEventArgsCancel(args, ev.Cancel);
		}
		// ReSharper restore UnusedMember.Local

		/// <summary>
		/// Reflectively construct a GeckoWebBrowser and set the Dock property.
		/// </summary>
		/// <returns>a reflectively created GeckoWebBrowser as a Control</returns>
		private Control InstantiateGeckoWebBrowser()
		{
			var browserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var constructor = browserType.GetConstructor(new Type[] { });
			var dockProp = browserType.GetProperty("Dock");
			var geckoWebBrowser = constructor.Invoke(new object[] { });
			dockProp.SetValue(geckoWebBrowser, DockStyle.Fill, BindingFlags.Default, null, null, null);
			return geckoWebBrowser as Control;
		}

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
				catch(FileNotFoundException)
				{
					//Fallback to geckofx version 14 name
					GeckoCoreAssembly = Assembly.LoadFrom("geckofx-core-14.dll");
				}
				try
				{
					GeckoWinAssembly = Assembly.Load("Geckofx-Winforms");
				}
				catch(FileNotFoundException)
				{
					//Fallback to geckofx version 14 name
					GeckoWinAssembly = Assembly.LoadFrom("Geckofx-Winforms-14.dll");
				}
			}
			catch(Exception e)
			{
				MessageBox.Show("Unable to load geckofx dependancy. Files may not have been included in the build.",
					"Failed to load geckofx", MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new ApplicationException("Unable to load geckofx dependancy", e);
			}
		}

		private T GetBrowserProperty<T>(object webBrowser, string propertyName)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var property = webBrowserType.GetProperty(propertyName, typeof(T));
			return (T)property.GetValue(webBrowser, BindingFlags.Default, null, null, null);
		}

		private void SetBrowserProperty<T>(object webBrowser, string propertyName, T propertyValue)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var property = webBrowserType.GetProperty(propertyName, typeof(T));
			property.SetValue(webBrowser, propertyValue, BindingFlags.Default, null, null, null);
		}

		/// <summary>
		/// Call a browser method which returns a specific type.
		/// Looks up the method name by reflection and calls that method
		/// on the given webbrowser instance and return the value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="webBrowser"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		private T CallBrowserMethod<T>(object webBrowser, string methodName)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var method = webBrowserType.GetMethod(methodName);
			return (T)method.Invoke(webBrowser, BindingFlags.Default, null, null, null);
		}

		private static bool IsXpcomInitialized()
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var initProp = xpcomType.GetProperty("IsInitialized");
			var initialized = initProp.GetValue(null, BindingFlags.Static, null, null, null);
			return (bool)initialized;
		}

		private static void InitializeXpcom(string xulRunnerPath)
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var initMethod = xpcomType.GetMethod("Initialize", new [] { typeof(string) });
			initMethod.Invoke(null, new object[] {xulRunnerPath});
		}

		private static void ShutdownXpcom()
		{
			var xpcomType = GeckoCoreAssembly.GetType(XpcomType);
			var initMethod = xpcomType.GetMethod("Shutdown");
			initMethod.Invoke(null, null);
		}

		/// <summary>
		/// Look up a method name from the browser that matches the method name and
		/// the type of the parameters given and then call that on the given webbrowser
		/// instance.
		/// </summary>
		/// <param name="webBrowser"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		private bool CallBrowserMethod(object webBrowser, string methodName, object[] parameters)
		{
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var types = new Type[parameters.Length];
			for(var i = 0; i < parameters.Length; ++i)
			{
				types[i] = parameters[i].GetType();
			}
			var method = webBrowserType.GetMethod(methodName, types);
			if(method == null)
			{
				return false;
			}
			method.Invoke(webBrowser, parameters);
			return true;
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

		private void DomClickHandler(object sender, EventArgs e)
		{
			var callbacks = _webBrowser.Parent as IWebBrowserCallbacks;
			callbacks.OnDomClick(e);
		}

		internal void SetClass(object target, string val)
		{
			if (target == null)
				return;
			// target.SetAttribute("class", "selected")
			var elementType = GeckoCoreAssembly.GetType("Gecko.GeckoHtmlElement");
			var setAttrMethod = elementType.GetMethod("SetAttribute");
			setAttrMethod.Invoke(target, new object[] {"class", val});
		}

		#region IWebBrowser Members

		/// <summary>
		/// Rather then adding more reflective methods just handle this property here at the adapter level.
		/// </summary>
		public bool AllowNavigation { get; set; }

		/// <summary>
		/// TODO: If this is necessary for the GeckoBrowser we need to figure out an implementation
		/// </summary>
		public bool AllowWebBrowserDrop { get; set; }

		public bool CanGoBack
		{
			get { return AllowNavigation && GetBrowserProperty<bool>(_webBrowser, "CanGoBack"); }
		}

		public bool CanGoForward
		{
			get { return AllowNavigation && GetBrowserProperty<bool>(_webBrowser, "CanGoForward"); }
		}

		public string DocumentText
		{
			set
			{
				// we used to use LoadContent and fall back to LoadHtml if that method didn't
				// work. However, I (EB) couldn't get LoadContent to work, so we now always use
				// LoadHtml which should work in most cases unless it is a complex HTML page.
				CallBrowserMethod(_webBrowser, "LoadHtml", new object[] { value });
			}
		}

		public string DocumentTitle
		{
			get { return GetBrowserProperty<string>(_webBrowser, "DocumentTitle"); }
		}

		public bool Focused
		{
			get { return _webBrowser.Focused; }
		}

		public bool IsBusy
		{
			get { return GetBrowserProperty<bool>(_webBrowser, "IsBusy"); }
		}

		public bool IsWebBrowserContextMenuEnabled
		{
			get { return !GetBrowserProperty<bool>(_webBrowser, "NoDefaultContextMenu"); }
			set { SetBrowserProperty(_webBrowser, "NoDefaultContextMenu", !value); }
		}

		public string StatusText
		{
			get { return GetBrowserProperty<string>(_webBrowser, "StatusText"); }
		}

		public Uri Url
		{
			get { return GetBrowserUrl(_webBrowser); }
			set { CallBrowserMethod(_webBrowser, "Navigate", new object[] { value.OriginalString }); }
		}

		public bool GoBack()
		{
			if(AllowNavigation)
				return CallBrowserMethod<bool>(_webBrowser, "GoBack");
			return false;
		}

		public bool GoForward()
		{
			if(AllowNavigation)
				return CallBrowserMethod<bool>(_webBrowser, "GoForward");
			return false;
		}

		public void Navigate(string urlString)
		{
			CallBrowserMethod(_webBrowser, "Navigate", new object[] { urlString });
		}

		public void Navigate(Uri url)
		{
			Navigate(url.AbsoluteUri);
		}

		public void Refresh()
		{
			_webBrowser.Refresh();
		}

		public void Refresh(WebBrowserRefreshOption opt)
		{
			_webBrowser.Refresh();
		}

		public void Stop()
		{
			CallBrowserMethod(_webBrowser, "Stop", new object[] {});
		}

		public void ScrollLastElementIntoView()
		{
			var geckoDocumentType = GeckoCoreAssembly.GetType("Gecko.GeckoDocument");
			var geckoHtmlElementType = GeckoCoreAssembly.GetType("Gecko.GeckoHtmlElement");
			var geckoNodeListType = GeckoCoreAssembly.GetType("Gecko.GeckoNodeCollection");
			var webBrowserType = GeckoWinAssembly.GetType(GeckoBrowserType);
			var documentProperty = webBrowserType.GetProperty("Document", geckoDocumentType);
			var document = documentProperty.GetValue(_webBrowser, BindingFlags.Default, null, null, null);
			if(document != null)
			{
				var bodyProperty = geckoDocumentType.GetProperty("Body", geckoHtmlElementType);
				var body = bodyProperty.GetValue(document, BindingFlags.Default, null, null, null);
				if(body != null)
				{
					var childrenProperty = geckoHtmlElementType.GetProperty("ChildNodes", geckoNodeListType);
					var children = childrenProperty.GetValue(body, BindingFlags.Default, null, null, null);
					var countLengthProp = geckoNodeListType.GetProperty("Length", typeof(int));
					var countLength = (int)countLengthProp.GetValue(children, BindingFlags.Default, null, null, null);
					if(countLength > 0)
					{
						var lastChildProp = geckoNodeListType.GetProperty("Item"); //Magic
						var lastchild = lastChildProp.GetValue(children, new object[] { countLength - 1 });
						var scrollIntoView = geckoHtmlElementType.GetMethod("ScrollIntoView");
						scrollIntoView.Invoke(lastchild, BindingFlags.Default, null, null, null);
					}
				}
			}
		}

		public object NativeBrowser
		{
			get { return _webBrowser; }
		}

		/// <summary>
		/// TODO: If Gecko browsers have keyboard shortcuts, write some code to enable/disable them here.
		/// </summary>
		public bool WebBrowserShortcutsEnabled { get; set; }

		#endregion
	}
}
