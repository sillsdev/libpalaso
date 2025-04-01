using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SIL.PlatformUtilities;

//This cam from a comment by Henrik Stromberg, posted on at http://www.codeproject.com/Messages/2238408/Like-the-code-below-you-dont-have-to-call-StartWat.aspx
namespace SIL.Windows.Forms.ImageToolbox
{
	/// <summary>
	/// Acts like a normal OpenFileDialog, but with the addition of letting you set the initial view (e.g. to Large_Icons)
	/// On Linux, this will use the GTK file open dialog instead of the WinForms dialog.
	/// </summary>
	public class OpenFileDialogWithViews : Component
	{

		#region Dll import

		[DllImport("user32.dll", EntryPoint = "SendMessageA", CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Ansi)]
		private static extern uint SendMessage(uint Hdc, uint Msg_Const, uint wParam, uint lParam);

		[DllImport("user32.dll", EntryPoint = "FindWindowExA", CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Ansi)]
		private static extern uint FindWindowEx(uint hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow", CallingConvention = CallingConvention.StdCall,
			CharSet = CharSet.Ansi)]
		private static extern uint GetForegroundWindow();

		#endregion


		public enum DialogViewTypes
		{
/*			LargeIcons = 0x7029,
			List = 0x702b,
			Details = 0x702c,
			Thumbnails = 0x702d, // Try between 0x7031 and 0x702d
			SmallIcons = 0x702a
 */
			//comment suggested these were correct for windows 7
			Details = 0x704B,
			Tiles = 0x704C,
			Extra_Large_Icons = 0x704D,
			Medium_Icons = 0x704E,
			Large_Icons = 0x704F,
			Small_Icons = 0x7050,
			List = 0x7051,
			Content = 0x7052
		}

		private bool m_IsWatching = false;
		private DialogViewTypes m_DialogViewTypes;
		private const int WM_COMMAND = 0x0111;
		private DialogAdapters.OpenFileDialogAdapter m_OpenFileDialog;

		public OpenFileDialogWithViews()
		{
			m_IsWatching = false;
			DialogViewType = DialogViewTypes.Small_Icons;
		}

		public OpenFileDialogWithViews(DialogViewTypes dialogViewType)
			: this()
		{
			DialogViewType = dialogViewType;
		}

		private DialogViewTypes DialogViewType
		{
			get { return m_DialogViewTypes; }
			set { m_DialogViewTypes = value; }
		}

		private DialogAdapters.OpenFileDialogAdapter OpenFileDialog
		{
			get
			{
				if (m_OpenFileDialog == null)
				{
					m_OpenFileDialog = new DialogAdapters.OpenFileDialogAdapter();
					if (DialogAdapters.CommonDialogAdapter.UseGtkDialogs)
					{
						DialogAdapters.CommonDialogAdapter.WindowType = Platform.IsCinnamon ?
							DialogAdapters.CommonDialogAdapter.WindowTypeHintAdaptor.Dialog :
							DialogAdapters.CommonDialogAdapter.WindowTypeHintAdaptor.Utility;
						DialogAdapters.CommonDialogAdapter.ForceKeepAbove = true;
					}
				}
				return m_OpenFileDialog;
			}

		}

		public bool Multiselect
		{
			get { return OpenFileDialog.Multiselect; }
			set { OpenFileDialog.Multiselect = value; }
		}

		public bool ReadOnlyChecked
		{
			get { return OpenFileDialog.ReadOnlyChecked; }
			set { OpenFileDialog.ReadOnlyChecked = value; }
		}


		public bool ShowReadOnly
		{
			get { return OpenFileDialog.ShowReadOnly; }
			set { OpenFileDialog.ShowReadOnly = value; }
		}

		public Stream OpenFile()
		{
			return OpenFileDialog.OpenFile();
		}

		[DefaultValue(true)]
		public bool AddExtension
		{
			get { return OpenFileDialog.AddExtension; }
			set { OpenFileDialog.AddExtension = value; }
		}

		[DefaultValue(true)]
		public bool CheckPathExists
		{
			get { return OpenFileDialog.CheckPathExists; }
			set { OpenFileDialog.CheckPathExists = value; }
		}

		[DefaultValue("")]
		public string DefaultExt
		{
			get { return OpenFileDialog.DefaultExt; }
			set { OpenFileDialog.DefaultExt = value; }
		}

		[DefaultValue(true)]
		public bool DereferenceLinks
		{
			get { return OpenFileDialog.DereferenceLinks; }
			set { OpenFileDialog.DereferenceLinks = value; }
		}

		[DefaultValue("")]
		public string FileName
		{
			get { return OpenFileDialog.FileName; }
			set { OpenFileDialog.FileName = value; }
		}

		[DesignerSerializationVisibility(0)]
		[Browsable(false)]
		public string[] FileNames
		{
			get { return OpenFileDialog.FileNames; }
		}

		[DefaultValue("")]
		[Localizable(true)]
		public string Filter
		{
			get { return OpenFileDialog.Filter; }
			set { OpenFileDialog.Filter = value; }
		}

		[DefaultValue(1)]
		public int FilterIndex
		{
			get { return OpenFileDialog.FilterIndex; }
			set { OpenFileDialog.FilterIndex = value; }
		}

		[DefaultValue("")]
		public string InitialDirectory
		{
			get { return OpenFileDialog.InitialDirectory; }
			set { OpenFileDialog.InitialDirectory = value; }
		}


		[DefaultValue(false)]
		public bool RestoreDirectory
		{
			get { return OpenFileDialog.RestoreDirectory; }
			set { OpenFileDialog.RestoreDirectory = value; }
		}

		[DefaultValue(false)]
		public bool ShowHelp
		{
			get { return OpenFileDialog.ShowHelp; }
			set { OpenFileDialog.ShowHelp = value; }
		}

		[DefaultValue(false)]
		public bool SupportMultiDottedExtensions
		{
			get { return OpenFileDialog.SupportMultiDottedExtensions; }
			set { OpenFileDialog.SupportMultiDottedExtensions = value; }
		}

		[Localizable(true)]
		[DefaultValue("")]

		public string Title
		{
			get { return OpenFileDialog.Title; }
			set { OpenFileDialog.Title = value; }
		}

		[DefaultValue(true)]
		public bool ValidateNames
		{
			get { return OpenFileDialog.ValidateNames; }
			set { OpenFileDialog.ValidateNames = value; }
		}

		private void StartWatching()
		{
			m_IsWatching = true;

			if (!Platform.IsWindows)
				return;

			var t = new Thread(CheckActiveWindow);
			t.Start();
		}

		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}


		public DialogResult ShowDialog(IWin32Window owner)
		{
			StartWatching();

			DialogResult dialogResult = OpenFileDialog.ShowDialog(owner);

			StopWatching();

			return dialogResult;

		}

		private void StopWatching()
		{
			m_IsWatching = false;
		}

		private void CheckActiveWindow()
		{
			Debug.Assert(Platform.IsWindows, "Should not be called on Linux!");

			lock (this)
			{
				uint listviewHandle = 0;

				while (listviewHandle == 0 && m_IsWatching)
				{
					listviewHandle = FindWindowEx(GetForegroundWindow(), 0, "SHELLDLL_DefView", "");
				}

				if (listviewHandle != 0)
				{
					SendMessage(listviewHandle, WM_COMMAND, (uint) this.DialogViewType, 0);
				}
			}
		}
	}

}
