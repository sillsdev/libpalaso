using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Widgets;
using Palaso.i18n;


namespace Palaso.UI.WindowsForms.i18n
{
	[Designer("LocalizationHelperDesigner, PalsoUIWindowsFormsDesign")]
	[ToolboxItem(true)]
	[ProvideProperty("ParentFo", typeof (Form))]
	public partial class LocalizationHelper: Component, ISupportInitialize, IExtenderProvider
	{
		private readonly Dictionary<Control, TextFontPair> _originalControlProperties =
			new Dictionary<Control, TextFontPair>();

		private bool _alreadyChanging;
		private bool _wiredToParent;
		private Control _parent;
		#if DEBUG
		private string _nameOfParentContainer;
		private StackTrace _constructionStackTrace;
		#endif

		public LocalizationHelper()
		{
			InitializeComponent();
		}

		public LocalizationHelper(IContainer container)
		{
			if (container != null)
			{
				container.Add(this);
			}
			InitializeComponent();

#if DEBUG
			_constructionStackTrace = new StackTrace();
#endif
		}

		public Control Parent
		{
			get { return _parent; }
			set
			{
				if (_parent == value)
				{
					return;
				}
				if (_wiredToParent && _parent != null)
				{
					UnwireFromControl(_parent);
					UnwireFromChildren(_parent);
				}
				_parent = value;
				if (_wiredToParent && _parent != null)
				{
					WireToControl(_parent);
					WireToChildren(_parent);
				}
			}
		}

		private void OnFontChanged(object sender, EventArgs e)
		{
			if (_alreadyChanging)
			{
				return;
			}
			Control control = (Control) sender;
			_alreadyChanging = true;
			_originalControlProperties[control].Font = control.Font;
			var hints = control as ILocalizableControl;
			if (hints==null || hints.ShouldModifyFont)
			{
				if (control is LinkLabel && ((LinkLabel)control).UseMnemonic == false)
				{
					//then that link is for user data, and shouldn't be localized (this came up in Chorus AnnotationEditorView)
				}
				else
				{
					Console.WriteLine("Control {0} with text {0} being fontalized.", control.GetType().Name, control.Text);
					var font = StringCatalog.ModifyFontForLocalization(control.Font);
					control.Font = font;
				}
			}
			_alreadyChanging = false;
		}

		private void OnTextChanged(object sender, EventArgs e)
		{
			if (_alreadyChanging)
			{
				return;
			}
			Control control = (Control) sender;

			if (control.Text.Contains("{0}"))
			{
				return;
				//they're going to have to format it anyways, so we can't fix it automatically
			}

			_alreadyChanging = true;
			_originalControlProperties[control].Text = control.Text;
			if (!String.IsNullOrEmpty(control.Text))
				//don't try to translation, for example, buttons with no label
			{
				control.Text = StringCatalog.Get(control.Text);
			}
			_alreadyChanging = false;
		}

		private void OnControlAdded(object sender, ControlEventArgs e)
		{
			WireToControl(e.Control);
			WireToChildren(e.Control);
		}

		private void OnControlRemoved(object sender, ControlEventArgs e)
		{
			UnwireFromControl(e.Control);
			UnwireFromChildren(e.Control);
		}

		private void OnControlDisposed(object sender, EventArgs e)
		{
			Control control = (Control) sender;
			if (control != Parent)
			{
				UnwireFromControl(control);
			}
			UnwireFromChildren(control);
		}

		private void WireToChildren(Control control)
		{
			control.SuspendLayout();
			Console.WriteLine("Suspended Control {0} with name {1}", control, control.Name);
			Debug.Assert(control != null);
			//Debug.WriteLine("Wiring to children of " + control.Name);
			control.ControlAdded += OnControlAdded;
			control.ControlRemoved += OnControlRemoved;
			control.Disposed += OnControlDisposed;
			foreach (Control child in control.Controls)
			{
				WireToControl(child);
				WireToChildren(child);
			}
			control.ResumeLayout();
			Console.WriteLine("Resumed Control {0} with name {1}", control, control.Name);
		}

		private void WireToControl(Control control)
		{
			#if DEBUG
				_nameOfParentContainer = control.Name;
			#endif

			Debug.Assert(control != null);
			if (IsAllowedControl(control))
			{
				// Debug.WriteLine("Wiring to " + control.Name);
				control.TextChanged += OnTextChanged;
				control.FontChanged += OnFontChanged;
				_originalControlProperties.Add(control, new TextFontPair(control.Text, control.Font));
				OnTextChanged(control, null);
				OnFontChanged(control, null);
			}
		}

		private void UnwireFromChildren(Control control)
		{
			Debug.Assert(control != null);
			control.ControlAdded -= OnControlAdded;
			control.ControlRemoved -= OnControlRemoved;
			control.Disposed -= OnControlDisposed;
			//Debug.WriteLine("Unwiring from children of " + control.Name);
			foreach (Control child in control.Controls)
			{
				UnwireFromControl(child);
				UnwireFromChildren(child);
			}
		}

		private void UnwireFromControl(Control control)
		{
			Debug.Assert(control != null);
			// In modal forms mono can call Dispose twice, resulting in two attempts to unwire
			// the control.
			if (IsAllowedControl(control) && _originalControlProperties.ContainsKey(control))//Added because once, on mono (WS-14891) somehow this wasn't true (probably not this control's fault, but still...))
			{
				control.TextChanged -= OnTextChanged;
				control.FontChanged -= OnFontChanged;
				control.Text = _originalControlProperties[control].Text;
				control.Font = _originalControlProperties[control].Font;
				_originalControlProperties.Remove(control);
			}
		}

		private static bool IsAllowedControl(Control control)
		{
			return control is Label ||
				   control is GroupBox ||
				   control is ButtonBase ||
				   control is IButtonControl ||
				   control is TabControl ||
				   control is TabPage ||
				   control is Form ||
				   control is BetterLabel;
		}

		#region ISupportInitialize Members

		///<summary>
		///Signals the object that initialization is starting.
		///</summary>
		///
		public void BeginInit() {}

		///<summary>
		///Signals the object that initialization is complete.
		///</summary>
		///
		public void EndInit()
		{
			if (DesignMode)
				return;

			if (!_wiredToParent && Parent != null)
			{
				_wiredToParent = true;
				WireToControl(_parent);
				WireToChildren(Parent);
			}
		}

		#endregion

		#region IExtenderProvider Members

		///<summary>
		///Specifies whether this object can provide its extender properties to the specified object.
		///</summary>
		///
		///<returns>
		///true if this object can provide extender properties to the specified object; otherwise, false.
		///</returns>
		///
		///<param name="extendee">The <see cref="T:System.Object"></see> to receive the extender properties. </param>
		public bool CanExtend(object extendee)
		{
			return (extendee is UserControl);
		}

		#endregion

		private class TextFontPair
		{
			private string _text;
			private Font _font;

			public TextFontPair(string text, Font font)
			{
				_text = text;
				_font = font;
			}

			public string Text
			{
				get { return _text; }
				set { _text = value; }
			}

			public Font Font
			{
				get { return _font; }
				set { _font = value; }
			}
		}



		~LocalizationHelper()
		{
			if (Parent !=null)
			{

#if DEBUG
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName + ".  Parent container name was '"+     _nameOfParentContainer+"'.\r\n"+this._constructionStackTrace.ToString());
#else
				;
#endif
			}

		}
	}


}