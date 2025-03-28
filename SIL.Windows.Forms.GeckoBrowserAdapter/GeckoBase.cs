// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.PlatformUtilities;
using SIL.Windows.Forms.Widgets;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.GeckoBrowserAdapter
{
	public class GeckoBase : UserControl
	{
		internal ReflectiveGeckoBrowser _browser;
		protected WritingSystemDefinition _writingSystem;
		protected bool _browserIsReadyToNavigate;
		protected bool _browserDocumentLoaded;
		protected EventHandler _loadHandler;
		protected EventHandler<EventArgs> _domKeyUpHandler;
		protected EventHandler<EventArgs> _domClickHandler;
		protected EventHandler<EventArgs> _domKeyDownHandler;
		protected EventHandler<EventArgs> _domFocusHandler;
		protected EventHandler<EventArgs> _domBlurHandler;
		protected EventHandler<EventArgs> _domDocumentCompletedHandler;
		protected EventHandler _backColorChangedHandler;
		protected EventHandler _foreColorChangedHandler;
		public event EventHandler UserLostFocus;
		public event EventHandler UserGotFocus;
		protected bool _inFocus;
		protected string _nameForLogging;
		protected bool _handleEnter;
		private Timer _timer;
		internal GeckoHtmlElement _focusElement;
		/// <summary>
		/// Set to true when OnEnter has been called, but OnLeave hasn't yet been called.
		/// </summary>
		bool _entered = false;

		public GeckoBase()
		{
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			ReadOnly = false;

			_inFocus = false;
			_handleEnter = true;
			_browser = new ReflectiveGeckoBrowser();
			_browser.Dock = DockStyle.Fill;
			_browser.Parent = this;
			_browser.NoDefaultContextMenu = true;

			SelectionStart = 0;  // Initialize value;

			_loadHandler = new EventHandler(OnGeckoBox_Load);
			this.Load += _loadHandler;
			Controls.Add(_browser.BrowserControl);

			_domDocumentCompletedHandler = new EventHandler<EventArgs>(OnDomDocumentCompleted);
			_browser.DocumentCompleted += _domDocumentCompletedHandler;
			_domFocusHandler = new EventHandler<EventArgs>(OnDomFocus);
			_browser.DomFocus += _domFocusHandler;
			_domKeyUpHandler = new EventHandler<EventArgs>(OnDomKeyUp);
			_browser.DomKeyUp += _domKeyUpHandler;
			_domKeyDownHandler = new EventHandler<EventArgs>(OnDomKeyDown);
			_browser.DomKeyDown += _domKeyDownHandler;
			_domBlurHandler = new EventHandler<EventArgs>(OnDomBlur);
			_browser.DomBlur += _domBlurHandler;
			_backColorChangedHandler = new EventHandler(OnBackColorChanged);
			this.BackColorChanged += _backColorChangedHandler;
			_foreColorChangedHandler = new EventHandler(OnForeColorChanged);
			this.ForeColorChanged += _foreColorChangedHandler;
			if (Platform.IsWindows)
				return;

			_domClickHandler = OnDomClick;
			_browser.DomClick += _domClickHandler;
		}
		public void Init(WritingSystemDefinition writingSystem, String name)
		{
			WritingSystem = writingSystem;
			_nameForLogging = name;
			Name = name;
		}
		protected virtual void Closing()
		{
			this.Load -= _loadHandler;
			this.BackColorChanged -= _backColorChangedHandler;
			this.ForeColorChanged -= _foreColorChangedHandler;
			_focusElement = null;
			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}
			if (_browser != null)
			{
				_browser.DomKeyDown -= _domKeyDownHandler;
				_browser.DomKeyUp -= _domKeyUpHandler;
				_browser.DomFocus -= _domFocusHandler;
				_browser.DomBlur -= _domBlurHandler;
				_browser.DocumentCompleted -= _domDocumentCompletedHandler;

				if (!Platform.IsWindows)
					_browser.DomClick -= _domClickHandler;

				_browser.Dispose();
				_browser = null;
			}
			_loadHandler = null;
			_domKeyDownHandler = null;
			_domKeyUpHandler = null;
			_domFocusHandler = null;
			_domDocumentCompletedHandler = null;
			_domClickHandler = null;
		}
		protected virtual void AdjustHeight()
		{
			if (_browser.Document == null)
			{
				return;
			}
			var content = (GeckoBodyElement)_browser.Document.GetElementById("mainbody");
			if (content != null)
			{
				Height = content.Parent.ScrollHeight;
			}
		}
		public virtual WritingSystemDefinition WritingSystem
		{
			get
			{
				return _writingSystem;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_writingSystem = value;
				Font = _writingSystem.CreateDefaultFont();
			}
		}
		public void AssignKeyboardFromWritingSystem()
		{
			_writingSystem?.LocalKeyboard?.Activate();
		}

		public void ClearKeyboard()
		{
			if (_writingSystem == null)
				return;
			Keyboard.Controller.ActivateDefaultKeyboard();
		}
		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendLostFocus()
		{
			OnLostFocus(new EventArgs());
		}

		/// <summary>
		/// for automated tests
		/// </summary>
		public void PretendSetFocus()
		{
			Debug.Assert(_browser != null, "_browser != null");
			_browser.Focus();
		}

		protected virtual void OnDomKeyDown(object sender, EventArgs ea)
		{
			DomKeyEventArgs e = new DomKeyEventArgs(ea);
			if (_inFocus)
			{
				if (_handleEnter && !MultiParagraph && e.KeyCode == (uint)Keys.Enter) // carriage return
				{
					e.Handled = true;
				}

				if (!Platform.IsWindows)
					SendKey(e);
				else
				{
					if ((e.KeyCode == (uint)Keys.Tab) && !e.CtrlKey && !e.AltKey)
					{
						e.Handled = true;
						if (e.ShiftKey)
						{
							if (!ParentForm.SelectNextControl(this, false, true, true, true))
							{
								Debug.WriteLine("Failed to advance");
							}
						}
						else
						{
							if (!ParentForm.SelectNextControl(this, true, true, true, true))
							{
								Debug.WriteLine("Failed to advance");
							}
						}
						e.Handled = true;
						return;
					}
				}

				OnKeyDown(new KeyEventArgs((Keys)e.KeyCode));
			}
		}

		protected virtual void SendKey(DomKeyEventArgs e)
		{
			var builder = new StringBuilder();
			switch (e.KeyCode)
			{
				case (uint)Keys.Tab:
					if (e.CtrlKey)
					{
						builder.Append("^");
					}
					if (e.AltKey)
					{
						builder.Append("%");
					}
					if (e.ShiftKey)
					{
						builder.Append("+");
					}
					builder.Append("{TAB}");
					break;
				case (uint)Keys.Up:
					builder.Append("{UP}");
					break;
				case (uint)Keys.Down:
					builder.Append("{DOWN}");
					break;
				case (uint)Keys.Left:
					builder.Append("{LEFT}");
					break;
				case (uint)Keys.Right:
					builder.Append("{RIGHT}");
					break;
				case (uint)Keys.Escape:
					builder.Append("{ESC}");
					break;
				case (uint)Keys.N:
					if (e.CtrlKey)
					{
						builder.Append("^n");
					}
					break;
				case (uint)Keys.F:
					if (e.CtrlKey)
					{
						builder.Append("^f");
					}
					break;
				case (uint)Keys.Delete:
					builder.Append("{DEL}");
					break;
				case (uint)Keys.Enter:
					builder.Append("{ENTER}");
					break;
			}
			string result = builder.ToString();
			if (!string.IsNullOrEmpty(result))
				SendKeys.Send(result);
		}
		public virtual bool InFocus
		{
			get => _inFocus;
			set => _inFocus = value;
		}

		public virtual bool Bold { get; set; }

		public virtual int SelectionStart { get; set; }
		public int SelectionLength { get; set; }
		public virtual bool Multiline { get; set; }
		public virtual bool MultiParagraph { get; set; }
		public virtual bool ReadOnly { get; set; }
		public virtual bool WordWrap { get; set; }
		public virtual View View { get; set; }
		public virtual DrawMode DrawMode { get; set; }
		public virtual FlatStyle FlatStyle { get; set; }

		protected virtual void OnDomBlur(object sender, EventArgs ea)
		{
			if (_inFocus)
			{
				_inFocus = false;
				UserLostFocus?.Invoke(this, null);
			}
		}
		protected virtual void OnDomFocus(object sender, EventArgs ea)
		{
			DomEventArgs e = new DomEventArgs(ea);
			if (!_browserDocumentLoaded)
				return;

			// Only handle DomFocus that occurs on an Element.
			// This is important, or it will mess with IME keyboard focus.
			if (e == null || e.Target == null || e.Target.CastToGeckoElement() == null)
				return;

			var content = _browser.Document.GetElementById("main");
			if (content is GeckoHtmlElement geckoHtmlElement && !_inFocus)
			{
				// The following is required because we get two in focus events every time this
				// is entered. This is normal for Gecko. But I don't want to be constantly
				// refocusing.
				_inFocus = true;
				EnsureFocusedGeckoControlHasInputFocus();
				_browser?.SetInputFocus();
				_focusElement = geckoHtmlElement;
				ChangeFocus();
			}
		}
		protected virtual void OnDomDocumentCompleted(object sender, EventArgs ea)
		{
			_browserDocumentLoaded = true;  // Document loaded once
			AdjustHeight();
			if (_entered)
			{
				this.Focus();

				IContainerControl containerControl = GetContainerControl();

				if (containerControl != null && containerControl != this && containerControl.ActiveControl != this)
					containerControl.ActiveControl = this;

				_browser.WebBrowserFocus.Activate();
				var element = (GeckoHtmlElement)_browser.Document.GetElementById("main");
				element.Focus();

				EnsureFocusedGeckoControlHasInputFocus();
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			_browser?.RemoveInputFocus();
			_browser?.WebBrowserFocus?.Deactivate();
			_entered = false;

			base.OnLeave(e);
			if (Platform.IsWindows)
				return;

			Action EnsureXInputFocusIsRemovedFromReceivedWinFormsControl = () =>
			{
				var control = Control.FromHandle(NativeReplacements.MonoGetFocus());
				var controlName = control.GetType().FullName;
				if (controlName == "Gecko.GeckoWebBrowser")
				{
					return;
				}
				MoveInputFocusBackToAWinFormsControl();

				// Setting the ActiveControl ensure a Focus event occurs on the control focus is moving to.
				// And this allows us to call RemoveinputFocus at the necessary time.
				// This prevents keypress still going to the gecko controls when a winform TextBox has focus
				// and the mouse is over a gecko control.
				Form.ActiveForm.ActiveControl = control;
				EventHandler focusEvent = null;
				// Attach an execute-once only focus handler to the Non GeckoWebBrowser control focus is moving too...
				focusEvent = (object sender, EventArgs eventArg) =>
				{
					control.GotFocus -= focusEvent;
					MoveInputFocusBackToAWinFormsControl();
				};

				control.GotFocus += focusEvent;
			};

			ProgressUtils.InvokeLaterOnUIThread(
				() => EnsureXInputFocusIsRemovedFromReceivedWinFormsControl());
		}

		protected void ChangeFocus()
		{
			_focusElement.Focus();
			if (UserGotFocus != null)
			{
				UserGotFocus.Invoke(this, null);
			}
		}

		protected override void OnEnter(EventArgs e)
		{
			_entered = true;

			// Do this only if the control's html has loaded.
			if (_browserDocumentLoaded)
			{
				_browser.WebBrowserFocus.Activate();
				EnsureFocusedGeckoControlHasInputFocus();
			}
			base.OnEnter(e);
		}
		/// <summary>
		/// This gives a move sensible result than the default winform implementation.
		/// </summary>
		/// <value><c>true</c> if focused; otherwise, <c>false</c>.</value>
		public override bool Focused
		{
			get
			{
				return base.Focused || ContainsFocus;
			}
		}
		/// <summary>
		/// If Browser control doesn't have X11 input focus.
		/// then Ensure that it does..
		/// </summary>
		protected void EnsureFocusedGeckoControlHasInputFocus()
		{
			if ((_browser == null) || (_browser.HasInputFocus()))
			{
				return;
			}

			// Attempt to do it right away.
			this._browser.SetInputFocus();

			// Otherwise do it on the first idle event.
			Action setInputFocus = null;
			setInputFocus = () =>
			{
				ProgressUtils.InvokeLaterOnIdle(() =>
				{
					if (_browser == null)
					{
						return;
					}
					if (Form.ActiveForm == null || _browser.ContainsFocus == false)
					{
						MoveInputFocusBackToAWinFormsControl();
						return;
					}

					if (!_browser.HasInputFocus())
						this._browser.SetInputFocus();
					if (!_browser.HasInputFocus())
						setInputFocus();
				}, null);
			};
			if (!_browser.HasInputFocus())
				setInputFocus();
		}

		public string GetLanguageHtml(WritingSystemDefinition ws)
		{
			String langName = "";
			// Add in the ISO language code in case font supports multiple regions
			if (ws != null)
			{
				String lang = ws.LanguageTag.IndexOf('-') == -1 ? ws.LanguageTag : ws.LanguageTag.Substring(0, ws.LanguageTag.IndexOf('-'));
				langName = "lang='" + lang + "' ";
			}
			return langName;
		}

		/// <summary>
		/// Set InputFocus to a WinForm controls using Mono winforms connection to the X11 server.
		/// GeckoWebBrowser.RemoveInputFocus uses the Gecko/Gtk connection to the X11 server.
		/// The undoes the call to _browser.SetInputFocus.
		/// Call this method when a winform control has gained focus, and X11 Input focus is still on the Gecko control.
		/// </summary>
		protected static void MoveInputFocusBackToAWinFormsControl()
		{
			if (Platform.IsWindows)
				return;

			IntPtr newTargetHandle = NativeReplacements.MonoGetFocus();
			IntPtr displayHandle = NativeReplacements.MonoGetDisplayHandle();

			// Remove the Focus from a Gtk window back to a mono winform X11 window.
			NativeX11Methods.XSetInputFocus(displayHandle, NativeReplacements.MonoGetX11Window(newTargetHandle), NativeX11Methods.RevertTo.None, IntPtr.Zero);
		}
		// Making these empty handlers rather than abstract so the class only
		// needs to implement the ones they need.
		protected virtual void OnDomKeyUp(object sender, EventArgs e)
		{
		}
		protected virtual void OnDomClick(object sender, EventArgs e)
		{
		}
		protected virtual void OnGeckoBox_Load(object sender, EventArgs e)
		{
		}
		protected virtual void OnBackColorChanged(object sender, EventArgs e)
		{
		}
		protected virtual void OnForeColorChanged(object sender, EventArgs e)
		{
		}
	}
}
